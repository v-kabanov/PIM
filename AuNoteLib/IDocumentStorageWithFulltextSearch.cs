// /**********************************************************************************************
// Author:		Vasily Kabanov
// Created		2017-05-25
// Comment		
// **********************************************************************************************/

using System;
using System.Collections.Generic;
using System.Reflection;
using log4net;

namespace AuNoteLib
{
    /// <summary>
    ///     Component which adds fulltext search to document storage. It contains storage because access to it is required for index
    ///     maintenance, e.g. for rebuilding or adding fulltext index.
    /// </summary>
    /// <typeparam name="TDoc">
    ///     Document type
    /// </typeparam>
    /// <typeparam name="TKey">
    ///     Document key type
    /// </typeparam>
    /// <typeparam name="THeader">
    ///     Document summary type representing part of the document added contained in fulltext indexes.
    /// </typeparam>
    public interface IDocumentStorageWithFulltextSearch<TDoc, TKey, THeader> : IDocumentStorage<TDoc, TKey>, IFulltextSearchEngine<THeader>
        where THeader : class
        where TDoc : class, THeader
    {
        /// <summary>
        ///     Opens existing indexes.
        /// </summary>
        void Open();

        /// <summary>
        ///     Start work ensuring indexes for specified stemmers exist and are active.
        /// </summary>
        /// <param name="stemmerNames">
        /// </param>
        /// <param name="progressReporter">
        ///     Optional delegate receiving progress updates (completion percent 0..1)
        /// </param>
        void EnsureIndexesAsync(IEnumerable<string> stemmerNames, Action<double> progressReporter = null);

        void RebuildIndex(string stemmerName, Action<double> progressReporter = null);

        void RebuildIndexes(IEnumerable<string> names, Action<double> progressReporter = null);

        void AddIndex(string stemmerName);

        void RemoveIndex(string stemmerName);

        void SetDefaultIndex(string stemmerName);
    }

    public class DocumentStorageWithFulltextSearch<TDoc, TKey, THeader> : IDocumentStorageWithFulltextSearch<TDoc, TKey, THeader>
        where THeader : class
        where TDoc : class, THeader
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public string RootDirectory { get; }

        public IMultiIndex MultiIndex { get; }

        public ILuceneEntityAdapter<TDoc, THeader> EntityAdapter { get; }

        public IDocumentStorage<TDoc, TKey> Storage { get; }

        public IFulltextSearchEngine<THeader> SearchEngine { get; }

        public void SaveOrUpdate(TDoc document)
        {
            Storage.SaveOrUpdate(document);

            SearchEngine.
        }

        public TDoc GetExisting(TKey id)
        {
            throw new NotImplementedException();
        }

        public TDoc Delete(TKey id)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<TDoc> GetAll()
        {
            throw new NotImplementedException();
        }

        public int CountAll()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> ActiveIndexNames { get; }
        public IList<THeader> Search(string queryText, int maxResults)
        {
            throw new NotImplementedException();
        }

        public IList<THeader> GetTopInPeriod(DateTime? periodStart, DateTime? periodEnd, int maxResults)
        {
            throw new NotImplementedException();
        }

        public void Open()
        {
            throw new NotImplementedException();
        }

        public void EnsureIndexesAsync(IEnumerable<string> stemmerNames, Action<double> progressReporter = null)
        {
            throw new NotImplementedException();
        }

        public void RebuildIndex(string stemmerName, Action<double> progressReporter = null)
        {
            throw new NotImplementedException();
        }

        public void RebuildIndexes(IEnumerable<string> names, Action<double> progressReporter = null)
        {
            throw new NotImplementedException();
        }

        public void AddIndex(string stemmerName)
        {
            throw new NotImplementedException();
        }

        public void RemoveIndex(string stemmerName)
        {
            throw new NotImplementedException();
        }

        public void SetDefaultIndex(string stemmerName)
        {
            throw new NotImplementedException();
        }
    }
}