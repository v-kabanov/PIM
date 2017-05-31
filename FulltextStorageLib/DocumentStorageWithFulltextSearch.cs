// /**********************************************************************************************
// Author:  Vasily Kabanov
// Created  2017-05-26
// Comment  
// **********************************************************************************************/
// 

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using FulltextStorageLib.Util;
using log4net;

namespace FulltextStorageLib
{
    public class DocumentStorageWithFulltextSearchBase
    {
        protected static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    }

    /// <summary>
    ///     async fulltext operations
    /// </summary>
    /// <typeparam name="TDoc"></typeparam>
    /// <typeparam name="TKey">
    ///     Document key type in storage.
    /// </typeparam>
    /// <typeparam name="THeader"></typeparam>
    public class DocumentStorageWithFulltextSearch<TDoc, TKey, THeader> : DocumentStorageWithFulltextSearchBase, IDocumentStorageWithFulltextSearch<TDoc, TKey, THeader>
        where THeader : IFulltextIndexEntry
        where TDoc : class
    {
        public IMultiIndex MultiIndex => SearchEngine.MultiIndex;

        public ILuceneEntityAdapter<TDoc, THeader, TKey> EntityAdapter { get; }

        public IDocumentStorage<TDoc, TKey> Storage { get; }

        public IStandaloneFulltextSearchEngine<TDoc, THeader, TKey> SearchEngine { get; }

        public double FulltextTaskCompletion { get; private set; }

        public string FulltextStatusSummary { get; private set; }

        private ITaskExecutor FulltextBackgroundTaskExecutor { get; }

        public DocumentStorageWithFulltextSearch(IDocumentStorage<TDoc, TKey> storage, IStandaloneFulltextSearchEngine<TDoc, THeader, TKey> searchEngine)
        {
            Storage = storage;
            SearchEngine = searchEngine;

            FulltextBackgroundTaskExecutor = new TaskExecutor();
        }

        public void SaveOrUpdate(TDoc document)
        {
            Storage.SaveOrUpdate(document);

            FulltextBackgroundTaskExecutor.Schedule(() =>
            {
                SearchEngine.Add(document);
                SearchEngine.CommitFulltextIndex();
            });
        }

        public void SaveOrUpdate(params TDoc[] docs)
        {
            foreach (var doc in docs)
                Storage.SaveOrUpdate(doc);

            FulltextBackgroundTaskExecutor.Schedule(() =>
            {
                SearchEngine.Add(docs);
                SearchEngine.CommitFulltextIndex();
            });
        }

        public void Delete(params THeader[] docHeaders)
        {
            foreach (var docHeader in docHeaders)
            {
                var storageKey = EntityAdapter.GetStorageKey(docHeader);
                Log.DebugFormat("Deleting {0}#{1}", docHeader.Name, storageKey);
                Storage.Delete(storageKey);
            }

            FulltextBackgroundTaskExecutor.Schedule(() =>
            {
                SearchEngine.Delete(docHeaders);
                SearchEngine.CommitFulltextIndex();
            });
        }

        public TDoc GetExisting(TKey id)
        {
            return Storage.GetExisting(id);
        }

        public TDoc Delete(TKey id)
        {
            var result = Storage.Delete(id);

            FulltextBackgroundTaskExecutor.Schedule(() =>
            {
                if (EntityAdapter.CanConvertStorageKey)
                    SearchEngine.Delete(EntityAdapter.GetFulltextFromStorageKey(id));
                else if (result != null)
                    SearchEngine.Delete(EntityAdapter.GetFulltextKey(result));

                if (EntityAdapter.CanConvertStorageKey || result != null)
                    SearchEngine.CommitFulltextIndex();
            });

            return result;
        }

        public IEnumerable<TDoc> GetAll()
        {
            return Storage.GetAll();
        }

        public int CountAll()
        {
            return Storage.CountAll();
        }

        public IEnumerable<string> ActiveIndexNames => SearchEngine.ActiveIndexNames;

        public IList<THeader> Search(string queryText, int maxResults)
        {
            return SearchEngine.Search(queryText, maxResults);
        }

        public IList<THeader> GetTopInPeriod(DateTime? periodStart, DateTime? periodEnd, int maxResults)
        {
            return SearchEngine.GetTopInPeriod(periodStart, periodEnd, maxResults);
        }

        /// <summary>
        ///     Opens existing indexes. Repeated calls result in exception if some indexes are already opened.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        ///     Some indexes are already open.
        /// </exception>
        public void Open()
        {
            Check.DoCheckOperationValid(SearchEngine.MultiIndex.AllIndexes.Length == 0, "Already opened");

            var existingIndexes = SearchEngine.GetExistingIndexNames();

            OpenOrCreateIndexes(existingIndexes);
        }

        /// <summary>
        ///     Start work ensuring indexes for specified stemmers exist and are active.
        /// </summary>
        /// <param name="stemmerNames">
        /// </param>
        /// <param name="progressReporter">
        ///     Optional delegate receiving progress updates (completion percent 0..1)
        /// </param>
        [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
        public void OpenOrCreateIndexes(IEnumerable<string> stemmerNames, Action<double> progressReporter = null)
        {
            Check.DoRequireArgumentNotNull(stemmerNames, nameof(stemmerNames));

            var languageNames = stemmerNames.ToCaseInsensitiveSet();

            var allSupportedLangs = LuceneIndex.GetAvailableSnowballStemmers().ToCaseInsensitiveSet();

            var unknownLangs = string.Join(",", languageNames.Where(n => !allSupportedLangs.Contains(n)));

            if (!string.IsNullOrEmpty(unknownLangs))
                throw new FulltextException($"The following configured languages are not supported: {unknownLangs}");

            var newIndexes = new List<IndexInformation>();

            foreach (var stemmerName in stemmerNames)
            {
                var info = SearchEngine.AddOrOpenSnowballIndex(stemmerName);
                if (info.IsNew)
                    newIndexes.Add(info);
            }

            if (languageNames.Count > 1)
                SearchEngine.SetDefaultIndex(stemmerNames.First());

            if (newIndexes.Count > 0)
            {
                RebuildIndexes(newIndexes.Select(x => x.Name).ToArray(), progressReporter);
            }
        }

        public void RebuildIndex(string stemmerName, Action<double> progressReporter = null)
        {
            RebuildIndexes(stemmerName.WrapInList(), progressReporter);
        }

        [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
        public void RebuildIndexes(IEnumerable<string> stemmerNames, Action<double> progressReporter = null)
        {
            Check.DoRequireArgumentNotNull(stemmerNames, nameof(stemmerNames));

            FulltextBackgroundTaskExecutor.Schedule(() =>
            {
                FulltextStatusSummary = $"Rebuilding {string.Join(", ", stemmerNames)}.";
                SearchEngine.RebuildIndexes(stemmerNames, Storage.GetAll(), Storage.CountAll(),
                    (progress) =>
                    {
                        FulltextTaskCompletion = progress;
                        progressReporter?.Invoke(progress);
                    });
            });
        }

        public void RebuildAllIndexes(Action<double> progressReporter = null)
        {
            RebuildIndexes(SearchEngine.ActiveIndexNames.ToList(), progressReporter);
        }

        public void AddIndex(string stemmerName)
        {
            Log.InfoFormat($"Adding index {0}", stemmerName);

            Check.DoCheckArgument(MultiIndex.GetIndex(stemmerName) == null, () => $"Index {stemmerName} is already active");

            var info = SearchEngine.AddOrOpenSnowballIndex(stemmerName);

            SearchEngine.RebuildIndex(stemmerName, Storage.GetAll(), Storage.CountAll());

            info.LuceneIndex.Commit();
        }

        public void RemoveIndex(string stemmerName)
        {
            SearchEngine.RemoveIndex(stemmerName);
        }

        public void SetDefaultIndex(string stemmerName)
        {
            SearchEngine.SetDefaultIndex(stemmerName);
        }

        /// <summary>
        ///     Block scheduling of new tasks and wait until already scheduled ones finish.
        /// </summary>
        /// <param name="maxWaitMilliseconds">
        ///     Maximum wait time in milliseconds; must be 0, positive or -1 for infinite.
        ///     When 0, method effectively checks if there are pending tasks.
        /// </param>
        public bool WaitForFulltextBackgroundWorkToComplete(int maxWaitMilliseconds)
        {
            return FulltextBackgroundTaskExecutor.WaitForAllCurrentTasksToFinish(maxWaitMilliseconds);
        }

        /// <summary>
        ///     Factory method creating typical configuration with Couchbase Lite as storage and Lucene fulltext search engine.
        ///     Initiates opening of existing fulltext indexes.
        /// </summary>
        /// <param name="rootDirectoryPath">
        ///     Mandatory, rot directory of indexed storage. Will contain 2 subfolders 'db' with Couchbase Lite database and 'ft' with fulltext indexes.
        /// </param>
        /// <param name="couchbaseAdapter">
        ///     Adapter for generic Couchbase storage implementation (see <see cref="CouchbaseStorage{TDoc}"/>)
        /// </param>
        /// <param name="luceneAdapter">
        ///     Adapter for generic Lucene multi index search engine implementation (see <see cref="SearchEngine"/>)
        /// </param>
        /// <returns>
        ///     Instantiated storage component in need of opening (see <see cref="IDocumentStorageWithFulltextSearch{TDoc,TKey,THeader}.Open"/>).
        ///     Fulltext indexes are not yet opened in the returned instance.
        /// </returns>
        public static DocumentStorageWithFulltextSearch<TDoc, string, THeader> CreateStandard(
            string rootDirectoryPath
            , ICouchbaseDocumentAdapter<TDoc> couchbaseAdapter
            , ILuceneEntityAdapter<TDoc, THeader, string> luceneAdapter)
        {
            Check.DoRequireArgumentNotNull(rootDirectoryPath, nameof(rootDirectoryPath));
            Check.DoRequireArgumentNotNull(couchbaseAdapter, nameof(couchbaseAdapter));
            Check.DoRequireArgumentNotNull(luceneAdapter, nameof(luceneAdapter));

            var dbPath = Path.Combine(rootDirectoryPath, "db");
            var fulltextPath = Path.Combine(rootDirectoryPath, "ft");

            Directory.CreateDirectory(dbPath);
            Directory.CreateDirectory(fulltextPath);

            var storage = new CouchbaseStorage<TDoc>(rootDirectoryPath, couchbaseAdapter);
            var multiIndex = new MultiIndex(luceneAdapter.DocumentKeyName);
            var searchEngine = new SearchEngine<TDoc, THeader, string>(fulltextPath, luceneAdapter, multiIndex);

            var result = new DocumentStorageWithFulltextSearch<TDoc, string, THeader>(storage, searchEngine);

            return result;
        }

        private bool _disposedValue;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    // dispose managed state (managed objects).
                    FulltextBackgroundTaskExecutor.WaitForAllCurrentTasksToFinish(5000);
                    FulltextBackgroundTaskExecutor.Dispose();

                    Storage?.Dispose();
                    SearchEngine?.Dispose();
                }

                // free unmanaged resources (unmanaged objects) and override a finalizer below.
                // set large fields to null.

                _disposedValue = true;
            }
        }

        // override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~DocumentStorageWithFulltextSearch() {
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