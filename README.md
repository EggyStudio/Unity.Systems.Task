# Unity Essentials

This module is part of the Unity Essentials ecosystem and follows the same lightweight, editor-first approach.
Unity Essentials is a lightweight, modular set of editor utilities and helpers that streamline Unity development. It focuses on clean, dependency-free tools that work well together.

All utilities are under the `UnityEssentials` namespace.

```csharp
using UnityEssentials;
```

## Installation

Install the Unity Essentials entry package via Unity's Package Manager, then install modules from the Tools menu.

- Add the entry package (via Git URL)
    - Window → Package Manager
    - "+" → "Add package from git URL…"
    - Paste: `https://github.com/EggyStudio/UnityEssentials.git`

- Install or update Unity Essentials packages
    - Tools → Install & Update UnityEssentials
    - Install all or select individual modules; run again anytime to update

---

# Task (async/await for Unity)

> Quick overview: Zero‑allocation async/await tailored for Unity. Await Unity operations, coroutines, and frame/timer primitives using a lightweight `Task`/`Task<T>` implementation that runs on the PlayerLoop. Includes cancellation, progress, combinators, async streams, channels, and an Editor Task Tracker window.

This module is derived from [UniTask](https://github.com/Cysharp/UniTask) by Cysharp, which is distributed under the MIT License, with its types in the `UnityEssentials` namespace and `UniTask` renamed to `Task`. UniTask's copyright and permission notice is in [THIRD-PARTY-NOTICES.md](THIRD-PARTY-NOTICES.md). It provides familiar `async/await` patterns without the allocations of `System.Threading.Tasks.Task` and integrates naturally with Unity’s main thread and frame timing.

## Features
- Unity‑native task primitives
  - Struct `Task` / `Task<T>` with custom builders to avoid allocations
  - PlayerLoop‑based scheduling: no background threads required
- Await Unity operations and coroutines
  - `AsyncOperation`, `ResourceRequest`, `AssetBundle*`, `UnityWebRequest` (when enabled)
  - `IEnumerator` coroutines are awaitable; bridge helpers provided
- Frame/timer utilities
  - `Task.Yield()`, `Task.NextFrame()`, `Task.Delay(...)`, `Task.WaitForSeconds(...)`, `Task.WaitForFixedUpdate()`
  - `Task.WaitUntil(...)`, `Task.SwitchToMainThread()`, `Task.SwitchToThreadPool()`
- Coordination and async utilities
  - `Task.WhenAll(...)`, `Task.WhenAny(...)`, `Task.WhenEach(...)`
  - `AsyncReactiveProperty`, async enumerable extensions, Channels
- Cancellation and progress
  - Full `CancellationToken` support, optional immediate cancellation
  - Progress reporting for supported operations
- Debug tooling
  - Task Tracker window (Editor) to find leaks and long‑running tasks

## Requirements
- Unity 6000.0+
- Runtime module; no external dependencies for core features
- Optional: UnityWebRequest awaiters require corresponding compilation symbols or Unity versions where supported

## Usage
1) Await Unity operations and use frame utilities
```csharp
using UnityEngine;
using UnityEssentials;

public class Example : MonoBehaviour
{
    async void Start()
    {
        // Await a ResourceRequest
        var textAsset = await Resources.LoadAsync<TextAsset>("Data/MyText");

        // Await a frame and a delay
        await Task.Yield();
        await Task.WaitForSeconds(0.25f);

        // Combine async operations
        var (a, b) = await Task.WhenAll(
            Resources.LoadAsync<Texture2D>("Tex/A").ToUniTask(),
            Resources.LoadAsync<Texture2D>("Tex/B").ToUniTask()
        );

        Debug.Log($"Loaded: {textAsset}, {a}, {b}");
    }
}
```

2) Cancellation and progress
```csharp
using System.Threading;

var cts = new CancellationTokenSource();
var request = Resources.LoadAsync<TextAsset>("Data/Big");

// Report progress while awaiting
float last = 0f;
await request.ToUniTask(progress: new Progress<float>(p =>
{
    if (p - last > 0.05f) { Debug.Log($"Loading {p:P0}"); last = p; }
}), cancellationToken: cts.Token);
```

3) Editor Task Tracker
- Window → Task Tracker
- Enable Tracking and (optionally) StackTrace to diagnose leaked or long‑running tasks

## How It Works
- A lightweight task type (`Task`/`Task<T>`) integrates with Unity’s PlayerLoop to schedule continuations on specific phases (e.g., Update, LateUpdate, FixedUpdate)
- Unity async types (`AsyncOperation`, `ResourceRequest`, etc.) are wrapped with awaiters that complete back on the main thread
- Frame/time helpers (Yield/NextFrame/Delay/WaitForSeconds) are implemented via PlayerLoop items rather than coroutines
- Optional thread‑pool helpers allow off‑main‑thread work with `SwitchToThreadPool()` and easy return to the main thread

## Notes and Limitations
- Most APIs assume the main thread; do not use from background threads unless explicitly supported
- `WaitForEndOfFrame` requires a `MonoBehaviour` runner on Unity versions prior to 2023.1
- Enabling Task Tracker with stack traces has a higher runtime overhead; use for debugging only
- Some awaiters (e.g., UnityWebRequest) depend on Unity version or compile flags

## Files in This Package
- `Runtime/Task*.cs`, `Runtime/UniTask.*.cs`, `Runtime/UnityAsyncExtensions*.cs`, the core task type, awaiters and frame and timer utilities
- `Runtime/*`, cancellation helpers, PlayerLoop integration, async enumerables, channels and pooling
- `Editor/UniTaskTrackerWindow.cs`, the Task Tracker window (Window → Task Tracker)
- `Runtime/UnityEssentials.Systems.Task.asmdef`, `Editor/UnityEssentials.Systems.Task.Editor.asmdef`, the assembly definitions
- `Runtime/Linq`, the LINQ operators for async enumerables, and `Runtime/External/Addressables`, `Runtime/External/DOTween` and `Runtime/External/TextMeshPro`, the integrations whose code compiles when the matching package is installed
- `THIRD-PARTY-NOTICES.md`, UniTask's MIT License notice

## Tags
unity, async, await, unitask, playerloop, coroutine, cancellation, progress, task, channels, reactive, tooling
