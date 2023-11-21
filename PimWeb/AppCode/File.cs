namespace PimWeb.AppCode;

public class File : EntityBase
{
    public File()
    {
        CreateTime = LastUpdateTime = DateTime.UtcNow;
    }

    public virtual int Id { get; set; }
    
    public virtual string RelativePath { get; set; }
    
    public virtual string Title { get; set; }
    
    public virtual string Description { get; set; }
    
    public virtual byte[] ContentHash { get; set; }
    
    /// <summary>
    ///     Text extracted from file if any
    /// </summary>
    public virtual string ExtractedText { get; set; }

    /// <summary>
    ///     UTC
    /// </summary>
    public virtual DateTime CreateTime { get; set; }

    /// <summary>
    ///     UTC
    /// </summary>
    public virtual DateTime LastUpdateTime { get; set; }
    
    public virtual ISet<Note> Notes { get; set; } = new HashSet<Note>();
    
    //public virtual NpgsqlTsVector SearchVector { get; set; }
}