using UnityEssentials;
using System;
using System.Threading;

namespace UnityEssentials
{
    public static partial class UniTaskAsyncEnumerable
    {
        public static Task<Boolean> AllAsync<TSource>(this IUniTaskAsyncEnumerable<TSource> source, Func<TSource, Boolean> predicate, CancellationToken cancellationToken = default)
        {
            Error.ThrowArgumentNullException(source, nameof(source));
            Error.ThrowArgumentNullException(predicate, nameof(predicate));

            return All.AllAsync(source, predicate, cancellationToken);
        }

        public static Task<Boolean> AllAwaitAsync<TSource>(this IUniTaskAsyncEnumerable<TSource> source, Func<TSource, Task<Boolean>> predicate, CancellationToken cancellationToken = default)
        {
            Error.ThrowArgumentNullException(source, nameof(source));
            Error.ThrowArgumentNullException(predicate, nameof(predicate));

            return All.AllAwaitAsync(source, predicate, cancellationToken);
        }

        public static Task<Boolean> AllAwaitWithCancellationAsync<TSource>(this IUniTaskAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, Task<Boolean>> predicate, CancellationToken cancellationToken = default)
        {
            Error.ThrowArgumentNullException(source, nameof(source));
            Error.ThrowArgumentNullException(predicate, nameof(predicate));

            return All.AllAwaitWithCancellationAsync(source, predicate, cancellationToken);
        }
    }

    internal static class All
    {
        internal static async Task<bool> AllAsync<TSource>(IUniTaskAsyncEnumerable<TSource> source, Func<TSource, Boolean> predicate, CancellationToken cancellationToken)
        {
            var e = source.GetAsyncEnumerator(cancellationToken);
            try
            {
                while (await e.MoveNextAsync())
                {
                    if (!predicate(e.Current))
                    {
                        return false;
                    }
                }

                return true;
            }
            finally
            {
                if (e != null)
                {
                    await e.DisposeAsync();
                }
            }
        }

        internal static async Task<bool> AllAwaitAsync<TSource>(IUniTaskAsyncEnumerable<TSource> source, Func<TSource, Task<Boolean>> predicate, CancellationToken cancellationToken)
        {
            var e = source.GetAsyncEnumerator(cancellationToken);
            try
            {
                while (await e.MoveNextAsync())
                {
                    if (!await predicate(e.Current))
                    {
                        return false;
                    }
                }

                return true;
            }
            finally
            {
                if (e != null)
                {
                    await e.DisposeAsync();
                }
            }
        }

        internal static async Task<bool> AllAwaitWithCancellationAsync<TSource>(IUniTaskAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, Task<Boolean>> predicate, CancellationToken cancellationToken)
        {
            var e = source.GetAsyncEnumerator(cancellationToken);
            try
            {
                while (await e.MoveNextAsync())
                {
                    if (!await predicate(e.Current, cancellationToken))
                    {
                        return false;
                    }
                }

                return true;
            }
            finally
            {
                if (e != null)
                {
                    await e.DisposeAsync();
                }
            }
        }
    }
}