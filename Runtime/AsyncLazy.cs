#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using System;
using System.Threading;

namespace UnityEssentials.Threading.Tasks
{
    public class AsyncLazy
    {
        static Action<object> continuation = SetCompletionSource;

        Func<Task> taskFactory;
        UniTaskCompletionSource completionSource;
        Task.Awaiter awaiter;

        object syncLock;
        bool initialized;

        public AsyncLazy(Func<Task> taskFactory)
        {
            this.taskFactory = taskFactory;
            this.completionSource = new UniTaskCompletionSource();
            this.syncLock = new object();
            this.initialized = false;
        }

        internal AsyncLazy(Task task)
        {
            this.taskFactory = null;
            this.completionSource = new UniTaskCompletionSource();
            this.syncLock = null;
            this.initialized = true;

            var awaiter = task.GetAwaiter();
            if (awaiter.IsCompleted)
            {
                SetCompletionSource(awaiter);
            }
            else
            {
                this.awaiter = awaiter;
                awaiter.SourceOnCompleted(continuation, this);
            }
        }

        public Task Task
        {
            get
            {
                EnsureInitialized();
                return completionSource.Task;
            }
        }


        public Task.Awaiter GetAwaiter() => Task.GetAwaiter();

        void EnsureInitialized()
        {
            if (Volatile.Read(ref initialized))
            {
                return;
            }

            EnsureInitializedCore();
        }

        void EnsureInitializedCore()
        {
            lock (syncLock)
            {
                if (!Volatile.Read(ref initialized))
                {
                    var f = Interlocked.Exchange(ref taskFactory, null);
                    if (f != null)
                    {
                        var task = f();
                        var awaiter = task.GetAwaiter();
                        if (awaiter.IsCompleted)
                        {
                            SetCompletionSource(awaiter);
                        }
                        else
                        {
                            this.awaiter = awaiter;
                            awaiter.SourceOnCompleted(continuation, this);
                        }

                        Volatile.Write(ref initialized, true);
                    }
                }
            }
        }

        void SetCompletionSource(in Task.Awaiter awaiter)
        {
            try
            {
                awaiter.GetResult();
                completionSource.TrySetResult();
            }
            catch (Exception ex)
            {
                completionSource.TrySetException(ex);
            }
        }

        static void SetCompletionSource(object state)
        {
            var self = (AsyncLazy)state;
            try
            {
                self.awaiter.GetResult();
                self.completionSource.TrySetResult();
            }
            catch (Exception ex)
            {
                self.completionSource.TrySetException(ex);
            }
            finally
            {
                self.awaiter = default;
            }
        }
    }

    public class AsyncLazy<T>
    {
        static Action<object> continuation = SetCompletionSource;

        Func<Task<T>> taskFactory;
        UniTaskCompletionSource<T> completionSource;
        Task<T>.Awaiter awaiter;

        object syncLock;
        bool initialized;

        public AsyncLazy(Func<Task<T>> taskFactory)
        {
            this.taskFactory = taskFactory;
            this.completionSource = new UniTaskCompletionSource<T>();
            this.syncLock = new object();
            this.initialized = false;
        }

        internal AsyncLazy(Task<T> task)
        {
            this.taskFactory = null;
            this.completionSource = new UniTaskCompletionSource<T>();
            this.syncLock = null;
            this.initialized = true;

            var awaiter = task.GetAwaiter();
            if (awaiter.IsCompleted)
            {
                SetCompletionSource(awaiter);
            }
            else
            {
                this.awaiter = awaiter;
                awaiter.SourceOnCompleted(continuation, this);
            }
        }

        public Task<T> Task
        {
            get
            {
                EnsureInitialized();
                return completionSource.Task;
            }
        }


        public Task<T>.Awaiter GetAwaiter() => Task.GetAwaiter();

        void EnsureInitialized()
        {
            if (Volatile.Read(ref initialized))
            {
                return;
            }

            EnsureInitializedCore();
        }

        void EnsureInitializedCore()
        {
            lock (syncLock)
            {
                if (!Volatile.Read(ref initialized))
                {
                    var f = Interlocked.Exchange(ref taskFactory, null);
                    if (f != null)
                    {
                        var task = f();
                        var awaiter = task.GetAwaiter();
                        if (awaiter.IsCompleted)
                        {
                            SetCompletionSource(awaiter);
                        }
                        else
                        {
                            this.awaiter = awaiter;
                            awaiter.SourceOnCompleted(continuation, this);
                        }

                        Volatile.Write(ref initialized, true);
                    }
                }
            }
        }

        void SetCompletionSource(in Task<T>.Awaiter awaiter)
        {
            try
            {
                var result = awaiter.GetResult();
                completionSource.TrySetResult(result);
            }
            catch (Exception ex)
            {
                completionSource.TrySetException(ex);
            }
        }

        static void SetCompletionSource(object state)
        {
            var self = (AsyncLazy<T>)state;
            try
            {
                var result = self.awaiter.GetResult();
                self.completionSource.TrySetResult(result);
            }
            catch (Exception ex)
            {
                self.completionSource.TrySetException(ex);
            }
            finally
            {
                self.awaiter = default;
            }
        }
    }
}
