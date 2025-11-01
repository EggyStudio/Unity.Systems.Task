#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using System;
using System.Collections;
using System.Runtime.ExceptionServices;
using System.Threading;
using UnityEssentials;

namespace UnityEssentials
{
    public static partial class UniTaskExtensions
    {
        /// <summary>
        /// Convert Task[T] -> Task[T].
        /// </summary>
        public static Task<T> AsUniTask<T>(this System.Threading.Tasks.Task<T> task, bool useCurrentSynchronizationContext = true)
        {
            var promise = new UniTaskCompletionSource<T>();

            task.ContinueWith((x, state) =>
            {
                var p = (UniTaskCompletionSource<T>)state;

                switch (x.Status)
                {
                    case System.Threading.Tasks.TaskStatus.Canceled:
                        p.TrySetCanceled();
                        break;
                    case System.Threading.Tasks.TaskStatus.Faulted:
                        p.TrySetException(x.Exception.InnerException ?? x.Exception);
                        break;
                    case System.Threading.Tasks.TaskStatus.RanToCompletion:
                        p.TrySetResult(x.Result);
                        break;
                    default:
                        throw new NotSupportedException();
                }
            }, promise, useCurrentSynchronizationContext ? System.Threading.Tasks.TaskScheduler.FromCurrentSynchronizationContext() : System.Threading.Tasks.TaskScheduler.Current);

            return promise.Task;
        }

        /// <summary>
        /// Convert System.Threading.Tasks.Task -> Task.
        /// </summary>
        public static Task AsUniTask(this System.Threading.Tasks.Task task, bool useCurrentSynchronizationContext = true)
        {
            var promise = new UniTaskCompletionSource();

            task.ContinueWith((x, state) =>
            {
                var p = (UniTaskCompletionSource)state;

                switch (x.Status)
                {
                    case System.Threading.Tasks.TaskStatus.Canceled:
                        p.TrySetCanceled();
                        break;
                    case System.Threading.Tasks.TaskStatus.Faulted:
                        p.TrySetException(x.Exception.InnerException ?? x.Exception);
                        break;
                    case System.Threading.Tasks.TaskStatus.RanToCompletion:
                        p.TrySetResult();
                        break;
                    default:
                        throw new NotSupportedException();
                }
            }, promise, useCurrentSynchronizationContext ? System.Threading.Tasks.TaskScheduler.FromCurrentSynchronizationContext() : System.Threading.Tasks.TaskScheduler.Current);

            return promise.Task;
        }

        public static System.Threading.Tasks.Task<T> AsTask<T>(this Task<T> task)
        {
            try
            {
                Task<T>.Awaiter awaiter;
                try
                {
                    awaiter = task.GetAwaiter();
                }
                catch (Exception ex)
                {
                    return System.Threading.Tasks.Task.FromException<T>(ex);
                }

                if (awaiter.IsCompleted)
                {
                    try
                    {
                        var result = awaiter.GetResult();
                        return System.Threading.Tasks.Task.FromResult(result);
                    }
                    catch (Exception ex)
                    {
                        return System.Threading.Tasks.Task.FromException<T>(ex);
                    }
                }

                var tcs = new System.Threading.Tasks.TaskCompletionSource<T>();

                awaiter.SourceOnCompleted(state =>
                {
                    using (var tuple = (StateTuple<System.Threading.Tasks.TaskCompletionSource<T>, Task<T>.Awaiter>)state)
                    {
                        var (inTcs, inAwaiter) = tuple;
                        try
                        {
                            var result = inAwaiter.GetResult();
                            inTcs.SetResult(result);
                        }
                        catch (Exception ex)
                        {
                            inTcs.SetException(ex);
                        }
                    }
                }, StateTuple.Create(tcs, awaiter));

                return tcs.Task;
            }
            catch (Exception ex)
            {
                return System.Threading.Tasks.Task.FromException<T>(ex);
            }
        }

        public static System.Threading.Tasks.Task AsTask(this Task task)
        {
            try
            {
                Task.Awaiter awaiter;
                try
                {
                    awaiter = task.GetAwaiter();
                }
                catch (Exception ex)
                {
                    return System.Threading.Tasks.Task.FromException(ex);
                }

                if (awaiter.IsCompleted)
                {
                    try
                    {
                        awaiter.GetResult(); // check token valid on Succeeded
                        return System.Threading.Tasks.Task.CompletedTask;
                    }
                    catch (Exception ex)
                    {
                        return System.Threading.Tasks.Task.FromException(ex);
                    }
                }

                var tcs = new System.Threading.Tasks.TaskCompletionSource<object>();

                awaiter.SourceOnCompleted(state =>
                {
                    using (var tuple = (StateTuple<System.Threading.Tasks.TaskCompletionSource<object>, Task.Awaiter>)state)
                    {
                        var (inTcs, inAwaiter) = tuple;
                        try
                        {
                            inAwaiter.GetResult();
                            inTcs.SetResult(null);
                        }
                        catch (Exception ex)
                        {
                            inTcs.SetException(ex);
                        }
                    }
                }, StateTuple.Create(tcs, awaiter));

                return tcs.Task;
            }
            catch (Exception ex)
            {
                return System.Threading.Tasks.Task.FromException(ex);
            }
        }

        public static AsyncLazy ToAsyncLazy(this Task task)
        {
            return new AsyncLazy(task);
        }

        public static AsyncLazy<T> ToAsyncLazy<T>(this Task<T> task)
        {
            return new AsyncLazy<T>(task);
        }

        /// <summary>
        /// Ignore task result when cancel raised first.
        /// </summary>
        public static Task AttachExternalCancellation(this Task task, CancellationToken cancellationToken)
        {
            if (!cancellationToken.CanBeCanceled)
            {
                return task;
            }

            if (cancellationToken.IsCancellationRequested)
            {
                task.Forget();
                return Task.FromCanceled(cancellationToken);
            }

            if (task.Status.IsCompleted())
            {
                return task;
            }

            return new Task(new AttachExternalCancellationSource(task, cancellationToken), 0);
        }

        /// <summary>
        /// Ignore task result when cancel raised first.
        /// </summary>
        public static Task<T> AttachExternalCancellation<T>(this Task<T> task, CancellationToken cancellationToken)
        {
            if (!cancellationToken.CanBeCanceled)
            {
                return task;
            }

            if (cancellationToken.IsCancellationRequested)
            {
                task.Forget();
                return Task.FromCanceled<T>(cancellationToken);
            }

            if (task.Status.IsCompleted())
            {
                return task;
            }

            return new Task<T>(new AttachExternalCancellationSource<T>(task, cancellationToken), 0);
        }

        sealed class AttachExternalCancellationSource : IUniTaskSource
        {
            static readonly Action<object> cancellationCallbackDelegate = CancellationCallback;

            CancellationToken cancellationToken;
            CancellationTokenRegistration tokenRegistration;
            UniTaskCompletionSourceCore<AsyncUnit> core;

            public AttachExternalCancellationSource(Task task, CancellationToken cancellationToken)
            {
                this.cancellationToken = cancellationToken;
                this.tokenRegistration = cancellationToken.RegisterWithoutCaptureExecutionContext(cancellationCallbackDelegate, this);
                RunTask(task).Forget();
            }

            async TaskVoid RunTask(Task task)
            {
                try
                {
                    await task;
                    core.TrySetResult(AsyncUnit.Default);
                }
                catch (Exception ex)
                {
                    core.TrySetException(ex);
                }
                finally
                {
                    tokenRegistration.Dispose();
                }
            }

            static void CancellationCallback(object state)
            {
                var self = (AttachExternalCancellationSource)state;
                self.core.TrySetCanceled(self.cancellationToken);
            }

            public void GetResult(short token)
            {
                core.GetResult(token);
            }

            public UniTaskStatus GetStatus(short token)
            {
                return core.GetStatus(token);
            }

            public void OnCompleted(Action<object> continuation, object state, short token)
            {
                core.OnCompleted(continuation, state, token);
            }

            public UniTaskStatus UnsafeGetStatus()
            {
                return core.UnsafeGetStatus();
            }
        }

        sealed class AttachExternalCancellationSource<T> : IUniTaskSource<T>
        {
            static readonly Action<object> cancellationCallbackDelegate = CancellationCallback;

            CancellationToken cancellationToken;
            CancellationTokenRegistration tokenRegistration;
            UniTaskCompletionSourceCore<T> core;

            public AttachExternalCancellationSource(Task<T> task, CancellationToken cancellationToken)
            {
                this.cancellationToken = cancellationToken;
                this.tokenRegistration = cancellationToken.RegisterWithoutCaptureExecutionContext(cancellationCallbackDelegate, this);
                RunTask(task).Forget();
            }

            async TaskVoid RunTask(Task<T> task)
            {
                try
                {
                    core.TrySetResult(await task);
                }
                catch (Exception ex)
                {
                    core.TrySetException(ex);
                }
                finally
                {
                    tokenRegistration.Dispose();
                }
            }

            static void CancellationCallback(object state)
            {
                var self = (AttachExternalCancellationSource<T>)state;
                self.core.TrySetCanceled(self.cancellationToken);
            }

            void IUniTaskSource.GetResult(short token)
            {
                core.GetResult(token);
            }

            public T GetResult(short token)
            {
                return core.GetResult(token);
            }

            public UniTaskStatus GetStatus(short token)
            {
                return core.GetStatus(token);
            }

            public void OnCompleted(Action<object> continuation, object state, short token)
            {
                core.OnCompleted(continuation, state, token);
            }

            public UniTaskStatus UnsafeGetStatus()
            {
                return core.UnsafeGetStatus();
            }
        }

#if UNITY_2018_3_OR_NEWER

        public static IEnumerator ToCoroutine<T>(this Task<T> task, Action<T> resultHandler = null, Action<Exception> exceptionHandler = null)
        {
            return new ToCoroutineEnumerator<T>(task, resultHandler, exceptionHandler);
        }

        public static IEnumerator ToCoroutine(this Task task, Action<Exception> exceptionHandler = null)
        {
            return new ToCoroutineEnumerator(task, exceptionHandler);
        }

        public static async Task Timeout(this Task task, TimeSpan timeout, DelayType delayType = DelayType.DeltaTime, PlayerLoopTiming timeoutCheckTiming = PlayerLoopTiming.Update, CancellationTokenSource taskCancellationTokenSource = null)
        {
            var delayCancellationTokenSource = new CancellationTokenSource();
            var timeoutTask = Task.Delay(timeout, delayType, timeoutCheckTiming, delayCancellationTokenSource.Token).SuppressCancellationThrow();

            int winArgIndex;
            bool taskResultIsCanceled;
            try
            {
                (winArgIndex, taskResultIsCanceled, _) = await Task.WhenAny(task.SuppressCancellationThrow(), timeoutTask);
            }
            catch
            {
                delayCancellationTokenSource.Cancel();
                delayCancellationTokenSource.Dispose();
                throw;
            }

            // timeout
            if (winArgIndex == 1)
            {
                if (taskCancellationTokenSource != null)
                {
                    taskCancellationTokenSource.Cancel();
                    taskCancellationTokenSource.Dispose();
                }

                throw new TimeoutException("Exceed Timeout:" + timeout);
            }
            else
            {
                delayCancellationTokenSource.Cancel();
                delayCancellationTokenSource.Dispose();
            }

            if (taskResultIsCanceled)
            {
                Error.ThrowOperationCanceledException();
            }
        }

        public static async Task<T> Timeout<T>(this Task<T> task, TimeSpan timeout, DelayType delayType = DelayType.DeltaTime, PlayerLoopTiming timeoutCheckTiming = PlayerLoopTiming.Update, CancellationTokenSource taskCancellationTokenSource = null)
        {
            var delayCancellationTokenSource = new CancellationTokenSource();
            var timeoutTask = Task.Delay(timeout, delayType, timeoutCheckTiming, delayCancellationTokenSource.Token).SuppressCancellationThrow();

            int winArgIndex;
            (bool IsCanceled, T Result) taskResult;
            try
            {
                (winArgIndex, taskResult, _) = await Task.WhenAny(task.SuppressCancellationThrow(), timeoutTask);
            }
            catch
            {
                delayCancellationTokenSource.Cancel();
                delayCancellationTokenSource.Dispose();
                throw;
            }

            // timeout
            if (winArgIndex == 1)
            {
                if (taskCancellationTokenSource != null)
                {
                    taskCancellationTokenSource.Cancel();
                    taskCancellationTokenSource.Dispose();
                }

                throw new TimeoutException("Exceed Timeout:" + timeout);
            }
            else
            {
                delayCancellationTokenSource.Cancel();
                delayCancellationTokenSource.Dispose();
            }

            if (taskResult.IsCanceled)
            {
                Error.ThrowOperationCanceledException();
            }

            return taskResult.Result;
        }

        /// <summary>
        /// Timeout with suppress OperationCanceledException. Returns (bool, IsCanceled).
        /// </summary>
        public static async Task<bool> TimeoutWithoutException(this Task task, TimeSpan timeout, DelayType delayType = DelayType.DeltaTime, PlayerLoopTiming timeoutCheckTiming = PlayerLoopTiming.Update, CancellationTokenSource taskCancellationTokenSource = null)
        {
            var delayCancellationTokenSource = new CancellationTokenSource();
            var timeoutTask = Task.Delay(timeout, delayType, timeoutCheckTiming, delayCancellationTokenSource.Token).SuppressCancellationThrow();

            int winArgIndex;
            bool taskResultIsCanceled;
            try
            {
                (winArgIndex, taskResultIsCanceled, _) = await Task.WhenAny(task.SuppressCancellationThrow(), timeoutTask);
            }
            catch
            {
                delayCancellationTokenSource.Cancel();
                delayCancellationTokenSource.Dispose();
                return true;
            }

            // timeout
            if (winArgIndex == 1)
            {
                if (taskCancellationTokenSource != null)
                {
                    taskCancellationTokenSource.Cancel();
                    taskCancellationTokenSource.Dispose();
                }

                return true;
            }
            else
            {
                delayCancellationTokenSource.Cancel();
                delayCancellationTokenSource.Dispose();
            }

            if (taskResultIsCanceled)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Timeout with suppress OperationCanceledException. Returns (bool IsTimeout, T Result).
        /// </summary>
        public static async Task<(bool IsTimeout, T Result)> TimeoutWithoutException<T>(this Task<T> task, TimeSpan timeout, DelayType delayType = DelayType.DeltaTime, PlayerLoopTiming timeoutCheckTiming = PlayerLoopTiming.Update, CancellationTokenSource taskCancellationTokenSource = null)
        {
            var delayCancellationTokenSource = new CancellationTokenSource();
            var timeoutTask = Task.Delay(timeout, delayType, timeoutCheckTiming, delayCancellationTokenSource.Token).SuppressCancellationThrow();

            int winArgIndex;
            (bool IsCanceled, T Result) taskResult;
            try
            {
                (winArgIndex, taskResult, _) = await Task.WhenAny(task.SuppressCancellationThrow(), timeoutTask);
            }
            catch
            {
                delayCancellationTokenSource.Cancel();
                delayCancellationTokenSource.Dispose();
                return (true, default);
            }

            // timeout
            if (winArgIndex == 1)
            {
                if (taskCancellationTokenSource != null)
                {
                    taskCancellationTokenSource.Cancel();
                    taskCancellationTokenSource.Dispose();
                }

                return (true, default);
            }
            else
            {
                delayCancellationTokenSource.Cancel();
                delayCancellationTokenSource.Dispose();
            }

            if (taskResult.IsCanceled)
            {
                return (true, default);
            }

            return (false, taskResult.Result);
        }

#endif

        public static void Forget(this Task task)
        {
            var awaiter = task.GetAwaiter();
            if (awaiter.IsCompleted)
            {
                try
                {
                    awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    UniTaskScheduler.PublishUnobservedTaskException(ex);
                }
            }
            else
            {
                awaiter.SourceOnCompleted(state =>
                {
                    using (var t = (StateTuple<Task.Awaiter>)state)
                    {
                        try
                        {
                            t.Item1.GetResult();
                        }
                        catch (Exception ex)
                        {
                            UniTaskScheduler.PublishUnobservedTaskException(ex);
                        }
                    }
                }, StateTuple.Create(awaiter));
            }
        }

        public static void Forget(this Task task, Action<Exception> exceptionHandler, bool handleExceptionOnMainThread = true)
        {
            if (exceptionHandler == null)
            {
                Forget(task);
            }
            else
            {
                ForgetCoreWithCatch(task, exceptionHandler, handleExceptionOnMainThread).Forget();
            }
        }

        static async TaskVoid ForgetCoreWithCatch(Task task, Action<Exception> exceptionHandler, bool handleExceptionOnMainThread)
        {
            try
            {
                await task;
            }
            catch (Exception ex)
            {
                try
                {
                    if (handleExceptionOnMainThread)
                    {
#if UNITY_2018_3_OR_NEWER
                        await Task.SwitchToMainThread();
#endif
                    }
                    exceptionHandler(ex);
                }
                catch (Exception ex2)
                {
                    UniTaskScheduler.PublishUnobservedTaskException(ex2);
                }
            }
        }

        public static void Forget<T>(this Task<T> task)
        {
            var awaiter = task.GetAwaiter();
            if (awaiter.IsCompleted)
            {
                try
                {
                    awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    UniTaskScheduler.PublishUnobservedTaskException(ex);
                }
            }
            else
            {
                awaiter.SourceOnCompleted(state =>
                {
                    using (var t = (StateTuple<Task<T>.Awaiter>)state)
                    {
                        try
                        {
                            t.Item1.GetResult();
                        }
                        catch (Exception ex)
                        {
                            UniTaskScheduler.PublishUnobservedTaskException(ex);
                        }
                    }
                }, StateTuple.Create(awaiter));
            }
        }

        public static void Forget<T>(this Task<T> task, Action<Exception> exceptionHandler, bool handleExceptionOnMainThread = true)
        {
            if (exceptionHandler == null)
            {
                task.Forget();
            }
            else
            {
                ForgetCoreWithCatch(task, exceptionHandler, handleExceptionOnMainThread).Forget();
            }
        }

        static async TaskVoid ForgetCoreWithCatch<T>(Task<T> task, Action<Exception> exceptionHandler, bool handleExceptionOnMainThread)
        {
            try
            {
                await task;
            }
            catch (Exception ex)
            {
                try
                {
                    if (handleExceptionOnMainThread)
                    {
#if UNITY_2018_3_OR_NEWER
                        await Task.SwitchToMainThread();
#endif
                    }
                    exceptionHandler(ex);
                }
                catch (Exception ex2)
                {
                    UniTaskScheduler.PublishUnobservedTaskException(ex2);
                }
            }
        }

        public static async Task ContinueWith<T>(this Task<T> task, Action<T> continuationFunction)
        {
            continuationFunction(await task);
        }

        public static async Task ContinueWith<T>(this Task<T> task, Func<T, Task> continuationFunction)
        {
            await continuationFunction(await task);
        }

        public static async Task<TR> ContinueWith<T, TR>(this Task<T> task, Func<T, TR> continuationFunction)
        {
            return continuationFunction(await task);
        }

        public static async Task<TR> ContinueWith<T, TR>(this Task<T> task, Func<T, Task<TR>> continuationFunction)
        {
            return await continuationFunction(await task);
        }

        public static async Task ContinueWith(this Task task, Action continuationFunction)
        {
            await task;
            continuationFunction();
        }

        public static async Task ContinueWith(this Task task, Func<Task> continuationFunction)
        {
            await task;
            await continuationFunction();
        }

        public static async Task<T> ContinueWith<T>(this Task task, Func<T> continuationFunction)
        {
            await task;
            return continuationFunction();
        }

        public static async Task<T> ContinueWith<T>(this Task task, Func<Task<T>> continuationFunction)
        {
            await task;
            return await continuationFunction();
        }

        public static async Task<T> Unwrap<T>(this Task<Task<T>> task)
        {
            return await await task;
        }

        public static async Task Unwrap(this Task<Task> task)
        {
            await await task;
        }

        public static async Task<T> Unwrap<T>(this System.Threading.Tasks.Task<Task<T>> task)
        {
            return await await task;
        }

        public static async Task<T> Unwrap<T>(this System.Threading.Tasks.Task<Task<T>> task, bool continueOnCapturedContext)
        {
            return await await task.ConfigureAwait(continueOnCapturedContext);
        }

        public static async Task Unwrap(this System.Threading.Tasks.Task<Task> task)
        {
            await await task;
        }

        public static async Task Unwrap(this System.Threading.Tasks.Task<Task> task, bool continueOnCapturedContext)
        {
            await await task.ConfigureAwait(continueOnCapturedContext);
        }

        public static async Task<T> Unwrap<T>(this Task<System.Threading.Tasks.Task<T>> task)
        {
            return await await task;
        }

        public static async Task<T> Unwrap<T>(this Task<System.Threading.Tasks.Task<T>> task, bool continueOnCapturedContext)
        {
            return await (await task).ConfigureAwait(continueOnCapturedContext);
        }

        public static async Task Unwrap(this Task<System.Threading.Tasks.Task> task)
        {
            await await task;
        }

        public static async Task Unwrap(this Task<System.Threading.Tasks.Task> task, bool continueOnCapturedContext)
        {
            await (await task).ConfigureAwait(continueOnCapturedContext);
        }

#if UNITY_2018_3_OR_NEWER

        sealed class ToCoroutineEnumerator : IEnumerator
        {
            bool completed;
            Task task;
            Action<Exception> exceptionHandler = null;
            bool isStarted = false;
            ExceptionDispatchInfo exception;

            public ToCoroutineEnumerator(Task task, Action<Exception> exceptionHandler)
            {
                completed = false;
                this.exceptionHandler = exceptionHandler;
                this.task = task;
            }

            async TaskVoid RunTask(Task task)
            {
                try
                {
                    await task;
                }
                catch (Exception ex)
                {
                    if (exceptionHandler != null)
                    {
                        exceptionHandler(ex);
                    }
                    else
                    {
                        this.exception = ExceptionDispatchInfo.Capture(ex);
                    }
                }
                finally
                {
                    completed = true;
                }
            }

            public object Current => null;

            public bool MoveNext()
            {
                if (!isStarted)
                {
                    isStarted = true;
                    RunTask(task).Forget();
                }

                if (exception != null)
                {
                    exception.Throw();
                    return false;
                }

                return !completed;
            }

            void IEnumerator.Reset()
            {
            }
        }

        sealed class ToCoroutineEnumerator<T> : IEnumerator
        {
            bool completed;
            Action<T> resultHandler = null;
            Action<Exception> exceptionHandler = null;
            bool isStarted = false;
            Task<T> task;
            object current = null;
            ExceptionDispatchInfo exception;

            public ToCoroutineEnumerator(Task<T> task, Action<T> resultHandler, Action<Exception> exceptionHandler)
            {
                completed = false;
                this.task = task;
                this.resultHandler = resultHandler;
                this.exceptionHandler = exceptionHandler;
            }

            async TaskVoid RunTask(Task<T> task)
            {
                try
                {
                    var value = await task;
                    current = value; // boxed if T is struct...
                    if (resultHandler != null)
                    {
                        resultHandler(value);
                    }
                }
                catch (Exception ex)
                {
                    if (exceptionHandler != null)
                    {
                        exceptionHandler(ex);
                    }
                    else
                    {
                        this.exception = ExceptionDispatchInfo.Capture(ex);
                    }
                }
                finally
                {
                    completed = true;
                }
            }

            public object Current => current;

            public bool MoveNext()
            {
                if (!isStarted)
                {
                    isStarted = true;
                    RunTask(task).Forget();
                }

                if (exception != null)
                {
                    exception.Throw();
                    return false;
                }

                return !completed;
            }

            void IEnumerator.Reset()
            {
            }
        }

#endif
    }
}

