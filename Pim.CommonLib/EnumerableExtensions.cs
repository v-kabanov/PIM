// /**********************************************************************************************
// Author:  Vasily Kabanov
// Created  2017-10-23
// Comment  
// **********************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;

namespace Pim.CommonLib
{
    public static class EnumerableExtensions
    {
        /// <summary>
        ///     Create hash set from collection
        /// </summary>
        /// <typeparam name="T">
        ///     Element type
        /// </typeparam>
        /// <param name="source">
        ///     Mandatory
        /// </param>
        /// <param name="comparer">
        ///     Optional comparer for elements, null for default comparer
        /// </param>
        /// <returns>
        ///     New collection
        /// </returns>
        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> source, IEqualityComparer<T> comparer = null)
        {
            Check.DoRequireArgumentNotNull(source, nameof(source));

            return new HashSet<T>(source, comparer);
        }

        /// <summary>
        ///     Create case insensitive hash set from collection of strings
        /// </summary>
        public static HashSet<string> ToCaseInsensitiveSet(this IEnumerable<string> source)
        {
            return source.ToHashSet(StringComparer.InvariantCultureIgnoreCase);
        }

        /// <summary>
        ///     Create sorted (using <paramref name="comparer"/> or default one) set from collection
        /// </summary>
        /// <typeparam name="T">
        ///     Element type
        /// </typeparam>
        /// <param name="source">
        ///     Mandatory
        /// </param>
        /// <param name="comparer">
        ///     Optional comparer for elements, null for default comparer
        /// </param>
        /// <returns>
        ///     New collection
        /// </returns>
        public static SortedSet<T> ToSortedSet<T>(this IEnumerable<T> source, IComparer<T> comparer = null)
        {
            Check.DoRequireArgumentNotNull(source, nameof(source));

            // sorted set actually accepts null for default comparer
            // ReSharper disable once AssignNullToNotNullAttribute
            return new SortedSet<T>(source, comparer);
        }

        /// <summary>
        ///     Create dictionary with case insensitive string keys.
        /// </summary>
        public static Dictionary<string, T> ToCaseInsensitiveDictionary<T>(this IEnumerable<T> source, Func<T, string> keyGetter)
        {
            Check.DoRequireArgumentNotNull(source, nameof(source));

            return source?.ToDictionary(keyGetter, StringComparer.InvariantCultureIgnoreCase);
        }

        /// <summary>
        ///     Create dictionary with case insensitive string keys.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public static Dictionary<string, V> ToCaseInsensitiveDictionary<T, V>(this IEnumerable<T> source, Func<T, string> keyGetter, Func<T, V> valueGetter)
        {
            Check.DoRequireArgumentNotNull(source, nameof(source));

            return source?.ToDictionary(keyGetter, valueGetter, StringComparer.InvariantCultureIgnoreCase);
        }
    }
}