// ----------------------------------------------------------------------------
// The MIT License
// MultiThreading for Entity Component System framework https://github.com/Leopotam/ecs
// Copyright (c) 2017-2018 Leopotam <leopotam@gmail.com>
// ----------------------------------------------------------------------------

using System;
using System.Threading;

#if ENABLE_IL2CPP
// Unity IL2CPP performance optimization attribute.
namespace Unity.IL2CPP.CompilerServices {
    enum Option {
        NullChecks = 1,
        ArrayBoundsChecks = 2
    }

    [AttributeUsage (AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
    class Il2CppSetOptionAttribute : Attribute {
        public Option Option { get; private set; }
        public object Value { get; private set; }

        public Il2CppSetOptionAttribute (Option option, object value) { Option = option; Value = value; }
    }
}
#endif

namespace Leopotam.Ecs.Threads {
    /// <summary>
    /// Base system for multithreading processing. In WebGL - will work like IEcsRunSystem system.
    /// </summary>
#if ENABLE_IL2CPP
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption (Unity.IL2CPP.CompilerServices.Option.NullChecks, false)]
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption (Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false)]
#endif
    public abstract class EcsMultiThreadSystem<T> : IEcsPreInitSystem, IEcsRunSystem where T : EcsFilter {
        WorkerDesc[] _descs;
        ManualResetEvent[] _syncs;
        T _filter;
        EcsMultiThreadWorker _worker;
        int _minJobSize;
        int _threadsCount;

        void IEcsPreInitSystem.PreInitialize () {
            _filter = GetFilter ();
            _worker = GetWorker ();
            _minJobSize = GetMinJobSize ();
            _threadsCount = GetThreadsCount ();
#if DEBUG
            if (_filter == null) {
                throw new Exception ("GetFilter() returned null");
            }
            if (_minJobSize < 1) {
                throw new Exception ("GetMinJobSize() returned invalid value");
            }
            if (_threadsCount < 1) {
                throw new Exception ("GetThreadsCount() returned invalid value");
            }
#endif
            _descs = new WorkerDesc[_threadsCount];
            _syncs = new ManualResetEvent[_threadsCount];
            for (var i = 0; i < _descs.Length; i++) {
                var desc = new WorkerDesc ();
                desc.Thread = new Thread (ThreadProc);
                desc.Thread.IsBackground = true;
#if DEBUG
                desc.Thread.Name = string.Format ("ECS-{0:X}-{1}", this.GetHashCode (), i);
#endif
                desc.HasWork = new ManualResetEvent (false);
                desc.WorkDone = new ManualResetEvent (true);
                desc.Worker = _worker;
                _descs[i] = desc;
                _syncs[i] = desc.WorkDone;
                desc.Thread.Start (desc);
            }
        }

        void IEcsPreInitSystem.PreDestroy () {
            for (var i = 0; i < _descs.Length; i++) {
                var desc = _descs[i];
                _descs[i] = null;
                desc.Thread.Interrupt ();
                desc.Thread.Join (10);
                _syncs[i].Close ();
                _syncs[i] = null;
            }
            _filter = null;
            _worker = null;
        }

        void IEcsRunSystem.Run () {
            var count = _filter.EntitiesCount;
            if (count > 0) {
                var processed = 0;
                var jobSize = count / (_threadsCount + 1);
                int workersCount;
                if (jobSize > _minJobSize) {
                    workersCount = _threadsCount + 1;
                } else {
                    workersCount = count / _minJobSize;
                    jobSize = _minJobSize;
                }
                for (var i = 0; i < workersCount - 1; i++) {
                    var desc = _descs[i];
                    desc.IndexFrom = processed;
                    processed += jobSize;
                    desc.IndexTo = processed;
                    desc.WorkDone.Reset ();
                    desc.HasWork.Set ();
                }
                // local worker.
                _worker (_filter, processed, count);
                WaitHandle.WaitAll (_syncs);
            }
        }

        void ThreadProc (object rawDesc) {
            var desc = (WorkerDesc) rawDesc;
            try {
                while (Thread.CurrentThread.IsAlive) {
                    desc.HasWork.WaitOne ();
                    desc.HasWork.Reset ();
                    desc.Worker (_filter, desc.IndexFrom, desc.IndexTo);
                    desc.WorkDone.Set ();
                }
            } catch { }
        }

        sealed class WorkerDesc {
            public Thread Thread;
            public ManualResetEvent HasWork;
            public ManualResetEvent WorkDone;
            public EcsMultiThreadWorker Worker;
            public int IndexFrom;
            public int IndexTo;
        }

        /// <summary>
        /// Source filter for processing entities from it.
        /// </summary>
        protected abstract T GetFilter ();

        /// <summary>
        /// Custom processor of received entities.
        /// </summary>
        protected abstract EcsMultiThreadWorker GetWorker ();

        /// <summary>
        /// Minimal amount of entities to process by one worker. Will be called only once.
        /// </summary>
        protected abstract int GetMinJobSize ();

        /// <summary>
        /// How many threads should be used by this system. Will be called only once.
        /// </summary>
        protected abstract int GetThreadsCount ();

        public delegate void EcsMultiThreadWorker (T filter, int from, int to);
    }
}