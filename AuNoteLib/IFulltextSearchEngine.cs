// /**********************************************************************************************
// Author:		Vasily Kabanov
// Created		2017-05-25
// Comment		
// **********************************************************************************************/

using System;
using System.Collections.Generic;

namespace AuNoteLib
{
    /// <summary>
    ///     Object implementing full text searching (but not maintenance).
    /// </summary>
    /// <typeparam name="THeader">
    ///     Type representing part of document stored in fulltext search.
    /// </typeparam>
    public interface IFulltextSearchEngine<THeader>
        where THeader : class
    {
        /// <summary>
        ///     Lists names of fulltext indexes; names may correspond to e.g. stemmer language.
        /// </summary>
        IEnumerable<string> ActiveIndexNames { get; }

        IList<THeader> Search(string queryText, int maxResults);

        IList<THeader> GetTopInPeriod(DateTime? periodStart, DateTime? periodEnd, int maxResults);

        void SetDefaultIndex(string name);

        /// <summary>
        ///     Add new documents to index or update existing ones.
        /// </summary>
        /// <param name="docHeaders">
        ///     New or existing documents.
        /// </param>
        void Add(params THeader[] docHeaders);

        void Remove(params THeader[] docHeaders);
    }
}