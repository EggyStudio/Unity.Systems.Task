#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using System;
using System.Collections.Generic;

namespace UnityEssentials.Threading.Tasks
{
    public static class EnumerableAsyncExtensions
    {
        // overload resolver - .Select(async x => { }) : IEnumerable<UniTask<T>>

        public static IEnumerable<Task> Select<T>(this IEnumerable<T> source, Func<T, Task> selector)
        {
            return System.Linq.Enumerable.Select(source, selector);
        }

        public static IEnumerable<Task<TR>> Select<T, TR>(this IEnumerable<T> source, Func<T, Task<TR>> selector)
        {
            return System.Linq.Enumerable.Select(source, selector);
        }

        public static IEnumerable<Task> Select<T>(this IEnumerable<T> source, Func<T, int, Task> selector)
        {
            return System.Linq.Enumerable.Select(source, selector);
        }

        public static IEnumerable<Task<TR>> Select<T, TR>(this IEnumerable<T> source, Func<T, int, Task<TR>> selector)
        {
            return System.Linq.Enumerable.Select(source, selector);
        }
    }
}


