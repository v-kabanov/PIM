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
        /// <param name="index">
        ///     Index for which queries will be generated. The same analyzer must be used for writing index and parsing queries for it.
        /// </param>
        public LuceneNoteAdapter(ILuceneIndex index)
        {
            Index = index;
        }

        public ILuceneIndex Index { get; private set; }

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

        public Query CreateQuery(string text, int maxResults = 100)
        {
            var terms = text.Trim()
                .Replace("-", " ")
                .Split(' ')
                .Where(x => !string.IsNullOrEmpty(x))
                .Select(x => x.Trim() + "*");

            var termsString = string.Join(" ", terms);

            var parser = new QueryParser(Version.LUCENE_30, FieldNameText, Index.Analyzer);
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

            return booleanQuery;
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