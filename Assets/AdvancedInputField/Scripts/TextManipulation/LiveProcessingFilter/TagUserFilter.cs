// Copyright (c) Jeroen van Pienbroek. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using AdvancedInputFieldPlugin;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TagUserFilter: LiveProcessingFilter
{
	private const string USER_TAG_FORMAT = "<link={0}><color=blue>{1}</color></link>";

	[SerializeField]
	private InputFieldDropdown dropdown;

	[SerializeField]
	private AdvancedInputField inputField;

	private List<InputFieldDropdown.OptionData> userDropdownOptions;
	private int wordStartIndex;
	private int wordEndIndex;

	private void Awake()
	{
		TextAsset namesAsset = Resources.Load<TextAsset>("names");
		if(namesAsset != null)
		{
			userDropdownOptions = new List<InputFieldDropdown.OptionData>();
			List<RichTextBindingData> userTags = new List<RichTextBindingData>();
			string[] names = namesAsset.text.Split('\n');

			int length = names.Length;
			for(int i = 0; i < length; i++)
			{
				string name = names[i];
				string link = name.ToUpper();
				string displayName = "@" + name.ToLower();
				userDropdownOptions.Add(new InputFieldDropdown.OptionData(name));
				userTags.Add(new RichTextBindingData(name, string.Format(USER_TAG_FORMAT, link, displayName)));
			}

			NativeKeyboardManager.RichTextBindingEngine.InitializeBindings(userTags);
		}
		else
		{
			Debug.LogWarning("Names resources file not found");
		}
	}

	public override TextEditFrame ProcessTextEditUpdate(TextEditFrame textEditFrame, TextEditFrame lastTextEditFrame)
	{
		if(textEditFrame.text == lastTextEditFrame.text) //No text change
		{
			dropdown.Hide();
		}
		else //Text change
		{
			DetectUserTagStart(textEditFrame);
		}

		return textEditFrame; //No processing needed, so allow change by returning current frame
	}

	public void DetectUserTagStart(TextEditFrame textEditFrame)
	{
		if(textEditFrame.selectionStartPosition != textEditFrame.selectionEndPosition)
		{
			dropdown.Hide();
		}

		string text = textEditFrame.text;
		int caretPosition = textEditFrame.selectionStartPosition;
		int startIndex = -1;

		for(int i = caretPosition - 1; i >= 0; i--) //Detect @ character
		{
			char c = text[i];
			if(c == '@')
			{
				if(i > 0)
				{
					char previousChar = text[i - 1];
					if(previousChar == ' ' || previousChar == '\n') //Check if the previous char is a space or newline character to make sure it's not an email address
					{
						startIndex = i + 1;
					}
				}
				else
				{
					startIndex = i + 1;
				}
				break;
			}
			else if(c == ' ' || c == '\n') //Just quit when you encounter a space or newline character
			{
				break;
			}
		}

		if(startIndex != -1) //Find the complete word after @ character
		{
			int amount = 0;

			int length = text.Length;
			for(int i = startIndex; i < length; i++)
			{
				char c = text[i];
				if(c == ' ' || c == '\n') //Just quit when you encounter a space or newline character
				{
					break;
				}

				amount++;

			}

			string searchText = text.Substring(startIndex, amount);
			wordStartIndex = startIndex - 1;
			wordEndIndex = startIndex + amount;
			OnShowDropdown(searchText, startIndex);
		}
		else
		{
			dropdown.Hide();
		}
	}

	public override void OnRichTextEditUpdate(TextEditFrame richTextEditFrame, TextEditFrame lastRichTextEditFrame)
	{
		if(richTextEditFrame.selectionStartPosition != lastRichTextEditFrame.selectionStartPosition
			|| richTextEditFrame.selectionEndPosition != lastRichTextEditFrame.selectionEndPosition) //Caret or selection changed
		{
			dropdown.Hide();
		}
	}

	public void OnShowDropdown(string searchText, int startIndex)
	{
		Debug.Log("OnShowDropdown: " + searchText + ", StartIndex: " + startIndex);
		int richTextPosition = inputField.DeterminePositionInRichText(startIndex);
		AdvancedInputFieldPlugin.CharacterInfo characterInfo = inputField.TextRenderer.GetCharacterInfo(richTextPosition);
		dropdown.GetComponent<RectTransform>().anchoredPosition = characterInfo.position;
		UpdateUserOptions(searchText);
	}

	public void UpdateUserOptions(string userText)
	{
		if(string.IsNullOrEmpty(userText))
		{
			dropdown.Hide();
		}
		else
		{
			Predicate<InputFieldDropdown.OptionData> predicate = (option => option.text.IndexOf(userText, StringComparison.InvariantCultureIgnoreCase) != -1);
			dropdown.options = userDropdownOptions.FindAll(predicate);
			dropdown.Show();
		}
	}

	public void OnUserDropdownChanged(int index)
	{
		Debug.Log("OnUserDropdownChanged: " + index);
		string userTag = dropdown.options[index].text;
		StartCoroutine(ApplyTag(userTag));
	}

	public IEnumerator ApplyTag(string userTag)
	{
		yield return null; //Wait 1 frame to make sure everything is processed

		Debug.Log("Tag user: " + userTag);
		Debug.Log("Word: " + wordStartIndex + " -> " + wordEndIndex);

		if(NativeKeyboardManager.RichTextBindingEngine.TryGetBindingFromName(userTag, out RichTextBindingData tagData))
		{
			inputField.SetTextSelection(wordStartIndex, wordEndIndex);
			inputField.ReplaceSelectedTextInRichText(tagData.codePoint.ToString());
		}
		else
		{
			Debug.LogWarningFormat("No tag data found for {0}", userTag);
		}
	}
}
