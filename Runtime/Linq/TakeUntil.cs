using UnityEssentials.Threading.Tasks.Internal;
using System;
using System.Threading;

namespace UnityEssentials.Threading.Tasks.Linq
{
    public static partial class UniTaskAsyncEnumerable
    {
        public static IUniTaskAsyncEnumerable<TSource> TakeUntil<TSource>(this IUniTaskAsyncEnumerable<TSource> source, Task other)
        {
            Error.ThrowArgumentNullException(source, nameof(source));

            return new TakeUntil<TSource>(source, other, null);
        }

        public static IUniTaskAsyncEnumerable<TSource> TakeUntil<TSource>(this IUniTaskAsyncEnumerable<TSource> source, Func<CancellationToken, Task> other)
        {
            Error.ThrowArgumentNullException(source, nameof(source));
            Error.ThrowArgumentNullException(source, nameof(other));

            return new TakeUntil<TSource>(source, default, other);
        }
    }

    internal sealed class TakeUntil<TSource> : IUniTaskAsyncEnumerable<TSource>
    {
        readonly IUniTaskAsyncEnumerable<TSource> source;
        readonly Task other;
        readonly Func<CancellationToken, Task> other2;

        public TakeUntil(IUniTaskAsyncEnumerable<TSource> source, Task other, Func<CancellationToken, Task> other2)
        {
            this.source = source;
            this.other = other;
            this.other2 = other2;
        }

        public IUniTaskAsyncEnumerator<TSource> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            if (other2 != null)
            {
                return new _TakeUntil(source, this.other2(cancellationToken), cancellationToken);
            }
            else
            {
                return new _TakeUntil(source, this.other, cancellationToken);
            }
        }

        sealed class _TakeUntil : MoveNextSource, IUniTaskAsyncEnumerator<TSource>
        {
            static readonly Action<object> CancelDelegate1 = OnCanceled1;
            static readonly Action<object> MoveNextCoreDelegate = MoveNextCore;

            readonly IUniTaskAsyncEnumerable<TSource> source;
            CancellationToken cancellationToken1;
            CancellationTokenRegistration cancellationTokenRegistration1;

            bool completed;
            Exception exception;
            IUniTaskAsyncEnumerator<TSource> enumerator;
            Task<bool>.Awaiter awaiter;

            public _TakeUntil(IUniTaskAsyncEnumerable<TSource> source, Task other, CancellationToken cancellationToken1)
            {
                this.source = source;
                this.cancellationToken1 = cancellationToken1;

                if (cancellationToken1.CanBeCanceled)
                {
                    this.cancellationTokenRegistration1 = cancellationToken1.RegisterWithoutCaptureExecutionContext(CancelDelegate1, this);
                }

                TaskTracker.TrackActiveTask(this, 3);

                RunOther(other).Forget();
            }

            public TSource Current { get; private set; }

            public Task<bool> MoveNextAsync()
            {
                if (completed)
                {
                    return CompletedTasks.False;
                }

                if (exception != null)
                {
                    return Task.FromException<bool>(exception);
                }

                if (cancellationToken1.IsCancellationRequested)
                {
                    return Task.FromCanceled<bool>(cancellationToken1);
                }

                if (enumerator == null)
                {
                    enumerator = source.GetAsyncEnumerator(cancellationToken1);
                }

                completionSource.Reset();
                SourceMoveNext();
                return new Task<bool>(this, completionSource.Version);
            }

            void SourceMoveNext()
            {
                try
                {
                    awaiter = enumerator.MoveNextAsync().GetAwaiter();
                    if (awaiter.IsCompleted)
                    {
                        MoveNextCore(this);
                    }
                    else
                    {
                        awaiter.SourceOnCompleted(MoveNextCoreDelegate, this);
                    }
                }
                catch (Exception ex)
                {
                    completionSource.TrySetException(ex);
                }
            }

            static void MoveNextCore(object state)
            {
                var self = (_TakeUntil)state;

                if (self.TryGetResult(self.awaiter, out var result))
                {
                    if (result)
                    {
                        if (self.exception != null)
                        {
                            self.completionSource.TrySetException(self.exception);
                        }
                        else if (self.cancellationToken1.IsCancellationRequested)
                        {
                            self.completionSource.TrySetCanceled(self.cancellationToken1);
                        }
                        else
                        {
                            self.Current = self.enumerator.Current;
                            self.completionSource.TrySetResult(true);
                        }
                    }
                    else
                    {
                        self.completionSource.TrySetResult(false);
                    }
                }
            }

            async UniTaskVoid RunOther(Task other)
            {
                try
                {
                    await other;
                    completed = true;
                    completionSource.TrySetResult(false);
                }
                catch (Exception ex)
                {
                    exception = ex;
                    completionSource.TrySetException(ex);
                }
            }

            static void OnCanceled1(object state)
            {
                var self = (_TakeUntil)state;
                self.completionSource.TrySetCanceled(self.cancellationToken1);
            }

            public Task DisposeAsync()
            {
                TaskTracker.RemoveTracking(this);
                cancellationTokenRegistration1.Dispose();
                if (enumerator != null)
                {
                    return enumerator.DisposeAsync();
                }
                return default;
            }
        }
    }
}