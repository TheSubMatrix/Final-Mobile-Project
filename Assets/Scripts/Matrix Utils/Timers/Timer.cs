namespace MatrixUtils.Timers
{
    using System;
    using UnityEngine;
    public abstract class Timer<T> : ITimer, IDisposable where T : Timer<T>
    {
        bool m_disposed;
        public float CurrentTime { get; protected set; }
        public bool IsRunning { get; private set; }
        protected float InitialTime = 0;
        public float Progress => Mathf.Clamp01(CurrentTime / InitialTime);
        public bool UseUnscaledTime { get; set; }

        public Action OnTimerStart = delegate { };
        public Action OnTimerStop = delegate { };
        public Action OnTimerPause = delegate { };
        public Action OnTimerResume = delegate { };

        public void Start()
        {
            CurrentTime = InitialTime;
            if (IsRunning) return;
            IsRunning = true;
            TimerManager.RegisterTimer(this);
            OnTimerStart.Invoke();
        }

        public void Stop()
        {
            if (!IsRunning) return;
            IsRunning = false;
            TimerManager.DeregisterTimer(this);
            OnTimerStop.Invoke();
        }

        public abstract void Tick();

        public abstract bool IsFinished { get; }

        public void Resume()
        {
            IsRunning = true;
            OnTimerResume.Invoke();
        }

        public void Pause()
        {
            IsRunning = false;
            OnTimerPause.Invoke();
        }

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

        /// <summary>
        /// Adds a callback to be invoked when the timer starts
        /// </summary>
        public T OnStart(Action callback)
        {
            OnTimerStart += callback;
            return this as T;
        }

        /// <summary>
        /// Adds a callback to be invoked when the timer stops/completes
        /// </summary>
        public T OnComplete(Action callback)
        {
            OnTimerStop += callback;
            return this as T;
        }

        /// <summary>
        /// Adds a callback to be invoked when the timer is paused
        /// </summary>
        public T OnPause(Action callback)
        {
            OnTimerPause += callback;
            return this as T;
        }

        /// <summary>
        /// Adds a callback to be invoked when the timer is resumed
        /// </summary>
        public T OnResume(Action callback)
        {
            OnTimerResume += callback;
            return this as T;
        }

        /// <summary>
        /// Sets whether this timer uses unscaled time (ignores Time.timeScale)
        /// </summary>
        public T SetUseUnscaledTime(bool useUnscaled)
        {
            UseUnscaledTime = useUnscaled;
            return this as T;
        }

        /// <summary>
        /// Resets the timer to its default state, clearing all callbacks and settings
        /// </summary>
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
            if(m_disposed) return;
            if (disposing)
            {
                TimerManager.DeregisterTimer(this);
            }
            m_disposed = true;
        }

        ~Timer()
        {
            Dispose(false);
        }
    }
}