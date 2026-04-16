using System;
using UnityEngine;

namespace MatrixUtils.Timers
{
    /// <summary>
    /// Countdown timer that fires an event at every interval until completion.
    /// </summary>
    public class IntervalTimer : Timer<IntervalTimer>
    {
        float m_interval;
        float m_nextInterval;

        public Action OnTimerInterval = delegate { };

        public IntervalTimer()
        {
            m_interval = 0.1f;
            m_nextInterval = -m_interval;
        }

        public IntervalTimer(float totalTime, float intervalSeconds)
        {
            InitialTime = totalTime;
            m_interval = intervalSeconds;
            m_nextInterval = totalTime - m_interval;
        }

        public override void Tick()
        {
            if (IsRunning && CurrentTime > 0)
            {
                CurrentTime -= GetDeltaTime();
                while (CurrentTime <= m_nextInterval && m_nextInterval >= 0)
                {
                    OnTimerInterval.Invoke();
                    m_nextInterval -= m_interval;
                }
            }
            if (!IsRunning || !(CurrentTime <= 0)) return;
            CurrentTime = 0;
            Stop();
        }

        public override void ResetState()
        {
	        base.ResetState();
	        OnTimerInterval = delegate { };
        }

        public override bool IsFinished => CurrentTime <= 0;


        /// <summary>
        /// Sets the action that occurs when the timer reaches its interval
        /// </summary>
        public IntervalTimer OnInterval(Action callback)
        {
            OnTimerInterval += callback;
            return this;
        }

        /// <summary>
        /// Sets the interval of the timer
        /// </summary>
        public IntervalTimer WithInterval(float intervalSeconds)
        {
            m_interval = intervalSeconds;
            m_nextInterval = CurrentTime - m_interval;
            return this;
        }
    }
}