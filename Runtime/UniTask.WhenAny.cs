#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using System;
using System.Collections.Generic;
using System.Threading;
using UnityEssentials;

namespace UnityEssentials
{
    public partial struct Task
    {
        public static Task<(bool hasResultLeft, T result)> WhenAny<T>(Task<T> leftTask, Task rightTask)
        {
            return new Task<(bool, T)>(new WhenAnyLRPromise<T>(leftTask, rightTask), 0);
        }

        public static Task<(int winArgumentIndex, T result)> WhenAny<T>(params Task<T>[] tasks)
        {
            return new Task<(int, T)>(new WhenAnyPromise<T>(tasks, tasks.Length), 0);
        }

        public static Task<(int winArgumentIndex, T result)> WhenAny<T>(IEnumerable<Task<T>> tasks)
        {
            using (var span = ArrayPoolUtil.Materialize(tasks))
            {
                return new Task<(int, T)>(new WhenAnyPromise<T>(span.Array, span.Length), 0);
            }
        }

        /// <summary>Return value is winArgumentIndex</summary>
        public static Task<int> WhenAny(params Task[] tasks)
        {
            return new Task<int>(new WhenAnyPromise(tasks, tasks.Length), 0);
        }

        /// <summary>Return value is winArgumentIndex</summary>
        public static Task<int> WhenAny(IEnumerable<Task> tasks)
        {
            using (var span = ArrayPoolUtil.Materialize(tasks))
            {
                return new Task<int>(new WhenAnyPromise(span.Array, span.Length), 0);
            }
        }

        sealed class WhenAnyLRPromise<T> : IUniTaskSource<(bool, T)>
        {
            int completedCount;
            UniTaskCompletionSourceCore<(bool, T)> core;

            public WhenAnyLRPromise(Task<T> leftTask, Task rightTask)
            {
                TaskTracker.TrackActiveTask(this, 3);

                {
                    Task<T>.Awaiter awaiter;
                    try
                    {
                        awaiter = leftTask.GetAwaiter();
                    }
                    catch (Exception ex)
                    {
                        core.TrySetException(ex);
                        goto RIGHT;
                    }

                    if (awaiter.IsCompleted)
                    {
                        TryLeftInvokeContinuation(this, awaiter);
                    }
                    else
                    {
                        awaiter.SourceOnCompleted(state =>
                        {
                            using (var t = (StateTuple<WhenAnyLRPromise<T>, Task<T>.Awaiter>)state)
                            {
                                TryLeftInvokeContinuation(t.Item1, t.Item2);
                            }
                        }, StateTuple.Create(this, awaiter));
                    }
                }
                RIGHT:
                {
                    Task.Awaiter awaiter;
                    try
                    {
                        awaiter = rightTask.GetAwaiter();
                    }
                    catch (Exception ex)
                    {
                        core.TrySetException(ex);
                        return;
                    }

                    if (awaiter.IsCompleted)
                    {
                        TryRightInvokeContinuation(this, awaiter);
                    }
                    else
                    {
                        awaiter.SourceOnCompleted(state =>
                        {
                            using (var t = (StateTuple<WhenAnyLRPromise<T>, Task.Awaiter>)state)
                            {
                                TryRightInvokeContinuation(t.Item1, t.Item2);
                            }
                        }, StateTuple.Create(this, awaiter));
                    }
                }
            }

            static void TryLeftInvokeContinuation(WhenAnyLRPromise<T> self, in Task<T>.Awaiter awaiter)
            {
                T result;
                try
                {
                    result = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 1)
                {
                    self.core.TrySetResult((true, result));
                }
            }

            static void TryRightInvokeContinuation(WhenAnyLRPromise<T> self, in Task.Awaiter awaiter)
            {
                try
                {
                    awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 1)
                {
                    self.core.TrySetResult((false, default));
                }
            }

            public (bool, T) GetResult(short token)
            {
                TaskTracker.RemoveTracking(this);
                GC.SuppressFinalize(this);
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

            void IUniTaskSource.GetResult(short token)
            {
                GetResult(token);
            }
        }


        sealed class WhenAnyPromise<T> : IUniTaskSource<(int, T)>
        {
            int completedCount;
            UniTaskCompletionSourceCore<(int, T)> core;

            public WhenAnyPromise(Task<T>[] tasks, int tasksLength)
            {
                if (tasksLength == 0)
                {
                    throw new ArgumentException("The tasks argument contains no tasks.");
                }

                TaskTracker.TrackActiveTask(this, 3);

                for (int i = 0; i < tasksLength; i++)
                {
                    Task<T>.Awaiter awaiter;
                    try
                    {
                        awaiter = tasks[i].GetAwaiter();
                    }
                    catch (Exception ex)
                    {
                        core.TrySetException(ex);
                        continue; // consume others.
                    }

                    if (awaiter.IsCompleted)
                    {
                        TryInvokeContinuation(this, awaiter, i);
                    }
                    else
                    {
                        awaiter.SourceOnCompleted(state =>
                        {
                            using (var t = (StateTuple<WhenAnyPromise<T>, Task<T>.Awaiter, int>)state)
                            {
                                TryInvokeContinuation(t.Item1, t.Item2, t.Item3);
                            }
                        }, StateTuple.Create(this, awaiter, i));
                    }
                }
            }

            static void TryInvokeContinuation(WhenAnyPromise<T> self, in Task<T>.Awaiter awaiter, int i)
            {
                T result;
                try
                {
                    result = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 1)
                {
                    self.core.TrySetResult((i, result));
                }
            }

            public (int, T) GetResult(short token)
            {
                TaskTracker.RemoveTracking(this);
                GC.SuppressFinalize(this);
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

            void IUniTaskSource.GetResult(short token)
            {
                GetResult(token);
            }
        }

        sealed class WhenAnyPromise : IUniTaskSource<int>
        {
            int completedCount;
            UniTaskCompletionSourceCore<int> core;

            public WhenAnyPromise(Task[] tasks, int tasksLength)
            {
                if (tasksLength == 0)
                {
                    throw new ArgumentException("The tasks argument contains no tasks.");
                }

                TaskTracker.TrackActiveTask(this, 3);

                for (int i = 0; i < tasksLength; i++)
                {
                    Task.Awaiter awaiter;
                    try
                    {
                        awaiter = tasks[i].GetAwaiter();
                    }
                    catch (Exception ex)
                    {
                        core.TrySetException(ex);
                        continue; // consume others.
                    }

                    if (awaiter.IsCompleted)
                    {
                        TryInvokeContinuation(this, awaiter, i);
                    }
                    else
                    {
                        awaiter.SourceOnCompleted(state =>
                        {
                            using (var t = (StateTuple<WhenAnyPromise, Task.Awaiter, int>)state)
                            {
                                TryInvokeContinuation(t.Item1, t.Item2, t.Item3);
                            }
                        }, StateTuple.Create(this, awaiter, i));
                    }
                }
            }

            static void TryInvokeContinuation(WhenAnyPromise self, in Task.Awaiter awaiter, int i)
            {
                try
                {
                    awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 1)
                {
                    self.core.TrySetResult(i);
                }
            }

            public int GetResult(short token)
            {
                TaskTracker.RemoveTracking(this);
                GC.SuppressFinalize(this);
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

            void IUniTaskSource.GetResult(short token)
            {
                GetResult(token);
            }
        }
    }
}

