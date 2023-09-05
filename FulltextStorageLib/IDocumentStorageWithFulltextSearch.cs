// /**********************************************************************************************
// Author:		Vasily Kabanov
// Created		2017-05-25
// Comment		
// **********************************************************************************************/

using System;
using System.Collections.Generic;

namespace FulltextStorageLib;

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
public interface IDocumentStorageWithFulltextSearch<TDoc, in TKey, THeader> : IDocumentStorage<TDoc, TKey>, IFulltextSearchEngine<THeader>
    where THeader : IFulltextIndexEntry
    where TDoc : class
{
    IList<string> SupportedStemmerNames { get; }

    /// <summary>
    ///     Opens existing indexes. Repeated calls result in exception if some indexes are already opened.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    ///     Some indexes are already open.
    /// </exception>
    void Open();

    /// <summary>
    ///     Schedule work ensuring indexes for specified stemmers exist and are active. Building of indexes is asynchronous.
    /// </summary>
    /// <param name="stemmerNames">
    ///     Lucene snowball stemmer names identifying indexes to open or create and build.
    /// </param>
    /// <param name="progressReporter">
    ///     Optional delegate receiving progress updates (completion percent 0..1)
    /// </param>
    void OpenOrCreateIndexes(IEnumerable<string> stemmerNames, Action<double> progressReporter = null);

    /// <summary>
    ///     Schedule async rebuilding of the specified index.
    /// </summary>
    /// <param name="stemmerName">
    ///     Lucene snowball stemmer name identifying indexes to rebuild.
    /// </param>
    /// <param name="progressReporter">
    ///     Optional delegate receiving progress updates (completion percent 0..1)
    /// </param>
    void RebuildIndex(string stemmerName, Action<double> progressReporter = null);

    /// <summary>
    ///     Schedule async rebuilding of the specified indexes.
    /// </summary>
    /// <param name="stemmerNames">
    ///     Lucene snowball stemmer names identifying indexes to rebuild.
    /// </param>
    /// <param name="progressReporter">
    ///     Optional delegate receiving progress updates (completion percent 0..1)
    /// </param>
    void RebuildIndexes(IEnumerable<string> stemmerNames, Action<double> progressReporter = null);

    /// <summary>
    ///     Schedule async rebuilding of all opened indexes.
    /// </summary>
    /// <param name="progressReporter">
    ///     Optional delegate receiving progress updates (completion percent 0..1)
    /// </param>
    void RebuildAllIndexes(Action<double> progressReporter = null);

    /// <summary>
    ///     Opens existing index or creates new and schedules its rebuilding.
    /// </summary>
    /// <param name="stemmerName">
    ///     Lucene snowball stemmer name identifying index.
    /// </param>
    /// <exception cref="ArgumentException">
    ///     Index for the specified stemmer is already open or stemmer is unknown.
    /// </exception>
    void AddOrOpenIndex(string stemmerName);

    /// <summary>
    ///     Remove specified index permanently.
    /// </summary>
    /// <param name="stemmerName">
    ///     Lucene snowball stemmer name identifying index.
    /// </param>
    void RemoveIndex(string stemmerName);

    /// <summary>
    ///     Delete documents from storage and indexes.
    /// </summary>
    /// <param name="docHeaders"></param>
    void Delete(params THeader[] docHeaders);

    TDoc GetExisting(THeader header);

    /// <summary>
    ///     Block the addition of new tasks and wait for all currently scheduled tasks to finish. Ensures that e.g. all previously saved documents are searchable.
    /// </summary>
    /// <param name="maxWaitMilliseconds">
    ///     Wait timeout.
    /// </param>
    /// <returns>
    ///     true if all tasks scheduled at the moment of invocation have completed.
    ///     false if timeout expired before the tasks finished.
    /// </returns>
    bool WaitForFulltextBackgroundWorkToComplete(int maxWaitMilliseconds);
}