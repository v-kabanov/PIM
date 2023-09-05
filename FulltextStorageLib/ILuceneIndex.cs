// /**********************************************************************************************
// Author:		Vasily Kabanov
// Created		2016-04-30
// Comment		
// **********************************************************************************************/


using System;
using System.Collections.Generic;
using Lucene.Net.Analysis;
using Lucene.Net.Documents;
using Lucene.Net.Search;
using Lucene.Net.Store;

namespace FulltextStorageLib;

/// <summary>
///     Domain agnostic lucene full-text index.
/// </summary>
public interface ILuceneIndex : IDisposable
{
    /// <summary>
    ///     Should generally match root directory name.
    /// </summary>
    string Name { get; }

    /// <summary>
    ///     Index root directory
    /// </summary>
    string Path { get; }

    Directory Directory { get; }

    Analyzer Analyzer { get; }

    /// <summary>
    ///     Name to pass to <see cref="Lucene.Net.Documents.Lucene.Net.Documents.Document.Get(string)" /> 'primary key'
    /// </summary>
    string KeyFieldName { get; }

    void Add(Document document);

    void Add(params Document[] docs);

    /// <summary>
    ///     Add all items. Does not commit.
    /// </summary>
    /// <param name="items">
    ///     Mandatory
    /// </param>
    /// <param name="progressReporter">
    ///     Optional delegate receiving number of items added so far.
    /// </param>
    void AddAll(IEnumerable<Document> items, Action<int> progressReporter = null);

    void Commit();

    void Delete(string key);

    void Delete(params string[] keys);

    void Delete(params Lucene.Net.Index.Term[] terms);

    void Delete(params Query[] queries);

    void Clear(bool commit = true);

    /// <summary>
    ///     Fully thread safe lazy instance
    /// </summary>
    IndexSearcher ScoringSearcher { get; }

    /// <summary>
    ///     Fully thread safe lazy instance
    /// </summary>
    IndexSearcher NonScoringSearcher { get; }

    IList<LuceneSearchHit> Search(Query query, int maxResults);

    Filter CreateTimeRangeFilter(string fieldName, DateTime? from, DateTime? to);

    Query CreateQuery(string searchFieldName, string queryText, bool fuzzy);

    Query AddFilter(Query query, Filter filter);

    /// <summary>
    ///     Create query which only applies filter to all documents
    /// </summary>
    Query CreateQueryFromFilter(Filter filter);

    int DocCount { get; }

    void CleanupDeletes();

    void Optimize();
}