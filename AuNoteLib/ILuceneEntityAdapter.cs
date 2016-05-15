// /**********************************************************************************************
// Author:		Vasily Kabanov
// Created		2016-05-15
// Comment		
// **********************************************************************************************/

using System;
using System.Collections.Generic;
using Lucene.Net.Documents;
using Lucene.Net.Search;

namespace AuNoteLib
{
    /// <summary>
    ///     
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
        Document GetIndexedDocument(TData item);

        IEnumerable<Document> GetIndexedDocuments(params TData[] items);

        THeader GetHeader(Document doc);

        IList<THeader> GetHeaders(IEnumerable<SearchHit> searchResult);
    }
}