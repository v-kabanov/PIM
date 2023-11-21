using System.ComponentModel;

namespace PimWeb.Models;

public class FileViewModel : SearchableEntityViewModelBase
{
    public string Title { get; set; }
    
    public string RelativePath { get; set; }
    
    public string FullPath { get; set; }
    
    public string Description { get; set; }
    
    public string MimeType { get; set; }
    
    public string ExtractedText { get; set; }
    
    public byte[] ContentHash { get; set; }
}