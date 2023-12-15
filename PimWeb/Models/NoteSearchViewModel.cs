// /**********************************************************************************************
// Author:  Vasily Kabanov
// Created  2017-06-13
// Comment  
// **********************************************************************************************/

namespace PimWeb.Models;

public class NoteSearchViewModel : SearchModelBase
{
    public NoteSearchViewModel()
    {
    }

    /// <summary>
    ///     Copies user input only
    /// </summary>
    /// <param name="other">
    ///     Mandatory
    /// </param>
    public NoteSearchViewModel(SearchModelBase other)
        :base(other)
    {
    }

    public int NoteId { get; set; }

    public List<NoteViewModel> SearchResultPage { get; set; } = new ();
}