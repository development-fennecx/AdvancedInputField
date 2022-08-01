//-----------------------------------------
//			Advanced Input Field
// Copyright (c) 2017 Jeroen van Pienbroek
//------------------------------------------

using AdvancedInputFieldPlugin;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AdvancedInputFieldSamples
{
	public class RichTextController: MonoBehaviour
	{
		[SerializeField]
		private ScrollRect scrollRect;

		private void OnEnable()
		{
			scrollRect.verticalNormalizedPosition = 1;
		}

		public void OnBoldClick()
		{
			GameObject selectedObject = EventSystem.current.currentSelectedGameObject;
			if(selectedObject != null)
			{
				AdvancedInputField inputField = selectedObject.GetComponent<AdvancedInputField>();
				if(inputField != null)
				{
					inputField.ToggleBold();
				}
			}
		}

		public void OnItalicClick()
		{
			GameObject selectedObject = EventSystem.current.currentSelectedGameObject;
			if(selectedObject != null)
			{
				AdvancedInputField inputField = selectedObject.GetComponent<AdvancedInputField>();
				if(inputField != null)
				{
					inputField.ToggleItalic();
				}
			}
		}

		public void OnUnderlineClick()
		{
			GameObject selectedObject = EventSystem.current.currentSelectedGameObject;
			if(selectedObject != null)
			{
				AdvancedInputField inputField = selectedObject.GetComponent<AdvancedInputField>();
				if(inputField != null)
				{
					inputField.ToggleUnderline();
				}
			}
		}

		public void OnStrikethroughClick()
		{
			GameObject selectedObject = EventSystem.current.currentSelectedGameObject;
			if(selectedObject != null)
			{
				AdvancedInputField inputField = selectedObject.GetComponent<AdvancedInputField>();
				if(inputField != null)
				{
					inputField.ToggleStrikethrough();
				}
			}
		}

		public void OnRedClick()
		{
			Debug.Log("OnRedClick");
			GameObject selectedObject = EventSystem.current.currentSelectedGameObject;
			if(selectedObject != null)
			{
				Debug.Log("SelectedObject: " + selectedObject);
				AdvancedInputField inputField = selectedObject.GetComponent<AdvancedInputField>();
				if(inputField != null)
				{
					inputField.ToggleColor("red");
				}
			}
		}

		public void OnGreenClick()
		{
			GameObject selectedObject = EventSystem.current.currentSelectedGameObject;
			if(selectedObject != null)
			{
				AdvancedInputField inputField = selectedObject.GetComponent<AdvancedInputField>();
				if(inputField != null)
				{
					inputField.ToggleColor("green");
				}
			}
		}

		public void OnBlueClick()
		{
			GameObject selectedObject = EventSystem.current.currentSelectedGameObject;
			if(selectedObject != null)
			{
				AdvancedInputField inputField = selectedObject.GetComponent<AdvancedInputField>();
				if(inputField != null)
				{
					inputField.ToggleColor("blue");
				}
			}
		}

		public void OnCustomFontClick()
		{
			GameObject selectedObject = EventSystem.current.currentSelectedGameObject;
			if(selectedObject != null)
			{
				AdvancedInputField inputField = selectedObject.GetComponent<AdvancedInputField>();
				if(inputField != null)
				{
					inputField.ToggleFont("\"DroidSansMono SDF\"");
				}
			}
		}
	}
}
