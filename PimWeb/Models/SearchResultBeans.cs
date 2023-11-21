
namespace PimWeb.Models;

public class SearchResultBase
{
    public int Id { get; set; }
    
    public DateTime CreateTime { get; set; }

    public DateTime LastUpdateTime { get; set; }

    public string Headline { get; set; }

    public float Rank { get; set; }
}

public class FileSearchResult : SearchResultBase
{
    public string RelativePath { get; set; }
    
    public virtual string Title { get; set; }
    
    public virtual string Description { get; set; }
}

public class NoteSearchResult : SearchResultBase
{
    public string Text { get; set; }
}