// Copyright (c) Jeroen van Pienbroek. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections;
using UnityEngine;

namespace AdvancedInputFieldPlugin
{
	public class InputFieldEventHandler
	{
		internal InputFieldEngine Engine { get; private set; }
		public AdvancedInputField InputField { get { return Engine.InputField; } }
		public MonoBehaviour ActiveBehaviour { get { return Engine.ActiveBehaviour; } }

		public InputFieldEventHandler(InputFieldEngine engine)
		{
			Engine = engine;
		}

		internal void InvokeSelectionChanged(bool selected)
		{
			InputField.OnSelectionChanged.Invoke(selected);
		}

		internal void InvokeBeginEdit(BeginEditReason reason)
		{
			InputField.OnBeginEdit.Invoke(reason);
		}

		internal void InvokeEndEdit(string result, EndEditReason reason)
		{
			InputField.OnEndEdit.Invoke(result, reason);
		}

		internal void InvokeSizeChanged(Vector2 size)
		{
			InputField.OnSizeChanged.Invoke(size);
		}

		internal void InvokeValueChanged(string text)
		{
			ActiveBehaviour?.StartCoroutine(DelayedValueChanged(text));
		}

		internal IEnumerator DelayedValueChanged(string text)
		{
			yield return null;
			if(InputField == null) { yield break; }

			if(InputField.OnValueChanged != null)
			{
				InputField.OnValueChanged.Invoke(text);
			}
		}

		internal void InvokeCaretPositionChanged(int caretPosition)
		{
			ActiveBehaviour?.StartCoroutine(DelayedCaretPositionChanged(caretPosition));
		}

		internal IEnumerator DelayedCaretPositionChanged(int caretPosition)
		{
			yield return null;
			if(InputField == null) { yield break; }

			if(InputField.OnCaretPositionChanged != null)
			{
				InputField.OnCaretPositionChanged.Invoke(caretPosition);
			}
		}

		internal void InvokeTextSelectionChanged(int selectionStartPosition, int selectionEndPosition)
		{
			ActiveBehaviour?.StartCoroutine(DelayedTextSelectionChanged(selectionStartPosition, selectionEndPosition));
		}

		internal IEnumerator DelayedTextSelectionChanged(int selectionStartPosition, int selectionEndPosition)
		{
			yield return null;
			if(InputField == null) { yield break; }

			if(InputField.OnTextSelectionChanged != null)
			{
				InputField.OnTextSelectionChanged.Invoke(selectionStartPosition, selectionEndPosition);
			}
		}

		internal void InvokeSpecialKeyPressed(SpecialKeyCode specialKeyCode)
		{
			ActiveBehaviour?.StartCoroutine(DelayedSpecialKeyPressed(specialKeyCode));
		}

		internal IEnumerator DelayedSpecialKeyPressed(SpecialKeyCode specialKeyCode)
		{
			yield return null;
			if(InputField == null) { yield break; }

			if(InputField.OnSpecialKeyPressed != null)
			{
				InputField.OnSpecialKeyPressed.Invoke(specialKeyCode);
			}
		}

		internal void InvokeTextTap(int tapCount, Vector2 position)
		{
			ActiveBehaviour?.StartCoroutine(DelayedTextTap(tapCount, position));
		}

		internal IEnumerator DelayedTextTap(int tapCount, Vector2 position)
		{
			yield return null;
			if(InputField == null) { yield break; }

			if(InputField.OnTextTap != null)
			{
				InputField.OnTextTap.Invoke(tapCount, position);
			}
		}

		internal void InvokeActionBarAction(ActionBarAction action)
		{
			if(InputField.OnActionBarAction != null)
			{
				InputField.OnActionBarAction.Invoke(action);
			}
		}
	}
}
