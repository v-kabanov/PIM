// /**********************************************************************************************
// Author:  Vasily Kabanov
// Created  2017-05-26
// Comment  
// **********************************************************************************************/
// 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using AuNoteLib.Util;
using log4net;

namespace AuNoteLib
{
    public class DocumentStorageWithFulltextSearchBase
    {
        protected static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    }

    /// <summary>
    ///     TODO: async fulltext operations
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
        private const string SnowballStemmerNameEnglish = "English";

        public string RootDirectory { get; }

        public IMultiIndex MultiIndex { get; }

        public ILuceneEntityAdapter<TDoc, THeader, TKey> EntityAdapter { get; }

        public IDocumentStorage<TDoc, TKey> Storage { get; }

        public IStandaloneFulltextSearchEngine<TDoc, THeader, TKey> SearchEngine { get; }

        //private TaskExecutor AsyncTaskExecutor = new TaskExecutor();

        public Exception LastFulltextIndexError { get; private set; }

        public void SaveOrUpdate(TDoc document)
        {
            Storage.SaveOrUpdate(document);

            SearchEngine.Add(document);
            SearchEngine.CommitFulltextIndex();
        }

        public void SaveOrUpdate(params TDoc[] docs)
        {
            foreach (var doc in docs)
                Storage.SaveOrUpdate(doc);

            SearchEngine.Add(docs);
            SearchEngine.CommitFulltextIndex();
        }

        public void Delete(params THeader[] docHeaders)
        {
            foreach (var docHeader in docHeaders)
            {
                var storageKey = EntityAdapter.GetStorageKey(docHeader);
                Log.DebugFormat("Deleting {0}#{1}", docHeader.Name, storageKey);
                Storage.Delete(storageKey);
            }

            SearchEngine.Delete(docHeaders);

            SearchEngine.CommitFulltextIndex();
        }

        public TDoc GetExisting(TKey id)
        {
            return Storage.GetExisting(id);
        }

        public TDoc Delete(TKey id)
        {
            var result = Storage.Delete(id);

            if (EntityAdapter.CanConvertStorageKey)
                SearchEngine.Delete(EntityAdapter.GetFulltextFromStorageKey(id));
            else if (result != null)
                SearchEngine.Delete(EntityAdapter.GetFulltextKey(result));

            if (EntityAdapter.CanConvertStorageKey || result != null)
                SearchEngine.CommitFulltextIndex();

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

        public void Open()
        {
            throw new NotImplementedException();
        }

        public void EnsureIndexesAsync(IEnumerable<string> stemmerNames, Action<double> progressReporter = null)
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
                _backgroundTaskExecutor.Schedule(() => RebuildIndexes(newIndexes.Select(x => x.Name).ToArray()));
            }
        }

        public void RebuildIndex(string stemmerName, Action<double> progressReporter = null)
        {
            throw new NotImplementedException();
        }

        public void RebuildIndexes(IEnumerable<string> names, Action<double> progressReporter = null)
        {
            throw new NotImplementedException();
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
            MultiIndex.RemoveIndex(stemmerName);
        }

        public void SetDefaultIndex(string stemmerName)
        {
            throw new NotImplementedException();
        }
    }
}