using UnityEssentials;
using System.Collections.Generic;
using System.Threading;

namespace UnityEssentials
{
    public static partial class UniTaskAsyncEnumerable
    {
        public static Task<HashSet<TSource>> ToHashSetAsync<TSource>(this IUniTaskAsyncEnumerable<TSource> source, CancellationToken cancellationToken = default)
        {
            Error.ThrowArgumentNullException(source, nameof(source));

            return UnityEssentials.ToHashSet.ToHashSetAsync(source, EqualityComparer<TSource>.Default, cancellationToken);
        }

        public static Task<HashSet<TSource>> ToHashSetAsync<TSource>(this IUniTaskAsyncEnumerable<TSource> source, IEqualityComparer<TSource> comparer, CancellationToken cancellationToken = default)
        {
            Error.ThrowArgumentNullException(source, nameof(source));
            Error.ThrowArgumentNullException(comparer, nameof(comparer));

            return UnityEssentials.ToHashSet.ToHashSetAsync(source, comparer, cancellationToken);
        }
    }

    internal static class ToHashSet
    {
        internal static async Task<HashSet<TSource>> ToHashSetAsync<TSource>(IUniTaskAsyncEnumerable<TSource> source, IEqualityComparer<TSource> comparer, CancellationToken cancellationToken)
        {
            var set = new HashSet<TSource>(comparer);

            var e = source.GetAsyncEnumerator(cancellationToken);
            try
            {
                while (await e.MoveNextAsync())
                {
                    set.Add(e.Current);
                }
            }
            finally
            {
                if (e != null)
                {
                    await e.DisposeAsync();
                }
            }

            return set;
        }
    }
}