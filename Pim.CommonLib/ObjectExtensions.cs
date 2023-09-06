// /**********************************************************************************************
// Author:  Vasily Kabanov
// Created  2017-10-23
// Comment  
// **********************************************************************************************/

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace Pim.CommonLib;

public static class ObjectExtensions
{
    public static bool In<T>(this T value, params T[] values)
    {
        return values.Contains(value);
    }

    public static bool In<T>(this T value, IEnumerable<T> values)
    {
        return values.Contains(value);
    }

    public static bool NotIn<T>(this T value, params T[] values)
    {
        return !values.Contains(value);
    }

    public static bool NotIn<T>(this T value, IEnumerable<T> values)
    {
        return !values.Contains(value);
    }

    public static List<T> WrapInList<T>(this T value)
    {
        return Enumerable.Repeat(value, 1).ToList();
    }

    public static HashSet<T> WrapInSet<T>(this T value, IEqualityComparer<T> comparer = null)
    {
        return Enumerable.Repeat(value, 1).ToHashSet(comparer);
    }
    
    public static bool IsNullOrEmpty(this string value) => string.IsNullOrEmpty(value);
    
    public static bool IsNullOrWhiteSpace(this string value) => string.IsNullOrWhiteSpace(value);

    [SourceTemplate]
    public static void CheckArgNotNull(this object value)
    {
        Check.DoRequireArgumentNotNull(value, nameof(value));
    }

    [SourceTemplate]
    public static void CheckArgNotBlank(this string value)
    {
        Check.DoRequireArgumentNotBlank(value, nameof(value));
    }
}