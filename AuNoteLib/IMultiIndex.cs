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
    public interface IMultiIndex
    {
        /// <summary>
        ///     Name to pass to <see cref="Lucene.Net.Documents.Lucene.Net.Documents.Document.Get(string)" /> 'primary key'
        /// </summary>
        string KeyFieldName { get; }

        void AddIndex(string name, ILuceneIndex index);

        ILuceneIndex GetIndex(string name);

        /// <summary>
        ///     Clear index contents (e.g. to rebuild).
        /// </summary>
        void Clear();

        List<SearchHit> Search(string searchFieldName, string queryText, int maxResults);

        void Add(Document doc);

        void Delete(string key);

        void CleanupDeletes();

        void Optimize();
    }
}