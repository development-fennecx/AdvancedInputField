using AdvancedInputFieldPlugin;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace AdvancedInputFieldSamples
{
	/// <summary>Example View class that underlines invalid words</summary>
	public class SpellCheckerView: MonoBehaviour
	{
		[SerializeField]
		private AdvancedInputField inputField;

		public AdvancedInputField InputField { get { return inputField; } }

		public void UpdateUnderlinedWords(List<TextRange> invalidRanges)
		{
			TextRenderer textRenderer = InputField.TextRenderer;
			if(textRenderer != null)
			{
				TMProTextUnderlineRenderer underlineRenderer = textRenderer.GetComponentInChildren<TMProTextUnderlineRenderer>();
				underlineRenderer.UpdateTextRanges(invalidRanges);
			}
		}

		public void UpdateSuggestions(List<string> suggestions)
		{
			if(InputField.ActionBar != null)
			{
				List<ActionBarAction> replaceActions = new List<ActionBarAction>();
				int length = suggestions.Count;
				for(int i = 0; i < length; i++)
				{
					replaceActions.Add(new ActionBarAction(ActionBarActionType.REPLACE, suggestions[i]));
				}
				inputField.ActionBar.UpdateReplaceActions(replaceActions);
			}
		}

		public void AddGoogleSearchAction(Action<ActionBarAction> clickListener)
		{
			if(InputField.ActionBar != null)
			{
				List<ActionBarAction> customActions = new List<ActionBarAction>();
				customActions.Add(new ActionBarAction(ActionBarActionType.CUSTOM, "Google", clickListener));
				inputField.ActionBar.UpdateCustomActions(customActions);
			}
		}

		public void ClearCustomActions()
		{
			if(InputField.ActionBar != null)
			{
				inputField.ActionBar.UpdateCustomActions(new List<ActionBarAction>()); //Just provide it an empty list
			}
		}
	}
}
