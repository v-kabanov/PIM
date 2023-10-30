using System.Text;

namespace PimWeb.AppCode;

public record SqlWhereClauseParameter (string ColumnName, string ParameterName, string Comparison, object ParameterValue);

public class SqlWhereClauseBuilder
{
    private readonly List<SqlWhereClauseParameter> _parameters = new ();
    
    private readonly List<SqlWhereClauseParameter> _dateTimeParameters = new ();
    
    private SqlWhereClauseParameter AddParameter(string columnReference, string parameterName, string comparison, object parameterValue)
    {
        var result = new SqlWhereClauseParameter(columnReference, parameterName, comparison, parameterValue);
        _parameters.Add(result);
        return result;
    }

    /// <summary>
    ///     Automatically generates parameters name
    /// </summary>
    public SqlWhereClauseBuilder AddOptionalParameter(string columnReference, string comparison, DateTime? parameterValue, bool convertToUtc = true)
        => AddOptionalParameter(columnReference, $"param{_parameters.Count}", comparison, parameterValue);

    public SqlWhereClauseBuilder AddOptionalRangeParameters(string columnReference, DateTime? periodStart, DateTime? periodEnd, bool convertToUtc = true)
    {
        AddOptionalParameter(columnReference, ">=", periodStart);
        AddOptionalParameter(columnReference, "<", periodEnd);
        return this;
    }
    
    public SqlWhereClauseBuilder AddOptionalParameter(string columnReference, string parameterName, string comparison, DateTime? parameterValue, bool convertToUtc = true)
    {
        if (parameterValue == null)
            return this;
        
        var value = parameterValue.Value;
        if (convertToUtc && value.Kind != DateTimeKind.Utc)
            value = value.ToUniversalTime(); 
        
        var p = AddParameter(columnReference, parameterName, comparison, value);
        
        _dateTimeParameters.Add(p);
        
        return this;
    }
    
    public bool IsEmpty => _parameters.Count == 0;
    
    public string GetCombinedClause()
    {
        if (IsEmpty) return null;
        
        var result = new StringBuilder();
        const string andExpression = " and ";
        
        foreach (var p in _dateTimeParameters)
        {
            result.Append(p.ColumnName).Append(" ").Append(p.Comparison).Append(':').Append(p.ParameterName)
                .Append(" ").Append(andExpression);
        }
        
        result.Length -= andExpression.Length;
        
        return result.ToString();
    }
    
    public void SetParameterValues(NHibernate.IQuery query)
    {
        foreach (var p in _dateTimeParameters)
            query.SetDateTime(p.ParameterName, (DateTime)p.ParameterValue);
    }
}