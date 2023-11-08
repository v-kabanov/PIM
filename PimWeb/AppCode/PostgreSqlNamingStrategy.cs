using System.Text.RegularExpressions;
using NHibernate.Cfg;

namespace PimWeb.AppCode;

public class PostgreSqlNamingStrategy : INamingStrategy
{
    private static readonly Regex SplitPascalCaseRegex = new (@"([\s]|(?<=[a-z])(?=[A-Z]|[0-9])|(?<=[A-Z])(?=[A-Z][a-z]|[0-9])|(?<=[0-9])(?=[^0-9]))");

    public string ClassToTableName(string className) => PascalCaseToSnake(className);

    public string PropertyToColumnName(string propertyName) => PascalCaseToSnake(propertyName);

    public string TableName(string tableName) => PascalCaseToSnake(tableName);

    public string ColumnName(string columnName) => PascalCaseToSnake(columnName);

    public string PropertyToTableName(string className, string propertyName) => PascalCaseToSnake(propertyName);

    public string LogicalColumnName(string columnName, string propertyName) =>
        string.IsNullOrWhiteSpace(columnName) ?
            PascalCaseToSnake(propertyName) :
            PascalCaseToSnake(columnName);

    private static string PascalCaseToSnake(string name) => SplitPascalCaseRegex.Replace(name, "_").ToLowerInvariant();
    
    private static string DoubleQuote(string raw)
    {
        // In some cases the identifier is single-quoted.
        // We simply remove the single quotes:
        raw = raw.Replace("`", "");
        return $"\"{raw}\"";
    }
}