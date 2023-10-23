// /**********************************************************************************************
// Author:  Vasily Kabanov
// Created  2017-06-13
// Comment  
// **********************************************************************************************/

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using PimWeb.AppCode;

namespace PimWeb.Models;

public class SearchViewModel
{
    public SearchViewModel()
    {
    }

    /// <summary>
    ///     Copies user input only
    /// </summary>
    /// <param name="other">
    ///     Mandatory
    /// </param>
    public SearchViewModel(SearchViewModel other)
    {
        if (other == null) throw new ArgumentNullException(nameof(other));
        Query = other.Query;
        PeriodStart = other.PeriodStart;
        PeriodEnd = other.PeriodEnd;
    }

    [Required(AllowEmptyStrings = false)]
    public string Query { get; set; }

    [DisplayName("Period start (inclusive):")]
    [DisplayFormat(DataFormatString = "{0:dd MMM yy}")]
    public DateTime? PeriodStart { get; set; }

    [DisplayName("Period end (exclusive):")]
    [DisplayFormat(DataFormatString = "{0:dd MMM yy}")]
    public DateTime? PeriodEnd { get; set; }

    public int NoteId { get; set; }

    /// <summary>
    ///     1 - based
    /// </summary>
    public int PageNumber { get; set; } = 1;

    /// <summary>
    ///     The number or counted notes is limited (e.g. to 10 pages past the selected page) for performance reasons.
    /// </summary>
    public int? TotalCountedPageCount { get; set; }
    
    public bool HasMore { get; set; }

    public List<Note> SearchResultPage { get; set; } = new ();
    
    public bool Fuzzy { get; set; }
}