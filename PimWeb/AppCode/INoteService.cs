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
    Task<NoteSearchViewModel> SearchAsync(NoteSearchViewModel model, bool withDelete);
    
    Task<FileSearchViewModel> SearchAsync(FileSearchViewModel model, bool withDelete);
    
    Task<NoteViewModel> GetNoteAsync(int id);
    
    //Task<AttachExistingFilesToNoteViewModel> GetAttachExistingFileToNoteAsync(int noteId);

    Task<AttachExistingFilesToNoteViewModel> ProcessAsync(AttachExistingFilesToNoteViewModel model, bool commit);
    
    Task<FileViewModel> GetFileAsync(int id);
    
    Task<NoteViewModel> SaveOrUpdateAsync(NoteViewModel model);
    
    /// <summary>
    ///     Does not delete associated files.
    /// </summary>
    /// <param name="model">
    ///     Mandatory
    /// </param>
    /// <param name="skipConcurrencyCheck">
    ///     Force delete even note has been updaed since note details were presented to the user.
    /// </param>
    /// <returns>
    ///     Deleted note.
    /// </returns>
    Task<Note> DeleteAsync(NoteViewModel model, bool skipConcurrencyCheck);
    
    /// <summary>
    ///     Delete from disk and database, remove all links from Notes if any.
    /// </summary>
    Task<FileViewModel> DeleteAsync(FileViewModel model);
    
    Task<HomeViewModel> GetLatestNotesAsync();
    
    Task<HomeViewModel> CreateNoteAsync(HomeViewModel model);
    
    Task<FileUploadResultViewModel> UploadFileForNoteAsync(int noteId, string fileName, byte[] content);
    
    /// <summary>
    ///     Remove association between a note and a file without removing the file.
    /// </summary>
    Task<NoteFileUnlinkResultViewModel> UnlinkFileFromNote(int noteId, int fileId);
    
    /// <summary>
    ///     Remove association between a file and a note without removing the file.
    /// </summary>
    Task<FileNoteUnlinkResultViewModel> UnlinkNoteFromFile(int fileId, int noteId);
    
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
    Task<(File File, bool Duplicate)> SaveFileAsync(string fileName, byte[] content);
    
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