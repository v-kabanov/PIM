using System;
using System.Data;
using System.Data.Common;
using NHibernate;
using NHibernate.Engine;
using NHibernate.SqlTypes;

namespace PimWeb.AppCode;

public class PostgresqlDatetimeOffsetType : NHibernate.UserTypes.IUserType
{
    /// <inheritdoc />
    public bool Equals(object x, object y)
    {
        if ( ReferenceEquals(x,y) ) return true;
        if (x == null || y == null) return false;
        return x.Equals(y);
    }

    /// <inheritdoc />
    public int GetHashCode(object x)
    {
        return x == null ? typeof(DateTimeOffset).GetHashCode() + 473 : x.GetHashCode();
    }

    /// <inheritdoc />
    public object NullSafeGet(DbDataReader rs, string[] names, ISessionImplementor session, object owner)
    {
        var obj = NHibernateUtil.DateTime.NullSafeGet(rs, names[0], session, owner);
            
        if (obj == null)
            return null;
        
        return (DateTimeOffset)((DateTime)obj).ToLocalTime();
    }

    /// <inheritdoc />
    public void NullSafeSet(DbCommand cmd, object value, int index, ISessionImplementor session)
    {
        if (value == null)
            cmd.Parameters[index].Value = DBNull.Value;
        else
            cmd.Parameters[index].Value = ((DateTimeOffset)value).UtcDateTime;
    }

    /// <inheritdoc />
    public object DeepCopy(object value)
    {
        if (value == null)
            return null;
        
        var result = (DateTimeOffset)value;
        return result;
    }

    /// <inheritdoc />
    public object Replace(object original, object target, object owner) => DeepCopy(original);

    /// <inheritdoc />
    public object Assemble(object cached, object owner) => cached;

    /// <inheritdoc />
    public object Disassemble(object value) => value;

    
    private static readonly SqlType[] SqlTypesDefined = { new (DbType.DateTime) };
    /// <inheritdoc />
    public SqlType[] SqlTypes => SqlTypesDefined;

    /// <inheritdoc />
    public Type ReturnedType => typeof(DateTimeOffset);

    /// <inheritdoc />
    public bool IsMutable => false;
}