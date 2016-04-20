// /**********************************************************************************************
// Author:		Vasily Kabanov
// Created		2016-04-20
// Comment		
// **********************************************************************************************/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Ru;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Version = Lucene.Net.Util.Version;

namespace PimTest
{
    public class LuceneIndex : IDisposable
    {
        private const string FieldNameId = "Id";
        private const string FieldNameCreateTime = "CreateTime";
        private const string FieldNameName = "Name";
        private const string FieldNameText = "Text";
        private string _directoryPath;

        public Lucene.Net.Store.Directory Directory { get; private set; }

        public Analyzer Analyzer { get; private set; }

        public LuceneIndex()
        {
            ConfigureRam();
            //new RussianAnalyzer();
            Analyzer = new Lucene.Net.Analysis.Snowball.SnowballAnalyzer(Version.LUCENE_30, "English");
        }

        public LuceneIndex(string directoryPath)
        {
            ConfigurePersistent(directoryPath);
            Analyzer = new StandardAnalyzer(Version.LUCENE_30);
        }

        private void ConfigurePersistent(string directoryPath)
        {
            _directoryPath = directoryPath;

            var directoryInfo = new DirectoryInfo(directoryPath);

            if (!directoryInfo.Exists)
                directoryInfo.Create();

            var dir = Lucene.Net.Store.FSDirectory.Open(directoryInfo);

            if (IndexWriter.IsLocked(dir))
                IndexWriter.Unlock(dir);

            var lockFilePath = Path.Combine(directoryPath, "write.lock");

            if (File.Exists(lockFilePath))
                File.Delete(lockFilePath);

            Directory = dir;
        }

        private void ConfigureRam()
        {
            Directory = new Lucene.Net.Store.RAMDirectory();
        }

        private TermQuery GetIdQuery(Note note)
        {
            return new TermQuery(new Term(FieldNameId, Convert.ToString(note.Id)));
        }

        private Document GetIndexedDocument(Note note)
        {
            var doc = new Document();
            doc.Add(new Field(FieldNameId, note.Id.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED));
            doc.Add(new Field(FieldNameCreateTime, note.CreateTime.ToString(CultureInfo.InvariantCulture), Field.Store.YES, Field.Index.NOT_ANALYZED));
            doc.Add(new Field(FieldNameName, note.Name, Field.Store.YES, Field.Index.ANALYZED));
            doc.Add(new Field(FieldNameText, note.Text, Field.Store.NO, Field.Index.ANALYZED));
            return doc;
        }

        private Note GetNote(Document indexeDocument)
        {
            return new Note()
            {
                Id = Convert.ToInt32(indexeDocument.Get(FieldNameId)),
                CreateTime = DateTime.Parse(indexeDocument.Get(FieldNameCreateTime), CultureInfo.InvariantCulture),
                Name = indexeDocument.Get(FieldNameName)
            };
        }

        public void Add(IEnumerable<Document> items)
        {
            using (var writer = new IndexWriter(Directory, Analyzer, IndexWriter.MaxFieldLength.UNLIMITED))
            {
                foreach (var item in items)
                {
                    writer.AddDocument(item);
                }
            }
        }

        public void Add(params Note[]  notes)
        {
            Add(notes.Select(n => GetIndexedDocument(n)));
        }

        private List<Note> Search(Query query, int maxResults)
        {
            using (var search = new IndexSearcher(Directory, true))
            {
                var hits = search.Search(query, null, maxResults, Sort.RELEVANCE).ScoreDocs;

                return hits.Select(h => GetNote(search.Doc(h.Doc))).ToList();
            }
        }

        public List<Note> Search(string text, int maxResults = 100)
        {
            var terms = text.Trim()
                .Replace("-", " ")
                .Split(' ')
                .Where(x => !string.IsNullOrEmpty(x))
                .Select(x => x.Trim() + "*");

            var termsString = string.Join(" ", terms);

            var parser = new QueryParser(Version.LUCENE_30, FieldNameText, Analyzer);
            parser.PhraseSlop = 2;
            parser.FuzzyMinSim = 0.1f;
            parser.DefaultOperator = QueryParser.Operator.OR;

            var parsedQuery = Parse(text, parser);

            var parsedTermsQuery = Parse(termsString, parser);
            parsedTermsQuery.Boost = 0.3f;

            var term = new Term(FieldNameText, text);
            var fuzzyQuery = new FuzzyQuery(term);

            var phraseQuery = new PhraseQuery();
            phraseQuery.Slop = 2;
            phraseQuery.Boost = 1.5f;
            phraseQuery.Add(term);

            var wildcardQuery = new WildcardQuery(term);

            var booleanQuery = new BooleanQuery();
            booleanQuery.Add(parsedQuery, Occur.SHOULD);
            //booleanQuery.Add(parsedTermsQuery, Occur.SHOULD);
            booleanQuery.Add(fuzzyQuery, Occur.SHOULD);
            booleanQuery.Add(phraseQuery, Occur.SHOULD);
            //booleanQuery.Add(wildcardQuery, Occur.SHOULD);

            return Search(booleanQuery, maxResults);
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
        #endregion
    }
}