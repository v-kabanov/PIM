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

    private const int TargetNoteTextSummaryLength = 200;

    public List<NoteViewModel> LastUpdatedNotes { get; set; }


    [Required(AllowEmptyStrings = false)]
    public string NewNoteText { get; set; }

    public static string GetNoteTextSummary(NoteViewModel note)
    {
        if (note == null) throw new ArgumentNullException(nameof(note));

        if (note.NoteText == null)
            return null;

        var bodyStartIndex = note.NoteText.IndexOf(note.Caption, StringComparison.Ordinal) + note.Caption.Length;

        return note.NoteText.GetTextWithLimit(bodyStartIndex, TargetNoteTextSummaryLength);
    }
}