// /**********************************************************************************************
// Author:		Vasily Kabanov
// Created		2016-05-15
// Comment		
// **********************************************************************************************/

using System;
using System.Collections.Generic;
using Lucene.Net.Documents;

namespace AuNoteLib
{
    public interface IMultiIndex : IDisposable
    {
        /// <summary>
        ///     Name to pass to <see cref="Lucene.Net.Documents.Lucene.Net.Documents.Document.Get(string)" /> 'primary key'
        /// </summary>
        string KeyFieldName { get; }

        /// <summary>
        ///     The index to use by default when e.g. they are [supposed to be] interchangeable such as when searching by time.
        ///     Default index cannot be deleted with <see cref="RemoveIndex"/>.
        /// </summary>
        string DefaultIndexName { get; set; }

        /// <summary>
        ///     Add new index to be used in parallel with existing ones. Index must be prepopulated already if necessary, no data is added to it here.
        /// </summary>
        /// <param name="name">
        ///     case insensitive
        /// </param>
        /// <param name="index">
        /// </param>
        void AddIndex(string name, ILuceneIndex index);

        /// <summary>
        ///     Close given index and remove it from searches. Storage is managed externally and can be removed by the caller afterwards.
        /// </summary>
        /// <param name="name">
        /// </param>
        void RemoveIndex(string name);

        ILuceneIndex GetIndex(string name);

        /// <summary>
        ///     Clear index contents (e.g. to rebuild).
        /// </summary>
        void Clear();

        IList<SearchHit> Search(string searchFieldName, string queryText, int maxResults);

        /// <summary>
        ///     Get last <paramref name="maxResults"/> documents with time field (identified by <paramref name="timeFieldName"/>) value
        ///     within the specified time range in reverse chronological order.
        /// </summary>
        /// <param name="timeFieldName">
        ///     Name of the field containing time, see <see cref="Document.Get"/>'s parameter
        /// </param>
        /// <param name="periodStart">
        ///     null means no restriction
        /// </param>
        /// <param name="periodEnd">
        ///     null means no restriction
        /// </param>
        /// <param name="maxResults">
        ///     must be positive
        /// </param>
        /// <returns>
        /// </returns>
        IList<SearchHit> GetTopInPeriod(string timeFieldName, DateTime periodStart, DateTime periodEnd, int maxResults);

        void Add(Document doc);

        void Delete(string key);

        void CleanupDeletes();

        void Optimize();
    }
}