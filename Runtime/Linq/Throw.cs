using UnityEssentials.Threading.Tasks.Internal;
using System;
using System.Threading;

namespace UnityEssentials.Threading.Tasks.Linq
{
    public static partial class UniTaskAsyncEnumerable
    {
        public static IUniTaskAsyncEnumerable<TValue> Throw<TValue>(Exception exception)
        {
            return new Throw<TValue>(exception);
        }
    }

    internal class Throw<TValue> : IUniTaskAsyncEnumerable<TValue>
    {
        readonly Exception exception;

        public Throw(Exception exception)
        {
            this.exception = exception;
        }

        public IUniTaskAsyncEnumerator<TValue> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            return new _Throw(exception, cancellationToken);
        }

        class _Throw : IUniTaskAsyncEnumerator<TValue>
        {
            readonly Exception exception;
            CancellationToken cancellationToken;

            public _Throw(Exception exception, CancellationToken cancellationToken)
            {
                this.exception = exception;
                this.cancellationToken = cancellationToken;
            }

            public TValue Current => default;

            public Task<bool> MoveNextAsync()
            {
                cancellationToken.ThrowIfCancellationRequested();
                return Task.FromException<bool>(exception);
            }

            public Task DisposeAsync()
            {
                return default;
            }
        }
    }
}