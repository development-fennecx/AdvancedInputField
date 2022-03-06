using System;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;

namespace NativeKeyboardUWP
{
	public class Util
	{
		public static async void RunOnUIThread(DispatchedHandler action)
		{
			await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, action);
		}

		public static int Clamp(int value, int min, int max)
		{
			value = Math.Max(value, min);
			value = Math.Min(value, max);

			return value;
		}

		public static float Clamp(float value, float min, float max)
		{
			value = Math.Max(value, min);
			value = Math.Min(value, max);

			return value;
		}

		public static bool Contains(char ch, char[] text, int textLength)
		{
			for(int i = 0; i < textLength; i++)
			{
				if(text[i] == ch) { return true; }
			}

			return false;
		}

		public static int IndexOf(char ch, char[] text, int textLength)
		{
			for(int i = 0; i < textLength; i++)
			{
				if(text[i] == ch) { return i; }
			}

			return -1;
		}

		public static int LastIndexOf(char ch, char[] text, int textLength)
		{
			for(int i = textLength - 1; i >= 0; i--)
			{
				if(text[i] == ch) { return i; }
			}

			return -1;
		}

		public static int CountOccurences(char ch, char[] text, int textLength)
		{
			int occurences = 0;

			for(int i = 0; i < textLength; i++)
			{
				if(text[i] == ch)
				{
					occurences++;
				}
			}

			return occurences;
		}

		public static void StringCopy(ref char[] destination, string source)
		{
			int length = source.Length;
			for(int i = 0; i < length; i++)
			{
				destination[i] = source[i];
			}
		}
	}
}
