using System;
using UnityEngine;

namespace MatrixUtils.Timers
{
	/// <summary>
	/// Timer that counts up from zero to infinity. Great for measuring durations.
	/// </summary>
	public class StopwatchTimer : Timer<StopwatchTimer>
	{
		public StopwatchTimer() { }
		public StopwatchTimer(float initialTime)
		{
			InitialTime = initialTime;
		}

		public override void Tick()
		{
			if (!IsRunning) return;
			CurrentTime += GetDeltaTime();
		}

		public override bool IsFinished => false;
	}
}