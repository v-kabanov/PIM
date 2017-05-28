﻿// /**********************************************************************************************
// Author:		Vasily Kabanov
// Created		2013-05-30
// Comment		
// **********************************************************************************************/

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace FulltextStorageLib.Util
{
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

        [SourceTemplate]
        public static void CheckArgNotNull(this object value)
        {
            Check.DoRequireArgumentNotNull(value, nameof(value));
        }

    }
}
