// /**********************************************************************************************
// Author:		Vasily Kabanov
// Created		2016-04-20
// Comment		
// **********************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using Lucene.Net.Analysis;
using Lucene.Net.Documents;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Version = Lucene.Net.Util.Version;
using Lucene.Net.Index;
using System.IO;
using Lucene.Net.QueryParsers;
using NFluent;

namespace AuNoteLib
{
    public class LuceneIndex : ILuceneIndex
    {
        private const string WriteLockFileName = "write.lock";

        public Lucene.Net.Store.Directory Directory { get; private set; }

        public Analyzer Analyzer { get; private set; }

        public LuceneIndex(Analyzer analyzer, Lucene.Net.Store.Directory fullTextDirectory, string documentKeyName = "Id")
        {
            Check.That(analyzer).IsNotNull();
            Check.That(fullTextDirectory).IsNotNull();
            Check.That(documentKeyName).IsNotEmpty();

            KeyFieldName = documentKeyName;
            Analyzer = analyzer;
            Directory = fullTextDirectory;

            using (var writer = CreateWriter())
            {
                CommitAndRefreshStats(writer, false);
            }
        }

        /// <summary>
        ///     Name to pass to <see cref="Lucene.Net.Documents.Lucene.Net.Documents.Document.Get(string) 'primary key'
        /// </summary>
        public string KeyFieldName { get; private set; }

        public int DocCount { get; private set; }
        public int DeletedDocCount { get; private set; }

        public void Add(params Document[] docs)
        {
            Add((IEnumerable<Document>) docs.AsEnumerable());
        }

        public void Add(IEnumerable<Document> items)
        {
            using (var writer = CreateWriter())
            {
                foreach (var item in items)
                {
                    writer.UpdateDocument(new Term(KeyFieldName, item.Get(KeyFieldName)), item);
                }

                CommitAndRefreshStats(writer);
            }
        }

        public void Delete(string key)
        {
            Delete(new Term(KeyFieldName, key));
        }

        public void Delete(params Term[] terms)
        {
            using (var writer = CreateWriter())
            {
                writer.DeleteDocuments(terms);

                CommitAndRefreshStats(writer);
            }
        }

        public void Delete(params Query[] queries)
        {
            using (var writer = CreateWriter())
            {
                writer.DeleteDocuments(queries);

                CommitAndRefreshStats(writer);
            }
        }

        public void Clear()
        {
            using (var writer = CreateWriter())
            {
                writer.DeleteAll();

                CommitAndRefreshStats(writer);
            }
        }

        public IndexSearcher CreateSearcher(bool readOnly, bool calcScore)
        {
            var result = new IndexSearcher(Directory, readOnly);
            if (calcScore)
                result.SetDefaultFieldSortScoring(true, true);
            return result;
        }

        /// <summary>
        ///     Wrap in FilteredQuery
        /// </summary>
        /// <param name="query"></param>
        /// <param name="maxResults"></param>
        /// <returns></returns>
        public IList<LuceneSearchHit> Search(Query query, int maxResults)
        {
            using (var search = CreateSearcher(true, true))
            {
                var hits = search.Search(query, null, maxResults, Sort.RELEVANCE).ScoreDocs;

                return hits.Select(h => new LuceneSearchHit(search.Doc(h.Doc), h.Score, KeyFieldName)).ToList();
            }
        }

        public Filter CreateTimeRangeFilter(string fieldName, DateTime? @from, DateTime? to)
        {
            Check.That(from.HasValue || to.HasValue).IsTrue();
            Check.That(!from.HasValue || !to.HasValue || from.Value < to.Value).IsTrue();

            string fromString = from.HasValue ? DateTools.DateToString(from.Value, DateTools.Resolution.SECOND) : null;
            string toString = to.HasValue ? DateTools.DateToString(to.Value, DateTools.Resolution.SECOND) : null;
            return new TermRangeFilter(fieldName, fromString, toString, true, false);
        }

        public Query CreateQuery(string fieldName, string queryText, bool fuzzy)
        {
            Check.That(queryText).IsNotEmpty();

            var terms = queryText.Trim()
                .Replace("-", " ")
                .Split(' ')
                .Where(x => !string.IsNullOrEmpty(x))
                .Select(x => x.Trim() + "*");

            var termsString = string.Join(" ", terms);

            var parser = new QueryParser(Version.LUCENE_30, fieldName, Analyzer);
            parser.PhraseSlop = 2;
            parser.FuzzyMinSim = 0.1f;
            parser.DefaultOperator = QueryParser.Operator.OR;

            var parsedQuery = Parse(queryText, parser);

            var booleanQuery = new BooleanQuery();
            booleanQuery.Add(parsedQuery, Occur.SHOULD);

            //var parsedTermsQuery = Parse(termsString, parser);
            //parsedTermsQuery.Boost = 0.3f;
            //booleanQuery.Add(parsedTermsQuery, Occur.SHOULD);

            var term = new Term(fieldName, queryText);

            if (fuzzy)
            {
                booleanQuery.Add(new FuzzyQuery(term), Occur.SHOULD);
            }

            var phraseQuery = new PhraseQuery();
            phraseQuery.Slop = 2;
            phraseQuery.Boost = 1.5f;
            phraseQuery.Add(term);

            booleanQuery.Add(phraseQuery, Occur.SHOULD);

            //booleanQuery.Add(new WildcardQuery(term), Occur.SHOULD);

            return booleanQuery;
        }

        public Query AddFilter(Query query, Filter filter)
        {
            Check.That(filter).IsNotNull();

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
            Check.That(filter).IsNotNull();

            return AddFilter(null, filter);
        }

        public void CleanupDeletes()
        {
            using (var writer = CreateWriter())
            {
                writer.ExpungeDeletes();
                writer.Commit();
            }
        }

        public void Optimize()
        {
            using (var writer = CreateWriter())
            {
                writer.Optimize();
            }
        }

        private IndexWriter CreateWriter()
        {
            return new IndexWriter(Directory, Analyzer, IndexWriter.MaxFieldLength.UNLIMITED);
        }

        /// <summary>
        ///     To be called after each update so that stats are reported correctly.
        /// </summary>
        private void CommitAndRefreshStats(IndexWriter writer, bool commit = true)
        {
            if (commit)
                writer.Commit();
            DocCount = writer.NumDocs();
        }

        private Query Parse(string text, QueryParser parser)
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

        public static FSDirectory PreparePersistentDirectory(string directoryPath)
        {
            var directoryInfo = new DirectoryInfo(directoryPath);

            if (!directoryInfo.Exists)
                directoryInfo.Create();

            var dir = FSDirectory.Open(directoryInfo);

            if (IndexWriter.IsLocked(dir))
                IndexWriter.Unlock(dir);

            var lockFilePath = Path.Combine(directoryPath, WriteLockFileName);

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
        /// <param name="name">
        ///     The name of a stemmer is the part of the class name before "Stemmer", e.g., the stemmer in EnglishStemmer is named "English". 
        /// </param>
        /// <returns></returns>
        public static Analyzer CreateSnowballAnalyzer(string stemmerName)
        {
            Check.That(GetAvailableSnowballStemmers()).Contains(stemmerName);

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

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (Directory != null)
                    {
                        Directory.Dispose();
                    }

                    if (Analyzer != null)
                    {
                        Analyzer.Dispose();
                    }
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~LuceneIndex() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion IDisposable Support

    }
}