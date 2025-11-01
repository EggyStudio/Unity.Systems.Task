#if UNITY_2023_1_OR_NEWER
namespace UnityEssentials
{
    public static class UnityAwaitableExtensions
    {
        public static async Task AsUniTask(this UnityEngine.Awaitable awaitable)
        {
            await awaitable;
        }
        
        public static async Task<T> AsUniTask<T>(this UnityEngine.Awaitable<T> awaitable)
        {
            return await awaitable;
        }
    }
}
#endif
