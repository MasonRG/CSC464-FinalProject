using Engine;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameConsole
{
	public class ConsoleDisplayElements
	{
		public Transform content;
		public ScrollRect scrollRect;
		public Scrollbar scrollBar;
		public TMP_InputField inputField;

		//For Hiding
		public Image mainBackground;
		public Image textBackground;
		public Image barBackground;
		public Image barHandle;
		public Image inputBackground;
		public TextMeshProUGUI inputPlaceholderText;

		public RectTransform scrollView;

		public ConsoleDisplayElements(Transform transform)
		{
			foreach (Transform child in transform.GetComponentsInChildren<Transform>(true))
			{
				switch (child.name)
				{
					case "chat_box_inner":
						mainBackground = child.GetComponent<Image>();
						break;
					case "input_field":
						inputField = child.GetComponent<TMP_InputField>();
						inputBackground = child.GetComponent<Image>();
						break;
					case "content":
						content = child;
						break;
					case "scroll_view":
						scrollView = child.GetComponent<RectTransform>();
						scrollRect = child.GetComponent<ScrollRect>();
						textBackground = child.GetComponent<Image>();
						break;
					case "scrollbar_vertical":
						scrollBar = child.GetComponent<Scrollbar>();
						barBackground = child.GetComponent<Image>();
						break;
					case "handle":
						barHandle = child.GetComponent<Image>();
						break;
					case "placeholder":
						inputPlaceholderText = child.GetComponent<TextMeshProUGUI>();
						break;
				}
			}
			if (!IsValid())
				Debug.LogError("ERROR: chat struct is not set! something was not initialized (probably an incorrect name assigned in the inspector)");
		}


		public bool IsValid()
		{
			return IsValid_Internal(content, scrollRect, scrollBar, inputField, mainBackground, textBackground, barBackground, barHandle, inputBackground, inputPlaceholderText);
		}
		private bool IsValid_Internal(params object[] objects)
		{
			foreach (object obj in objects) if (obj == null) return false;
			return true;
		}


		public void ActivateInputField()
		{
			inputField.ActivateInputField();
		}

		public void DeactivateInputField()
		{
			inputField.text = string.Empty;
			inputField.DeactivateInputField();
		}
	}
}
 
 
 
 