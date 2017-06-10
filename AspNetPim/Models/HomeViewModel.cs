// /**********************************************************************************************
// Author:  Vasily Kabanov
// Created  2017-06-09
// Comment  
// **********************************************************************************************/

using System;
using System.Collections.Generic;
using FulltextStorageLib;
using FulltextStorageLib.Util;

namespace AspNetPim.Models
{
    public class HomeViewModel
    {
        private const int MaxNumberOfNotesInRecentList = 20;

        public IList<INoteHeader> LastUpdatedNotes { get; private set; }

        public INoteStorage NoteStorage { get; }

        private Note _changedNote;

        private bool _changeIsDeletion;

        public HomeViewModel(INoteStorage noteStorage)
        {
            Check.DoRequireArgumentNotNull(noteStorage, nameof(noteStorage));

            NoteStorage = noteStorage;
        }

        public string NewNoteText { get; set; }

        public void LoadLatest()
        {
            int resultCount = MaxNumberOfNotesInRecentList;

            if (_changedNote != null && _changeIsDeletion)
                ++resultCount;

            LastUpdatedNotes = NoteStorage.GetTopInPeriod(null, DateTime.Now, resultCount, SearchableDocumentTime.LastUpdate);

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
            _changedNote = Note.Create(NewNoteText);

            NoteStorage.SaveOrUpdate(_changedNote);

            return _changedNote;
        }

        public Note Delete(string noteId)
        {
            _changedNote = NoteStorage.Delete(noteId);

            _changeIsDeletion = true;

            return _changedNote;
        }
    }
}