using System.Data.Entity;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Pim.CommonLib;

namespace PimWeb.AppCode;


/// <summary>
///     Identifies searchable time property
/// </summary>
public enum SearchableDocumentTime
{
    Creation,
    LastUpdate
}

public interface INoteService
{
    Task<List<Note>> GetTopInPeriodAsync(DateTime? start, DateTime? end, int pageSize, int pageNumber, SearchableDocumentTime documentTime, out bool moreExist);
    
    Task<List<Note>> SearchInPeriodAsync(DateTime? start, DateTime? end, string searchText, int pageSize, int pageNumber, SearchableDocumentTime documentTime, out int totalCount);
    
    Task<Note> GetAsync(int id);
    
    Task<Note> SaveOrUpdateAsync(Note note);
    
    Task<Note> DeleteAsync(int id);
}

public class NoteService : INoteService
{
    public DatabaseContext DataContext { get; }

    public NoteService([NotNull] DatabaseContext dataContext)
    {
        DataContext = dataContext ?? throw new ArgumentNullException(nameof(dataContext));
    }

    /// <inheritdoc />
    public async Task<List<Note>> GetTopInPeriodAsync(DateTime? start, DateTime? end, int pageSize, int pageNumber, SearchableDocumentTime documentTime, out bool moreExist)
    {
        var query = CreateQuery(start, end, documentTime);
        
        var skip = pageSize * pageNumber;
        
        var result = await query.Skip(skip).Take(pageSize + 1).ToListAsync();
        moreExist = result.Count > pageSize;
        if (moreExist)
            result.RemoveAt(result.Count - 1);
        
        return result;
    }

    /// <inheritdoc />
    public async Task<List<Note>> SearchInPeriodAsync(DateTime? start, DateTime? end, string searchText, int pageSize, int pageNumber, SearchableDocumentTime documentTime, out int totalCount)
    {
        var query = CreateQuery(start, end, documentTime);

        if (!searchText.IsNullOrWhiteSpace())
            query = query.Where(x => x.SearchVector.Matches(EF.Functions.WebSearchToTsQuery(searchText)));
        
        // no futures in EF core; reluctant to use Z.EF
        totalCount = await query.CountAsync().ConfigureAwait(false);
        
        var result = await ApplyPage(Sort(query, documentTime), pageSize, pageNumber)
            .ToListAsync();
        
        return result;
    }

    /// <inheritdoc />
    public Task<Note> GetAsync(int id)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public Task<Note> SaveOrUpdateAsync(Note note)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public Task<Note> DeleteAsync(int id)
    {
        throw new NotImplementedException();
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
    
    private static IQueryable<Note> ApplyPage(IQueryable<Note> query, int pageSize, int pageNumber)
    {
        var result = query;
        if (pageNumber > 0)
            result = result.Skip(pageSize * pageNumber);
        
        return result.Take(pageSize);
    }
    
    private static IQueryable<Note> Sort(IQueryable<Note> query, SearchableDocumentTime documentTime)
    {
        if (documentTime == SearchableDocumentTime.Creation)
            return query.OrderByDescending(x => x.CreateTime);
        
        return query.OrderByDescending(x => x.LastUpdateTime);
    }
}