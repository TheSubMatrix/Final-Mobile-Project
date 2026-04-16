using System;

namespace MatrixUtils.Timers
{
	/// <summary>
	/// A timer that counts down until its completion
	/// </summary>
	public class CountdownTimer : Timer<CountdownTimer>
	{
		public CountdownTimer() { }
		public CountdownTimer(float initialTime)
		{
			InitialTime = initialTime;
		}

		public override void Tick()
		{
			if (IsRunning && CurrentTime > 0)
			{
				CurrentTime -= GetDeltaTime();
			}

			if (IsRunning && CurrentTime <= 0)
			{
				Stop();
			}
		}

		public override bool IsFinished => CurrentTime <= 0;

		/// <summary>
		/// Sets the countdown time
		/// </summary>
		public CountdownTimer WithTime(float time)
		{
			InitialTime = time;
			Reset();
			return this;
		}
	}
}