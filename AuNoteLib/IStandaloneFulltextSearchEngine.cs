// /**********************************************************************************************
// Author:		Vasily Kabanov
// Created		2017-05-25
// Comment		
// **********************************************************************************************/

using System;
using System.Collections.Generic;
using Lucene.Net.Analysis;

namespace AuNoteLib
{
    /// <summary>
    ///     Adds maintenance and management operation to fulltext search functionality.
    /// </summary>
    /// <typeparam name="TDoc">
    ///     Indexed document type
    /// </typeparam>
    /// <typeparam name="THeader">
    ///     Type representing part of <typeparamref name="TDoc"/> stored in fulltext search.
    /// </typeparam>
    public interface IStandaloneFulltextSearchEngine<in TDoc, THeader> : IFulltextSearchEngine<THeader>
        where THeader : class
        where TDoc : THeader
    {
        ILuceneEntityAdapter<TDoc, THeader> EntityAdapter { get; }

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
        IndexInformation AddOrOpenIndex(string name, Analyzer analyzer, bool dropExisting = false);

        IndexInformation AddOrOpenSnowballIndex(string snowballStemmerName);

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
        void RebuildIndex(string indexName, IEnumerable<TDoc> docs, int docCount, Action<double> progressReporter = null);

        /// <summary>
        ///     Rebuild all specified indexes
        /// </summary>
        /// <param name="indexNames"></param>
        /// <param name="documents">
        ///     All documents in the database; collection not expected to be fully loaded into RAM
        /// </param>
        /// <param name="docCount">
        ///     Optional, document count to be used in progress reporting.
        /// </param>
        /// <param name="progressReporter">
        ///     Optional delegate receiving progress report.
        /// </param>
        void RebuildIndexes(IEnumerable<string> indexNames, IEnumerable<TDoc> documents, int docCount = -1, Action<double> progressReporter = null);

        /// <summary>
        ///     Remove index
        /// </summary>
        /// <param name="name">
        ///     Mandatory, must not be default
        /// </param>
        void RemoveIndex(string name);

        bool UseFuzzySearch { get; set; }

        void Add(params TDoc[] docs);
    }
}