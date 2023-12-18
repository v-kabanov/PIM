
public class AppOptions
{
    public string FulltextConfig { get; set; }
    
    /// <summary>
    ///     Virtual root path under which the application is running; takes precedence over 'APP_VIRTUAL_PATH' env. variable.
    /// </summary>
    public string WebAppPath { get; set; }
    
    public string FileStoragePath { get; set; }
    
    public List<string> TextFileExtensions { get; set; }
    
    public string DataProtectionKeyStoragePath { get; set; }
    
    public bool RemoveDataProtection { get; set; }
}

public record SeedUser
{
    public string Name { get; init; }
    
    public string Email { get; init; }
    
    public string Password { get; init; }
    
    public List<string> Roles { get; init; } = new ();
}

public record SeedUsers
{
    public List<SeedUser> Users { get; init; } = new ();
}