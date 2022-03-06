// Copyright (c) Jeroen van Pienbroek. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AdvancedInputFieldPlugin.Samples
{
	[RequireComponent(typeof(AdvancedInputField))]
	public class MultiTextSelectionHandler: MonoBehaviour
	{
		[SerializeField]
		private MultiTextSelectionRenderer selectionRenderer;

		private AdvancedInputField inputField;
		private string fullHighlightedText;
		private List<MultiTextSelectionRenderer.TextSelectionRegion> textSelectionRegions;
		private bool backspacePressed;

		private void Awake()
		{
			inputField = GetComponent<AdvancedInputField>();
			textSelectionRegions = new List<MultiTextSelectionRenderer.TextSelectionRegion>();

			selectionRenderer.Initialize(inputField);
			fullHighlightedText = inputField.Text;
		}

		public void OnTextChanged(string text)
		{
			StartCoroutine(DelayedClearHighlight());
		}

		/// <summary>Delayed the highlight clear, because OnSpecialKeyPressed event might be called after OnValueChanged event</summary>
		private IEnumerator DelayedClearHighlight()
		{
			yield return new WaitForSeconds(0.1f);
			if(!backspacePressed) //Only clear text if backspace key wasn't pressed
			{
				ClearHighlight();
			}

			backspacePressed = false;
		}

		public void OnSpecialKeyPressed(SpecialKeyCode keyCode)
		{
			if(keyCode == SpecialKeyCode.BACKSPACE)
			{
				RemoveHighlightedWords();
				backspacePressed = true;
			}
		}

		public void OnTextTap(int tapCount, Vector2 position)
		{
			if(tapCount == 1)
			{
				ClearHighlight();
			}
			else if(tapCount == 3)
			{
				inputField.CaretPosition = inputField.TextSelectionStartPosition;
				HighlightAllWords();
			}
		}

		private void RemoveHighlightedWords()
		{
			if(textSelectionRegions.Count == 0) //Nothing highlighted
			{
				return;
			}

			inputField.Text = string.Empty;
			ClearHighlight();
		}

		private void ClearHighlight()
		{
			textSelectionRegions.Clear();
			selectionRenderer.UpdateSelectionRegions(textSelectionRegions);
		}

		private void HighlightAllWords()
		{
			string text = inputField.Text;
			textSelectionRegions.Clear();
			int startPosition = -1;

			int length = text.Length;
			for(int i = 0; i < length; i++)
			{
				char c = text[i];
				if(c == ' ')
				{
					if(startPosition != -1 && startPosition < i)
					{
						textSelectionRegions.Add(new MultiTextSelectionRenderer.TextSelectionRegion(startPosition, i));
						startPosition = -1;
					}
				}
				else if(startPosition == -1)
				{
					startPosition = i;
				}
			}

			if(startPosition != -1 && startPosition < length)
			{
				textSelectionRegions.Add(new MultiTextSelectionRenderer.TextSelectionRegion(startPosition, length));
			}

			selectionRenderer.UpdateSelectionRegions(textSelectionRegions);
			fullHighlightedText = text;
		}
	}
}
