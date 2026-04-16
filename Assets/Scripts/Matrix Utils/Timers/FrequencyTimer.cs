namespace MatrixUtils.Timers
{
    using System;

    public class FrequencyTimer : Timer<FrequencyTimer>
    {
        public uint TicksPerSecond { get; private set; }
        float m_timeThreshold;
        public Action OnTimerTick = delegate { };

        public FrequencyTimer() => CalculateTimeThreshold(1);
        public FrequencyTimer(uint ticksPerSecond) => CalculateTimeThreshold(ticksPerSecond);

        public override void Tick()
        {
            if (!IsRunning) return;
            CurrentTime += GetDeltaTime();
            while (CurrentTime >= m_timeThreshold)
            {
                CurrentTime -= m_timeThreshold;
                OnTimerTick.Invoke();
            }
        }

        public override bool IsFinished => !IsRunning;

        public override void Reset()
        {
            base.Reset();
            CurrentTime = 0;
        }

        public void Reset(uint newTicksPerSecond)
        {
            CalculateTimeThreshold(newTicksPerSecond);
            Reset();
        }

        void CalculateTimeThreshold(uint ticksPerSecond)
        {
            TicksPerSecond = ticksPerSecond;
            m_timeThreshold = ticksPerSecond > 0 ? 1f / ticksPerSecond : float.MaxValue;
        }

        public FrequencyTimer WithTicksPerSecond(uint ticksPerSecond)
        {
            Reset(ticksPerSecond);
            return this;
        }
        public FrequencyTimer OnTick(Action callback)
        {
            OnTimerTick += callback;
            return this;
        }

        public override void ResetState()
        {
            base.ResetState();
            OnTimerTick = delegate { };
        }
    }
}