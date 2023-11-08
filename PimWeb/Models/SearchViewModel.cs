// /**********************************************************************************************
// Author:  Vasily Kabanov
// Created  2017-06-13
// Comment  
// **********************************************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using PimWeb.AppCode;

namespace PimWeb.Models;

public enum SortProperty
{
    LastUpdateTime,
    CreationTime,
    SearchRank
}

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
        LastUpdatePeriodStart = other.LastUpdatePeriodStart;
        LastUpdatePeriodEnd = other.LastUpdatePeriodEnd;
    }

    public string Query { get; set; }

    [DisplayName("Last update time from:")]
    [DisplayFormat(DataFormatString = "{0:dd MMM yy}")]
    public DateTime? LastUpdatePeriodStart { get; set; }

    [DisplayName("to:")]
    [DisplayFormat(DataFormatString = "{0:dd MMM yy}")]
    public DateTime? LastUpdatePeriodEnd { get; set; }

    [DisplayName("Creation time from:")]
    [DisplayFormat(DataFormatString = "{0:dd MMM yy}")]
    public DateTime? CreationPeriodStart { get; set; }

    [DisplayName("to:")]
    [DisplayFormat(DataFormatString = "{0:dd MMM yy}")]
    public DateTime? CreationPeriodEnd { get; set; }
    
    public bool HasTimeFilter => CreationPeriodStart.HasValue
        || CreationPeriodEnd.HasValue
        || LastUpdatePeriodStart.HasValue
        || LastUpdatePeriodEnd.HasValue;

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

    public List<NoteViewModel> SearchResultPage { get; set; } = new ();
    
    public bool Fuzzy { get; set; }
    
    [DisplayName("Sort")]
    public SortProperty? SortProperty { get; set; }

    [DisplayName("Asc")]
    public bool SortAscending { get; set; }
    
    public List<SelectListItem> SortOptions { get; set; } = new ();
}