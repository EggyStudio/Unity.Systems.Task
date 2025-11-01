using UnityEssentials;
using System;
using System.Threading;

namespace UnityEssentials
{
    public static partial class UniTaskAsyncEnumerable
    {
        public static Task<long> LongCountAsync<TSource>(this IUniTaskAsyncEnumerable<TSource> source, CancellationToken cancellationToken = default)
        {
            Error.ThrowArgumentNullException(source, nameof(source));

            return LongCount.LongCountAsync(source, cancellationToken);
        }

        public static Task<long> LongCountAsync<TSource>(this IUniTaskAsyncEnumerable<TSource> source, Func<TSource, Boolean> predicate, CancellationToken cancellationToken = default)
        {
            Error.ThrowArgumentNullException(source, nameof(source));
            Error.ThrowArgumentNullException(predicate, nameof(predicate));

            return LongCount.LongCountAsync(source, predicate, cancellationToken);
        }

        public static Task<long> LongCountAwaitAsync<TSource>(this IUniTaskAsyncEnumerable<TSource> source, Func<TSource, Task<Boolean>> predicate, CancellationToken cancellationToken = default)
        {
            Error.ThrowArgumentNullException(source, nameof(source));
            Error.ThrowArgumentNullException(predicate, nameof(predicate));

            return LongCount.LongCountAwaitAsync(source, predicate, cancellationToken);
        }

        public static Task<long> LongCountAwaitWithCancellationAsync<TSource>(this IUniTaskAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, Task<Boolean>> predicate, CancellationToken cancellationToken = default)
        {
            Error.ThrowArgumentNullException(source, nameof(source));
            Error.ThrowArgumentNullException(predicate, nameof(predicate));

            return LongCount.LongCountAwaitWithCancellationAsync(source, predicate, cancellationToken);
        }
    }

    internal static class LongCount
    {
        internal static async Task<long> LongCountAsync<TSource>(IUniTaskAsyncEnumerable<TSource> source, CancellationToken cancellationToken)
        {
            long count = 0;

            var e = source.GetAsyncEnumerator(cancellationToken);
            try
            {
                while (await e.MoveNextAsync())
                {
                    checked { count++; }
                }
            }
            finally
            {
                if (e != null)
                {
                    await e.DisposeAsync();
                }
            }

            return count;
        }

        internal static async Task<long> LongCountAsync<TSource>(IUniTaskAsyncEnumerable<TSource> source, Func<TSource, Boolean> predicate, CancellationToken cancellationToken)
        {
            long count = 0;

            var e = source.GetAsyncEnumerator(cancellationToken);
            try
            {
                while (await e.MoveNextAsync())
                {
                    if (predicate(e.Current))
                    {
                        checked { count++; }
                    }
                }
            }
            finally
            {
                if (e != null)
                {
                    await e.DisposeAsync();
                }
            }

            return count;
        }

        internal static async Task<long> LongCountAwaitAsync<TSource>(IUniTaskAsyncEnumerable<TSource> source, Func<TSource, Task<Boolean>> predicate, CancellationToken cancellationToken)
        {
            long count = 0;

            var e = source.GetAsyncEnumerator(cancellationToken);
            try
            {
                while (await e.MoveNextAsync())
                {
                    if (await predicate(e.Current))
                    {
                        checked { count++; }
                    }
                }
            }
            finally
            {
                if (e != null)
                {
                    await e.DisposeAsync();
                }
            }

            return count;
        }

        internal static async Task<long> LongCountAwaitWithCancellationAsync<TSource>(IUniTaskAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, Task<Boolean>> predicate, CancellationToken cancellationToken)
        {
            long count = 0;

            var e = source.GetAsyncEnumerator(cancellationToken);
            try
            {
                while (await e.MoveNextAsync())
                {
                    if (await predicate(e.Current, cancellationToken))
                    {
                        checked { count++; }
                    }
                }
            }
            finally
            {
                if (e != null)
                {
                    await e.DisposeAsync();
                }
            }

            return count;
        }
    }
}