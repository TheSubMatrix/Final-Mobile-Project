namespace MatrixUtils.Timers
{
	public interface ITimer
	{
		void Tick();
		void Start();
		void Stop();
		void Pause();
		void Resume();
		void Reset();
		bool IsRunning { get; }
		bool IsFinished { get; }
		float CurrentTime { get; }
		void Dispose();
	}
}