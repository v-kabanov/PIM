using System.Threading.Tasks;
using PimWeb.Models;

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
    Task<SearchViewModel> SearchAsync(SearchViewModel model, bool withDelete);
    
    Task<NoteViewModel> GetAsync(int id);
    
    Task<NoteViewModel> SaveOrUpdateAsync(NoteViewModel model);
    
    Task<Note> DeleteAsync(NoteViewModel model, bool skipConcurrencyCheck);
    
    Task<HomeViewModel> GetLatestAsync();
    
    Task<HomeViewModel> CreateAsync(HomeViewModel model); 
}