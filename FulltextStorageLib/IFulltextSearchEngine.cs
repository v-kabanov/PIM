// /**********************************************************************************************
// Author:		Vasily Kabanov
// Created		2017-05-25
// Comment		
// **********************************************************************************************/

using System;
using System.Collections.Generic;

namespace FulltextStorageLib
{
    /// <summary>
    ///     Identifies searchable time property
    /// </summary>
    public enum SearchableDocumentTime
    {
        Creation,
        LastUpdate
    }

    /// <summary>
    ///     Object implementing full text searching on existing indexes with data for indexing provided by client.
    ///     Encapsulates multiple fulltext indexes for e.g. multiple languages.
    /// </summary>
    /// <typeparam name="THeader">
    ///     Type representing part of document stored in fulltext search. This is what can be returned from search. A searchable field
    ///     may not be stored in FT index.
    /// </typeparam>
    public interface IFulltextSearchEngine<THeader>
        where THeader : IFulltextIndexEntry
    {
        /// <summary>
        ///     Lists names of fulltext indexes; names may correspond to e.g. stemmer language.
        /// </summary>
        IEnumerable<string> ActiveIndexNames { get; }

        IList<THeader> Search(string queryText, int maxResults);

        /// <summary>
        ///     Get top <paramref name="maxResults"/> documents in the period filtered and sorted in descending order by <paramref name="searchableDocumentTime"/>.
        /// </summary>
        /// <param name="periodStart">
        ///     Inclusive, truncated to seconds.
        /// </param>
        /// <param name="periodEnd">
        ///     Exclusive, truncated to seconds.
        /// </param>
        /// <param name="maxResults">
        ///     Max number of documents to return.
        /// </param>
        /// <param name="searchableDocumentTime">
        ///     One of the supported document time properties to filter on.
        /// </param>
        IList<THeader> GetTopInPeriod(DateTime? periodStart, DateTime? periodEnd, int maxResults, SearchableDocumentTime searchableDocumentTime = SearchableDocumentTime.LastUpdate);

        /// <summary>
        ///     Search documents in the period for <paramref name="queryText"/>.
        /// </summary>
        /// <param name="periodStart">
        ///     Optional, no restriction if unspecified, inclusive, truncated to seconds precision
        /// </param>
        /// <param name="periodEnd">
        ///     Optional, no restriction if unspecified, exclusive, truncated to seconds precision.
        /// </param>
        /// <param name="queryText">
        ///     Optional, results sorted by time in ascending order if not specified
        /// </param>
        /// <param name="maxResults">
        ///     Max number of documents to return.
        /// </param>
        /// <param name="searchableDocumentTime">
        ///     One of the supported document time properties to filter on.
        /// </param>
        /// <returns>
        ///     If <paramref name="queryText"/> is specified, result is ordered by relevance, otherwise by time in ascending order.
        /// </returns>
        IList<THeader> SearchInPeriod(DateTime? periodStart, DateTime? periodEnd, string queryText, int maxResults, SearchableDocumentTime searchableDocumentTime = SearchableDocumentTime.LastUpdate);

        void SetDefaultIndex(string name);
    }
}