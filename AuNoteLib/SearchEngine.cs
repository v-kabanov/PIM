// /**********************************************************************************************
// Author:		Vasily Kabanov
// Created		2016-04-30
// Comment		
// **********************************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using NFluent;
using log4net;
using System.Reflection;
using Lucene.Net.Analysis;

namespace AuNoteLib
{
    /// <summary>
    ///     Generic lucene search engine supporting multiple parallel indexes per e.g. language and generic entity
    ///     with 1 or 2 searchable fields (in lucene index): text and optional time.
    /// </summary>
    public class SearchEngine<TData, THeader>
        where THeader : class
        where TData : THeader
    {
        private static readonly ILog _log = LogManager.GetLogger(MethodInfo.GetCurrentMethod().DeclaringType);

        public string RootDirectory { get; private set; }

        public IMultiIndex MultiIndex { get; private set; }

        public ILuceneEntityAdapter<TData, THeader> EntityAdapter { get; private set; }

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="rootDirectory">
        ///     must exist
        /// </param>
        /// <param name="entityAdapter">
        /// </param>
        /// <param name="multiIndex">
        ///     Implements collection of named <see cref="ILuceneIndex"/> used in parallel for e.g. multi-lingual search.
        /// </param>
        public SearchEngine(string rootDirectory, ILuceneEntityAdapter<TData, THeader> entityAdapter, IMultiIndex multiIndex)
        {
            Check.That(rootDirectory).IsNotEmpty();
            Check.That(entityAdapter).IsNotNull();
            Check.That(multiIndex).IsNotNull();

            Check.That(Directory.Exists(rootDirectory));

            RootDirectory = rootDirectory;
            EntityAdapter = entityAdapter;
            MultiIndex = multiIndex;
        }

        /// <summary>
        ///     Sets index as default if it's the first one.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="analyzer"></param>
        public void AddIndex(string name, Analyzer analyzer)
        {
            Check.That(name).IsNotEmpty();
            Check.That(analyzer).IsNotNull();

            var indexDirectoryPath = GetIndexRootFolder(name);
            var dirInfo = new DirectoryInfo(indexDirectoryPath);
            if (dirInfo.Exists)
            {
                Check.That(IsDirectoryEmpty(dirInfo));
            }
            else
            {
                dirInfo.Create();
            }

            var luceneDir = LuceneIndex.PreparePersistentDirectory(indexDirectoryPath);

            var newIndex = new LuceneIndex(analyzer, luceneDir, EntityAdapter.DocumentKeyName);

            MultiIndex.AddIndex(name, newIndex);

            if (MultiIndex.IndexCount == 1)
            {
                SetDefaultIndex(name);
            }
        }

        public void RemoveIndex(string name)
        {
            Check.That(name).IsNotEmpty();

            string path = GetIndexRootFolder(name);
            MultiIndex.RemoveIndex(name);

            Directory.Delete(path, true);
        }

        public void SetDefaultIndex(string name)
        {
            Check.That(name).IsNotEmpty();
            Check.That(MultiIndex.GetIndex(name) != null);

            MultiIndex.DefaultIndexName = name;
        }

        public void Add(params TData[] docs)
        {
            var luceneDocs = EntityAdapter.GetIndexedDocuments(docs);

            MultiIndex.Add(luceneDocs);
        }

        public bool UseFuzzySearch
        {
            get { return MultiIndex.UseFuzzySearch; }
            set { MultiIndex.UseFuzzySearch = value; }
        }

        public IList<THeader> Search(string queryText, int maxResults)
        {
            _log.Debug($"Searching '{queryText}', maxResults = {maxResults}");

            var result = MultiIndex.Search(EntityAdapter.SearchFieldName, queryText, maxResults);

            return EntityAdapter.GetHeaders(result);
        }

        public IList<THeader> GetTopInPeriod(DateTime? periodStart, DateTime? periodEnd, int maxResults)
        {
            return EntityAdapter.GetHeaders(
                MultiIndex.GetTopInPeriod(EntityAdapter.TimeFieldName, periodStart, periodEnd, maxResults));
        }

        private bool IsDirectoryEmpty(DirectoryInfo dir)
        {
            return dir.GetDirectories().Length == 0 && dir.GetFiles().Length == 0;
        }

        private string GetIndexRootFolder(string name)
        {
            Check.That(name).IsNotEmpty();

            return Path.Combine(RootDirectory, name);
        }
    }
}