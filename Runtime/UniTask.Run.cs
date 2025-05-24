#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using System;
using System.Threading;

namespace UnityEssentials.Threading.Tasks
{
    public partial struct Task
    {
        #region OBSOLETE_RUN

        [Obsolete("Task.Run is similar as System.Threading.Tasks.Task.Run, it uses ThreadPool. For equivalent behaviour, use Task.RunOnThreadPool instead. If you don't want to use ThreadPool, you can use Task.Void(async void) or Task.Create(async Task) too.")]
        public static Task Run(Action action, bool configureAwait = true, CancellationToken cancellationToken = default)
        {
            return RunOnThreadPool(action, configureAwait, cancellationToken);
        }

        [Obsolete("Task.Run is similar as System.Threading.Tasks.Task.Run, it uses ThreadPool. For equivalent behaviour, use Task.RunOnThreadPool instead. If you don't want to use ThreadPool, you can use Task.Void(async void) or Task.Create(async Task) too.")]
        public static Task Run(Action<object> action, object state, bool configureAwait = true, CancellationToken cancellationToken = default)
        {
            return RunOnThreadPool(action, state, configureAwait, cancellationToken);
        }

        [Obsolete("Task.Run is similar as System.Threading.Tasks.Task.Run, it uses ThreadPool. For equivalent behaviour, use Task.RunOnThreadPool instead. If you don't want to use ThreadPool, you can use Task.Void(async void) or Task.Create(async Task) too.")]
        public static Task Run(Func<Task> action, bool configureAwait = true, CancellationToken cancellationToken = default)
        {
            return RunOnThreadPool(action, configureAwait, cancellationToken);
        }

        [Obsolete("Task.Run is similar as System.Threading.Tasks.Task.Run, it uses ThreadPool. For equivalent behaviour, use Task.RunOnThreadPool instead. If you don't want to use ThreadPool, you can use Task.Void(async void) or Task.Create(async Task) too.")]
        public static Task Run(Func<object, Task> action, object state, bool configureAwait = true, CancellationToken cancellationToken = default)
        {
            return RunOnThreadPool(action, state, configureAwait, cancellationToken);
        }

        [Obsolete("Task.Run is similar as System.Threading.Tasks.Task.Run, it uses ThreadPool. For equivalent behaviour, use Task.RunOnThreadPool instead. If you don't want to use ThreadPool, you can use Task.Void(async void) or Task.Create(async Task) too.")]
        public static Task<T> Run<T>(Func<T> func, bool configureAwait = true, CancellationToken cancellationToken = default)
        {
            return RunOnThreadPool(func, configureAwait, cancellationToken);
        }

        [Obsolete("Task.Run is similar as System.Threading.Tasks.Task.Run, it uses ThreadPool. For equivalent behaviour, use Task.RunOnThreadPool instead. If you don't want to use ThreadPool, you can use Task.Void(async void) or Task.Create(async Task) too.")]
        public static Task<T> Run<T>(Func<Task<T>> func, bool configureAwait = true, CancellationToken cancellationToken = default)
        {
            return RunOnThreadPool(func, configureAwait, cancellationToken);
        }

        [Obsolete("Task.Run is similar as System.Threading.Tasks.Task.Run, it uses ThreadPool. For equivalent behaviour, use Task.RunOnThreadPool instead. If you don't want to use ThreadPool, you can use Task.Void(async void) or Task.Create(async Task) too.")]
        public static Task<T> Run<T>(Func<object, T> func, object state, bool configureAwait = true, CancellationToken cancellationToken = default)
        {
            return RunOnThreadPool(func, state, configureAwait, cancellationToken);
        }

        [Obsolete("Task.Run is similar as System.Threading.Tasks.Task.Run, it uses ThreadPool. For equivalent behaviour, use Task.RunOnThreadPool instead. If you don't want to use ThreadPool, you can use Task.Void(async void) or Task.Create(async Task) too.")]
        public static Task<T> Run<T>(Func<object, Task<T>> func, object state, bool configureAwait = true, CancellationToken cancellationToken = default)
        {
            return RunOnThreadPool(func, state, configureAwait, cancellationToken);
        }

        #endregion

        /// <summary>Run action on the threadPool and return to main thread if configureAwait = true.</summary>
        public static async Task RunOnThreadPool(Action action, bool configureAwait = true, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await Task.SwitchToThreadPool();

            cancellationToken.ThrowIfCancellationRequested();

            if (configureAwait)
            {
                try
                {
                    action();
                }
                finally
                {
                    await Task.Yield();
                }
            }
            else
            {
                action();
            }

            cancellationToken.ThrowIfCancellationRequested();
        }

        /// <summary>Run action on the threadPool and return to main thread if configureAwait = true.</summary>
        public static async Task RunOnThreadPool(Action<object> action, object state, bool configureAwait = true, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await Task.SwitchToThreadPool();

            cancellationToken.ThrowIfCancellationRequested();

            if (configureAwait)
            {
                try
                {
                    action(state);
                }
                finally
                {
                    await Task.Yield();
                }
            }
            else
            {
                action(state);
            }

            cancellationToken.ThrowIfCancellationRequested();
        }

        /// <summary>Run action on the threadPool and return to main thread if configureAwait = true.</summary>
        public static async Task RunOnThreadPool(Func<Task> action, bool configureAwait = true, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await Task.SwitchToThreadPool();

            cancellationToken.ThrowIfCancellationRequested();

            if (configureAwait)
            {
                try
                {
                    await action();
                }
                finally
                {
                    await Task.Yield();
                }
            }
            else
            {
                await action();
            }

            cancellationToken.ThrowIfCancellationRequested();
        }

        /// <summary>Run action on the threadPool and return to main thread if configureAwait = true.</summary>
        public static async Task RunOnThreadPool(Func<object, Task> action, object state, bool configureAwait = true, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await Task.SwitchToThreadPool();

            cancellationToken.ThrowIfCancellationRequested();

            if (configureAwait)
            {
                try
                {
                    await action(state);
                }
                finally
                {
                    await Task.Yield();
                }
            }
            else
            {
                await action(state);
            }

            cancellationToken.ThrowIfCancellationRequested();
        }

        /// <summary>Run action on the threadPool and return to main thread if configureAwait = true.</summary>
        public static async Task<T> RunOnThreadPool<T>(Func<T> func, bool configureAwait = true, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await Task.SwitchToThreadPool();

            cancellationToken.ThrowIfCancellationRequested();

            if (configureAwait)
            {
                try
                {
                    return func();
                }
                finally
                {
                    await Task.Yield();
                    cancellationToken.ThrowIfCancellationRequested();
                }
            }
            else
            {
                return func();
            }
        }

        /// <summary>Run action on the threadPool and return to main thread if configureAwait = true.</summary>
        public static async Task<T> RunOnThreadPool<T>(Func<Task<T>> func, bool configureAwait = true, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await Task.SwitchToThreadPool();

            cancellationToken.ThrowIfCancellationRequested();

            if (configureAwait)
            {
                try
                {
                    return await func();
                }
                finally
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    await Task.Yield();
                    cancellationToken.ThrowIfCancellationRequested();
                }
            }
            else
            {
                var result = await func();
                cancellationToken.ThrowIfCancellationRequested();
                return result;
            }
        }

        /// <summary>Run action on the threadPool and return to main thread if configureAwait = true.</summary>
        public static async Task<T> RunOnThreadPool<T>(Func<object, T> func, object state, bool configureAwait = true, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await Task.SwitchToThreadPool();

            cancellationToken.ThrowIfCancellationRequested();

            if (configureAwait)
            {
                try
                {
                    return func(state);
                }
                finally
                {
                    await Task.Yield();
                    cancellationToken.ThrowIfCancellationRequested();
                }
            }
            else
            {
                return func(state);
            }
        }

        /// <summary>Run action on the threadPool and return to main thread if configureAwait = true.</summary>
        public static async Task<T> RunOnThreadPool<T>(Func<object, Task<T>> func, object state, bool configureAwait = true, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await Task.SwitchToThreadPool();

            cancellationToken.ThrowIfCancellationRequested();

            if (configureAwait)
            {
                try
                {
                    return await func(state);
                }
                finally
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    await Task.Yield();
                    cancellationToken.ThrowIfCancellationRequested();
                }
            }
            else
            {
                var result = await func(state);
                cancellationToken.ThrowIfCancellationRequested();
                return result;
            }
        }
    }
}

