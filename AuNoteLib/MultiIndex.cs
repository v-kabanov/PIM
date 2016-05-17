// /**********************************************************************************************
// Author:		Vasily Kabanov
// Created		2016-05-15
// Comment		
// **********************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using log4net;
using Lucene.Net.Documents;
using NFluent;

namespace AuNoteLib
{
    /// <summary>
    ///     Domain agnostic multi lingual lucene index.
    /// </summary>
    public class MultiIndex : IMultiIndex
    {
        private static readonly ILog _log = LogManager.GetLogger(MethodInfo.GetCurrentMethod().DeclaringType);

        public string RootDirectory { get; private set; }
        private Dictionary<string, ILuceneIndex> Indexes { get; set; }

        public MultiIndex(string rootDirectory, string keyFieldName)
        {
            Check.That(rootDirectory).IsNotEmpty();
            Check.That(keyFieldName).IsNotEmpty();

            RootDirectory = rootDirectory;

            Indexes = new Dictionary<string, ILuceneIndex>();
            KeyFieldName = keyFieldName;
        }

        /// <summary>
        ///     Name to pass to <see cref="Lucene.Net.Documents.Lucene.Net.Documents.Document.Get(string)" /> 'primary key'
        /// </summary>
        public string KeyFieldName { get; private set; }

        public void AddIndex(string name, ILuceneIndex index)
        {
            throw new NotImplementedException();
        }

        public ILuceneIndex GetIndex(string name)
        {
            ILuceneIndex result = null;

            Indexes.TryGetValue(name, out result);

            return result;
        }

        /// <summary>
        ///     Clear index contents (e.g. to rebuild).
        /// </summary>
        public void Clear()
        {
            foreach (var luceneIndex in Indexes.Values)
            {
                luceneIndex.Clear();
            }
        }

        public List<SearchHit> Search(string searchFieldName, string queryText, int maxResults)
        {
            _log.DebugFormat("Searching '{0}', {1} - {2}, fuzzy = {3}, maxResults = {4}", queryText, maxResults);

            var results = new Dictionary<string, List<SearchHit>>();
            foreach (var key in Indexes.Keys)
            {
                var index = Indexes[key];

                var result = index.Search(index.CreateQuery(searchFieldName, queryText, false), maxResults);

                _log.DebugFormat("Index {0} matched {1} items", key, result.Count);

                results.Add(key, result);
            }

            var allHits = results.SelectMany(p => p.Value);

            var combinedResult = allHits.GroupBy(h => h.EntityId, (key, g) => new SearchHit(g.Select(h => h.Document).First(), g.Sum(h => h.Score), KeyFieldName))
                .OrderByDescending(i => i.Score)
                .Take(maxResults)
                .ToList();

            return combinedResult;
        }

        public void Add(Document doc)
        {
            foreach (var index in Indexes.Values)
            {
                index.Add(doc);
            }
        }

        public void Delete(string key)
        {
            foreach (var index in Indexes.Values)
            {
                index.Delete(key);
            }
        }

        public void CleanupDeletes()
        {
            foreach (var index in Indexes.Values)
            {
                index.CleanupDeletes();
            }
        }

        public void Optimize()
        {
            foreach (var index in Indexes.Values)
            {
                index.Optimize();
            }
        }
    }
}