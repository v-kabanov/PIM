// /**********************************************************************************************
// Author:		Vasily Kabanov
// Created		2016-05-15
// Comment		
// **********************************************************************************************/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using log4net;
using Lucene.Net.Documents;
using Lucene.Net.Search;
using NFluent;

namespace AuNoteLib
{
    /// <summary>
    ///     Domain agnostic multi lingual lucene index.
    /// </summary>
    public class MultiIndex : IMultiIndex
    {
        private static readonly ILog _log = LogManager.GetLogger(MethodInfo.GetCurrentMethod().DeclaringType);

        private Dictionary<string, ILuceneIndex> Indexes { get; set; }

        public MultiIndex(string keyFieldName)
        {
            Check.That(keyFieldName).IsNotEmpty();

            Indexes = new Dictionary<string, ILuceneIndex>(StringComparer.InvariantCultureIgnoreCase);
            KeyFieldName = keyFieldName;
        }

        /// <summary>
        ///     Name to pass to <see cref="Lucene.Net.Documents.Lucene.Net.Documents.Document.Get(string)" /> 'primary key'
        /// </summary>
        public string KeyFieldName { get; private set; }

        /// <summary>
        ///     The index to use by default when e.g. they are [supposed to be] interchangeable such as when searching by time.
        ///     Default index cannot be deleted with <see cref="RemoveIndex"/>.
        /// </summary>
        public string DefaultIndexName { get; set; }

        public void AddIndex(string name, ILuceneIndex index)
        {
            CheckNotDisposed();

            Check.That(GetIndex(name)).IsNull();
            Check.That(index).IsNotNull();

            Indexes.Add(name, index);
        }

        public void RemoveIndex(string name)
        {
            CheckNotDisposed();

            Check.That(name).IsNotEmpty();

            if (string.Equals(name, DefaultIndexName, StringComparison.InvariantCultureIgnoreCase))
                throw new ApplicationException("Default index cannot be deleted; change default before deleting the index");

            GetIndex(name).Dispose();
            Indexes.Remove(name);
        }

        public ILuceneIndex GetIndex(string name)
        {
            CheckNotDisposed();

            ILuceneIndex result = null;

            Indexes.TryGetValue(name, out result);

            return result;
        }

        /// <summary>
        ///     Clear index contents (e.g. to rebuild).
        /// </summary>
        public void Clear()
        {
            CheckNotDisposed();

            foreach (var luceneIndex in Indexes.Values)
            {
                luceneIndex.Clear();
            }
        }

        public IList<SearchHit> Search(string searchFieldName, string queryText, int maxResults)
        {
            CheckNotDisposed();

            _log.DebugFormat("Searching '{0}', {1} - {2}, fuzzy = {3}, maxResults = {4}", queryText, maxResults);

            var results = new Dictionary<string, IList<SearchHit>>();
            foreach (var key in Indexes.Keys)
            {
                var index = Indexes[key];

                var result = index.Search(index.CreateQuery(searchFieldName, queryText, false), maxResults);

                _log.DebugFormat("Index {0} matched {1} items", key, result.Count);

                results.Add(key, result);
            }

            var allHits = results.SelectMany(p => p.Value);

            var combinedResult = allHits.GroupBy(h => h.EntityId, (key, g) => new SearchHit(g.Select(h => h.Document).First(), g.Sum(h => h.Score), KeyFieldName))
                .OrderByDescending(i => i.Score)
                .Take(maxResults)
                .ToList();

            return combinedResult;
        }

        /// <summary>
        ///     Get last <paramref name="maxResults"/> documents with time field (identified by <paramref name="timeFieldName"/>) value
        ///     within the specified time range in reverse chronological order.
        /// </summary>
        /// <param name="timeFieldName">
        ///     Name of the field containing time, see <see cref="Document.Get"/>
        /// </param>
        /// <param name="periodStart">
        ///     null means no restriction
        /// </param>
        /// <param name="periodEnd">
        ///     null means no restriction
        /// </param>
        /// <param name="maxResults">
        ///     must be positive
        /// </param>
        /// <returns>
        /// </returns>
        public IList<SearchHit> GetTopInPeriod(string timeFieldName, DateTime periodStart, DateTime periodEnd, int maxResults)
        {
            CheckNotDisposed();

            Check.That(timeFieldName).IsNotEmpty();

            if (Indexes.Count == 0)
            {
                throw new InvalidOperationException("No active indexes");
            }

            var index = GetIndex(DefaultIndexName);
            if (null == index)
                index = Indexes.Values.First();

            var searcher = index.CreateSearcher(true, false);
            var sort = new Sort(new SortField(timeFieldName, CultureInfo.InvariantCulture, true));
            List<SearchHit> result = searcher.Search((Query)null, index.CreateTimeRangeFilter(timeFieldName, periodStart, periodEnd), maxResults, sort)
                .ScoreDocs.Select(d => new SearchHit(searcher.Doc(d.Doc), d.Score, KeyFieldName))
                .ToList();

            return result;
        }

        public void Add(Document doc)
        {
            CheckNotDisposed();

            foreach (var index in Indexes.Values)
            {
                index.Add(doc);
            }
        }

        public void Delete(string key)
        {
            CheckNotDisposed();

            foreach (var index in Indexes.Values)
            {
                index.Delete(key);
            }
        }

        public void CleanupDeletes()
        {
            CheckNotDisposed();

            foreach (var index in Indexes.Values)
            {
                index.CleanupDeletes();
            }
        }

        public void Optimize()
        {
            CheckNotDisposed();

            foreach (var index in Indexes.Values)
            {
                index.Optimize();
            }
        }

        private void CheckNotDisposed()
        {
            if (disposedValue)
                throw new ObjectDisposedException(MethodInfo.GetCurrentMethod().DeclaringType.Name);
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    foreach(var index in Indexes)
                    {
                        index.Value.Dispose();
                    }
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~MultiIndex() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}