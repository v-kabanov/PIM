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
    /// <inheritdoc />
    public List<Note> GetTopInPeriod(DateTime? start, DateTime? end, int pageSize, int pageNumber, SearchableDocumentTime documentTime, out bool moreExist)
    {
        throw new NotImplementedException();
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