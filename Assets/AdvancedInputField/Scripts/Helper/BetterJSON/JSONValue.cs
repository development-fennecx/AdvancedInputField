using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace BetterJSON
{
	public enum JSONValueType
	{
		String,
		Integer,
		Double,
		Boolean,
		Object,
		Array
	}

	public abstract class JSONValue
	{
		public abstract JSONValueType Type { get; }
		public virtual string String { get { return null; } }
		public virtual int Integer { get { return -1; } }
		public virtual double Double { get { return -1; } }
		public virtual bool Boolean { get { return false; } }
		public virtual JSONObject Object { get { return null; } }
		public virtual JSONArray Array { get { return null; } }

		public JSONValue(string valueString)
		{
			Parse(valueString);
		}

		public abstract void Parse(string valueString);
	}

	public class StringValue: JSONValue
	{
		private static readonly Regex unicodeRegex = new Regex(@"\\u([0-9a-fA-F]{4})");
		private static readonly byte[] unicodeBytes = new byte[2];

		private string value;

		public override JSONValueType Type { get { return JSONValueType.String; } }
		public override string String { get { return value; } }

		public StringValue(string valueString) : base(valueString) { }

		public override void Parse(string valueString)
		{
			while(true)
			{
				Match m = unicodeRegex.Match(valueString);
				if(!m.Success)
				{
					break;
				}

				string s = m.Groups[1].Captures[0].Value;
				unicodeBytes[1] = byte.Parse(s.Substring(0, 2), NumberStyles.HexNumber);
				unicodeBytes[0] = byte.Parse(s.Substring(2, 2), NumberStyles.HexNumber);
				s = Encoding.Unicode.GetString(unicodeBytes, 0, unicodeBytes.Length);

				valueString = valueString.Replace(m.Value, s);
			}

			this.value = SanitizeJSONString(valueString);
		}

		private static string SanitizeJSONString(string jsonStringValue)
		{
			StringBuilder stringBuilder = new StringBuilder(jsonStringValue);
			stringBuilder.Replace("\\\"", "\"");
			stringBuilder.Replace("\\\\", "\\");
			stringBuilder.Replace("\\/", "/");
			stringBuilder.Replace("\\\b", "\b");
			stringBuilder.Replace("\\\f", "\f");
			stringBuilder.Replace("\\\n", "\n");
			stringBuilder.Replace("\\\r", "\r");
			stringBuilder.Replace("\\\t", "\t");
			return stringBuilder.ToString();
		}

		public override string ToString()
		{
			return "\"" + String + "\"";
		}
	}

	public class IntegerValue: JSONValue
	{
		private int value;

		public override JSONValueType Type { get { return JSONValueType.Integer; } }
		public override int Integer { get { return value; } }

		public IntegerValue(string valueString) : base(valueString) { }

		public override void Parse(string valueString)
		{
			int.TryParse(valueString, out value);
		}

		public override string ToString()
		{
			return Integer.ToString();
		}
	}

	public class DoubleValue: JSONValue
	{
		private double value;

		public override JSONValueType Type { get { return JSONValueType.Double; } }
		public override double Double { get { return value; } }

		public DoubleValue(string valueString) : base(valueString) { }

		public override void Parse(string valueString)
		{
			double.TryParse(valueString, out value);
		}

		public override string ToString()
		{
			return Double.ToString();
		}
	}

	public class BooleanValue: JSONValue
	{
		private bool value;

		public override JSONValueType Type { get { return JSONValueType.Boolean; } }
		public override bool Boolean { get { return value; } }

		public BooleanValue(string valueString) : base(valueString) { }

		public override void Parse(string valueString)
		{
			bool.TryParse(valueString, out value);
		}

		public override string ToString()
		{
			return Boolean ? "true" : "false";
		}
	}

	public class ObjectValue: JSONValue
	{
		private JSONObject value;

		public override JSONValueType Type { get { return JSONValueType.Object; } }
		public override JSONObject Object { get { return value; } }

		public ObjectValue(string valueString) : base(valueString) { }

		public override void Parse(string valueString)
		{
			value = new JSONObject(valueString);
		}

		public override string ToString()
		{
			return Object.ToString();
		}
	}

	public class ArrayValue: JSONValue
	{
		private JSONArray value;

		public override JSONValueType Type { get { return JSONValueType.Array; } }
		public override JSONArray Array { get { return value; } }

		public ArrayValue(string valueString) : base(valueString) { }

		public override void Parse(string valueString)
		{
			value = new JSONArray(valueString);
		}

		public override string ToString()
		{
			return Array.ToString();
		}
	}
}
