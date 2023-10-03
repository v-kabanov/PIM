// /**********************************************************************************************
// Author:  Vasily Kabanov
// Created  2017-06-13
// Comment  
// **********************************************************************************************/

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using log4net;
using PimWeb.AppCode;

namespace PimWeb.Models;

public class SearchViewModel
{
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    public const int MaxPageCount = 10;
    public const int DefaultResultsPerPage = 10;

    private Note _deletedNote;

    public INoteService NoteService { get; private set; }

    public void Initialize(INoteService noteService)
    {
        NoteService = noteService;
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

    public int TotalPageCount { get; private set; }

    public IList<Note> SearchResultPage { get; private set; }

    public void Delete()
    {
        if (NoteId > 0)
            _deletedNote = NoteService.Delete(NoteId);
    }

    public void ExecuteSearch()
    {
        if (PageNumber < 1)
            PageNumber = 1;
        else if (PageNumber > MaxPageCount)
            PageNumber = MaxPageCount;

        var maxResults = MaxPageCount * DefaultResultsPerPage;

        var headers = NoteService.SearchInPeriod(
            // ReSharper disable once RedundantArgumentDefaultValue
            PeriodStart, PeriodEnd, Query, maxResults + 1, PageNumber, SearchableDocumentTime.LastUpdate, out var totalCount);

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
            .Select(h => NoteService.Get(h.Id))
            .Where(x => x != null)
            .ToList();

        if (headersPage.Count > SearchResultPage.Count)
            Log.Warn("Fulltext index out of sync: rebuild it");
    }
}