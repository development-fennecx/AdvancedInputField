using AdvancedInputFieldPlugin;
using System.Collections.Generic;
using UnityEngine;

namespace AdvancedInputFieldSamples
{
	public class SpellCheckerController: MonoBehaviour
	{
		/// <summary>Similarity threshold for Levenshtein Distance algorithm (https://en.wikipedia.org/wiki/Levenshtein_distance)</summary>
		private const int SUGGESTION_THRESHOLD = 1;

		/// <summary>List of all valid words</summary>
		private readonly string[] WORD_DICTIONARY = new string[]
		{
			"a", "an", "at", "the", "on", "in", "and", "or", "with", "by", "out", "for", "too",
			"beach", "day", "sand", "ocean", "forest", "jellyfishes",
			"walking", "swimming", "watch",
			"bay", "say", "ray", "may", "hay",
			"halving",
			"band", "hand", "land",
			"our",
		};

		private const string GOOGLE_SEARCH_FORMAT = "https://www.google.com/search?q={0}";

		[SerializeField]
		private SpellCheckerView view;

		private SpellChecker spellChecker;

		private void Awake()
		{
			spellChecker = new SpellChecker(WORD_DICTIONARY, SUGGESTION_THRESHOLD);
		}

		private void Start()
		{
			string currentText = view.InputField.Text;
			List<TextRange> invalidRanges = spellChecker.CheckText(currentText);
			view.UpdateUnderlinedWords(invalidRanges);
		}

		public void OnTextChanged(string text)
		{
			List<TextRange> invalidRanges = spellChecker.CheckText(text);
			view.UpdateUnderlinedWords(invalidRanges);
		}

		public void OnCaretChanged(int caretPosition)
		{
			AdvancedInputField inputField = view.InputField;
			string currentText = inputField.Text;
			string word = null;
			List<string> suggestions = new List<string>();

			if(inputField.HasSelection)
			{
				word = inputField.SelectedText;
				Debug.Log("Word focused: " + word);
				suggestions = spellChecker.GetSuggestions(word);
			}
			else if(TryGetWord(currentText, caretPosition, out word))
			{
				Debug.Log("Word focused: " + word);
				suggestions = spellChecker.GetSuggestions(word);
			}

			view.UpdateSuggestions(suggestions);
			if(!string.IsNullOrEmpty(word))
			{
				view.AddGoogleSearchAction(
					(action) =>
					{
						OnGoogleSearchActionClick(action, word);
					}
				);
			}
			else
			{
				view.ClearCustomActions(); //Since the Google Search action is the only custom action, just clear them all
			}
		}

		public bool TryGetWord(string text, int position, out string word)
		{
			if(position >= text.Length || !char.IsLetter(text[position])) //Not in a word
			{
				word = null;
				return false;
			}

			int wordStart = position;
			int wordEnd = position;

			int length = text.Length;
			for(int i = position - 1; i >= 0; i--)
			{
				if(!char.IsLetter(text[i]))
				{
					break;
				}

				wordStart = i;
			}
			for(int i = position + 1; i < length; i++)
			{
				if(!char.IsLetter(text[i]))
				{
					break;
				}

				wordEnd = i;
			}

			if(wordEnd > wordStart)
			{
				word = text.Substring(wordStart, (wordEnd - wordStart) + 1);
				return true;
			}
			else
			{
				word = null;
				return false;
			}
		}

		public void OnGoogleSearchActionClick(ActionBarAction action, string word)
		{
			Debug.Log("OnGoogleSearchActionClick: " + word);
			Application.OpenURL(string.Format(GOOGLE_SEARCH_FORMAT, word));
		}
	}
}
