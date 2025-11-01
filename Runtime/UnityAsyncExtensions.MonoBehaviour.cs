using System;
using System.Threading;

namespace UnityEssentials
{
    public static partial class UnityAsyncExtensions
    {
        public static Task StartAsyncCoroutine(this UnityEngine.MonoBehaviour monoBehaviour, Func<CancellationToken, Task> asyncCoroutine)
        {
            var token = monoBehaviour.GetCancellationTokenOnDestroy();
            return asyncCoroutine(token);
        }
    }
}