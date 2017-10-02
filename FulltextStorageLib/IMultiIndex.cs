// /**********************************************************************************************
// Author:		Vasily Kabanov
// Created		2016-05-15
// Comment		
// **********************************************************************************************/

using System;
using System.Collections.Generic;
using Lucene.Net.Documents;

namespace FulltextStorageLib
{
    public interface IMultiIndex : IDisposable
    {
        /// <summary>
        ///     Name to pass to <see cref="Lucene.Net.Documents.Lucene.Net.Documents.Document.Get(string)" /> 'primary key'
        /// </summary>
        string KeyFieldName { get; }

        /// <summary>
        ///     The index to use by default when e.g. they are [supposed to be] interchangeable such as when searching by time.
        ///     Default index cannot be deleted with <see cref="RemoveIndex"/>.
        /// </summary>
        string DefaultIndexName { get; set; }

        /// <summary>
        ///     Number of currently active lucene indexes
        /// </summary>
        int IndexCount { get; }

        ILuceneIndex[] AllIndexes { get; }

        IEnumerable<string> AllIndexNames { get; }

        /// <summary>
        ///     Add new index to be used in parallel with existing ones. Index must be prepopulated already if necessary, no data is added to it here.
        /// </summary>
        /// <param name="name">
        ///     case insensitive
        /// </param>
        /// <param name="index">
        /// </param>
        void AddIndex(string name, ILuceneIndex index);

        /// <summary>
        ///     Close given index and remove it from searches. Storage is managed externally and can be removed by the caller afterwards.
        /// </summary>
        /// <param name="name">
        /// </param>
        /// <exception cref="ApplicationException">
        ///     index does not exist
        /// </exception>
        void RemoveIndex(string name);

        /// <summary>
        ///     Get index by name
        /// </summary>
        /// <param name="name">
        ///     Case-insensitive
        /// </param>
        /// <returns>
        ///     null if does not exist
        /// </returns>
        ILuceneIndex GetIndex(string name);

        /// <summary>
        ///     Clear index contents (e.g. to rebuild).
        /// </summary>
        void Clear();

        bool UseFuzzySearch { get; set; }

        IList<LuceneSearchHit> Search(string searchFieldName, string queryText, int maxResults);

        /// <summary>
        ///     Get last <paramref name="maxResults"/> documents with time field (identified by <paramref name="timeFieldName"/>) value
        ///     within the specified time range in reverse chronological order.
        /// </summary>
        /// <param name="timeFieldName">
        ///     Name of the field containing time, see <see cref="Document.Get"/>'s parameter
        /// </param>
        /// <param name="periodStart">
        ///     Optional, null means no restriction, inclusive, truncated to seconds precision.
        /// </param>
        /// <param name="periodEnd">
        ///     Optional, null means no restriction, exclusive, truncated to seconds precision.
        /// </param>
        /// <param name="maxResults">
        ///     must be positive
        /// </param>
        /// <returns>
        /// </returns>
        IList<LuceneSearchHit> GetTopInPeriod(string timeFieldName, DateTime? periodStart, DateTime? periodEnd, int maxResults);

        /// <summary>
        ///     Get last <paramref name="maxResults"/> documents with time field (identified by <paramref name="timeFieldName"/>) value
        ///     within the specified time range.
        /// </summary>
        /// <param name="timeFieldName">
        ///     Name of the field containing time, see <see cref="Document.Get"/>'s parameter
        /// </param>
        /// <param name="periodStart">
        ///     Optional, null means no restriction, inclusive, truncated to seconds precision.
        /// </param>
        /// <param name="periodEnd">
        ///     Optional, null means no restriction, exclusive, truncated to seconds precision.
        /// </param>
        /// <param name="reverseChronologicalOrder">
        ///     Whether to sort if reverse (if true) or straight chronological order.
        /// </param>
        /// <param name="maxResults">
        ///     must be positive
        /// </param>
        /// <returns>
        ///     List ordered in reverse chronological order.
        /// </returns>
        IList<LuceneSearchHit> GetTopInPeriod(
            string timeFieldName, DateTime? periodStart, DateTime? periodEnd, bool reverseChronologicalOrder, int maxResults);

        /// <summary>
        ///     Combine fulltext search with date-time filter - search within specified period.
        /// </summary>
        /// <param name="timeFieldName">
        ///     Name of the field containing time, see <see cref="Document.Get"/>'s parameter
        /// </param>
        /// <param name="periodStart">
        ///     Optional, null means no restriction, inclusive, truncated to seconds precision.
        /// </param>
        /// <param name="periodEnd">
        ///     Optional, null means no restriction, exclusive, truncated to seconds precision.
        /// </param>
        /// <param name="searchFieldName">
        ///     Document (header) text field name to perform fulltext search on.
        /// </param>
        /// <param name="queryText">
        ///     Optional; if omitted behavior is same as <see cref="GetTopInPeriod(string, DateTime?, DateTime?, bool, int)"/>
        ///     with results returned in straight chronological order
        /// </param>
        /// <param name="maxResults"></param>
        /// <returns>
        ///     Ordered by relevance if <paramref name="queryText"/> is specified, otherwise by time field
        /// </returns>
        IList<LuceneSearchHit> SearchInPeriod(string timeFieldName, DateTime? periodStart, DateTime? periodEnd, string searchFieldName, string queryText, int maxResults);

        void Add(Document doc);

        void Add(params Document[] docs);

        /// <summary>
        ///     Does not commit
        /// </summary>
        /// <param name="docs">
        /// </param>
        void Add(IEnumerable<Document> docs);

        void Delete(string key);

        void Delete(params string[] keys);

        void Delete(params Lucene.Net.Index.Term[] terms);

        void CleanupDeletes();

        void Optimize();

        void Commit();

        /// <summary>
        ///     Rebuild all or only specified indexes.
        /// </summary>
        /// <param name="names"></param>
        /// <param name="documents"></param>
        /// <param name="docCount"></param>
        /// <param name="progressReporter"></param>
        void RebuildIndexes(IEnumerable<string> names, IEnumerable<Document> documents, int docCount, Action<double> progressReporter = null);
    }
}