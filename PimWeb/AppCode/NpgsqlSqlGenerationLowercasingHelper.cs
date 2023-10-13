using System.Text;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal;

namespace PimWeb.AppCode;

/// <summary>
///     A replacement for <see cref="NpgsqlSqlGenerationHelper"/> to convert PascalCaseCsharpyIdentifiers to snake_case_names.
///     So table and column names with no embedded punctuation get generated with no quotes or delimiters.
/// </summary>
public class NpgsqlSqlGenerationLowercasingHelper : NpgsqlSqlGenerationHelper
{
    private static readonly Regex SplitPascalCaseRegex = new (@"([\s]|(?<=[a-z])(?=[A-Z]|[0-9])|(?<=[A-Z])(?=[A-Z][a-z]|[0-9])|(?<=[0-9])(?=[^0-9]))");
    
    private static bool ShouldLowercase(string name) => name != "__EFMigrationsHistory" && !name.StartsWith("AspNet");
    
    private static string Customize(string name)
        => !ShouldLowercase(name)
            ? name
            : SplitPascalCaseRegex.Replace(name, "_");
    
    public NpgsqlSqlGenerationLowercasingHelper(RelationalSqlGenerationHelperDependencies dependencies) 
        : base(dependencies)
    { }
    
    public override string DelimitIdentifier(string identifier) => base.DelimitIdentifier(Customize(identifier));
    
    public override void DelimitIdentifier(StringBuilder builder, string identifier) => base.DelimitIdentifier(builder, Customize(identifier));
}