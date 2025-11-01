#pragma warning disable CS1591
#pragma warning disable CS0436

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;using UnityEssentials;

namespace UnityEssentials
{
    [AsyncMethodBuilder(typeof(AsyncUniTaskVoidMethodBuilder))]
    public readonly struct TaskVoid
    {
        public void Forget()
        {
        }
    }
}

