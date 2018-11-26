using System.Collections;
using UnityEngine;
using TMPro;

namespace GameConsole
{
	public class ConsoleMessage : MonoBehaviour
	{
		public TextMeshProUGUI Text;

		private Transform messageParent;
		public Transform MessageParent { set { messageParent = value; } }

		private bool isActive = true;
		public bool IsActive { get { return isActive; } }

		public void Enable()
		{
			isActive = true;
			transform.SetParent(messageParent);
			transform.SetSiblingIndex(messageParent.childCount - 1);
		}

		public void Disable()
		{
			isActive = false;
			transform.SetParent(null);
		}


	}
}