namespace PimWeb.AppCode;

public static class IdentityConstants
{
    public const string AdminRoleName = "Admin";
    public const string AdminUserName = "admin";
    public const string DefaultAdminPassword = "password";
    public const string ReaderRoleName = "Reader";
    public const string WriterRoleName = "Writer";
    
    public static readonly string[] AllRoleNames = {AdminRoleName, ReaderRoleName, WriterRoleName}; 
}