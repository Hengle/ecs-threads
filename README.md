[![gitter](https://img.shields.io/gitter/room/leopotam/ecs.svg)](https://gitter.im/leopotam/ecs)
[![license](https://img.shields.io/github/license/Leopotam/ecs-threads.svg)](https://github.com/Leopotam/ecs-threads/blob/develop/LICENSE)
# Multithreading extension for Entity Component System framework
Multithreading support for [ECS framework](https://github.com/Leopotam/ecs) - main goal of this extension.

> Tested on unity 2018.3 (not dependent on Unity engine) and contains assembly definition for compiling to separate assembly file for performance reason.

> Dependent on [ECS framework](https://github.com/Leopotam/ecs) - ECS framework should be imported first.

# Installation

## As unity module
This repository can be installed as unity module directly from git url. In this way new line should be added to `Packages/manifest.json`:
```
"com.leopotam.ecs-threads": "https://github.com/Leopotam/ecs-threads.git",
```
By default last released version will be used. If you need trunk / developing version then `develop` name of branch should be added after hash:
```
"com.leopotam.ecs-threads": "https://github.com/Leopotam/ecs-threads.git#develop",
```

## As source
If you can't / don't want to use unity modules, code can be downloaded as sources archive of required release from [Releases page](`https://github.com/Leopotam/ecs-threads/releases`).

# Systems

## EcsMultiThreadSystem
Multithreaded entities processing system, like "jobs" at unity engine, but not dependent on any engine.
```csharp
sealed class ThreadComponent {
    public float A = 1;
    public float B = 2;
    public float C = 3;
    public float D = 4;
    public float E = 5;
    public float F = 6;
    public float G = 7;
    public float H = 8;
    public float I = 9;
    public float J = 10;
    public float Result;
}
[EcsInject]
sealed class ThreadTestSystem : EcsMultiThreadSystem<EcsFilter<ThreadComponent>> {
    EcsWorld _world = null;
    EcsFilter<ThreadComponent> _filter = null;

    /// <summary>
    /// Returns filter for processing entities in it at background threads.
    /// </summary>
    protected override EcsFilter<ThreadComponent> GetFilter () {
        return _filter;
    }

    /// <summary>
    /// Returns minimal amount of entities for splitting to threads instead processing in one.
    /// </summary>
    protected override int GetMinJobSize () {
        return 1000;
    }

    /// <summary>
    /// Returns background threads amount. Main thread will be used as additional worker (+1 thread).
    /// </summary>
    protected override int GetThreadsCount () {
        return System.Environment.ProcessorCount - 1;
    }

    /// <summary>
    /// Returns our worker callback.
    /// </summary>
    protected override EcsMultiThreadWorker GetWorker () {
        return Worker;
    }

    /// <summary>
    /// Our worker callback for processing entities.
    /// Important: better to use static methods as workers - you cant touch any instance data without additional sync.
    /// </summary>
static void Worker (EcsMultiThreadWorkerDesc workerDesc) {
    foreach (var idx in workerDesc) {
        var c = workerDesc.Filter.Components1[idx];
        c.Result = (float) System.Math.Sqrt (c.A + c.B + c.C + c.D + c.E + c.F + c.G + c.H + c.I + c.J);
        c.Result = (float) System.Math.Sin (c.A + c.B + c.C + c.D + c.E + c.F + c.G + c.H + c.I + c.J);
        c.Result = (float) System.Math.Cos (c.A + c.B + c.C + c.D + c.E + c.F + c.G + c.H + c.I + c.J);
        c.Result = (float) System.Math.Tan (c.A + c.B + c.C + c.D + c.E + c.F + c.G + c.H + c.I + c.J);
        c.Result = (float) System.Math.Log10 (c.A + c.B + c.C + c.D + c.E + c.F + c.G + c.H + c.I + c.J);
        c.Result = (float) System.Math.Sqrt (c.A + c.B + c.C + c.D + c.E + c.F + c.G + c.H + c.I + c.J);
        c.Result = (float) System.Math.Sin (c.A + c.B + c.C + c.D + c.E + c.F + c.G + c.H + c.I + c.J);
        c.Result = (float) System.Math.Cos (c.A + c.B + c.C + c.D + c.E + c.F + c.G + c.H + c.I + c.J);
        c.Result = (float) System.Math.Tan (c.A + c.B + c.C + c.D + c.E + c.F + c.G + c.H + c.I + c.J);
        c.Result = (float) System.Math.Log10 (c.A + c.B + c.C + c.D + c.E + c.F + c.G + c.H + c.I + c.J);
    }
}
}
```

# License
The software released under the terms of the [MIT license](./LICENSE). Enjoy.

# Donate
Its free opensource software, but you can buy me a coffee:

<a href="https://www.buymeacoffee.com/leopotam" target="_blank"><img src="https://www.buymeacoffee.com/assets/img/custom_images/yellow_img.png" alt="Buy Me A Coffee" style="height: auto !important;width: auto !important;" ></a>