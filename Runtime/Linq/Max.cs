using System;
using System.Collections.Generic;
using System.Threading;
using UnityEssentials;

namespace UnityEssentials
{
    public static partial class UniTaskAsyncEnumerable
    {
        public static Task<TSource> MaxAsync<TSource>(this IUniTaskAsyncEnumerable<TSource> source, CancellationToken cancellationToken = default)
        {
            Error.ThrowArgumentNullException(source, nameof(source));

            return Max.MaxAsync(source, cancellationToken);
        }

        public static Task<TResult> MaxAsync<TSource, TResult>(this IUniTaskAsyncEnumerable<TSource> source, Func<TSource, TResult> selector, CancellationToken cancellationToken = default)
        {
            Error.ThrowArgumentNullException(source, nameof(source));
            Error.ThrowArgumentNullException(source, nameof(selector));

            return Max.MaxAsync(source, selector, cancellationToken);
        }

        public static Task<TResult> MaxAwaitAsync<TSource, TResult>(this IUniTaskAsyncEnumerable<TSource> source, Func<TSource, Task<TResult>> selector, CancellationToken cancellationToken = default)
        {
            Error.ThrowArgumentNullException(source, nameof(source));
            Error.ThrowArgumentNullException(source, nameof(selector));

            return Max.MaxAwaitAsync(source, selector, cancellationToken);
        }

        public static Task<TResult> MaxAwaitWithCancellationAsync<TSource, TResult>(this IUniTaskAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, Task<TResult>> selector, CancellationToken cancellationToken = default)
        {
            Error.ThrowArgumentNullException(source, nameof(source));
            Error.ThrowArgumentNullException(source, nameof(selector));

            return Max.MaxAwaitWithCancellationAsync(source, selector, cancellationToken);
        }
    }

    internal static partial class Max
    {
        public static async Task<TSource> MaxAsync<TSource>(IUniTaskAsyncEnumerable<TSource> source, CancellationToken cancellationToken)
        {
            TSource value = default;
            var comparer = Comparer<TSource>.Default;

            var e = source.GetAsyncEnumerator(cancellationToken);
            try
            {
                while (await e.MoveNextAsync())
                {
                    value = e.Current;

                    goto NEXT_LOOP;
                }

                return value;

                NEXT_LOOP:

                while (await e.MoveNextAsync())
                {
                    var x = e.Current;
                    if (comparer.Compare(value, x) < 0)
                    {
                        value = x;
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

            return value;
        }

        public static async Task<TResult> MaxAsync<TSource, TResult>(IUniTaskAsyncEnumerable<TSource> source, Func<TSource, TResult> selector, CancellationToken cancellationToken)
        {
            TResult value = default;
            var comparer = Comparer<TResult>.Default;

            var e = source.GetAsyncEnumerator(cancellationToken);
            try
            {
                while (await e.MoveNextAsync())
                {
                    value = selector(e.Current);

                    goto NEXT_LOOP;
                }

                return value;

                NEXT_LOOP:

                while (await e.MoveNextAsync())
                {
                    var x = selector(e.Current);
                    if (comparer.Compare(value, x) < 0)
                    {
                        value = x;
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

            return value;
        }

        public static async Task<TResult> MaxAwaitAsync<TSource, TResult>(IUniTaskAsyncEnumerable<TSource> source, Func<TSource, Task<TResult>> selector, CancellationToken cancellationToken)
        {
            TResult value = default;
            var comparer = Comparer<TResult>.Default;

            var e = source.GetAsyncEnumerator(cancellationToken);
            try
            {
                while (await e.MoveNextAsync())
                {
                    value = await selector(e.Current);

                    goto NEXT_LOOP;
                }

                return value;

                NEXT_LOOP:

                while (await e.MoveNextAsync())
                {
                    var x = await selector(e.Current);
                    if (comparer.Compare(value, x) < 0)
                    {
                        value = x;
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

            return value;
        }

        public static async Task<TResult> MaxAwaitWithCancellationAsync<TSource, TResult>(IUniTaskAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, Task<TResult>> selector, CancellationToken cancellationToken)
        {
            TResult value = default;
            var comparer = Comparer<TResult>.Default;

            var e = source.GetAsyncEnumerator(cancellationToken);
            try
            {
                while (await e.MoveNextAsync())
                {
                    value = await selector(e.Current, cancellationToken);

                    goto NEXT_LOOP;
                }

                return value;

                NEXT_LOOP:

                while (await e.MoveNextAsync())
                {
                    var x = await selector(e.Current, cancellationToken);
                    if (comparer.Compare(value, x) < 0)
                    {
                        value = x;
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

            return value;
        }
    }
}