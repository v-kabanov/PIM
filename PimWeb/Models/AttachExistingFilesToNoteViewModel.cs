namespace PimWeb.Models;

public class AttachExistingFilesToNoteViewModel
{
    public NoteViewModel Note { get; set; } = new ();
    
    public List<int> SelectedFiles { get; set; } = new ();
    
    public FileSearchViewModel FileSearchViewModel { get; set; } = new ();
    
    public List<FileViewModel> AttachedFiles = new ();
}