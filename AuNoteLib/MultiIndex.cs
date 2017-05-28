// /**********************************************************************************************
// Author:		Vasily Kabanov
// Created		2016-05-15
// Comment		
// **********************************************************************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using FulltextStorageLib.Util;
using log4net;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;

namespace FulltextStorageLib
{
    /// <summary>
    ///     Domain agnostic multi lingual lucene index.
    /// </summary>
    // ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
    public class MultiIndex : IMultiIndex
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private Dictionary<string, ILuceneIndex> Indexes { get; }

        public MultiIndex(string keyFieldName)
        {
            Check.DoRequireArgumentNotBlank(keyFieldName, nameof(keyFieldName));

            Indexes = new Dictionary<string, ILuceneIndex>(StringComparer.InvariantCultureIgnoreCase);
            KeyFieldName = keyFieldName;
        }

        /// <summary>
        ///     Name to pass to <see cref="Lucene.Net.Documents.Lucene.Net.Documents.Document.Get(string)" /> 'primary key'
        /// </summary>
        public string KeyFieldName { get; }

        /// <summary>
        ///     The index to use by default when e.g. they are [supposed to be] interchangeable such as when searching by time.
        ///     Default index cannot be deleted with <see cref="RemoveIndex"/>.
        /// </summary>
        public string DefaultIndexName { get; set; }

        /// <summary>
        ///     Number of currently active lucene indexes
        /// </summary>
        public int IndexCount => Indexes.Count;

        public ILuceneIndex[] AllIndexes => Indexes.Values.ToArray();

        public IEnumerable<string> AllIndexNames => Indexes.Keys;

        public void AddIndex(string name, ILuceneIndex index)
        {
            CheckNotDisposed();

            Check.DoCheckArgument(GetIndex(name) != null, () => $"Index {name} already exists.");
            Check.DoRequireArgumentNotNull(index, nameof(index));

            Indexes.Add(name, index);
        }

        public void RemoveIndex(string name)
        {
            CheckNotDisposed();

            Check.DoRequireArgumentNotBlank(name, nameof(name));
            if (string.Equals(name, DefaultIndexName, StringComparison.InvariantCultureIgnoreCase))
                throw new ApplicationException("Default index cannot be deleted; change default before deleting the index");

            var index = GetIndex(name);
            Check.DoCheckArgument(index != null, () => $"Index {name} is not active.");

            index.Dispose();
            Indexes.Remove(name);
        }

        public ILuceneIndex GetIndex(string name)
        {
            CheckNotDisposed();

            ILuceneIndex result;

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
                luceneIndex.Clear();
        }

        public bool UseFuzzySearch { get; set; }

        public IList<LuceneSearchHit> Search(string searchFieldName, string queryText, int maxResults)
        {
            CheckNotDisposed();

            Log.DebugFormat("Searching '{0}', {1} - {2}, fuzzy = {3}, maxResults = {4}", queryText, maxResults);

            var results = new Dictionary<string, IList<LuceneSearchHit>>();
            foreach (var key in Indexes.Keys)
            {
                var index = Indexes[key];

                var result = index.Search(index.CreateQuery(searchFieldName, queryText, UseFuzzySearch), maxResults);

                Log.DebugFormat("Index {0} matched {1} items", key, result.Count);

                results.Add(key, result);
            }

            var allHits = results.SelectMany(p => p.Value);

            var combinedResult = allHits.GroupBy(h => h.EntityId, (key, g) => new LuceneSearchHit(g.Select(h => h.Document).First(), g.Sum(h => h.Score), KeyFieldName))
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
        public IList<LuceneSearchHit> GetTopInPeriod(string timeFieldName, DateTime? periodStart, DateTime? periodEnd, int maxResults)
        {
            CheckNotDisposed();

            Check.DoRequireArgumentNotBlank(timeFieldName, nameof(timeFieldName));

            CheckActive();

            var index = GetIndex(DefaultIndexName);
            if (null == index)
                index = Indexes.Values.First();

            var searcher = index.NonScoringSearcher;
            var sort = new Sort(new SortField(timeFieldName, CultureInfo.InvariantCulture, true));
            var query = index.CreateQueryFromFilter(index.CreateTimeRangeFilter(timeFieldName, periodStart, periodEnd));

            var result = searcher.Search(query, null, maxResults, sort)
                .ScoreDocs.Select(d => new LuceneSearchHit(searcher.Doc(d.Doc), d.Score, KeyFieldName))
                .ToList();

            return result;
        }

        public void Add(Document doc)
        {
            CheckNotDisposed();
            CheckActive();

            foreach (var index in Indexes.Values)
            {
                index.Add(doc);
            }
        }

        public void Add(params Document[] docs)
        {
            CheckNotDisposed();
            CheckActive();

            foreach (var index in Indexes.Values)
                index.Add(docs);
        }

        public void Add(IEnumerable<Document> docs)
        {
            CheckNotDisposed();
            CheckActive();

            foreach (var index in Indexes.Values)
            {
                // ReSharper disable once PossibleMultipleEnumeration
                index.AddAll(docs);
            }
        }

        public void Delete(string key)
        {
            CheckNotDisposed();

            foreach (var index in Indexes.Values)
            {
                try
                {
                    index.Delete(key);
                }
                catch (Exception exception)
                {
                    Log.ErrorFormat("Exception deleting item#{0} index {1}: {2}", key, index.Name, exception.Message);
                    throw new FulltextException($"Failed to delete item from index {index.Name}", exception);
                }
            }
        }

        public void Delete(params string[] keys)
        {
            CheckNotDisposed();

            foreach (var index in Indexes.Values)
                index.Delete(keys);
        }

        public void Delete(params Term[] terms)
        {
            CheckNotDisposed();

            foreach (var index in Indexes.Values)
                index.Delete(terms);
        }

        public void CleanupDeletes()
        {
            CheckNotDisposed();

            foreach (var index in Indexes.Values)
                index.CleanupDeletes();
        }

        public void Optimize()
        {
            CheckNotDisposed();

            foreach (var index in Indexes.Values)
                index.Optimize();
        }

        public void Commit()
        {
            CheckNotDisposed();

            foreach (var index in Indexes.Values)
                index.Commit();
        }

        /// <summary>
        ///     Rebuild all or specified indexes
        /// </summary>
        /// <param name="names">
        ///     Optional, indexes to rebuild; null means all
        /// </param>
        /// <param name="documents">
        ///     All documents in the database.
        /// </param>
        /// <param name="docCount">
        ///     Total number of documents in the database, for progress reporting
        /// </param>
        /// <param name="progressReporter">
        ///     Optional, delegate to receive progress report (0..1)
        /// </param>
        [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
        public void RebuildIndexes(IEnumerable<string> names, IEnumerable<Document> documents, int docCount, Action<double> progressReporter = null)
        {
            Util.Check.DoRequireArgumentNotNull(documents, nameof(documents));

            var firstBadName = names?.Select(name => new { Name = name, Index = GetIndex(name) })
                .FirstOrDefault(x => x.Index == null);
            if (firstBadName != null)
                throw new ArgumentException($"Unknown index {firstBadName.Name}");

            var indexesToRebuild = names?.Select(GetIndex).ToArray() ?? AllIndexes;

            if (indexesToRebuild.Length == 0)
                return;

            foreach (var index in indexesToRebuild)
                try
                {
                    index.Clear(false);
                }
                catch (Exception exception)
                {
                    Log.ErrorFormat("Exception clearing index {0}: {1}", index.Name, exception.Message);
                    throw new FulltextException($"Failed to clear index {index.Name}", exception);
                }

            var docIndex = 0;
            foreach (var document in documents)
            {
                ++docIndex;
                for (var indexIndex = 0; indexIndex < indexesToRebuild.Length; ++indexIndex)
                {
                    var index = indexesToRebuild[indexIndex];
                    try
                    {
                        index.Add(document);
                    }
                    catch (Exception exception)
                    {
                        Log.ErrorFormat("Exception adding document to index {0}: {1}", index.Name, exception.Message);
                        throw new FulltextException($"Failed to add document to index {index.Name}", exception);
                    }
                    progressReporter?.Invoke(0.5D * (indexIndex * docCount + docIndex) / docCount * indexesToRebuild.Length);
                }
            }

            for (var indexIndex = 0; indexIndex < indexesToRebuild.Length; ++indexIndex)
            {
                var index = indexesToRebuild[indexIndex];
                try
                {
                    index.Commit();
                }
                catch (Exception exception)
                {
                    Log.ErrorFormat("Exception committing changes to index {0}: {1}", index.Name, exception.Message);
                    throw new FulltextException($"Failed commit changes to index {index.Name}", exception);
                }
                progressReporter?.Invoke(0.5D * (1 + ((double)indexIndex + 1) / indexesToRebuild.Length));
            }
        }

        /// <summary>
        ///     Check readiness to accept docs and search
        /// </summary>
        private void CheckActive()
        {
            if (Indexes.Count == 0)
                throw new InvalidOperationException("No active fulltext indexes");
        }

        private void CheckNotDisposed()
        {
            if (_disposedValue)
                throw new ObjectDisposedException(MethodBase.GetCurrentMethod().DeclaringType.Name);
        }

        private bool _disposedValue; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    // dispose managed state (managed objects).
                    foreach(var index in Indexes)
                    {
                        index.Value.Dispose();
                    }
                }

                // free unmanaged resources (unmanaged objects) and override a finalizer below.
                // set large fields to null.

                _disposedValue = true;
            }
        }

        // override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~MultiIndex() {
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