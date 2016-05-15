// /**********************************************************************************************
// Author:		Vasily Kabanov
// Created		2016-04-30
// Comment		
// **********************************************************************************************/

using System;
using System.Collections.Generic;
using NFluent;
using log4net;
using System.Reflection;
using System.Linq;
using Lucene.Net.Search;

namespace AuNoteLib
{
    /// <summary>
    ///     Wraps <see cref="IMultiIndex"/> and adapts it to .
    /// </summary>
    public class SearchEngine
    {
        private static readonly ILog _log = LogManager.GetLogger(MethodInfo.GetCurrentMethod().DeclaringType);

        public string RootDirectory { get; private set; }
        private Dictionary<string, ILuceneIndex> Indexes { get; set; }

        public SearchEngine(string rootDirectory)
        {
            RootDirectory = rootDirectory;

            Indexes = new Dictionary<string, ILuceneIndex>();

            Adapter = new LuceneNoteAdapter();
        }

        private LuceneNoteAdapter Adapter { get; set; }

        public void AddIndex(string name, ILuceneIndex index)
        {
            Check.That(index).IsNotNull();
            Check.That(name).IsNotEmpty();

            if (Indexes.ContainsKey(name))
            {
                throw new ApplicationException("Index with the specified name already exists");
            }

            if (Indexes.ContainsValue(index))
            {
                throw new ApplicationException("Index instance already exists");
            }

            Indexes.Add(name, index);
        }

        public List<INoteHeader> Search(string queryText, DateTime? from, DateTime? to, bool fuzzy = false, int maxResults = 20)
        {
            _log.DebugFormat("Searching '{0}', {1} - {2}, fuzzy = {3}, maxResults = {4}", queryText, from, to, fuzzy, maxResults);

            var results = new Dictionary<string, List<SearchHit>>();
            foreach (var key in Indexes.Keys)
            {
                var index = Indexes[key];

                var result = Adapter.Search(index, Adapter.CreateQuery(index, queryText, from, to, fuzzy), maxResults);

                _log.DebugFormat("Index {0} matched {1} items", key, result.Count);

                results.Add(key, result);
            }

            var allHits = results.SelectMany(p => p.Value);

            var combinedResult = allHits.GroupBy(h => h.EntityId, (key, g) => new { Document = g.Select(h => h.Document).First(), Score = g.Sum(h => h.Score) })
                .OrderByDescending(i => i.Score)
                .Take(maxResults)
                .ToList();

            return combinedResult.Select(r => Adapter.GetHeader(r.Document)).ToList();
        }

        private Query CreateQuery(ILuceneIndex index, string queryText, DateTime? from, DateTime? to, bool fuzzy)
        {
            Query query = null;

            if (!string.IsNullOrEmpty(queryText))
            {
                query = Adapter.CreateQuery(index, queryText, fuzzy);
            }
            
            if (from.HasValue || to.HasValue)
            {
                query = Adapter.AddFilter(query, Adapter.CreateTimeRangeFilter(from, to));
            }

            return query;
        }
    }
}