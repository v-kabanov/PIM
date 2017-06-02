// /**********************************************************************************************
// Author:		Vasily Kabanov
// Created		2016-05-15
// Comment		
// **********************************************************************************************/

using System;
using System.Collections.Generic;
using Lucene.Net.Documents;
using Lucene.Net.Index;

namespace FulltextStorageLib
{
    /// <summary>
    ///     Adapter of entity with 1 or 2 searchable fields: text and optional time.
    /// </summary>
    /// <typeparam name="TDoc">
    ///     The type exposing both data stored in full-text index and searchable data not stored in index
    /// </typeparam>
    /// <typeparam name="THeader">
    ///     Metadata or part of entity that will be stored in the lucene index; note that some data may be indexed, but not stored in index.
    /// </typeparam>
    /// <typeparam name="TStorageKey">
    ///     Type of the primary key used by document database
    /// </typeparam>
    public interface ILuceneEntityAdapter<in TDoc, THeader, TStorageKey>
        where THeader : IFulltextIndexEntry
        where TDoc : class
    {
        string DocumentKeyName { get; }

        /// <summary>
        ///     Optional, name of the field containing last document update time.
        ///     null or empty if no such field exists.
        /// </summary>
        string LastUpdateTimeFieldName { get; }

        /// <summary>
        ///     Optional, name of the field containing document creation time.
        ///     null or empty if no such field exists.
        /// </summary>
        string CreationTimeFieldName { get; }

        /// <summary>
        ///     Name of the field in lucene <see cref="Document"/> which is analyzed and searched by full text queries.
        /// </summary>
        string SearchFieldName { get; }

        Term GetKeyTerm(THeader header);

        string GetFulltextKey(TDoc doc);

        TStorageKey GetStorageKey(THeader header);

        /// <summary>
        ///     Convert storage key to fulltext key.
        /// </summary>
        /// <param name="storageKey">
        ///     Primary key used by document storage.
        /// </param>
        /// <returns>
        ///     Fulltext key
        /// </returns>
        /// <exception cref="InvalidOperationException">
        ///     <see cref="CanConvertStorageKey"/> is false
        /// </exception>
        string GetFulltextFromStorageKey(TStorageKey storageKey);

        /// <summary>
        ///     Convert fulltext key to storage key.
        /// </summary>
        /// <param name="fulltextKey">
        ///     Mandatory
        /// </param>
        /// <returns>
        ///     Storage key
        /// </returns>
        /// <exception cref="InvalidOperationException">
        ///     <see cref="CanConvertFulltextKey"/> is false
        /// </exception>
        TStorageKey GetStorageFromFulltextKey(string fulltextKey);

        /// <summary>
        ///     Whether adapter can convert storage key to fulltext key (see <see cref="GetFulltextFromStorageKey"/>)
        /// </summary>
        bool CanConvertStorageKey { get; }

        /// <summary>
        ///     Whether adapter can convert fulltext key to storage key (see <see cref="GetStorageFromFulltextKey"/>)
        /// </summary>
        bool CanConvertFulltextKey { get; }

        Document GetIndexedDocument(TDoc item);

        IEnumerable<Document> GetIndexedDocuments(params TDoc[] items);

        IEnumerable<Document> GetIndexedDocuments(IEnumerable<TDoc> items);

        THeader GetHeader(Document doc);

        IList<THeader> GetHeaders(IEnumerable<LuceneSearchHit> searchResult);
    }
}