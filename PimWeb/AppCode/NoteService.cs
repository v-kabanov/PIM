using JetBrains.Annotations;

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
    List<Note> GetTopInPeriod(DateTime? start, DateTime? end, int pageSize, int pageNumber, SearchableDocumentTime documentTime, out bool moreExist);
    
    List<Note> SearchInPeriod(DateTime? start, DateTime? end, string searchText, int pageSize, int pageNumber, SearchableDocumentTime documentTime, out int totalCount);
    
    Note Get(int id);
    
    Note SaveOrUpdate(Note note);
    
    Note Delete(int id);
}

public class NoteService : INoteService
{
    public DatabaseContext DataContext { get; }

    public NoteService([NotNull] DatabaseContext dataContext)
    {
        DataContext = dataContext ?? throw new ArgumentNullException(nameof(dataContext));
    }

    /// <inheritdoc />
    public List<Note> GetTopInPeriod(DateTime? start, DateTime? end, int pageSize, int pageNumber, SearchableDocumentTime documentTime, out bool moreExist)
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

    /// <inheritdoc />
    public List<Note> SearchInPeriod(DateTime? start, DateTime? end, string searchText, int pageSize, int pageNumber, SearchableDocumentTime documentTime, out int totalCount)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public Note Get(int id)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public Note SaveOrUpdate(Note note)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public Note Delete(int id)
    {
        throw new NotImplementedException();
    }
}