using System.ComponentModel.DataAnnotations;

namespace PimWeb.Models;

public class FileViewModel : SearchableEntityViewModelBase
{
    [Required]
    public string Title { get; set; }
    
    public string RelativePath { get; set; }
    
    public string FullPath { get; set; }
    
    public string Description { get; set; }
    
    public string MimeType { get; set; }
    
    public string ExtractedText { get; set; }
    
    public bool ContentHashMismatch { get; set; }
    
    public bool ExistsOnDisk { get; set; }
    
    public List<NoteViewModel> Notes { get; set; } = new ();
}