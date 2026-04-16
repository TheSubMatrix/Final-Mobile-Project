using System.Collections.Generic;

namespace MatrixUtils.Timers
{
	public static class TimerManager {
		static readonly List<ITimer> s_timers = new();
		static readonly List<ITimer> s_sweep = new();

		public static void RegisterTimer(ITimer timer) => s_timers.Add(timer);
		public static void DeregisterTimer(ITimer timer) => s_timers.Remove(timer);

		public static void UpdateTimers() {
			if (s_timers.Count == 0) return;

			s_sweep.RefreshWith(s_timers);
			foreach (ITimer timer in s_sweep) {
				timer.Tick();
			}
		}

		public static void Clear() {
			s_sweep.RefreshWith(s_timers);
			foreach (ITimer timer in s_sweep) {
				timer.Dispose();
			}

			s_timers.Clear();
			s_sweep.Clear();
		}
	}
}