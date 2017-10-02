// /**********************************************************************************************
// Author:		Vasily Kabanov
// Created		2016-04-20
// Comment		
// **********************************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FulltextStorageLib.Util;
using Lucene.Net.Analysis;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Version = Lucene.Net.Util.Version;

namespace FulltextStorageLib
{
    // ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
    public class LuceneIndex : ILuceneIndex
    {
        private const string WriteLockFileName = "write.lock";

        public Lucene.Net.Store.Directory Directory { get; }

        public Analyzer Analyzer { get; }

        private readonly IndexWriter _indexWriter;

        private volatile Lazy<IndexSearcher> _scoringSearcher;
        private volatile Lazy<IndexSearcher> _nonScoringSearcher;

        public LuceneIndex(string path, Analyzer analyzer, Lucene.Net.Store.Directory fullTextDirectory, string documentKeyName = "Id")
        {
            Check.DoRequireArgumentNotNull(path, nameof(path));
            Check.DoRequireArgumentNotNull(analyzer, nameof(analyzer));
            Check.DoRequireArgumentNotNull(fullTextDirectory, nameof(fullTextDirectory));
            Check.DoRequireArgumentNotBlank(documentKeyName, nameof(documentKeyName));

            Name = new DirectoryInfo(path).Name;

            KeyFieldName = documentKeyName;
            Analyzer = analyzer;
            Directory = fullTextDirectory;

            _indexWriter = CreateWriter();

            RefreshStats();
            ResetSearch();
        }

        public string Name { get; }

        public string Path { get; }

        /// <summary>
        ///     Fully thread safe lazy instance
        /// </summary>
        public IndexSearcher ScoringSearcher => _scoringSearcher.Value;

        /// <summary>
        ///     Fully thread safe lazy instance
        /// </summary>
        public IndexSearcher NonScoringSearcher => _nonScoringSearcher.Value;

        /// <summary>
        ///     Name to pass to <see cref="Lucene.Net.Documents.Lucene.Net.Documents.Document.Get(string) 'primary key'
        /// </summary>
        public string KeyFieldName { get; }

        public int DocCount { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="document"></param>
        /// <remarks>
        ///     Lucen throws <see cref="CorruptIndexException"/> or passes through low level <see cref="IOException"/>.
        ///     Also <see cref="OutOfMemoryException"/> may result in which case index should be closed immediately.
        /// </remarks>
        public void Add(Document document)
        {
            _indexWriter.UpdateDocument(new Term(KeyFieldName, document.Get(KeyFieldName)), document);
        }

        public void Commit()
        {
            _indexWriter.Commit();
            RefreshStats();
            ResetSearch();
        }

        public void Add(params Document[] docs)
        {
            AddAll(docs.AsEnumerable());
        }

        /// <summary>
        ///     Add all documents to index.
        /// </summary>
        /// <param name="items"></param>
        /// <param name="progressReporter">
        ///     Optional delegate receiving number of items added so far
        /// </param>
        public void AddAll(IEnumerable<Document> items, Action<int> progressReporter = null)
        {
            var documentIndex = 0;
            foreach (var item in items)
            {
                Add(item);
                progressReporter?.Invoke(++documentIndex);
            }
        }

        public void Delete(string key)
        {
            Delete(new Term(KeyFieldName, key));
        }

        public void Delete(params string[] keys)
        {
            foreach (var key in keys)
                Delete(key);
        }

        public void Delete(params Term[] terms)
        {
            _indexWriter.DeleteDocuments(terms);
        }

        public void Delete(params Query[] queries)
        {
            _indexWriter.DeleteDocuments(queries);
        }

        public void Clear(bool commit = true)
        {
            _indexWriter.DeleteAll();
            if (commit)
                Commit();
        }

        /// <summary>
        ///     Wrap in FilteredQuery
        /// </summary>
        /// <param name="query"></param>
        /// <param name="maxResults"></param>
        /// <returns></returns>
        public IList<LuceneSearchHit> Search(Query query, int maxResults)
        {
            var search = ScoringSearcher;

            var hits = search.Search(query, null, maxResults, Sort.RELEVANCE).ScoreDocs;

            return hits.Select(h => new LuceneSearchHit(search.Doc(h.Doc), h.Score, KeyFieldName)).ToList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fieldName">
        ///     Datetime field to filter on.
        /// </param>
        /// <param name="from">
        ///     Inclusive
        /// </param>
        /// <param name="to">
        ///     Exclusive
        /// </param>
        /// <returns></returns>
        public Filter CreateTimeRangeFilter(string fieldName, DateTime? @from, DateTime? to)
        {
            Check.DoCheckArgument(from.HasValue || to.HasValue);
            Check.DoCheckArgument(!from.HasValue || !to.HasValue || from.Value < to.Value);

            var fromString = from.HasValue ? DateTools.DateToString(from.Value, DateTools.Resolution.SECOND) : null;
            var toString = to.HasValue ? DateTools.DateToString(to.Value, DateTools.Resolution.SECOND) : null;
            return new TermRangeFilter(fieldName, fromString, toString, true, false);
        }

        public Query CreateQuery(string fieldName, string queryText, bool fuzzy)
        {
            Check.DoRequireArgumentNotBlank(queryText, nameof(queryText));

            var terms = queryText.Trim()
                .Replace("-", " ")
                .Split(' ')
                .Where(x => !string.IsNullOrEmpty(x))
                .Select(x => x.Trim() + "*");

            var termsString = string.Join(" ", terms);

            var parser = new QueryParser(Version.LUCENE_30, fieldName, Analyzer)
            {
                PhraseSlop = 2,
                FuzzyMinSim = 0.1f,
                DefaultOperator = QueryParser.Operator.OR
            };

            var parsedQuery = Parse(queryText, parser);

            var booleanQuery = new BooleanQuery().Or(parsedQuery);

            var parsedTermsQuery = Parse(termsString, parser);
            parsedTermsQuery.Boost = 0.3f;

            booleanQuery.Or(parsedTermsQuery);

            var term = new Term(fieldName, queryText);

            if (fuzzy)
                booleanQuery.Or(new FuzzyQuery(term));

            var phraseQuery = new PhraseQuery
            {
                Slop = 2,
                Boost = 1.5f
            };
            phraseQuery.Add(term);

            booleanQuery.Or(phraseQuery);

            //booleanQuery.Add(new WildcardQuery(term), Occur.SHOULD);

            return booleanQuery;
        }

        public Query AddFilter(Query query, Filter filter)
        {
            Check.DoRequireArgumentNotNull(filter, nameof(filter));

            if (query == null)
                query = new MatchAllDocsQuery();

            return new FilteredQuery(query, filter);
        }

        /// <summary>
        ///     Create query which only applies filter to all documents
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public Query CreateQueryFromFilter(Filter filter)
        {
            Check.DoRequireArgumentNotNull(filter, nameof(filter));

            return AddFilter(null, filter);
        }

        public void CleanupDeletes()
        {
            _indexWriter.ExpungeDeletes();
        }

        public void Optimize()
        {
            _indexWriter.Optimize();
        }

        public IndexSearcher CreateSearcher(bool readOnly, bool calcScore)
        {
            var result = new IndexSearcher(Directory, readOnly);
            if (calcScore)
                result.SetDefaultFieldSortScoring(true, true);
            return result;
        }

        private IndexWriter CreateWriter()
        {
            return new IndexWriter(Directory, Analyzer, IndexWriter.MaxFieldLength.UNLIMITED);
        }

        /// <summary>
        ///     To be called after each update so that stats are reported correctly.
        /// </summary>
        private void RefreshStats()
        {
            DocCount = _indexWriter.NumDocs();
        }

        private static Query Parse(string text, QueryParser parser)
        {
            Query result;
            try
            {
                result = parser.Parse(text);
            }
            catch (ParseException)
            {
                result = parser.Parse(QueryParser.Escape(text.Trim()));
            }

            return result;
        }

        /// <summary>
        ///     Needs to be done after every write.
        /// </summary>
        private void ResetSearch()
        {
            _scoringSearcher = new Lazy<IndexSearcher>(() => CreateSearcher(true, true));

            _nonScoringSearcher = new Lazy<IndexSearcher>(() => CreateSearcher(true, false));
        }

        public static FSDirectory PreparePersistentDirectory(string directoryPath)
        {
            var directoryInfo = new DirectoryInfo(directoryPath);

            if (!directoryInfo.Exists)
                directoryInfo.Create();

            var dir = FSDirectory.Open(directoryInfo);

            if (IndexWriter.IsLocked(dir))
                IndexWriter.Unlock(dir);

            var lockFilePath = System.IO.Path.Combine(directoryPath, WriteLockFileName);

            if (File.Exists(lockFilePath))
                File.Delete(lockFilePath);

            return dir;
        }

        public static RAMDirectory CreateTransientDirectory()
        {
            return new RAMDirectory();
        }

        /// <summary>
        ///     
        /// </summary>
        /// <param name="stemmerName">
        ///     The name of a stemmer is the part of the class name before "Stemmer", e.g., the stemmer in EnglishStemmer is named "English". 
        /// </param>
        /// <returns></returns>
        public static Analyzer CreateSnowballAnalyzer(string stemmerName)
        {
            Check.DoCheckArgument(GetAvailableSnowballStemmers().Contains(stemmerName), () => $"Snowball stemmer {stemmerName} is not supported.");

            return new Lucene.Net.Analysis.Snowball.SnowballAnalyzer(Version.LUCENE_30, stemmerName);
        }

        /// <summary>
        ///     Get name
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<string> GetAvailableSnowballStemmers()
        {
            var namespaceName = typeof(SF.Snowball.Ext.EnglishStemmer).Namespace;
            const string stemmerSuffix = "Stemmer";

            return (typeof(SF.Snowball.Ext.EnglishStemmer)).Assembly.GetTypes()
                .Where(t => t.IsClass)
                .Where(t => t.Namespace == namespaceName)
                .Where(t => t.Name.EndsWith(stemmerSuffix))
                .Select(t => t.Name.Substring(0, t.Name.Length - stemmerSuffix.Length));
        }

        private bool _disposedValue; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _indexWriter?.Dispose();

                    if (_scoringSearcher.IsValueCreated)
                        _scoringSearcher.Value.Dispose();

                    if (_nonScoringSearcher.IsValueCreated)
                        _nonScoringSearcher.Value.Dispose();

                    Directory?.Dispose();

                    Analyzer?.Dispose();
                }

                // free unmanaged resources (unmanaged objects) and override a finalizer below.
                // set large fields to null.

                _disposedValue = true;
            }
        }

        // override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~LuceneIndex() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
    }
}