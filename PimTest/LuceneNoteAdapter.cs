// /**********************************************************************************************
// Author:		Vasily Kabanov
// Created		2016-04-30
// Comment		
// **********************************************************************************************/

using System;
using System.Linq;
using Lucene.Net.Analysis;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Version = Lucene.Net.Util.Version;
using System.Collections.Generic;
using NFluent;

namespace PimTest
{
    public class LuceneNoteAdapter
    {
        private const string FieldNameId = "Id";
        private const string FieldNameCreateTime = "CreateTime";
        private const string FieldNameName = "Name";
        private const string FieldNameText = "Text";

        /// <summary>
        ///     
        /// </summary>
        public LuceneNoteAdapter()
        {
        }

        public Term GetKeyTerm(Note note)
        {
            return new Term(FieldNameId, Convert.ToString(note.Id));
        }

        public Document GetIndexedDocument(Note note)
        {
            var doc = new Document();
            var timeString = DateTools.DateToString(note.CreateTime, DateTools.Resolution.SECOND);

            doc.Add(new Field(FieldNameId, note.Id.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED));
            doc.Add(new Field(FieldNameCreateTime, timeString, Field.Store.YES, Field.Index.ANALYZED));
            doc.Add(new Field(FieldNameName, note.Name, Field.Store.YES, Field.Index.ANALYZED));
            doc.Add(new Field(FieldNameText, note.Text, Field.Store.NO, Field.Index.ANALYZED));

            return doc;
        }

        public IEnumerable<Document> GetIndexedDocuments(params Note[] notes)
        {
            return notes.Select(n => GetIndexedDocument(n));
        }

        public Note GetNote(Document indexeDocument)
        {
            return new Note()
            {
                Id = Convert.ToInt32(indexeDocument.Get(FieldNameId)),
                CreateTime = DateTools.StringToDate(indexeDocument.Get(FieldNameCreateTime)),
                Name = indexeDocument.Get(FieldNameName)
            };
        }

        public List<SearchHit> Search(ILuceneIndex index, string text, bool fuzzy = false, int maxResults = 100)
        {
            return index.Search(CreateQuery(index, text, fuzzy), maxResults);
        }

        public List<SearchHit> Search(ILuceneIndex index, Query query, int maxResults = 100)
        {
            Check.That(query).IsNotNull();
            Check.That(index).IsNotNull();

            return index.Search(query, maxResults);
        }

        /// <summary>
        ///     Adds filter to query; combined with logical AND
        /// </summary>
        /// <param name="query"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public Query AddFilter(Query query, Filter filter)
        {
            Check.That(query).IsNotNull();
            Check.That(filter).IsNotNull();

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

            return AddFilter(new MatchAllDocsQuery(), filter);
        }

        public Query CreateQuery(ILuceneIndex index, string text, bool fuzzy = false)
        {
            var terms = text.Trim()
                .Replace("-", " ")
                .Split(' ')
                .Where(x => !string.IsNullOrEmpty(x))
                .Select(x => x.Trim() + "*");

            var termsString = string.Join(" ", terms);

            var parser = new QueryParser(Version.LUCENE_30, FieldNameText, index.Analyzer);
            parser.PhraseSlop = 2;
            parser.FuzzyMinSim = 0.1f;
            parser.DefaultOperator = QueryParser.Operator.OR;

            var parsedQuery = Parse(text, parser);

            var booleanQuery = new BooleanQuery();
            booleanQuery.Add(parsedQuery, Occur.SHOULD);

            //var parsedTermsQuery = Parse(termsString, parser);
            //parsedTermsQuery.Boost = 0.3f;
            //booleanQuery.Add(parsedTermsQuery, Occur.SHOULD);

            var term = new Term(FieldNameText, text);

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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="from">inclusive, seconds precision</param>
        /// <param name="to">exclusive, seconds precision</param>
        /// <returns></returns>
        public Filter CreateTimeRangeFilter(DateTime? from, DateTime? to)
        {
            Check.That(from.HasValue || to.HasValue).IsTrue();
            Check.That(!from.HasValue || !to.HasValue || from.Value < to.Value).IsTrue();

            string fromString = from.HasValue ? DateTools.DateToString(from.Value, DateTools.Resolution.SECOND) : null;
            string toString = to.HasValue ? DateTools.DateToString(to.Value, DateTools.Resolution.SECOND) : null;
            return new TermRangeFilter(FieldNameCreateTime, fromString, toString, true, false);
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
    }
}