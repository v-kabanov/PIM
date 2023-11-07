
// DO NOT REFERENCE!
//using System.Data.Entity;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using NHibernate;
using NHibernate.Linq;
using NHibernate.Transform;
using Pim.CommonLib;
using PimWeb.AppCode.Map;
using PimWeb.Models;
using ISession = NHibernate.ISession;

namespace PimWeb.AppCode;

public record SortPropertyOption (SortProperty SortProperty, SelectListItem SelectListItem);

public class NoteService : INoteService
{
    public const int DefaultPageSize = 20;
    
    public ISession Session { get; }

    public int PageSize => DefaultPageSize;
    
    public AppOptions AppOptions { get;  }
    
    private IQueryable<Note> Query => Session.Query<Note>();
    
    private static readonly List<SortPropertyOption> AllSortOptions;

    static NoteService()
    {
        AllSortOptions = Enum.GetValues<SortProperty>()
            .Select(x => (Value: x, Text: $"{x}"))
            .Select(x => new SortPropertyOption(x.Value, new SelectListItem(Util.InsertDelimitersIntoPascalCaseString(x.Text), x.Text)))
            .ToList();
    }
    
    public NoteService(ISession session, AppOptions appOptions)
    {
        Session = session ?? throw new ArgumentNullException(nameof(session));
        AppOptions = appOptions ?? throw new ArgumentNullException(nameof(appOptions));
    }

    public async Task<SearchViewModel> SearchAsync(SearchViewModel model, bool withDelete)
    {
        IQuery query = null;
        
        if (withDelete && model.NoteId > 0)
            await Query.Where(x => x.Id == model.NoteId).DeleteAsync().ConfigureAwait(false);
        
        var sortProperty = model.SortProperty ?? SortProperty.LastUpdateTime;
        
        var sortAscending = model.SortAscending;
        
        var whereClauseBuilder = new SqlWhereClauseBuilder()
            .AddOptionalRangeParameters(NoteMap.ColumnNameLastUpdateTime, model.LastUpdatePeriodStart, model.LastUpdatePeriodEnd)
            .AddOptionalRangeParameters(NoteMap.ColumnNameCreationTime, model.CreationPeriodStart, model.CreationPeriodEnd);
        
        var queryText = new StringBuilder();
        
        var useSqlFunction = !model.Query.IsNullOrWhiteSpace();
        
        if (useSqlFunction)
            queryText.AppendLine("select id as \"Id\", text as \"Text\", last_update_time as \"LastUpdateTime\", create_time as \"CreateTime\", headline as \"Headline\", rank as \"Rank\" from public.search(:query, :fuzzy)");
        else
        {
            queryText.AppendLine("select id as \"Id\", text as \"Text\", last_update_time as \"LastUpdateTime\", create_time as \"CreateTime\", cast(null as text) as \"Headline\", cast(null as real) as \"Rank\" from public.note");
            if (sortProperty == SortProperty.SearchRank)
                sortProperty = SortProperty.LastUpdateTime;
        }

        if (!whereClauseBuilder.IsEmpty)
            queryText.Append("where     ").AppendLine(whereClauseBuilder.GetCombinedClause());
        
        var maxNotesToCount = (model.PageNumber + 10) * PageSize;
        
        var totalCountQuery = Session.CreateSQLQuery($"select count(*) from ({queryText} limit :maxNotesToCount) s")
            .SetInt32("maxNotesToCount", maxNotesToCount);
        if (useSqlFunction)
            totalCountQuery
                .SetString("query", model.Query)
                .SetBoolean("fuzzy", model.Fuzzy);
        whereClauseBuilder.SetParameterValues(totalCountQuery);


        // the function sorts by rank, reorder only if different
        if (!useSqlFunction || !(sortProperty == SortProperty.SearchRank && !sortAscending))
        {
            var columnName = sortProperty switch
            {
                SortProperty.CreationTime => NoteMap.ColumnNameCreationTime
                , SortProperty.LastUpdateTime => NoteMap.ColumnNameLastUpdateTime
                , SortProperty.SearchRank => nameof(NoteSearchResult.Rank)
                , _ => throw new PostconditionException($"Unexpected sort property '{sortProperty}'.")
            };
            if (!columnName.IsNullOrEmpty())
            {
                var ascendance = sortAscending ? "asc" : "desc";
                queryText.Append("order by ").Append(columnName).Append(' ').Append(ascendance).Append(", id ").AppendLine(ascendance);
            }
        }
        
        var noteCountFuture = totalCountQuery.FutureValue<long>();
        
        query = Session.CreateSQLQuery(queryText.ToString())
            .ApplyPage(PageSize, model.PageNumber - 1);
        whereClauseBuilder.SetParameterValues(query);
        
        if (useSqlFunction)
            query
                .SetString("query", model.Query)
                .SetBoolean("fuzzy", model.Fuzzy);
        
        query.SetResultTransformer(Transformers.AliasToBean(typeof(NoteSearchResult)));
        
        var notes = (await query.Future<NoteSearchResult>().GetEnumerableAsync().ConfigureAwait(false))
            .ToList();

        var totalCount = noteCountFuture.Value;
        
        if (notes.Count > PageSize)
            notes.RemoveAt(notes.Count - 1);

        var pageCount = (int)Math.Ceiling((double)totalCount / PageSize);
        var result = new SearchViewModel(model)
        {
            SearchResultPage = notes.Select(CreateModel).ToList(),
            TotalCountedPageCount = pageCount,
            HasMore = pageCount > (model.PageNumber + 1),
            SortProperty = sortProperty,
            SortAscending = sortAscending,
            SortOptions = AllSortOptions.Select(x => new SelectListItem(x.SelectListItem.Text, x.SelectListItem.Value, x.SortProperty == sortProperty))
                .ToList(),
            PageNumber = model.PageNumber
        };
        
        return result;
    }
    
    public async Task<HomeViewModel> GetLatestAsync()
    {
        var notes = await Query
            .OrderByDescending(x => x.LastUpdateTime)
            .Take(PageSize)
            .ToListAsync()
            .ConfigureAwait(false);
        
        return new HomeViewModel { LastUpdatedNotes = notes.Select(CreateModel).ToList() };
    }

    /// <inheritdoc />
    public async Task<NoteViewModel> GetAsync(int id)
    {
        var note = await Session.GetAsync<Note>(id).ConfigureAwait(false);
        NoteViewModel result;
        if (note != null)
            result = CreateModel(note);
        else
            result = new NoteViewModel();
        
        return result;
    }

    /// <inheritdoc />
    public async Task<NoteViewModel> SaveOrUpdateAsync(NoteViewModel model)
    {
        var note = await Session.GetAsync<Note>(model.NoteId).ConfigureAwait(false);
        
        var newText = model.NoteText?.Trim();
        
        if (note != null)
        {
            if (note.IntegrityVersion != model.Version)
                throw new OptimisticConcurrencyException($"Version mismatch: updating {model.Version ?? 0} while server has {note.IntegrityVersion}.");

            if (newText != note.Text)
            {
                note.Text = model.NoteText;
                note.LastUpdateTime = DateTime.UtcNow;
                //note.IntegrityVersion = (model.Version ?? 0) + 1;
            }
        }
        else
        {
            note = new Note { Text = model.NoteText };
            await Session.SaveAsync(note).ConfigureAwait(false);
        }
        
        await Session.GetCurrentTransaction().CommitAsync().ConfigureAwait(false);
        
        return CreateModel(note);
    }


    /// <inheritdoc />
    public async Task<Note> DeleteAsync(NoteViewModel model, bool skipConcurrencyCheck)
    {
        var note = await Session.GetAsync<Note>(model.NoteId).ConfigureAwait(false);
        
        if (note != null)
        {
            if (!skipConcurrencyCheck && note.IntegrityVersion != model.Version)
                throw new OptimisticConcurrencyException($"Note was updated on the server: deleting {model.Version ?? 0} while server has {note.IntegrityVersion}. Review changes before deleting.");

            await Session.DeleteAsync(note).ConfigureAwait(false);
            await Session.GetCurrentTransaction().CommitAsync().ConfigureAwait(false);
        }
        
        return note;
    }

    /// <inheritdoc />
    public async Task<HomeViewModel> CreateAsync(HomeViewModel model)
    {
        var created = !model.NewNoteText.IsNullOrWhiteSpace();
        if (created)
        {
            var newNote = Note.Create(model.NewNoteText);
            await Session.SaveAsync(newNote).ConfigureAwait(false);
            await Session.GetCurrentTransaction().CommitAsync().ConfigureAwait(false);
        }
        
        var result = await GetLatestAsync().ConfigureAwait(false);
        
        if (!created)
            result.NewNoteText = model.NewNoteText;
        
        return result;
    }
    
    private NoteViewModel CreateModel(Note note) => new ()
        {
            NoteId = note.Id,
            NoteText = note.Text,
            Caption = note.Name,
            Version = note.IntegrityVersion,
            CreateTime = note.CreateTime.ToLocalTime(),
            LastUpdateTime = note.LastUpdateTime.ToLocalTime(),
        };

    private NoteViewModel CreateModel(NoteSearchResult note) => new ()
    {
        NoteId = note.Id,
        NoteText = note.Text,
        Caption = Note.ExtractName(note.Text),
        SearchHeadline = note.Headline,
        Rank = note.Rank,
        CreateTime = note.CreateTime.ToLocalTime(),
        LastUpdateTime = note.LastUpdateTime.ToLocalTime(),
    };
}