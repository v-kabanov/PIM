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
    ///     Object implementing full text searching on existing indexes with data for indexing provided by client.
    ///     Encapsulates multiple fulltext indexes for e.g. multiple languages.
    /// </summary>
    /// <typeparam name="TDoc">
    ///     Indexed document type
    /// </typeparam>
    /// <typeparam name="THeader">
    ///     Type representing part of document stored in fulltext search. This is what can be returned from search. A searchable field
    ///     may not be stored in FT index.
    /// </typeparam>
    public interface IFulltextSearchEngine<in TDoc, THeader>
        where THeader : IFulltextIndexEntry
        where TDoc : class
    {
        /// <summary>
        ///     Lists names of fulltext indexes; names may correspond to e.g. stemmer language.
        /// </summary>
        IEnumerable<string> ActiveIndexNames { get; }

        IList<THeader> Search(string queryText, int maxResults);

        IList<THeader> GetTopInPeriod(DateTime? periodStart, DateTime? periodEnd, int maxResults);

        void SetDefaultIndex(string name);
    }
}