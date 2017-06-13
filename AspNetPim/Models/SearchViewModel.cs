// /**********************************************************************************************
// Author:  Vasily Kabanov
// Created  2017-06-13
// Comment  
// **********************************************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using FulltextStorageLib;
using log4net;

namespace AspNetPim.Models
{
    public class SearchViewModel
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public const int MaxPageCount = 10;
        public const int DefaultResultsPerPage = 10;

        private Note _deletedNote;

        public INoteStorage NoteStorage { get; private set; }

        public void Initialize(INoteStorage noteStorage)
        {
            NoteStorage = noteStorage ?? DependencyResolver.Current.GetService<INoteStorage>();
        }

        [Required(AllowEmptyStrings = false)]
        public string Query { get; set; }

        /// <summary>
        ///     1 - based
        /// </summary>
        public int PageNumber { get; set; } = 1;

        public int TotalPageCount { get; private set; }

        public IList<Note> SearchResultPage { get; private set; }

        public void Delete(string noteId)
        {
            _deletedNote = NoteStorage.Delete(noteId);
        }

        public void ExecuteSearch()
        {
            if (PageNumber < 1)
                PageNumber = 1;
            else if (PageNumber > MaxPageCount)
                PageNumber = MaxPageCount;

            var maxResults = MaxPageCount * DefaultResultsPerPage;

            var headers = NoteStorage.Search(Query, maxResults + 1);
            if (_deletedNote != null)
                headers.Remove(_deletedNote);
            else if (headers.Any() && headers.Count > maxResults)
                headers.RemoveAt(headers.Count - 1);

            TotalPageCount = (int)Math.Ceiling((double)headers.Count / DefaultResultsPerPage);

            var headersPage = headers
                .Skip((PageNumber - 1) * DefaultResultsPerPage)
                .Take(DefaultResultsPerPage)
                .ToList();

            SearchResultPage = headersPage
                .Select(h => NoteStorage.GetExisting(h.Id))
                .Where(x => x != null)
                .ToList();

            if (headersPage.Count > SearchResultPage.Count)
                Log.Warn("Fulltext index out of sync: rebuild it");
        }
    }
}