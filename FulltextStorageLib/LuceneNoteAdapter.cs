// /**********************************************************************************************
// Author:		Vasily Kabanov
// Created		2016-04-30
// Comment		
// **********************************************************************************************/

using System.Collections.Generic;
using System.Linq;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Pim.CommonLib;

namespace FulltextStorageLib;

public interface ILuceneNoteAdapter : ILuceneEntityAdapter<INote, INoteHeader, string> { }

public class LuceneNoteAdapter : ILuceneNoteAdapter
{
    public const string FieldNameId = "Id";
    public const string FieldNameCreateTime = "CreateTime";
    public const string FieldNameLastUpdateTime = "LastUpdateTime";
    public const string FieldNameName = "Name";
    public const string FieldNameText = "Text";
    public const string FieldNameVersion = "Version";


    public string DocumentKeyName => FieldNameId;

    /// <summary>
    ///     Name (in the lucene <see cref="Document"/>) of the field containing last note update time.
    /// </summary>
    public string LastUpdateTimeFieldName => FieldNameLastUpdateTime;

    /// <summary>
    ///     Name (in the lucene <see cref="Document"/>) of the field containing note creation time.
    /// </summary>
    public string CreationTimeFieldName => FieldNameCreateTime;

    /// <summary>
    ///     Name of the field in lucene <see cref="Document"/> which is analyzed and searched by full text queries.
    /// </summary>
    public string SearchFieldName => FieldNameText;

    public string GetFulltextKey(INote doc)
    {
        return doc.Id;
    }

    public string GetStorageKey(INoteHeader header)
    {
        return header.Id;
    }

    public string GetFulltextFromStorageKey(string storageKey)
    {
        return storageKey;
    }

    public string GetStorageFromFulltextKey(string fulltextKey)
    {
        return fulltextKey;
    }

    public bool CanConvertStorageKey => true;
    public bool CanConvertFulltextKey => true;

    public Term GetKeyTerm(INoteHeader note)
    {
        return new Term(FieldNameId, note.Id);
    }

    public Document GetIndexedDocument(INote note)
    {
        var doc = new Document();
        var lastUpdateTimeString = DateTools.DateToString(note.LastUpdateTime, DateTools.Resolution.SECOND);
        var creationTimeString = DateTools.DateToString(note.CreateTime, DateTools.Resolution.SECOND);

        doc.Add(new Field(FieldNameId, note.Id, Field.Store.YES, Field.Index.NOT_ANALYZED));
        doc.Add(new Field(FieldNameCreateTime, creationTimeString, Field.Store.YES, Field.Index.ANALYZED));
        doc.Add(new Field(FieldNameLastUpdateTime, lastUpdateTimeString, Field.Store.YES, Field.Index.ANALYZED));
        doc.Add(new Field(FieldNameName, note.Name, Field.Store.YES, Field.Index.NOT_ANALYZED));
        doc.Add(new Field(FieldNameText, note.Text, Field.Store.NO, Field.Index.ANALYZED));

        return doc;
    }

    public IEnumerable<Document> GetIndexedDocuments(params INote[] notes)
    {
        return notes.Select(GetIndexedDocument);
    }

    public IEnumerable<Document> GetIndexedDocuments(IEnumerable<INote> notes)
    {
        return notes.Select(GetIndexedDocument);
    }

    public INoteHeader GetHeader(Document indexeDocument)
    {
        return new NoteHeader()
        {
            Id = indexeDocument.Get(FieldNameId),
            CreateTime = DateTools.StringToDate(indexeDocument.Get(FieldNameCreateTime)),
            LastUpdateTime = DateTools.StringToDate(indexeDocument.Get(FieldNameLastUpdateTime)),
            Name = indexeDocument.Get(FieldNameName)
        };
    }

    public IList<INoteHeader> GetHeaders(IEnumerable<LuceneSearchHit> searchResult)
    {
        Check.DoRequireArgumentNotNull(searchResult, nameof(searchResult));

        return searchResult.Select(h => GetHeader(h.Document)).ToList();
    }

}