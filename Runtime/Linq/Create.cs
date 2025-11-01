using UnityEssentials;
using System;
using System.Threading;

namespace UnityEssentials
{
    public static partial class UniTaskAsyncEnumerable
    {
        public static IUniTaskAsyncEnumerable<T> Create<T>(Func<IAsyncWriter<T>, CancellationToken, Task> create)
        {
            Error.ThrowArgumentNullException(create, nameof(create));
            return new Create<T>(create);
        }
    }

    public interface IAsyncWriter<T>
    {
        Task YieldAsync(T value);
    }

    internal sealed class Create<T> : IUniTaskAsyncEnumerable<T>
    {
        readonly Func<IAsyncWriter<T>, CancellationToken, Task> create;

        public Create(Func<IAsyncWriter<T>, CancellationToken, Task> create)
        {
            this.create = create;
        }

        public IUniTaskAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            return new _Create(create, cancellationToken);
        }

        sealed class _Create : MoveNextSource, IUniTaskAsyncEnumerator<T>
        {
            readonly Func<IAsyncWriter<T>, CancellationToken, Task> create;
            readonly CancellationToken cancellationToken;

            int state = -1;
            AsyncWriter writer;

            public _Create(Func<IAsyncWriter<T>, CancellationToken, Task> create, CancellationToken cancellationToken)
            {
                this.create = create;
                this.cancellationToken = cancellationToken;
                TaskTracker.TrackActiveTask(this, 3);
            }

            public T Current { get; private set; }

            public Task DisposeAsync()
            {
                TaskTracker.RemoveTracking(this);
                writer.Dispose();
                return default;
            }

            public Task<bool> MoveNextAsync()
            {
                if (state == -2) return default;

                completionSource.Reset();
                MoveNext();
                return new Task<bool>(this, completionSource.Version);
            }

            void MoveNext()
            {
                try
                {
                    switch (state)
                    {
                        case -1: // init
                            {
                                writer = new AsyncWriter(this);
                                RunWriterTask(create(writer, cancellationToken)).Forget();
                                if (Volatile.Read(ref state) == -2)
                                {
                                    return; // complete synchronously
                                }
                                state = 0; // wait YieldAsync, it set TrySetResult(true)
                                return;
                            }
                        case 0:
                            writer.SignalWriter();
                            return;
                        default:
                            goto DONE;
                    }
                }
                catch (Exception ex)
                {
                    state = -2;
                    completionSource.TrySetException(ex);
                    return;
                }

                DONE:
                state = -2;
                completionSource.TrySetResult(false);
                return;
            }

            async TaskVoid RunWriterTask(Task task)
            {
                try
                {
                    await task;
                    goto DONE;
                }
                catch (Exception ex)
                {
                    Volatile.Write(ref state, -2);
                    completionSource.TrySetException(ex);
                    return;
                }

                DONE:
                Volatile.Write(ref state, -2);
                completionSource.TrySetResult(false);
            }

            public void SetResult(T value)
            {
                Current = value;
                completionSource.TrySetResult(true);
            }
        }

        sealed class AsyncWriter : IUniTaskSource, IAsyncWriter<T>, IDisposable
        {
            readonly _Create enumerator;

            UniTaskCompletionSourceCore<AsyncUnit> core;

            public AsyncWriter(_Create enumerator)
            {
                this.enumerator = enumerator;
            }
            
            public void Dispose()
            {
                var status = core.GetStatus(core.Version);
                if (status == UniTaskStatus.Pending)
                {
                    core.TrySetCanceled();
                }
            }            

            public void GetResult(short token)
            {
                core.GetResult(token);
            }

            public UniTaskStatus GetStatus(short token)
            {
                return core.GetStatus(token);
            }

            public UniTaskStatus UnsafeGetStatus()
            {
                return core.UnsafeGetStatus();
            }

            public void OnCompleted(Action<object> continuation, object state, short token)
            {
                core.OnCompleted(continuation, state, token);
            }

            public Task YieldAsync(T value)
            {
                core.Reset();
                enumerator.SetResult(value);
                return new Task(this, core.Version);
            }

            public void SignalWriter()
            {
                core.TrySetResult(AsyncUnit.Default);
            }
        }
    }
}
