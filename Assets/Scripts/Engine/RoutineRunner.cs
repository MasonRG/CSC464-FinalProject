using System;
using System.Collections;
using UnityEngine;

namespace Engine
{
	public static class RoutineRunner
	{
		public static IEnumerator StartTimer(Action callback, float delay)
		{
			IEnumerator routine = TimerRoutine(callback, delay);
			GeneralMonoBehaviour.Instance.StartCoroutine(routine);
			return routine;
		}

		static IEnumerator TimerRoutine(Action callback, float delay)
		{
			if (delay <= 0)
			{
				if (callback != null) callback();
				yield break;
			}
			yield return new WaitForSeconds(delay);
			if (callback != null) callback();
		}

		


		public static IEnumerator StartFrameTimer(Action callback, int frames)
		{
			IEnumerator routine = FrameTimerRoutine(callback, frames);
			GeneralMonoBehaviour.Instance.StartCoroutine(routine);
			return routine;
		}

		static IEnumerator FrameTimerRoutine(Action callback, int frames)
		{
			if (frames <= 0)
			{
				if (callback != null) callback();
				yield break;
			}

			for (int i = 0; i < frames; i++)
			{
				yield return null;
			}

			if (callback != null) callback();
		}




		public static IEnumerator StartUntil(Action callback, Func<bool> until)
		{
			IEnumerator routine = UntilRoutine(callback, until);
			GeneralMonoBehaviour.Instance.StartCoroutine(routine);
			return routine;
		}

		static IEnumerator UntilRoutine(Action callback, Func<bool> until)
		{
			float escape = 5f;
			float t = 0;
			while (t < escape)
			{
				t += Time.deltaTime;
				if (until())
				{
					if (callback != null) callback();
					yield break;
				}
				yield return null;
			}
		}



		public static IEnumerator StartTween(Action<float> updateCallback, float runTime, float start = 0f, float end = 1f, YieldInstruction yieldType = null, Action onFinished = null)
		{
			IEnumerator routine = TweenRoutine(updateCallback, runTime, start, end, yieldType, onFinished);
			GeneralMonoBehaviour.Instance.StartCoroutine(routine);
			return routine;
		}

		static IEnumerator TweenRoutine(Action<float> updateCallback, float runTime, float start, float end, YieldInstruction yieldType, Action onFinished)
		{
			float startTime = Time.time;
			float ratio = 0f;
			float t = 0f;
			while (t <= runTime)
			{
				ratio = t / runTime;

				if (updateCallback != null) updateCallback(start + (end - start) * ratio);

				if (yieldType is WaitForFixedUpdate) t += Time.fixedDeltaTime;
				else t += Time.deltaTime;

				yield return yieldType;
			}

			if (updateCallback != null) updateCallback(end);
			if (onFinished != null) onFinished();
		}




        public static void StartRoutine(IEnumerator routine)
        {
            if (routine == null) return;
            GeneralMonoBehaviour.Instance.StartCoroutine(routine);
        }

		public static void StopRoutine(IEnumerator routine)
		{
			if (routine == null) return;
			GeneralMonoBehaviour.Instance.StopCoroutine(routine);
		}
	}
}