using System;
using System.Collections.Generic;
using System.Linq;

namespace AssetInventory
{
    public static class ListUtils
    {
        public static void Populate<T>(this T[] arr, T value)
        {
            for (int i = 0; i < arr.Length; i++)
            {
                arr[i] = value;
            }
        }

        public static IOrderedEnumerable<TSource> SortBy<TSource, TKey>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            bool ascending,
            IComparer<TKey> comparer = null)
        {
            return ascending
                ? source.OrderBy(keySelector, comparer)
                : source.OrderByDescending(keySelector, comparer);
        }

        public static IOrderedEnumerable<TSource> ThenSortBy<TSource, TKey>(
            this IOrderedEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            bool ascending,
            IComparer<TKey> comparer = null)
        {
            return ascending
                ? source.ThenBy(keySelector, comparer)
                : source.ThenByDescending(keySelector, comparer);
        }
    }
}