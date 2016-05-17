// /**********************************************************************************************
// Author:		Vasily Kabanov
// Created		2016-04-30
// Comment		
// **********************************************************************************************/

using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using NFluent;
using System;
using System.Collections.Generic;
using System.Linq;
using Version = Lucene.Net.Util.Version;

namespace AuNoteLib
{
    public interface ILuceneNoteAdapter : ILuceneEntityAdapter<INote, INoteHeader> { }

    public class LuceneNoteAdapter : ILuceneNoteAdapter
    {
        public const string FieldNameId = "Id";
        public const string FieldNameCreateTime = "CreateTime";
        public const string FieldNameLastUpdateTime = "LastUpdateTime";
        public const string FieldNameName = "Name";
        public const string FieldNameText = "Text";
        public const string FieldNameVersion = "Version";

        /// <summary>
        ///     
        /// </summary>
        public LuceneNoteAdapter()
        {
        }

        public Term GetKeyTerm(INoteHeader note)
        {
            return new Term(FieldNameId, Convert.ToString(note.Id));
        }

        public Document GetIndexedDocument(INote note)
        {
            var doc = new Document();
            var timeString = DateTools.DateToString(note.CreateTime, DateTools.Resolution.SECOND);

            doc.Add(new Field(FieldNameId, note.Id, Field.Store.YES, Field.Index.NOT_ANALYZED));
            doc.Add(new Field(FieldNameCreateTime, timeString, Field.Store.YES, Field.Index.ANALYZED));
            doc.Add(new Field(FieldNameName, note.Name, Field.Store.YES, Field.Index.NOT_ANALYZED));
            doc.Add(new Field(FieldNameText, note.Text, Field.Store.NO, Field.Index.ANALYZED));

            return doc;
        }

        public IEnumerable<Document> GetIndexedDocuments(params INote[] notes)
        {
            return notes.Select(n => GetIndexedDocument(n));
        }

        public INoteHeader GetHeader(Document indexeDocument)
        {
            return new NoteHeader()
            {
                Id = indexeDocument.Get(FieldNameId),
                CreateTime = DateTools.StringToDate(indexeDocument.Get(FieldNameCreateTime)),
                Name = indexeDocument.Get(FieldNameName)
            };
        }

        public IList<INoteHeader> GetHeaders(IEnumerable<SearchHit> searchResult)
        {
            Check.That(searchResult).IsNotNull();

            return searchResult.Select(h => GetHeader(h.Document)).ToList();
        }

    }
}