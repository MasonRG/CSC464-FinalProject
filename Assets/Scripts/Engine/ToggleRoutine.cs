using System.Collections;

namespace AoTGameEngine.CustomTypes
{
	/// <summary>
	/// You should give this class routines that run until manually stopped.
	/// </summary>
	public class ToggleRoutine
	{
		public bool IsRunning { get; private set; }
		private IEnumerator routine;

		public ToggleRoutine(IEnumerator routine)
		{
			this.routine = routine;
		}

		public void SetRoutine(IEnumerator routine)
		{
			Stop();
			this.routine = routine;
		}

		public void Start()
		{
			if (routine == null)
				return;

			if (IsRunning)
			{
				Stop();
			}

			IsRunning = true;
			RoutineRunner.StartRoutine(routine);
		}

		public void Stop()
		{
			if (routine == null)
				return;

			if (IsRunning)
			{
				RoutineRunner.StopRoutine(routine);
			}

			IsRunning = false;
		}
	}
}
