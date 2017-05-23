using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AuNoteLib.Util
{
    public static class EnumerableExtensions
    {
        // Calculate the stdev value for a collection of type Double.
        public static double? StDev(this IEnumerable<double> stDevAggregate)
        {
            return StatisticalFunctions.StDev(stDevAggregate.ToArray());
        }

        // Project the collection of generic items as type Double and calculate the stdev value.
        public static double? StDev<T>(this IEnumerable<T> stDevAggregate, Func<T, double> selector)
        {
            var values = (from element in stDevAggregate select selector(element)).ToArray();

            return StatisticalFunctions.StDev(values);
        }


        //// Calculate the stdevp value for a collection of type Double.
        public static double? StDevP(this IEnumerable<double> stDevAggregate)
        {
            return StatisticalFunctions.StDevP(stDevAggregate.ToArray());
        }


        // Project the collection of generic items as type Double and calculate the stdevp value.
        public static double? StDevP<T>(this IEnumerable<T> stDevAggregate, Func<T, double> selector)
        {
            var values = (from element in stDevAggregate select selector(element)).ToArray();
            return StatisticalFunctions.StDevP(values);
        }

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
        public static ISet<string> ToCaseInsensitiveSet(this IEnumerable<string> source)
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
        public static ISet<T> ToSortedSet<T>(this IEnumerable<T> source, IComparer<T> comparer = null)
        {
            Check.DoRequireArgumentNotNull(source, nameof(source));

            // sorted set actually accepts null for default comparer
            // ReSharper disable once AssignNullToNotNullAttribute
            return new SortedSet<T>(source, comparer);
        }

        /// <summary>
        ///     Create dictionary with case insensitive string keys.
        /// </summary>
        public static IDictionary<string, T> ToCaseInsensitiveDictionary<T>(this IEnumerable<T> source, Func<T, string> keyGetter)
        {
            Check.DoRequireArgumentNotNull(source, nameof(source));

            return source?.ToDictionary(keyGetter, StringComparer.InvariantCultureIgnoreCase);
        }
    }
}