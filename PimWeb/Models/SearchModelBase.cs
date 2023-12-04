using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace PimWeb.Models;

public enum SortProperty
{
    LastUpdateTime,
    CreationTime,
    SearchRank
}

public class SearchFormData
{
    public SearchFormData()
    {
    }
    
    /// <summary>
    ///     Copy constructor
    /// </summary>
    /// <param name="other">
    ///     Mandatory
    /// </param>
    public SearchFormData(SearchFormData other)
    {
        if (other == null) throw new ArgumentNullException(nameof(other));
        Query = other.Query;
        LastUpdatePeriodStart = other.LastUpdatePeriodStart;
        LastUpdatePeriodEnd = other.LastUpdatePeriodEnd;
        PageNumber = other.PageNumber;
        Fuzzy = other.Fuzzy;
        SortProperty = other.SortProperty;
        SortAscending = other.SortAscending;
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

    /// <summary>
    ///     1 - based
    /// </summary>
    public int PageNumber { get; set; } = 1;

    public bool Fuzzy { get; set; }
    
    [DisplayName("Sort")]
    public SortProperty? SortProperty { get; set; }

    [DisplayName("Asc")]
    public bool SortAscending { get; set; }
}

public class SearchModelBase : SearchFormData
{
    public SearchModelBase()
    {
    }

    /// <summary>
    ///     Copy constructor
    /// </summary>
    /// <param name="other">
    ///     Mandatory
    /// </param>
    public SearchModelBase(SearchModelBase other)
        : base(other)
    {
        SortOptions = other.SortOptions;
        TotalCountedPageCount = other.TotalCountedPageCount;
        HasMore = other.HasMore;
    }


    /// <summary>
    ///     The number or counted notes is limited (e.g. to 10 pages past the selected page) for performance reasons.
    /// </summary>
    public int? TotalCountedPageCount { get; set; }
    
    public bool HasMore { get; set; }
    
    public List<SelectListItem> SortOptions { get; set; } = new ();
}