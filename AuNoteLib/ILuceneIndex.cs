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

namespace AuNoteLib
{

    /// <summary>
    ///     Domain agnostic lucene full-text index.
    /// </summary>
    public interface ILuceneIndex
    {
        Directory Directory { get; }

        Analyzer Analyzer { get; }

        /// <summary>
        ///     Name to pass to <see cref="Lucene.Net.Documents.Lucene.Net.Documents.Document.Get(string)" /> 'primary key'
        /// </summary>
        string KeyFieldName { get; }

        void Add(params Document[] docs);

        /// <summary>
        ///     Exists because repeated creation and disposal of IndexWriter for every document to be saved is inefficient.
        /// </summary>
        /// <param name="doc"></param>
        void Add(IEnumerable<Document> doc);

        void Delete(string key);

        void Delete(params Lucene.Net.Index.Term[] terms);

        void Delete(params Query[] queries);

        void Clear();

        IndexSearcher CreateSearcher(bool readOnly, bool calcScore);

        List<SearchHit> Search(Query query, int maxResults);

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
}