using System;
using System.Collections.Generic;
using System.Text;

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
				int keyStartIndex = contentString.IndexOf("\"") + 1;
				int keyEndIndex = contentString.IndexOf("\":");
				if(keyEndIndex == -1) { break; } //Invalid format

				string keyString = contentString.Substring(keyStartIndex, (keyEndIndex - keyStartIndex));

				contentString = contentString.Substring(keyEndIndex + 2).Trim();
				length = contentString.Length;

				int valueStartIndex = 0;
				int valueEndIndex = 0;
				string valueString;

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

					if(length > 0 && (contentString[0] == '\"'))
					{
						contentString = contentString.Substring(1);
						length = contentString.Length;
					}

					if(length > 0 && (contentString[0] == ','))
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

		public JSONValue GetValue(string key)
		{
			JSONValue value;
			dictionary.TryGetValue(key, out value);
			return value;
		}

		public List<JSONValue> GetValues()
		{
			return new List<JSONValue>(dictionary.Values);
		}

		public bool TryGetString(string key, out string stringValue)
		{
			var value = GetValue(key);
			if(value != null)
			{
				stringValue = value.String;
				return true;
			}
			else
			{
				stringValue = default(string);
				return false;
			}
		}

		public bool TryGetDouble(string key, out double doubleValue)
		{
			var value = GetValue(key);
			if(value != null)
			{
				doubleValue = value.Double;
				return true;
			}
			else
			{
				doubleValue = default(double);
				return false;
			}
		}

		public bool TryGetInteger(string key, out int intValue)
		{
			var value = GetValue(key);
			if(value != null)
			{
				intValue = value.Integer;
				return true;
			}
			else
			{
				intValue = default(int);
				return false;
			}
		}

		public bool TryGetByte(string key, out byte byteValue)
		{
			var value = GetValue(key);
			if(value != null)
			{
				byteValue = (byte)value.Integer;
				return true;
			}
			else
			{
				byteValue = default(byte);
				return false;
			}
		}

		public bool TryGetObject(string key, out JSONObject objectValue)
		{
			var value = GetValue(key);
			if(value != null)
			{
				objectValue = value.Object;
				return true;
			}
			else
			{
				objectValue = default(JSONObject);
				return false;
			}
		}


		public bool TryGetBoolean(string key, out bool boolValue)
		{
			var value = GetValue(key);
			if(value != null)
			{
				boolValue = value.Boolean;
				return true;
			}
			else
			{
				boolValue = default(bool);
				return false;
			}
		}

		public bool TryGetArray(string key, out JSONArray arrayValue)
		{
			var value = GetValue(key);
			if(value != null)
			{
				arrayValue = value.Array;
				return true;
			}
			else
			{
				arrayValue = default(JSONArray);
				return false;
			}
		}

		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append('{');

			foreach(var pair in dictionary)
			{
				stringBuilder.Append("\"" + pair.Key + "\"");
				stringBuilder.Append(':');
				stringBuilder.Append(pair.Value.ToString());
				stringBuilder.Append(',');
			}
			if(dictionary.Count > 0)
			{
				stringBuilder.Remove(stringBuilder.Length - 1, 1);
			}
			stringBuilder.Append('}');
			return stringBuilder.ToString();
		}
	}
}
