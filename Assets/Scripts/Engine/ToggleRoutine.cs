using System.Collections;

namespace Engine
{
	public class ToggleRoutine
	{
		public bool IsRunning { get; set; }
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
