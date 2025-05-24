#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using UnityEssentials.Threading.Tasks.Internal;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Threading;

namespace UnityEssentials.Threading.Tasks
{
    public partial struct Task
    {
        static readonly Task CanceledUniTask = new Func<Task>(() =>
        {
            return new Task(new CanceledResultSource(CancellationToken.None), 0);
        })();

        static class CanceledUniTaskCache<T>
        {
            public static readonly Task<T> Task;

            static CanceledUniTaskCache()
            {
                Task = new Task<T>(new CanceledResultSource<T>(CancellationToken.None), 0);
            }
        }

        public static readonly Task CompletedTask = new Task();

        public static Task FromException(Exception ex)
        {
            if (ex is OperationCanceledException oce)
            {
                return FromCanceled(oce.CancellationToken);
            }

            return new Task(new ExceptionResultSource(ex), 0);
        }

        public static Task<T> FromException<T>(Exception ex)
        {
            if (ex is OperationCanceledException oce)
            {
                return FromCanceled<T>(oce.CancellationToken);
            }

            return new Task<T>(new ExceptionResultSource<T>(ex), 0);
        }

        public static Task<T> FromResult<T>(T value)
        {
            return new Task<T>(value);
        }

        public static Task FromCanceled(CancellationToken cancellationToken = default)
        {
            if (cancellationToken == CancellationToken.None)
            {
                return CanceledUniTask;
            }
            else
            {
                return new Task(new CanceledResultSource(cancellationToken), 0);
            }
        }

        public static Task<T> FromCanceled<T>(CancellationToken cancellationToken = default)
        {
            if (cancellationToken == CancellationToken.None)
            {
                return CanceledUniTaskCache<T>.Task;
            }
            else
            {
                return new Task<T>(new CanceledResultSource<T>(cancellationToken), 0);
            }
        }

        public static Task Create(Func<Task> factory)
        {
            return factory();
        }

        public static Task Create(Func<CancellationToken, Task> factory, CancellationToken cancellationToken)
        {
            return factory(cancellationToken);
        }

        public static Task Create<T>(T state, Func<T, Task> factory)
        {
            return factory(state);
        }

        public static Task<T> Create<T>(Func<Task<T>> factory)
        {
            return factory();
        }

        public static AsyncLazy Lazy(Func<Task> factory)
        {
            return new AsyncLazy(factory);
        }

        public static AsyncLazy<T> Lazy<T>(Func<Task<T>> factory)
        {
            return new AsyncLazy<T>(factory);
        }

        /// <summary>
        /// helper of fire and forget void action.
        /// </summary>
        public static void Void(Func<TaskVoid> asyncAction)
        {
            asyncAction().Forget();
        }

        /// <summary>
        /// helper of fire and forget void action.
        /// </summary>
        public static void Void(Func<CancellationToken, TaskVoid> asyncAction, CancellationToken cancellationToken)
        {
            asyncAction(cancellationToken).Forget();
        }

        /// <summary>
        /// helper of fire and forget void action.
        /// </summary>
        public static void Void<T>(Func<T, TaskVoid> asyncAction, T state)
        {
            asyncAction(state).Forget();
        }

        /// <summary>
        /// helper of create add UniTaskVoid to delegate.
        /// For example: FooAction = UniTask.Action(async () => { /* */ })
        /// </summary>
        public static Action Action(Func<TaskVoid> asyncAction)
        {
            return () => asyncAction().Forget();
        }

        /// <summary>
        /// helper of create add UniTaskVoid to delegate.
        /// </summary>
        public static Action Action(Func<CancellationToken, TaskVoid> asyncAction, CancellationToken cancellationToken)
        {
            return () => asyncAction(cancellationToken).Forget();
        }

        /// <summary>
        /// helper of create add UniTaskVoid to delegate.
        /// </summary>
        public static Action Action<T>(T state, Func<T, TaskVoid> asyncAction)
        {
            return () => asyncAction(state).Forget();
        }

#if UNITY_2018_3_OR_NEWER

        /// <summary>
        /// Create async void(UniTaskVoid) UnityAction.
        /// For example: onClick.AddListener(UniTask.UnityAction(async () => { /* */ } ))
        /// </summary>
        public static UnityEngine.Events.UnityAction UnityAction(Func<TaskVoid> asyncAction)
        {
            return () => asyncAction().Forget();
        }

        /// <summary>
        /// Create async void(UniTaskVoid) UnityAction.
        /// For example: onClick.AddListener(UniTask.UnityAction(FooAsync, this.GetCancellationTokenOnDestroy()))
        /// </summary>
        public static UnityEngine.Events.UnityAction UnityAction(Func<CancellationToken, TaskVoid> asyncAction, CancellationToken cancellationToken)
        {
            return () => asyncAction(cancellationToken).Forget();
        }

        /// <summary>
        /// Create async void(UniTaskVoid) UnityAction.
        /// For example: onClick.AddListener(UniTask.UnityAction(FooAsync, Argument))
        /// </summary>
        public static UnityEngine.Events.UnityAction UnityAction<T>(T state, Func<T, TaskVoid> asyncAction)
        {
            return () => asyncAction(state).Forget();
        }

        /// <summary>
        /// Create async void(UniTaskVoid) UnityAction.
        /// For example: onClick.AddListener(UniTask.UnityAction(async (T arg) => { /* */ } ))
        /// </summary>
        public static UnityEngine.Events.UnityAction<T> UnityAction<T>(Func<T, TaskVoid> asyncAction)
        {
            return (arg) => asyncAction(arg).Forget();
        }

        /// <summary>
        /// Create async void(UniTaskVoid) UnityAction.
        /// For example: onClick.AddListener(UniTask.UnityAction(async (T0 arg0, T1 arg1) => { /* */ } ))
        /// </summary>
        public static UnityEngine.Events.UnityAction<T0, T1> UnityAction<T0, T1>(Func<T0, T1, TaskVoid> asyncAction)
        {
            return (arg0, arg1) => asyncAction(arg0, arg1).Forget();
        }

        /// <summary>
        /// Create async void(UniTaskVoid) UnityAction.
        /// For example: onClick.AddListener(UniTask.UnityAction(async (T0 arg0, T1 arg1, T2 arg2) => { /* */ } ))
        /// </summary>
        public static UnityEngine.Events.UnityAction<T0, T1, T2> UnityAction<T0, T1, T2>(Func<T0, T1, T2, TaskVoid> asyncAction)
        {
            return (arg0, arg1, arg2) => asyncAction(arg0, arg1, arg2).Forget();
        }

        /// <summary>
        /// Create async void(UniTaskVoid) UnityAction.
        /// For example: onClick.AddListener(UniTask.UnityAction(async (T0 arg0, T1 arg1, T2 arg2, T3 arg3) => { /* */ } ))
        /// </summary>
        public static UnityEngine.Events.UnityAction<T0, T1, T2, T3> UnityAction<T0, T1, T2, T3>(Func<T0, T1, T2, T3, TaskVoid> asyncAction)
        {
            return (arg0, arg1, arg2, arg3) => asyncAction(arg0, arg1, arg2, arg3).Forget();
        }

        // <summary>
        /// Create async void(UniTaskVoid) UnityAction.
        /// For example: onClick.AddListener(UniTask.UnityAction(async (T arg, CancellationToken cancellationToken) => { /* */ } ))
        /// </summary>
        public static UnityEngine.Events.UnityAction<T> UnityAction<T>(Func<T, CancellationToken, TaskVoid> asyncAction, CancellationToken cancellationToken)
        {
            return (arg) => asyncAction(arg, cancellationToken).Forget();
        }

        /// <summary>
        /// Create async void(UniTaskVoid) UnityAction.
        /// For example: onClick.AddListener(UniTask.UnityAction(async (T0 arg0, T1 arg1, CancellationToken cancellationToken) => { /* */ } ))
        /// </summary>
        public static UnityEngine.Events.UnityAction<T0, T1> UnityAction<T0, T1>(Func<T0, T1, CancellationToken, TaskVoid> asyncAction, CancellationToken cancellationToken)
        {
            return (arg0, arg1) => asyncAction(arg0, arg1, cancellationToken).Forget();
        }

        /// <summary>
        /// Create async void(UniTaskVoid) UnityAction.
        /// For example: onClick.AddListener(UniTask.UnityAction(async (T0 arg0, T1 arg1, T2 arg2, CancellationToken cancellationToken) => { /* */ } ))
        /// </summary>
        public static UnityEngine.Events.UnityAction<T0, T1, T2> UnityAction<T0, T1, T2>(Func<T0, T1, T2, CancellationToken, TaskVoid> asyncAction, CancellationToken cancellationToken)
        {
            return (arg0, arg1, arg2) => asyncAction(arg0, arg1, arg2, cancellationToken).Forget();
        }

        /// <summary>
        /// Create async void(UniTaskVoid) UnityAction.
        /// For example: onClick.AddListener(UniTask.UnityAction(async (T0 arg0, T1 arg1, T2 arg2, T3 arg3, CancellationToken cancellationToken) => { /* */ } ))
        /// </summary>
        public static UnityEngine.Events.UnityAction<T0, T1, T2, T3> UnityAction<T0, T1, T2, T3>(Func<T0, T1, T2, T3, CancellationToken, TaskVoid> asyncAction, CancellationToken cancellationToken)
        {
            return (arg0, arg1, arg2, arg3) => asyncAction(arg0, arg1, arg2, arg3, cancellationToken).Forget();
        }

#endif

        /// <summary>
        /// Defer the task creation just before call await.
        /// </summary>
        public static Task Defer(Func<Task> factory)
        {
            return new Task(new DeferPromise(factory), 0);
        }

        /// <summary>
        /// Defer the task creation just before call await.
        /// </summary>
        public static Task<T> Defer<T>(Func<Task<T>> factory)
        {
            return new Task<T>(new DeferPromise<T>(factory), 0);
        }

        /// <summary>
        /// Defer the task creation just before call await.
        /// </summary>
        public static Task Defer<TState>(TState state, Func<TState, Task> factory)
        {
            return new Task(new DeferPromiseWithState<TState>(state, factory), 0);
        }

        /// <summary>
        /// Defer the task creation just before call await.
        /// </summary>
        public static Task<TResult> Defer<TState, TResult>(TState state, Func<TState, Task<TResult>> factory)
        {
            return new Task<TResult>(new DeferPromiseWithState<TState, TResult>(state, factory), 0);
        }

        /// <summary>
        /// Never complete.
        /// </summary>
        public static Task Never(CancellationToken cancellationToken)
        {
            return new Task<AsyncUnit>(new NeverPromise<AsyncUnit>(cancellationToken), 0);
        }

        /// <summary>
        /// Never complete.
        /// </summary>
        public static Task<T> Never<T>(CancellationToken cancellationToken)
        {
            return new Task<T>(new NeverPromise<T>(cancellationToken), 0);
        }

        sealed class ExceptionResultSource : IUniTaskSource
        {
            readonly ExceptionDispatchInfo exception;
            bool calledGet;

            public ExceptionResultSource(Exception exception)
            {
                this.exception = ExceptionDispatchInfo.Capture(exception);
            }

            public void GetResult(short token)
            {
                if (!calledGet)
                {
                    calledGet = true;
                    GC.SuppressFinalize(this);
                }
                exception.Throw();
            }

            public UniTaskStatus GetStatus(short token)
            {
                return UniTaskStatus.Faulted;
            }

            public UniTaskStatus UnsafeGetStatus()
            {
                return UniTaskStatus.Faulted;
            }

            public void OnCompleted(Action<object> continuation, object state, short token)
            {
                continuation(state);
            }

            ~ExceptionResultSource()
            {
                if (!calledGet)
                {
                    UniTaskScheduler.PublishUnobservedTaskException(exception.SourceException);
                }
            }
        }

        sealed class ExceptionResultSource<T> : IUniTaskSource<T>
        {
            readonly ExceptionDispatchInfo exception;
            bool calledGet;

            public ExceptionResultSource(Exception exception)
            {
                this.exception = ExceptionDispatchInfo.Capture(exception);
            }

            public T GetResult(short token)
            {
                if (!calledGet)
                {
                    calledGet = true;
                    GC.SuppressFinalize(this);
                }
                exception.Throw();
                return default;
            }

            void IUniTaskSource.GetResult(short token)
            {
                if (!calledGet)
                {
                    calledGet = true;
                    GC.SuppressFinalize(this);
                }
                exception.Throw();
            }

            public UniTaskStatus GetStatus(short token)
            {
                return UniTaskStatus.Faulted;
            }

            public UniTaskStatus UnsafeGetStatus()
            {
                return UniTaskStatus.Faulted;
            }

            public void OnCompleted(Action<object> continuation, object state, short token)
            {
                continuation(state);
            }

            ~ExceptionResultSource()
            {
                if (!calledGet)
                {
                    UniTaskScheduler.PublishUnobservedTaskException(exception.SourceException);
                }
            }
        }

        sealed class CanceledResultSource : IUniTaskSource
        {
            readonly CancellationToken cancellationToken;

            public CanceledResultSource(CancellationToken cancellationToken)
            {
                this.cancellationToken = cancellationToken;
            }

            public void GetResult(short token)
            {
                throw new OperationCanceledException(cancellationToken);
            }

            public UniTaskStatus GetStatus(short token)
            {
                return UniTaskStatus.Canceled;
            }

            public UniTaskStatus UnsafeGetStatus()
            {
                return UniTaskStatus.Canceled;
            }

            public void OnCompleted(Action<object> continuation, object state, short token)
            {
                continuation(state);
            }
        }

        sealed class CanceledResultSource<T> : IUniTaskSource<T>
        {
            readonly CancellationToken cancellationToken;

            public CanceledResultSource(CancellationToken cancellationToken)
            {
                this.cancellationToken = cancellationToken;
            }

            public T GetResult(short token)
            {
                throw new OperationCanceledException(cancellationToken);
            }

            void IUniTaskSource.GetResult(short token)
            {
                throw new OperationCanceledException(cancellationToken);
            }

            public UniTaskStatus GetStatus(short token)
            {
                return UniTaskStatus.Canceled;
            }

            public UniTaskStatus UnsafeGetStatus()
            {
                return UniTaskStatus.Canceled;
            }

            public void OnCompleted(Action<object> continuation, object state, short token)
            {
                continuation(state);
            }
        }

        sealed class DeferPromise : IUniTaskSource
        {
            Func<Task> factory;
            Task task;
            Task.Awaiter awaiter;

            public DeferPromise(Func<Task> factory)
            {
                this.factory = factory;
            }

            public void GetResult(short token)
            {
                awaiter.GetResult();
            }

            public UniTaskStatus GetStatus(short token)
            {
                var f = Interlocked.Exchange(ref factory, null);
                if (f != null)
                {
                    task = f();
                    awaiter = task.GetAwaiter();
                }

                return task.Status;
            }

            public void OnCompleted(Action<object> continuation, object state, short token)
            {
                awaiter.SourceOnCompleted(continuation, state);
            }

            public UniTaskStatus UnsafeGetStatus()
            {
                return task.Status;
            }
        }

        sealed class DeferPromise<T> : IUniTaskSource<T>
        {
            Func<Task<T>> factory;
            Task<T> task;
            Task<T>.Awaiter awaiter;

            public DeferPromise(Func<Task<T>> factory)
            {
                this.factory = factory;
            }

            public T GetResult(short token)
            {
                return awaiter.GetResult();
            }

            void IUniTaskSource.GetResult(short token)
            {
                awaiter.GetResult();
            }

            public UniTaskStatus GetStatus(short token)
            {
                var f = Interlocked.Exchange(ref factory, null);
                if (f != null)
                {
                    task = f();
                    awaiter = task.GetAwaiter();
                }

                return task.Status;
            }

            public void OnCompleted(Action<object> continuation, object state, short token)
            {
                awaiter.SourceOnCompleted(continuation, state);
            }

            public UniTaskStatus UnsafeGetStatus()
            {
                return task.Status;
            }
        }

        sealed class DeferPromiseWithState<TState> : IUniTaskSource
        {
            Func<TState, Task> factory;
            TState argument;
            Task task;
            Task.Awaiter awaiter;

            public DeferPromiseWithState(TState argument, Func<TState, Task> factory)
            {
                this.argument = argument;
                this.factory = factory;
            }

            public void GetResult(short token)
            {
                awaiter.GetResult();
            }

            public UniTaskStatus GetStatus(short token)
            {
                var f = Interlocked.Exchange(ref factory, null);
                if (f != null)
                {
                    task = f(argument);
                    awaiter = task.GetAwaiter();
                }

                return task.Status;
            }

            public void OnCompleted(Action<object> continuation, object state, short token)
            {
                awaiter.SourceOnCompleted(continuation, state);
            }

            public UniTaskStatus UnsafeGetStatus()
            {
                return task.Status;
            }
        }

        sealed class DeferPromiseWithState<TState, TResult> : IUniTaskSource<TResult>
        {
            Func<TState, Task<TResult>> factory;
            TState argument;
            Task<TResult> task;
            Task<TResult>.Awaiter awaiter;

            public DeferPromiseWithState(TState argument, Func<TState, Task<TResult>> factory)
            {
                this.argument = argument;
                this.factory = factory;
            }

            public TResult GetResult(short token)
            {
                return awaiter.GetResult();
            }

            void IUniTaskSource.GetResult(short token)
            {
                awaiter.GetResult();
            }

            public UniTaskStatus GetStatus(short token)
            {
                var f = Interlocked.Exchange(ref factory, null);
                if (f != null)
                {
                    task = f(argument);
                    awaiter = task.GetAwaiter();
                }

                return task.Status;
            }

            public void OnCompleted(Action<object> continuation, object state, short token)
            {
                awaiter.SourceOnCompleted(continuation, state);
            }

            public UniTaskStatus UnsafeGetStatus()
            {
                return task.Status;
            }
        }

        sealed class NeverPromise<T> : IUniTaskSource<T>
        {
            static readonly Action<object> cancellationCallback = CancellationCallback;

            CancellationToken cancellationToken;
            UniTaskCompletionSourceCore<T> core;

            public NeverPromise(CancellationToken cancellationToken)
            {
                this.cancellationToken = cancellationToken;
                if (this.cancellationToken.CanBeCanceled)
                {
                    this.cancellationToken.RegisterWithoutCaptureExecutionContext(cancellationCallback, this);
                }
            }

            static void CancellationCallback(object state)
            {
                var self = (NeverPromise<T>)state;
                self.core.TrySetCanceled(self.cancellationToken);
            }

            public T GetResult(short token)
            {
                return core.GetResult(token);
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

            void IUniTaskSource.GetResult(short token)
            {
                core.GetResult(token);
            }
        }
    }

    internal static class CompletedTasks
    {
        public static readonly Task<AsyncUnit> AsyncUnit = Task.FromResult(UnityEssentials.Threading.Tasks.AsyncUnit.Default);
        public static readonly Task<bool> True = Task.FromResult(true);
        public static readonly Task<bool> False = Task.FromResult(false);
        public static readonly Task<int> Zero = Task.FromResult(0);
        public static readonly Task<int> MinusOne = Task.FromResult(-1);
        public static readonly Task<int> One = Task.FromResult(1);
    }
}
