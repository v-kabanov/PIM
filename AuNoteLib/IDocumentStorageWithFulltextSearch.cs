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
    ///     Component which adds fulltext search to document storage. It contains storage because access to it is required for index
    ///     maintenance, e.g. for rebuilding or adding fulltext index.
    /// </summary>
    /// <typeparam name="TDoc">
    ///     Document type
    /// </typeparam>
    /// <typeparam name="TKey">
    ///     Document key type
    /// </typeparam>
    /// <typeparam name="THeader">
    ///     Document summary type representing part of the document added contained in fulltext indexes.
    /// </typeparam>
    public interface IDocumentStorageWithFulltextSearch<TDoc, TKey, THeader> : IDocumentStorage<TDoc, TKey>, IFulltextSearchEngine<TDoc, THeader>
        where THeader : IFulltextIndexEntry
        where TDoc : class
    {
        /// <summary>
        ///     Opens existing indexes.
        /// </summary>
        void Open();

        /// <summary>
        ///     Start work ensuring indexes for specified stemmers exist and are active.
        /// </summary>
        /// <param name="stemmerNames">
        /// </param>
        /// <param name="progressReporter">
        ///     Optional delegate receiving progress updates (completion percent 0..1)
        /// </param>
        void OpenOrCreateIndexes(IEnumerable<string> stemmerNames, Action<double> progressReporter = null);

        void RebuildIndex(string stemmerName, Action<double> progressReporter = null);

        void RebuildIndexes(IEnumerable<string> stemmerNames, Action<double> progressReporter = null);

        void RebuildAllIndexes(Action<double> progressReporter = null);

        void AddIndex(string stemmerName);

        void RemoveIndex(string stemmerName);

        void Delete(params THeader[] docHeaders);
    }
}