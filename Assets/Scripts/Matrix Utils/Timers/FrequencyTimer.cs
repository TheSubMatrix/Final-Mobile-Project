namespace MatrixUtils.Timers
{
    using System;

    public class FrequencyTimer : Timer<FrequencyTimer>
    {
        public uint TicksPerSecond { get; private set; }
        float m_timeThreshold;
        public Action FrequencyTick = delegate { };

        public FrequencyTimer() => CalculateTimeThreshold(1);
        public FrequencyTimer(uint ticksPerSecond) => CalculateTimeThreshold(ticksPerSecond);

        protected override void HandleTick()
        {
            if (!IsRunning) return;
            CurrentTime += GetDeltaTime();
            while (CurrentTime >= m_timeThreshold)
            {
                CurrentTime -= m_timeThreshold;
                FrequencyTick.Invoke();
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
        public FrequencyTimer OnFrequencyTick(Action callback)
        {
            FrequencyTick += callback;
            return this;
        }

        public override void ResetState()
        {
            base.ResetState();
            FrequencyTick = delegate { };
        }
    }
}