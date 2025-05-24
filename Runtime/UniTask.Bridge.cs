#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using System;
using System.Collections;

namespace UnityEssentials.Threading.Tasks
{
    // UnityEngine Bridges.

    public partial struct Task
    {
        public static IEnumerator ToCoroutine(Func<Task> taskFactory)
        {
            return taskFactory().ToCoroutine();
        }
    }
}

