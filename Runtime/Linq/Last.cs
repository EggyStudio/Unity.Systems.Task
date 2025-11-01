using UnityEssentials;
using System;
using System.Threading;

namespace UnityEssentials
{
    public static partial class UniTaskAsyncEnumerable
    {
        public static Task<TSource> LastAsync<TSource>(this IUniTaskAsyncEnumerable<TSource> source, CancellationToken cancellationToken = default)
        {
            Error.ThrowArgumentNullException(source, nameof(source));

            return Last.LastAsync(source, cancellationToken, false);
        }

        public static Task<TSource> LastAsync<TSource>(this IUniTaskAsyncEnumerable<TSource> source, Func<TSource, Boolean> predicate, CancellationToken cancellationToken = default)
        {
            Error.ThrowArgumentNullException(source, nameof(source));
            Error.ThrowArgumentNullException(predicate, nameof(predicate));

            return Last.LastAsync(source, predicate, cancellationToken, false);
        }

        public static Task<TSource> LastAwaitAsync<TSource>(this IUniTaskAsyncEnumerable<TSource> source, Func<TSource, Task<Boolean>> predicate, CancellationToken cancellationToken = default)
        {
            Error.ThrowArgumentNullException(source, nameof(source));
            Error.ThrowArgumentNullException(predicate, nameof(predicate));

            return Last.LastAwaitAsync(source, predicate, cancellationToken, false);
        }

        public static Task<TSource> LastAwaitWithCancellationAsync<TSource>(this IUniTaskAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, Task<Boolean>> predicate, CancellationToken cancellationToken = default)
        {
            Error.ThrowArgumentNullException(source, nameof(source));
            Error.ThrowArgumentNullException(predicate, nameof(predicate));

            return Last.LastAwaitWithCancellationAsync(source, predicate, cancellationToken, false);
        }

        public static Task<TSource> LastOrDefaultAsync<TSource>(this IUniTaskAsyncEnumerable<TSource> source, CancellationToken cancellationToken = default)
        {
            Error.ThrowArgumentNullException(source, nameof(source));

            return Last.LastAsync(source, cancellationToken, true);
        }

        public static Task<TSource> LastOrDefaultAsync<TSource>(this IUniTaskAsyncEnumerable<TSource> source, Func<TSource, Boolean> predicate, CancellationToken cancellationToken = default)
        {
            Error.ThrowArgumentNullException(source, nameof(source));
            Error.ThrowArgumentNullException(predicate, nameof(predicate));

            return Last.LastAsync(source, predicate, cancellationToken, true);
        }

        public static Task<TSource> LastOrDefaultAwaitAsync<TSource>(this IUniTaskAsyncEnumerable<TSource> source, Func<TSource, Task<Boolean>> predicate, CancellationToken cancellationToken = default)
        {
            Error.ThrowArgumentNullException(source, nameof(source));
            Error.ThrowArgumentNullException(predicate, nameof(predicate));

            return Last.LastAwaitAsync(source, predicate, cancellationToken, true);
        }

        public static Task<TSource> LastOrDefaultAwaitWithCancellationAsync<TSource>(this IUniTaskAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, Task<Boolean>> predicate, CancellationToken cancellationToken = default)
        {
            Error.ThrowArgumentNullException(source, nameof(source));
            Error.ThrowArgumentNullException(predicate, nameof(predicate));

            return Last.LastAwaitWithCancellationAsync(source, predicate, cancellationToken, true);
        }
    }

    internal static class Last
    {
        public static async Task<TSource> LastAsync<TSource>(IUniTaskAsyncEnumerable<TSource> source, CancellationToken cancellationToken, bool defaultIfEmpty)
        {
            var e = source.GetAsyncEnumerator(cancellationToken);
            try
            {
                TSource value = default;
                if (await e.MoveNextAsync())
                {
                    value = e.Current;
                }
                else
                {
                    if (defaultIfEmpty)
                    {
                        return value;
                    }
                    else
                    {
                        throw Error.NoElements();
                    }
                }

                while (await e.MoveNextAsync())
                {
                    value = e.Current;
                }
                return value;
            }
            finally
            {
                if (e != null)
                {
                    await e.DisposeAsync();
                }
            }
        }

        public static async Task<TSource> LastAsync<TSource>(IUniTaskAsyncEnumerable<TSource> source, Func<TSource, Boolean> predicate, CancellationToken cancellationToken, bool defaultIfEmpty)
        {
            var e = source.GetAsyncEnumerator(cancellationToken);
            try
            {
                TSource value = default;

                bool found = false;
                while (await e.MoveNextAsync())
                {
                    var v = e.Current;
                    if (predicate(v))
                    {
                        found = true;
                        value = v;
                    }
                }

                if (defaultIfEmpty)
                {
                    return value;
                }
                else
                {
                    if (found)
                    {
                        return value;
                    }
                    else
                    {
                        throw Error.NoElements();
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
        }

        public static async Task<TSource> LastAwaitAsync<TSource>(IUniTaskAsyncEnumerable<TSource> source, Func<TSource, Task<Boolean>> predicate, CancellationToken cancellationToken, bool defaultIfEmpty)
        {
            var e = source.GetAsyncEnumerator(cancellationToken);
            try
            {
                TSource value = default;

                bool found = false;
                while (await e.MoveNextAsync())
                {
                    var v = e.Current;
                    if (await predicate(v))
                    {
                        found = true;
                        value = v;
                    }
                }

                if (defaultIfEmpty)
                {
                    return value;
                }
                else
                {
                    if (found)
                    {
                        return value;
                    }
                    else
                    {
                        throw Error.NoElements();
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
        }

        public static async Task<TSource> LastAwaitWithCancellationAsync<TSource>(IUniTaskAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, Task<Boolean>> predicate, CancellationToken cancellationToken, bool defaultIfEmpty)
        {
            var e = source.GetAsyncEnumerator(cancellationToken);
            try
            {
                TSource value = default;

                bool found = false;
                while (await e.MoveNextAsync())
                {
                    var v = e.Current;
                    if (await predicate(v, cancellationToken))
                    {
                        found = true;
                        value = v;
                    }
                }

                if (defaultIfEmpty)
                {
                    return value;
                }
                else
                {
                    if (found)
                    {
                        return value;
                    }
                    else
                    {
                        throw Error.NoElements();
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
        }
    }
}