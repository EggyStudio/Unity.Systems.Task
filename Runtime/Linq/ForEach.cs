using UnityEssentials;
using System;
using System.Threading;

namespace UnityEssentials
{
    public static partial class UniTaskAsyncEnumerable
    {
        public static Task ForEachAsync<TSource>(this IUniTaskAsyncEnumerable<TSource> source, Action<TSource> action, CancellationToken cancellationToken = default)
        {
            Error.ThrowArgumentNullException(source, nameof(source));
            Error.ThrowArgumentNullException(action, nameof(action));

            return UnityEssentials.ForEach.ForEachAsync(source, action, cancellationToken);
        }

        public static Task ForEachAsync<TSource>(this IUniTaskAsyncEnumerable<TSource> source, Action<TSource, Int32> action, CancellationToken cancellationToken = default)
        {
            Error.ThrowArgumentNullException(source, nameof(source));
            Error.ThrowArgumentNullException(action, nameof(action));

            return UnityEssentials.ForEach.ForEachAsync(source, action, cancellationToken);
        }

        /// <summary>Obsolete(Error), Use Use ForEachAwaitAsync instead.</summary>
        [Obsolete("Use ForEachAwaitAsync instead.", true)]
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public static Task ForEachAsync<T>(this IUniTaskAsyncEnumerable<T> source, Func<T, Task> action, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException("Use ForEachAwaitAsync instead.");
        }

        /// <summary>Obsolete(Error), Use Use ForEachAwaitAsync instead.</summary>
        [Obsolete("Use ForEachAwaitAsync instead.", true)]
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public static Task ForEachAsync<T>(this IUniTaskAsyncEnumerable<T> source, Func<T, int, Task> action, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException("Use ForEachAwaitAsync instead.");
        }

        public static Task ForEachAwaitAsync<TSource>(this IUniTaskAsyncEnumerable<TSource> source, Func<TSource, Task> action, CancellationToken cancellationToken = default)
        {
            Error.ThrowArgumentNullException(source, nameof(source));
            Error.ThrowArgumentNullException(action, nameof(action));

            return UnityEssentials.ForEach.ForEachAwaitAsync(source, action, cancellationToken);
        }

        public static Task ForEachAwaitAsync<TSource>(this IUniTaskAsyncEnumerable<TSource> source, Func<TSource, Int32, Task> action, CancellationToken cancellationToken = default)
        {
            Error.ThrowArgumentNullException(source, nameof(source));
            Error.ThrowArgumentNullException(action, nameof(action));

            return UnityEssentials.ForEach.ForEachAwaitAsync(source, action, cancellationToken);
        }

        public static Task ForEachAwaitWithCancellationAsync<TSource>(this IUniTaskAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, Task> action, CancellationToken cancellationToken = default)
        {
            Error.ThrowArgumentNullException(source, nameof(source));
            Error.ThrowArgumentNullException(action, nameof(action));

            return UnityEssentials.ForEach.ForEachAwaitWithCancellationAsync(source, action, cancellationToken);
        }

        public static Task ForEachAwaitWithCancellationAsync<TSource>(this IUniTaskAsyncEnumerable<TSource> source, Func<TSource, Int32, CancellationToken, Task> action, CancellationToken cancellationToken = default)
        {
            Error.ThrowArgumentNullException(source, nameof(source));
            Error.ThrowArgumentNullException(action, nameof(action));

            return UnityEssentials.ForEach.ForEachAwaitWithCancellationAsync(source, action, cancellationToken);
        }
    }

    internal static class ForEach
    {
        public static async Task ForEachAsync<TSource>(IUniTaskAsyncEnumerable<TSource> source, Action<TSource> action, CancellationToken cancellationToken)
        {
            var e = source.GetAsyncEnumerator(cancellationToken);
            try
            {
                while (await e.MoveNextAsync())
                {
                    action(e.Current);
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

        public static async Task ForEachAsync<TSource>(IUniTaskAsyncEnumerable<TSource> source, Action<TSource, Int32> action, CancellationToken cancellationToken)
        {
            var e = source.GetAsyncEnumerator(cancellationToken);
            try
            {
                int index = 0;
                while (await e.MoveNextAsync())
                {
                    action(e.Current, checked(index++));
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

        public static async Task ForEachAwaitAsync<TSource>(IUniTaskAsyncEnumerable<TSource> source, Func<TSource, Task> action, CancellationToken cancellationToken)
        {
            var e = source.GetAsyncEnumerator(cancellationToken);
            try
            {
                while (await e.MoveNextAsync())
                {
                    await action(e.Current);
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

        public static async Task ForEachAwaitAsync<TSource>(IUniTaskAsyncEnumerable<TSource> source, Func<TSource, Int32, Task> action, CancellationToken cancellationToken)
        {
            var e = source.GetAsyncEnumerator(cancellationToken);
            try
            {
                int index = 0;
                while (await e.MoveNextAsync())
                {
                    await action(e.Current, checked(index++));
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

        public static async Task ForEachAwaitWithCancellationAsync<TSource>(IUniTaskAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, Task> action, CancellationToken cancellationToken)
        {
            var e = source.GetAsyncEnumerator(cancellationToken);
            try
            {
                while (await e.MoveNextAsync())
                {
                    await action(e.Current, cancellationToken);
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

        public static async Task ForEachAwaitWithCancellationAsync<TSource>(IUniTaskAsyncEnumerable<TSource> source, Func<TSource, Int32, CancellationToken, Task> action, CancellationToken cancellationToken)
        {
            var e = source.GetAsyncEnumerator(cancellationToken);
            try
            {
                int index = 0;
                while (await e.MoveNextAsync())
                {
                    await action(e.Current, checked(index++), cancellationToken);
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