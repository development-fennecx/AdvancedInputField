using System;
using System.Collections.Generic;

namespace BetterJSON
{
	public class JSONObject
	{
		private Dictionary<string, JSONValue> dictionary;

		public JSONObject(string jsonString)
		{
			dictionary = new Dictionary<string, JSONValue>();

			Parse(jsonString);
		}

		public JSONValue this[string key]
		{
			get { return dictionary[key]; }
			set { dictionary[key] = value; }
		}

		private void Parse(string jsonString)
		{
			if(string.IsNullOrEmpty(jsonString)) { return; } //Empty
			if(!(jsonString[0] == '{' && jsonString[jsonString.Length - 1] == '}')) { return; } //Invalid format
			if(jsonString.Length <= 2) { return; } //No contents

			string contentString = jsonString.Substring(1, jsonString.Length - 2).Trim();

			int length = contentString.Length;
			while(length > 0)
			{
				if(contentString[0] != '"') { break; } //Invalid format

				int keyStartIndex = 1;
				int keyEndIndex = contentString.IndexOf("\":");
				if(keyEndIndex == -1) { break; } //Invalid format

				string keyString = contentString.Substring(keyStartIndex, (keyEndIndex - keyStartIndex));

				contentString = contentString.Substring(keyEndIndex + 2).Trim();
				length = contentString.Length;

				int valueStartIndex = 0;
				int valueEndIndex = 0;
				string valueString = null;

				char valueStartChar = contentString[0];
				switch(valueStartChar)
				{
					case '[':
						DetermineRange(contentString, ref valueStartIndex, ref valueEndIndex);
						valueString = contentString.Substring(valueStartIndex, (valueEndIndex - valueStartIndex) + 1);
						dictionary.Add(keyString, new ArrayValue(valueString));
						break;
					case '{':
						DetermineRange(contentString, ref valueStartIndex, ref valueEndIndex);
						valueString = contentString.Substring(valueStartIndex, (valueEndIndex - valueStartIndex) + 1);
						dictionary.Add(keyString, new ObjectValue(valueString));
						break;
					case '\"':
						valueStartIndex = 1;
						valueEndIndex = contentString.IndexOf("\",") - 1;
						if(valueEndIndex < 0)
						{
							if(contentString[length - 1] == '\"')
							{
								valueEndIndex = length - 2;
							}

							if(valueEndIndex == -1) { break; } //Invalid format
						}

						if(valueEndIndex < 0 || valueEndIndex < valueStartIndex)
						{
							valueString = string.Empty;
							valueEndIndex = valueStartIndex;
						}
						else
						{
							valueString = contentString.Substring(valueStartIndex, (valueEndIndex - valueStartIndex) + 1);
						}
						dictionary.Add(keyString, new StringValue(valueString));
						break;
					default:
						valueStartIndex = 0;
						valueEndIndex = contentString.IndexOf(',') - 1;
						if(valueEndIndex < 0)
						{
							valueEndIndex = contentString.IndexOf('}') - 1;
						}
						if(valueEndIndex < 0)
						{
							valueEndIndex = contentString.Length - 1;
						}

						valueString = contentString.Substring(valueStartIndex, (valueEndIndex - valueStartIndex) + 1);
						if(valueString.EndsWith("}"))
						{
							valueString = valueString.Substring(0, valueString.Length - 1);
						}

						if(valueString.Equals("false", StringComparison.OrdinalIgnoreCase) || valueString.Equals("true", StringComparison.OrdinalIgnoreCase))
						{
							dictionary.Add(keyString, new BooleanValue(valueString));
						}
						else if(valueString.Contains("."))
						{
							dictionary.Add(keyString, new DoubleValue(valueString));
						}
						else
						{
							dictionary.Add(keyString, new IntegerValue(valueString));
						}
						break;
				}

				if(valueEndIndex + 1 < length)
				{
					contentString = contentString.Substring(valueEndIndex + 1);
					length = contentString.Length;

					if(length > 0 && contentString[0] == ',')
					{
						contentString = contentString.Substring(1);
						length = contentString.Length;
					}
				}
				else
				{
					break;
				}
			}
		}

		public void DetermineRange(string contentString, ref int startIndex, ref int endIndex)
		{
			startIndex = 0;
			int depth = 0;

			int length = contentString.Length;
			for(int i = 0; i < length; i++)
			{
				char c = contentString[i];
				if(c == '[' || c == '{')
				{
					depth++;
				}
				else if(c == ']' || c == '}')
				{
					depth--;
					if(depth == 0)
					{
						endIndex = i;
						break;
					}
				}
			}
		}
	}
}
