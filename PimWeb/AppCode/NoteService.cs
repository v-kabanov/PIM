
// DO NOT REFERENCE!
//using System.Data.Entity;

using System.Data.Entity.Core;
using Microsoft.EntityFrameworkCore;
using Pim.CommonLib;
using PimWeb.Models;

namespace PimWeb.AppCode;

public class NoteService : INoteService
{
    public const int DefaultPageSize = 20;
    
    public DatabaseContext DataContext { get; }

    public int PageSize => DefaultPageSize;
    
    public NoteService(DatabaseContext dataContext)
    {
        DataContext = dataContext ?? throw new ArgumentNullException(nameof(dataContext));
    }

    public async Task<SearchViewModel> SearchAsync(SearchViewModel model, bool withDelete)
    {
        var query = CreateQuery(model.PeriodStart, model.PeriodEnd, SearchableDocumentTime.LastUpdate);

        if (!model.Query.IsNullOrWhiteSpace())
            query = query.Where(x => x.SearchVector.Matches(EF.Functions.WebSearchToTsQuery("public.mysearch", model.Query)));
        
        var maxNotesToCount = (model.PageNumber + 10) * PageSize;
        
        // no futures in EF core; reluctant to use Z.EF
        var totalCount = await query.Take(maxNotesToCount).CountAsync().ConfigureAwait(false);
        
        var notes = await query.Sort(SearchableDocumentTime.LastUpdate)
            .ApplyPage(PageSize, model.PageNumber - 1, withDelete)
            .ToListAsync();
        
        if (withDelete)
            notes.RemoveAll(x => x.Id == model.NoteId);
        
        if (notes.Count > PageSize)
            notes.RemoveAt(notes.Count - 1);

        var pageCount = (int)Math.Ceiling((double)totalCount / PageSize);
        var result = new SearchViewModel(model)
        {
            SearchResultPage = notes,
            TotalCountedPageCount = pageCount,
            HasMore = pageCount > (model.PageNumber + 1),
        };
        
        return result;
    }
    
    public async Task<HomeViewModel> GetLatestAsync()
    {
        var notes = await DataContext.Notes
            .OrderByDescending(x => x.LastUpdateTime)
            .Take(PageSize)
            .ToListAsync()
            .ConfigureAwait(false);
        
        return new HomeViewModel { LastUpdatedNotes = notes };
    }

    /// <inheritdoc />
    public async Task<NoteViewModel> GetAsync(int id)
    {
        var note = await DataContext.Notes.FindAsync(id).ConfigureAwait(false);
        NoteViewModel result;
        if (note != null)
        {
            result = new NoteViewModel
            {
                NoteId = id,
                CreateTime = note.CreateTime,
                LastUpdateTime = note.LastUpdateTime,
                NoteText = note.Text,
                Caption = note.Name,
                Version = note.IntegrityVersion,
            };
        }
        else
            result = new NoteViewModel();
        
        return result;
    }

    /// <inheritdoc />
    public async Task<NoteViewModel> SaveOrUpdateAsync(NoteViewModel model)
    {
        var note = await DataContext.FindAsync<Note>(model.NoteId).ConfigureAwait(false);
        
        var newText = model.NoteText?.Trim();
        
        if (note != null)
        {
            if (note.IntegrityVersion != model.Version)
                throw new OptimisticConcurrencyException($"Version mismatch: updating {model.Version ?? 0} while server has {note.IntegrityVersion}.");

            if (newText != note.Text)
            {
                note.Text = model.NoteText;
                note.LastUpdateTime = DateTime.Now;
                note.IntegrityVersion = (model.Version ?? 0) + 1;
            }
        }
        else
        {
            note = new Note { Text = model.NoteText };
            await DataContext.Notes.AddAsync(note).ConfigureAwait(false);
        }
        
        await DataContext.SaveChangesAsync().ConfigureAwait(false);
        
        return new NoteViewModel
        {
            NoteId = note.Id,
            NoteText = note.Text,
            Caption = note.Name,
            Version = note.IntegrityVersion,
            CreateTime = note.CreateTime,
            LastUpdateTime = note.LastUpdateTime,
        };
    }


    /// <inheritdoc />
    public async Task<Note> DeleteAsync(NoteViewModel model, bool skipConcurrencyCheck)
    {
        var note = await DataContext.Notes.FindAsync(model.NoteId).ConfigureAwait(false);
        
        if (note != null)
        {
            if (!skipConcurrencyCheck && note.IntegrityVersion != model.Version)
                throw new OptimisticConcurrencyException($"Note was updated on the server: deleting {model.Version ?? 0} while server has {note.IntegrityVersion}. Review changes before deleting.");

            DataContext.Remove(note);
            await DataContext.SaveChangesAsync().ConfigureAwait(false);
        }
        
        return note;
    }

    /// <inheritdoc />
    public async Task<HomeViewModel> CreateAsync(HomeViewModel model)
    {
        var created = !model.NewNoteText.IsNullOrWhiteSpace();
        if (created)
        {
            var newNote = new Note { Text = model.NewNoteText };
            await DataContext.AddAsync(newNote).ConfigureAwait(false);
            await DataContext.SaveChangesAsync().ConfigureAwait(false);
        }
        
        var result = await GetLatestAsync().ConfigureAwait(false);
        
        if (!created)
            result.NewNoteText = model.NewNoteText;
        
        return result;
    }

    private IQueryable<Note> CreateQuery(DateTime? start, DateTime? end, SearchableDocumentTime documentTime)
    {
        var query = DataContext.Notes.AsQueryable();
        if (documentTime == SearchableDocumentTime.Creation)
        {
            if (start.HasValue)
                query = query.Where(x => x.CreateTime >= start);
            if (end.HasValue)
                query = query.Where(x => x.CreateTime < end);

            query = query.OrderByDescending(x => x.CreateTime);
        }
        else
        {
            if (start.HasValue)
                query = query.Where(x => x.LastUpdateTime >= start);
            if (end.HasValue)
                query = query.Where(x => x.LastUpdateTime < end);
            
            query = query.OrderByDescending(x => x.LastUpdateTime);
        }
        
        return query;
    }
}