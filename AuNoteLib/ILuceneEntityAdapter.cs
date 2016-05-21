// /**********************************************************************************************
// Author:		Vasily Kabanov
// Created		2016-05-15
// Comment		
// **********************************************************************************************/

using System;
using System.Collections.Generic;
using Lucene.Net.Documents;
using Lucene.Net.Index;

namespace AuNoteLib
{
    /// <summary>
    ///     Adapter of entity with 1 or 2 searchable fields: text and optional time.
    /// </summary>
    /// <typeparam name="TData">
    ///     The type exposing both data stored in full-text index and searchable data not stored in index
    /// </typeparam>
    /// <typeparam name="THeader">
    ///     Metadata or part of entity that will be stored in the lucene index
    /// </typeparam>
    public interface ILuceneEntityAdapter<TData, THeader>
        where THeader : class
        where TData : THeader
    {
        string DocumentKeyName { get; }

        /// <summary>
        ///     Optional, name of the field containing default searchable time field.
        ///     null or empty if no such field exists.
        /// </summary>
        string TimeFieldName { get; }

        /// <summary>
        ///     Name of the field in lucene <see cref="Document"/> which is analyzed and searched by full text queries.
        /// </summary>
        string SearchFieldName { get; }

        Term GetKeyTerm(THeader header);

        Document GetIndexedDocument(TData item);

        IEnumerable<Document> GetIndexedDocuments(params TData[] items);

        THeader GetHeader(Document doc);

        IList<THeader> GetHeaders(IEnumerable<LuceneSearchHit> searchResult);
    }
}