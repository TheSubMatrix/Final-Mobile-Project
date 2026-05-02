using JetBrains.Annotations;

namespace MatrixUtils.Timers
{
    using System;
    using UnityEngine;
    [PublicAPI]
    public abstract class Timer<T> : ITimer, IDisposable where T : Timer<T>
    {
        bool m_disposed;
        public float CurrentTime { get; protected set; }
        public bool IsRunning { get; private set; }
        protected float InitialTime;
        public float Progress => Mathf.Clamp01(CurrentTime / InitialTime);
        public bool UseUnscaledTime { get; set; }

        public Action OnTimerStart = delegate { };
        public Action OnTimerStop = delegate { };
        public Action OnTimerPause = delegate { };
        public Action OnTimerResume = delegate { };
        public Action OnTimerTick = delegate { };

        public void Start()
        {
            CurrentTime = InitialTime;
            if (IsRunning) return;
            IsRunning = true;
            TimerManager.RegisterTimer(this);
            OnTimerStart.Invoke();
            HandleStart();
        }

        public void Stop()
        {
            if (!IsRunning) return;
            IsRunning = false;
            TimerManager.DeregisterTimer(this);
            OnTimerStop.Invoke();
            HandleStop();
        }

        public void Pause()
        {
            IsRunning = false;
            OnTimerPause.Invoke();
            HandlePause();
        }

        public void Resume()
        {
            IsRunning = true;
            OnTimerResume.Invoke();
            HandleResume();
        }

        public void Tick()
        {
            OnTimerTick.Invoke();
            HandleTick();
        }

        protected virtual void HandleStart() { }
        protected virtual void HandleStop() { }
        protected virtual void HandlePause() { }
        protected virtual void HandleResume() { }
        protected abstract void HandleTick();

        public abstract bool IsFinished { get; }

        public virtual void Reset() => CurrentTime = InitialTime;

        public virtual void Reset(float newTime)
        {
            InitialTime = newTime;
            Reset();
        }

        protected float GetDeltaTime()
        {
            return UseUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        }

        public T OnStart(Action callback)
        {
            OnTimerStart += callback;
            return this as T;
        }

        public T OnComplete(Action callback)
        {
            OnTimerStop += callback;
            return this as T;
        }

        public T OnPause(Action callback)
        {
            OnTimerPause += callback;
            return this as T;
        }

        public T OnResume(Action callback)
        {
            OnTimerResume += callback;
            return this as T;
        }

        public T SetUseUnscaledTime(bool useUnscaled)
        {
            UseUnscaledTime = useUnscaled;
            return this as T;
        }

        public T OnTick(Action callback)
        {
            OnTimerTick += callback;
            return this as T;
        }
        
        public virtual void ResetState()
        {
            Stop();
            OnTimerStart = delegate { };
            OnTimerStop = delegate { };
            OnTimerPause = delegate { };
            OnTimerResume = delegate { };
            UseUnscaledTime = false;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (m_disposed) return;
            if (disposing) TimerManager.DeregisterTimer(this);
            m_disposed = true;
        }

        ~Timer() => Dispose(false);
    }
}