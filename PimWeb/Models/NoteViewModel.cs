// /**********************************************************************************************
// Author:  Vasily Kabanov
// Created  2017-06-12
// Comment  
// **********************************************************************************************/
// 

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using PimWeb.AppCode;

namespace PimWeb.Models;

public class NoteViewModel
{
    public int NoteId { get; set; }

    [DisplayName("Text")]
    [Required(AllowEmptyStrings = false)]
    public string NoteText { get; set; }

    [DisplayFormat(DataFormatString = "{0:f}")]
    [DisplayName("Creation Time")]
    public DateTime? CreateTime => Note?.CreateTime;

    [DisplayName("Last Update Time")]
    [DisplayFormat(DataFormatString = "{0:f}")]
    public DateTime? LastUpdateTime => Note?.LastUpdateTime;

    [DisplayName("Version")]
    public int? Version => Note?.IntegrityVersion;

    public bool NoteDeleted { get; private set; }

    public INoteService NoteService { get; private set; }

    public Note Note { get; private set; }

    public NoteViewModel()
    {
    }

    public NoteViewModel(INoteService noteService)
    {
        NoteService = noteService;
    }

    public void Initialize(INoteService noteService)
    {
        NoteService = noteService;
    }

    public void Load()
    {
        Note = ReadFromStorage();

        NoteText = Note.Text;
    }

    public void Update()
    {
        var newText = NoteText?.Trim();

        if (string.IsNullOrEmpty(newText))
            throw new Exception("Note text must not be empty.");

        if (Note == null)
            Note = ReadFromStorage();

        if (Note.Text != newText)
        {
            Note.Text = newText;
            Note.LastUpdateTime = DateTime.Now;

            NoteService.SaveOrUpdateAsync(Note);
        }
    }

    public void Delete()
    {
        Note = NoteService.DeleteAsync(NoteId);

        if(Note == null)
            throw new Exception($"Note {NoteId} does not exist");

        NoteDeleted = true;
    }

    private Note ReadFromStorage()
    {
        var result = NoteService.Get(NoteId);

        if(Note == null)
            throw new Exception($"Note {NoteId} does not exist");

        return result;
    }
}