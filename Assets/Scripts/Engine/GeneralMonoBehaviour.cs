using UnityEngine;

namespace Engine
{
	public class GeneralMonoBehaviour : MonoBehaviour
	{
		private static volatile GeneralMonoBehaviour instance;
		private static readonly object lockObj = new object();

		private GeneralMonoBehaviour() { }

		public static GeneralMonoBehaviour Instance
		{
			get
			{
				if (instance == null)
				{
					lock (lockObj)
					{
						if (instance == null)
						{
							instance = new GameObject("General MonoBehaviour", typeof(GeneralMonoBehaviour)).GetComponent<GeneralMonoBehaviour>();
						}
					}
				}
				return instance;
			}
		}

		public delegate void MonoEventHandler();
		public event MonoEventHandler OnUpdate;
		public event MonoEventHandler OnFixedUpdate;
		public event MonoEventHandler OnLateUpdate;

		public void FireOnUpdate() { if (OnUpdate != null) OnUpdate(); }
		public void FireOnFixedUpdate() { if (OnFixedUpdate != null) OnFixedUpdate(); }
		public void FireOnLateUpdate() { if (OnLateUpdate != null) OnLateUpdate(); }

		private void Update()
		{
			FireOnUpdate();
		}

		private void FixedUpdate()
		{
			FireOnFixedUpdate();
		}

		private void LateUpdate()
		{
			FireOnLateUpdate();
		}
	}
}