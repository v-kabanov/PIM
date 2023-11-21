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
    
    Task<FileSearchViewModel> SearchAsync(FileSearchViewModel model, bool withDelete);
    
    Task<NoteViewModel> GetAsync(int id);
    
    Task<NoteViewModel> SaveOrUpdateAsync(NoteViewModel model);
    
    Task<Note> DeleteAsync(NoteViewModel model, bool skipConcurrencyCheck);
    
    Task<HomeViewModel> GetLatestAsync();
    
    Task<HomeViewModel> CreateAsync(HomeViewModel model);
    
    /// <summary>
    ///     Save new uploaded file in the repository or return reference to first existing one if duplicate by content.
    /// </summary>
    /// <param name="fileName">
    ///     File name at the source
    /// </param>
    /// <param name="content">
    ///     Mandatory blob
    /// </param>
    /// <returns>
    ///     First identical existing file if already exists. Otherwise new persistent entity.
    /// </returns>
    Task<File> SaveFile(string fileName, byte[] content);
    
    /// <summary>
    ///     Update title or description of an existing file. Content is read only.
    /// </summary>
    /// <param name="model">
    ///     Mandatory
    /// </param>
    /// <returns>
    ///     Updated file model.
    /// </returns>
    /// <exception cref="ArgumentException">
    ///     File with the specified id does not exist.
    /// </exception>
    Task<FileViewModel> UpdateAsync(FileViewModel model);
}