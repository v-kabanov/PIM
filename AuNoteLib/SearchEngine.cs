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
    public class IndexInformation
    {
        public IndexInformation(string name)
        {
            Name = name;
        }

        public ILuceneIndex LuceneIndex { get; set; }

        /// <summary>
        ///     Just created
        /// </summary>
        public bool IsNew { get; set; }

        public string Name { get; }
    }

    /// <summary>
    ///     Generic lucene search engine supporting multiple parallel indexes per e.g. language and generic entity
    ///     with 1 or 2 searchable fields (in lucene index): text and optional time.
    /// </summary>
    public class SearchEngine<TData, THeader>
        where THeader : class
        where TData : THeader
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public string RootDirectory { get; private set; }

        public IMultiIndex MultiIndex { get; private set; }

        public ILuceneEntityAdapter<TData, THeader> EntityAdapter { get; private set; }

        private readonly string[] languages;

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
        /// <param name="name">
        ///     Index name translated into name of the folder under FT catalog directory
        /// </param>
        /// <param name="analyzer">
        /// </param>
        /// <param name="dropExisting">
        ///     Whether to drop existing index if exists
        /// </param>
        /// <returns>
        ///     The new index
        /// </returns>
        public IndexInformation AddIndex(string name, Analyzer analyzer, bool dropExisting = false)
        {
            Check.That(name).IsNotEmpty();
            Check.That(analyzer).IsNotNull();

            var result = new IndexInformation(name);

            var indexDirectoryPath = GetIndexRootFolder(name);
            var dirInfo = new DirectoryInfo(indexDirectoryPath);
            result.IsNew = !dirInfo.Exists;

            if (!result.IsNew)
                dirInfo.Create();
            else if (dropExisting)
                dirInfo.Delete(true);

            var luceneDir = LuceneIndex.PreparePersistentDirectory(indexDirectoryPath);

            var newIndex = new LuceneIndex(analyzer, luceneDir, EntityAdapter.DocumentKeyName);

            MultiIndex.AddIndex(name, newIndex);

            if (MultiIndex.IndexCount == 1)
                SetDefaultIndex(name);

            return result;
        }



        public IndexInformation AddOrOpenSnowballIndex(string snowballStemmerName)
        {
            var analyzer = LuceneIndex.CreateSnowballAnalyzer(snowballStemmerName);

            return AddIndex(snowballStemmerName, analyzer, false);
        }

        /// <summary>
        ///     Does not load all documents into memory at once - safe to invoke for big databases if <paramref name="docs"/> is lazy.
        /// </summary>
        /// <param name="indexName">
        ///     Name of the snowball stemmer (language) which is used as index name.
        /// </param>
        /// <param name="docs">
        ///     All documents in the storage.
        /// </param>
        /// <param name="progressReporter">
        ///     Optional delegate receiving number of items added so far.
        /// </param>
        public void RebuildIndex(string indexName, IEnumerable<TData> docs, Action<int> progressReporter = null)
        {
            var index = MultiIndex.GetIndex(indexName);
            Check.That(index).IsNotNull();

            index.Clear();
            index.Add(EntityAdapter.GetIndexedDocuments(docs), progressReporter);
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
            Log.Debug($"Searching '{queryText}', maxResults = {maxResults}");

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