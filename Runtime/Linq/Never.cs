using System.Threading;

namespace UnityEssentials
{
    public static partial class UniTaskAsyncEnumerable
    {
        public static IUniTaskAsyncEnumerable<T> Never<T>()
        {
            return UnityEssentials.Never<T>.Instance;
        }
    }

    internal class Never<T> : IUniTaskAsyncEnumerable<T>
    {
        public static readonly IUniTaskAsyncEnumerable<T> Instance = new Never<T>();

        Never()
        {
        }

        public IUniTaskAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            return new _Never(cancellationToken);
        }

        class _Never : IUniTaskAsyncEnumerator<T>
        {
            CancellationToken cancellationToken;

            public _Never(CancellationToken cancellationToken)
            {
                this.cancellationToken = cancellationToken;
            }

            public T Current => default;

            public Task<bool> MoveNextAsync()
            {
                var tcs = new UniTaskCompletionSource<bool>();

                cancellationToken.Register(state =>
                {
                    var task = (UniTaskCompletionSource<bool>)state;
                    task.TrySetCanceled(cancellationToken);
                }, tcs);

                return tcs.Task;
            }

            public Task DisposeAsync()
            {
                return default;
            }
        }
    }
}