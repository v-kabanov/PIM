// /**********************************************************************************************
// Author:		Vasily Kabanov
// Created		2016-04-30
// Comment		
// **********************************************************************************************/

using System.Collections.Generic;
using Lucene.Net.Analysis;
using Lucene.Net.Documents;
using Lucene.Net.Search;
using Lucene.Net.Store;

namespace PimTest
{
    public interface ILuceneIndex
    {
        Directory Directory { get; }

        Analyzer Analyzer { get; }

        /// <summary>
        ///     Name to pass to <see cref="Document.Get(string)"/> to get document's 'primary key'
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

        void DeleteAll();

        IndexSearcher CreateSearcher(bool readOnly, bool calcScore);

        List<SearchHit> Search(Query query, int maxResults);

        int DocCount { get; }

        void CleanupDeletes();

        void Optimize();
    }
}