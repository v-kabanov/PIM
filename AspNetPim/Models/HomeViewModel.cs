// /**********************************************************************************************
// Author:  Vasily Kabanov
// Created  2017-06-09
// Comment  
// **********************************************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using FulltextStorageLib;
using FulltextStorageLib.Util;
using log4net;

namespace AspNetPim.Models
{
    public class HomeViewModel
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private const int MaxNumberOfNotesInRecentList = 10;
        private const int TargetNoteTextSummaryLength = 200;

        public IList<Note> LastUpdatedNotes { get; private set; }

        public INoteStorage NoteStorage { get; private set; }

        private Note _changedNote;

        private bool _changeIsDeletion;

        public HomeViewModel()
        {
        }

        public HomeViewModel(INoteStorage noteStorage)
        {
            Check.DoRequireArgumentNotNull(noteStorage, nameof(noteStorage));

            NoteStorage = noteStorage;
        }

        public void Initialize(INoteStorage noteStorage)
        {
            NoteStorage = noteStorage ?? DependencyResolver.Current.GetService<INoteStorage>();
        }

        [AllowHtml]
        [Required(AllowEmptyStrings = false)]
        public string NewNoteText { get; set; }

        public void LoadLatest()
        {
            int resultCount = MaxNumberOfNotesInRecentList;

            if (_changedNote != null && _changeIsDeletion)
                ++resultCount;

            var lastHeaders = NoteStorage.GetTopInPeriod(null, DateTime.Now, resultCount, SearchableDocumentTime.LastUpdate);
            LastUpdatedNotes = lastHeaders.Select(h => NoteStorage.GetExisting(h.Id)).Where(x => x != null).ToList();

            if (lastHeaders.Count > LastUpdatedNotes.Count)
                Log.WarnFormat("Fulltext index out of sync: rebuild it");

            // FT index is updated asynchronously, so when we've just created new note it may not be returned
            if (_changedNote != null)
            {
                if (!_changeIsDeletion && !LastUpdatedNotes.Contains(_changedNote))
                    LastUpdatedNotes.Insert(0, _changedNote);
                else if (_changeIsDeletion && LastUpdatedNotes.Contains(_changedNote))
                    LastUpdatedNotes.Remove(_changedNote);
            }

            if (LastUpdatedNotes.Count > MaxNumberOfNotesInRecentList)
                LastUpdatedNotes.RemoveAt(LastUpdatedNotes.Count - 1);
        }

        public Note CreateNew()
        {
            if (string.IsNullOrWhiteSpace(NewNoteText))
                return null;

            _changedNote = Note.Create(NewNoteText);

            NoteStorage.SaveOrUpdate(_changedNote);
            NewNoteText = string.Empty;

            return _changedNote;
        }

        public Note Delete(string noteId)
        {
            _changedNote = NoteStorage.Delete(noteId);

            _changeIsDeletion = true;

            return _changedNote;
        }

        public static string GetNoteTextSummary(INote note)
        {
            Check.DoRequireArgumentNotNull(note, nameof(note));
            if (note.Text == null)
                return null;

            var bodyStartIndex = note.Text.IndexOf(note.Name) + note.Name.Length;

            return StringHelper.GetTextWithLimit(note.Text, bodyStartIndex, TargetNoteTextSummaryLength);
        }
    }
}