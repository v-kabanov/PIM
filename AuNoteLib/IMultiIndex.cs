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
        void AddIndex(string name, ILuceneIndex index);

        ILuceneIndex GetIndex(string name);

        /// <summary>
        ///     Clear index contents (e.g. to rebuild).
        /// </summary>
        void Clear();

        List<SearchHit> Search(string queryText, DateTime? from, DateTime? to, bool fuzzy = false, int maxResults = 20);

        void Add(Document doc);

        void Delete(string key);

        void CleanupDeletes();

        void Optimize();
    }
}