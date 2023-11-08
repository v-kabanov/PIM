using System;

namespace PimWeb.Models;

public class NoteSearchResult
{
    public int Id { get; set; }
    
    public DateTime CreateTime { get; set; }

    public DateTime LastUpdateTime { get; set; }

    public string Text { get; set; }
    
    public string Headline { get; set; }

    public float Rank { get; set; }
}