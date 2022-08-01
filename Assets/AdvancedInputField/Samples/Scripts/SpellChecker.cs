using AdvancedInputFieldPlugin;
using System;
using System.Collections.Generic;
using System.Text;

namespace AdvancedInputFieldSamples
{
	public class SpellChecker
	{
		/// <summary>List of all valid words</summary>
		private string[] wordDictionary;

		/// <summary>Similarity threshold for Levenshtein Distance algorithm (https://en.wikipedia.org/wiki/Levenshtein_distance)</summary>
		private int suggestionThreshold;

		private StringBuilder stringBuilder;

		public SpellChecker(string[] wordDictionary, int suggestionThreshold)
		{
			this.wordDictionary = wordDictionary;
			this.suggestionThreshold = suggestionThreshold;
			stringBuilder = new StringBuilder();
		}

		public List<TextRange> CheckText(string text)
		{
			List<TextRange> invalidRanges = new List<TextRange>();
			stringBuilder.Length = 0;
			int start = 0;
			int end = 0;

			int length = text.Length;
			for(int i = 0; i < length; i++)
			{
				char c = text[i];
				if(char.IsLetter(c))
				{
					if(stringBuilder.Length == 0)
					{
						start = i;
					}

					end = i;
					stringBuilder.Append(c);
				}
				else
				{
					if(stringBuilder.Length > 0)
					{
						string word = stringBuilder.ToString();
						if(!IsValidWord(word))
						{
							invalidRanges.Add(new TextRange(start, end));
						}

						stringBuilder.Length = 0; //Reset
					}
				}
			}

			if(stringBuilder.Length > 0) //Check last word if any
			{
				string word = stringBuilder.ToString();
				if(!IsValidWord(word))
				{
					invalidRanges.Add(new TextRange(start, end));
				}
			}

			return invalidRanges;
		}

		public bool IsValidWord(string word)
		{
			int length = wordDictionary.Length;
			for(int i = 0; i < length; i++)
			{
				if(string.Compare(wordDictionary[i], word, true) == 0)
				{
					return true;
				}
			}

			return false;
		}

		public List<string> GetSuggestions(string invalidWord)
		{
			List<string> suggestions = new List<string>();

			int length = wordDictionary.Length;
			for(int i = 0; i < length; i++)
			{
				string word = wordDictionary[i];
				int distance = LevenshteinDistance(word, invalidWord);
				if(distance > 0 && distance <= suggestionThreshold)
				{
					suggestions.Add(word);
				}
			}

			return suggestions;
		}

		/// <summary>Compares similarity between 2 strings used the Levenshtein Distance algorithm</summary>
		public static int LevenshteinDistance(string string1, string string2)
		{
			if(string.IsNullOrEmpty(string1))
			{
				if(string.IsNullOrEmpty(string2))
				{
					return 0;
				}
				return string2.Length;
			}

			if(string.IsNullOrEmpty(string2))
			{
				return string1.Length;
			}

			int n = string1.Length;
			int m = string2.Length;
			int[,] d = new int[n + 1, m + 1];

			// initialize the top and right of the table to 0, 1, 2, ...
			for(int i = 0; i <= n; d[i, 0] = i++) ;
			for(int j = 1; j <= m; d[0, j] = j++) ;

			for(int i = 1; i <= n; i++)
			{
				for(int j = 1; j <= m; j++)
				{
					int cost = (string2[j - 1] == string1[i - 1]) ? 0 : 1;
					int min1 = d[i - 1, j] + 1;
					int min2 = d[i, j - 1] + 1;
					int min3 = d[i - 1, j - 1] + cost;
					d[i, j] = Math.Min(Math.Min(min1, min2), min3);
				}
			}
			return d[n, m];
		}
	}
}
