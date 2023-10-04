// /**********************************************************************************************
// Author:  Vasily Kabanov
// Created  2017-06-09
// Comment  
// **********************************************************************************************/

using System.ComponentModel.DataAnnotations;
using System.Reflection;
using log4net;
using PimWeb.AppCode;

namespace PimWeb.Models;

public class HomeViewModel
{
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    private const int MaxNumberOfNotesInRecentList = 10;
    private const int TargetNoteTextSummaryLength = 200;

    public IList<Note> LastUpdatedNotes { get; private set; }

    public INoteService NoteService { get; private set; }

    private Note _changedNote;

    private bool _changeIsDeletion;

    public HomeViewModel()
    {
    }

    public HomeViewModel(INoteService noteService)
    {
        if (noteService == null) throw new ArgumentNullException(nameof(noteService));

        NoteService = noteService;
    }

    public void Initialize(INoteService noteStorage)
    {
        NoteService = noteStorage;
    }

    [Required(AllowEmptyStrings = false)]
    public string NewNoteText { get; set; }

    public void LoadLatest()
    {
        var resultCount = MaxNumberOfNotesInRecentList;

        if (_changedNote != null && _changeIsDeletion)
            ++resultCount;

        // ReSharper disable once RedundantArgumentDefaultValue
        var lastHeaders = NoteService.GetTopInPeriod(null, DateTime.Now, resultCount, 0, SearchableDocumentTime.LastUpdate, out bool moreExist);
        LastUpdatedNotes = lastHeaders.Select(h => NoteService.Get(h.Id)).Where(x => x != null).ToList();

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

        NoteService.SaveOrUpdate(_changedNote);
        NewNoteText = string.Empty;

        return _changedNote;
    }

    public Note Delete(int id)
    {
        _changedNote = NoteService.Delete(id);

        _changeIsDeletion = true;

        return _changedNote;
    }

    public static string GetNoteTextSummary(Note note)
    {
        if (note == null) throw new ArgumentNullException(nameof(note));

        if (note.Text == null)
            return null;

        var bodyStartIndex = note.Text.IndexOf(note.Name, StringComparison.Ordinal) + note.Name.Length;

        return note.Text.GetTextWithLimit(bodyStartIndex, TargetNoteTextSummaryLength);
    }
}