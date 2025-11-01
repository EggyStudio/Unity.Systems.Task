using UnityEssentials;
using System;
using System.Collections.Generic;
using System.Threading;

namespace UnityEssentials
{
    public static partial class UniTaskAsyncEnumerable
    {
        public static Task<TSource> AggregateAsync<TSource>(this IUniTaskAsyncEnumerable<TSource> source, Func<TSource, TSource, TSource> accumulator, CancellationToken cancellationToken = default)
        {
            Error.ThrowArgumentNullException(source, nameof(source));
            Error.ThrowArgumentNullException(accumulator, nameof(accumulator));

            return Aggregate.AggregateAsync(source, accumulator, cancellationToken);
        }

        public static Task<TAccumulate> AggregateAsync<TSource, TAccumulate>(this IUniTaskAsyncEnumerable<TSource> source, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> accumulator, CancellationToken cancellationToken = default)
        {
            Error.ThrowArgumentNullException(source, nameof(source));
            Error.ThrowArgumentNullException(accumulator, nameof(accumulator));

            return Aggregate.AggregateAsync(source, seed, accumulator, cancellationToken);
        }

        public static Task<TResult> AggregateAsync<TSource, TAccumulate, TResult>(this IUniTaskAsyncEnumerable<TSource> source, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> accumulator, Func<TAccumulate, TResult> resultSelector, CancellationToken cancellationToken = default)
        {
            Error.ThrowArgumentNullException(source, nameof(source));
            Error.ThrowArgumentNullException(accumulator, nameof(accumulator));
            Error.ThrowArgumentNullException(accumulator, nameof(resultSelector));

            return Aggregate.AggregateAsync(source, seed, accumulator, resultSelector, cancellationToken);
        }

        public static Task<TSource> AggregateAwaitAsync<TSource>(this IUniTaskAsyncEnumerable<TSource> source, Func<TSource, TSource, Task<TSource>> accumulator, CancellationToken cancellationToken = default)
        {
            Error.ThrowArgumentNullException(source, nameof(source));
            Error.ThrowArgumentNullException(accumulator, nameof(accumulator));

            return Aggregate.AggregateAwaitAsync(source, accumulator, cancellationToken);
        }

        public static Task<TAccumulate> AggregateAwaitAsync<TSource, TAccumulate>(this IUniTaskAsyncEnumerable<TSource> source, TAccumulate seed, Func<TAccumulate, TSource, Task<TAccumulate>> accumulator, CancellationToken cancellationToken = default)
        {
            Error.ThrowArgumentNullException(source, nameof(source));
            Error.ThrowArgumentNullException(accumulator, nameof(accumulator));

            return Aggregate.AggregateAwaitAsync(source, seed, accumulator, cancellationToken);
        }

        public static Task<TResult> AggregateAwaitAsync<TSource, TAccumulate, TResult>(this IUniTaskAsyncEnumerable<TSource> source, TAccumulate seed, Func<TAccumulate, TSource, Task<TAccumulate>> accumulator, Func<TAccumulate, Task<TResult>> resultSelector, CancellationToken cancellationToken = default)
        {
            Error.ThrowArgumentNullException(source, nameof(source));
            Error.ThrowArgumentNullException(accumulator, nameof(accumulator));
            Error.ThrowArgumentNullException(accumulator, nameof(resultSelector));

            return Aggregate.AggregateAwaitAsync(source, seed, accumulator, resultSelector, cancellationToken);
        }

        public static Task<TSource> AggregateAwaitWithCancellationAsync<TSource>(this IUniTaskAsyncEnumerable<TSource> source, Func<TSource, TSource, CancellationToken, Task<TSource>> accumulator, CancellationToken cancellationToken = default)
        {
            Error.ThrowArgumentNullException(source, nameof(source));
            Error.ThrowArgumentNullException(accumulator, nameof(accumulator));

            return Aggregate.AggregateAwaitWithCancellationAsync(source, accumulator, cancellationToken);
        }

        public static Task<TAccumulate> AggregateAwaitWithCancellationAsync<TSource, TAccumulate>(this IUniTaskAsyncEnumerable<TSource> source, TAccumulate seed, Func<TAccumulate, TSource, CancellationToken, Task<TAccumulate>> accumulator, CancellationToken cancellationToken = default)
        {
            Error.ThrowArgumentNullException(source, nameof(source));
            Error.ThrowArgumentNullException(accumulator, nameof(accumulator));

            return Aggregate.AggregateAwaitWithCancellationAsync(source, seed, accumulator, cancellationToken);
        }

        public static Task<TResult> AggregateAwaitWithCancellationAsync<TSource, TAccumulate, TResult>(this IUniTaskAsyncEnumerable<TSource> source, TAccumulate seed, Func<TAccumulate, TSource, CancellationToken, Task<TAccumulate>> accumulator, Func<TAccumulate, CancellationToken, Task<TResult>> resultSelector, CancellationToken cancellationToken = default)
        {
            Error.ThrowArgumentNullException(source, nameof(source));
            Error.ThrowArgumentNullException(accumulator, nameof(accumulator));
            Error.ThrowArgumentNullException(accumulator, nameof(resultSelector));

            return Aggregate.AggregateAwaitWithCancellationAsync(source, seed, accumulator, resultSelector, cancellationToken);
        }
    }

    internal static class Aggregate
    {
        internal static async Task<TSource> AggregateAsync<TSource>(IUniTaskAsyncEnumerable<TSource> source, Func<TSource, TSource, TSource> accumulator, CancellationToken cancellationToken)
        {
            var e = source.GetAsyncEnumerator(cancellationToken);
            try
            {
                TSource value;
                if (await e.MoveNextAsync())
                {
                    value = e.Current;
                }
                else
                {
                    throw Error.NoElements();
                }

                while (await e.MoveNextAsync())
                {
                    value = accumulator(value, e.Current);
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

        internal static async Task<TAccumulate> AggregateAsync<TSource, TAccumulate>(IUniTaskAsyncEnumerable<TSource> source, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> accumulator, CancellationToken cancellationToken)
        {
            var e = source.GetAsyncEnumerator(cancellationToken);
            try
            {
                TAccumulate value = seed;
                while (await e.MoveNextAsync())
                {
                    value = accumulator(value, e.Current);
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

        internal static async Task<TResult> AggregateAsync<TSource, TAccumulate, TResult>(IUniTaskAsyncEnumerable<TSource> source, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> accumulator, Func<TAccumulate, TResult> resultSelector, CancellationToken cancellationToken)
        {
            var e = source.GetAsyncEnumerator(cancellationToken);
            try
            {
                TAccumulate value = seed;
                while (await e.MoveNextAsync())
                {
                    value = accumulator(value, e.Current);
                }
                return resultSelector(value);

            }
            finally
            {
                if (e != null)
                {
                    await e.DisposeAsync();
                }
            }
        }

        // with async

        internal static async Task<TSource> AggregateAwaitAsync<TSource>(IUniTaskAsyncEnumerable<TSource> source, Func<TSource, TSource, Task<TSource>> accumulator, CancellationToken cancellationToken)
        {
            var e = source.GetAsyncEnumerator(cancellationToken);
            try
            {
                TSource value;
                if (await e.MoveNextAsync())
                {
                    value = e.Current;
                }
                else
                {
                    throw Error.NoElements();
                }

                while (await e.MoveNextAsync())
                {
                    value = await accumulator(value, e.Current);
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

        internal static async Task<TAccumulate> AggregateAwaitAsync<TSource, TAccumulate>(IUniTaskAsyncEnumerable<TSource> source, TAccumulate seed, Func<TAccumulate, TSource, Task<TAccumulate>> accumulator, CancellationToken cancellationToken)
        {
            var e = source.GetAsyncEnumerator(cancellationToken);
            try
            {
                TAccumulate value = seed;
                while (await e.MoveNextAsync())
                {
                    value = await accumulator(value, e.Current);
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

        internal static async Task<TResult> AggregateAwaitAsync<TSource, TAccumulate, TResult>(IUniTaskAsyncEnumerable<TSource> source, TAccumulate seed, Func<TAccumulate, TSource, Task<TAccumulate>> accumulator, Func<TAccumulate, Task<TResult>> resultSelector, CancellationToken cancellationToken)
        {
            var e = source.GetAsyncEnumerator(cancellationToken);
            try
            {
                TAccumulate value = seed;
                while (await e.MoveNextAsync())
                {
                    value = await accumulator(value, e.Current);
                }
                return await resultSelector(value);

            }
            finally
            {
                if (e != null)
                {
                    await e.DisposeAsync();
                }
            }
        }


        // with cancellation

        internal static async Task<TSource> AggregateAwaitWithCancellationAsync<TSource>(IUniTaskAsyncEnumerable<TSource> source, Func<TSource, TSource, CancellationToken, Task<TSource>> accumulator, CancellationToken cancellationToken)
        {
            var e = source.GetAsyncEnumerator(cancellationToken);
            try
            {
                TSource value;
                if (await e.MoveNextAsync())
                {
                    value = e.Current;
                }
                else
                {
                    throw Error.NoElements();
                }

                while (await e.MoveNextAsync())
                {
                    value = await accumulator(value, e.Current, cancellationToken);
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

        internal static async Task<TAccumulate> AggregateAwaitWithCancellationAsync<TSource, TAccumulate>(IUniTaskAsyncEnumerable<TSource> source, TAccumulate seed, Func<TAccumulate, TSource, CancellationToken, Task<TAccumulate>> accumulator, CancellationToken cancellationToken)
        {
            var e = source.GetAsyncEnumerator(cancellationToken);
            try
            {
                TAccumulate value = seed;
                while (await e.MoveNextAsync())
                {
                    value = await accumulator(value, e.Current, cancellationToken);
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

        internal static async Task<TResult> AggregateAwaitWithCancellationAsync<TSource, TAccumulate, TResult>(IUniTaskAsyncEnumerable<TSource> source, TAccumulate seed, Func<TAccumulate, TSource, CancellationToken, Task<TAccumulate>> accumulator, Func<TAccumulate, CancellationToken, Task<TResult>> resultSelector, CancellationToken cancellationToken)
        {
            var e = source.GetAsyncEnumerator(cancellationToken);
            try
            {
                TAccumulate value = seed;
                while (await e.MoveNextAsync())
                {
                    value = await accumulator(value, e.Current, cancellationToken);
                }
                return await resultSelector(value, cancellationToken);

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