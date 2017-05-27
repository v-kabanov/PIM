// /**********************************************************************************************
// Author:		Vasily Kabanov
// Created		2016-04-30
// Comment		
// **********************************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using log4net;
using System.Reflection;
using AuNoteLib.Util;
using Lucene.Net.Analysis;

namespace AuNoteLib
{
    public class SearchEngineBase
    {
        protected static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    }

    /// <summary>
    ///     Generic lucene search engine supporting multiple parallel indexes per e.g. language and generic entity
    ///     with 1 or 2 searchable fields (in lucene index): text and optional time.
    ///     To <see cref="MultiIndex"/> it adds bridging between source data and indexed part of it.
    /// </summary>
    public class SearchEngine<TDoc, THeader, TStorageKey> : SearchEngineBase, IStandaloneFulltextSearchEngine<TDoc, THeader, TStorageKey>
        where THeader : IFulltextIndexEntry
        where TDoc : class
    {
        public string RootDirectory { get; }

        public IMultiIndex MultiIndex { get; }

        public ILuceneEntityAdapter<TDoc, THeader, TStorageKey> EntityAdapter { get; }

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
        public SearchEngine(string rootDirectory, ILuceneEntityAdapter<TDoc, THeader, TStorageKey> entityAdapter, IMultiIndex multiIndex)
        {
            Check.DoRequireArgumentNotBlank(rootDirectory, nameof(rootDirectory));
            Check.DoRequireArgumentNotNull(entityAdapter, nameof(entityAdapter));
            Check.DoRequireArgumentNotNull(multiIndex, nameof(multiIndex));

            Check.DoCheckArgument(Directory.Exists(rootDirectory), "Root directory does not exist");

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
        public IndexInformation AddOrOpenIndex(string name, Analyzer analyzer, bool dropExisting = false)
        {
            Check.DoRequireArgumentNotBlank(name, nameof(name));
            Check.DoRequireArgumentNotNull(analyzer, nameof(analyzer));

            var result = new IndexInformation(name);

            var indexDirectoryPath = GetIndexRootFolder(name);
            var dirInfo = new DirectoryInfo(indexDirectoryPath);
            result.IsNew = !dirInfo.Exists;

            if (!result.IsNew)
                dirInfo.Create();
            else if (dropExisting)
                dirInfo.Delete(true);

            var luceneDir = LuceneIndex.PreparePersistentDirectory(indexDirectoryPath);

            var newIndex = new LuceneIndex(indexDirectoryPath, analyzer, luceneDir, EntityAdapter.DocumentKeyName);

            MultiIndex.AddIndex(name, newIndex);

            if (MultiIndex.IndexCount == 1)
                SetDefaultIndex(name);

            return result;
        }

        public IndexInformation AddOrOpenSnowballIndex(string snowballStemmerName)
        {
            var analyzer = LuceneIndex.CreateSnowballAnalyzer(snowballStemmerName);

            return AddOrOpenIndex(snowballStemmerName, analyzer, false);
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
        /// <param name="docCount">
        ///     Optional, total number of documents for progress reporting
        /// </param>
        /// <param name="progressReporter">
        ///     Optional delegate receiving number of items added so far.
        /// </param>
        public void RebuildIndex(string indexName, IEnumerable<TDoc> docs, int docCount, Action<double> progressReporter = null)
        {
            RebuildIndexes(new[] { indexName }, docs, docCount, progressReporter);
        }

        /// <summary>
        ///     Rebuild all specified indexes
        /// </summary>
        /// <param name="indexNames"></param>
        /// <param name="documents">
        ///     All documents in the database; collection not expected to be fully loaded into RAM
        /// </param>
        /// <param name="docCount">
        ///     Optional, total number of documents for progress reporting
        /// </param>
        /// <param name="progressReporter">
        ///     Optional delegate receiving progress report.
        /// </param>
        public void RebuildIndexes(IEnumerable<string> indexNames, IEnumerable<TDoc> documents, int docCount = -1, Action<double> progressReporter = null)
        {
            MultiIndex.RebuildIndexes(indexNames, EntityAdapter.GetIndexedDocuments(documents), docCount, progressReporter);
        }

        public void RemoveIndex(string name)
        {
            Check.DoRequireArgumentNotBlank(name, nameof(name));

            var path = GetIndexRootFolder(name);
            MultiIndex.RemoveIndex(name);

            Directory.Delete(path, true);
        }

        public void SetDefaultIndex(string name)
        {
            Check.DoRequireArgumentNotBlank(name, nameof(name));
            Check.DoCheckArgument(MultiIndex.GetIndex(name) != null, () => $"Index {name} does not exist.");

            MultiIndex.DefaultIndexName = name;
        }

        public void Add(params TDoc[] docs)
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
            Log.DebugFormat("Searching '{0}', maxResults = {1}", queryText, maxResults);

            var result = MultiIndex.Search(EntityAdapter.SearchFieldName, queryText, maxResults);

            return EntityAdapter.GetHeaders(result);
        }

        public IList<THeader> GetTopInPeriod(DateTime? periodStart, DateTime? periodEnd, int maxResults)
        {
            return EntityAdapter.GetHeaders(
                MultiIndex.GetTopInPeriod(EntityAdapter.TimeFieldName, periodStart, periodEnd, maxResults));
        }

        public IEnumerable<string> ActiveIndexNames { get; }

        public void Delete(params THeader[] docHeaders)
        {
            var terms = docHeaders.Select(h => EntityAdapter.GetKeyTerm(h)).ToArray();

            if (terms.Length > 0)
                MultiIndex.Delete(terms);
        }

        public void Delete(params string[] keys)
        {
            MultiIndex.Delete(keys[0]);
        }

        public void CommitFulltextIndex()
        {
            MultiIndex.Commit();
        }

        private string GetIndexRootFolder(string name)
        {
            Check.DoRequireArgumentNotBlank(name, nameof(name));

            return Path.Combine(RootDirectory, name);
        }

        private bool _disposedValue; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    // dispose managed state (managed objects).
                    MultiIndex?.Dispose();
                }

                // free unmanaged resources (unmanaged objects) and override a finalizer below.
                // set large fields to null.

                _disposedValue = true;
            }
        }

        // override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~SearchEngine() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
    }
}