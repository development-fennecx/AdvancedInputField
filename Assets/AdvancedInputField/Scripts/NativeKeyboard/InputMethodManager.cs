//-----------------------------------------
//			Advanced Input Field
// Copyright (c) 2017 Jeroen van Pienbroek
//------------------------------------------

using System.Collections.Generic;
using UnityEngine;

namespace AdvancedInputFieldPlugin
{
	public enum InputEventType { CHARACTER, TEXT }

	public abstract class InputEvent
	{
		public abstract InputEventType Type { get; }
	}

	public class CharacterInputEvent: InputEvent
	{
		public override InputEventType Type { get { return InputEventType.CHARACTER; } }
		public char character;

		public CharacterInputEvent(char character)
		{
			this.character = character;
		}
	}

	public class TextInputEvent: InputEvent
	{
		public override InputEventType Type { get { return InputEventType.TEXT; } }
		public string text;

		public TextInputEvent(string text)
		{
			this.text = text;
		}
	}

	public class InputMethodManager: MonoBehaviour
	{
		/// <summary>The singleton instance of InputMethodManager</summary>
		private static InputMethodManager instance;

		private Queue<InputEvent> eventQueue;

		private static InputMethodManager Instance
		{
			get
			{
				if(instance == null)
				{
					instance = GameObject.FindObjectOfType<InputMethodManager>();
					if(instance == null)
					{
						GameObject gameObject = new GameObject("InputMethodManager");
						instance = gameObject.AddComponent<InputMethodManager>();
					}
				}

				return instance;
			}
		}

		private void Awake()
		{
			eventQueue = new Queue<InputEvent>();
		}

		private void OnDestroy()
		{
			instance = null;
		}

		public static void TryDestroy()
		{
			if(instance != null && instance.gameObject != null)
			{
				Destroy(instance.gameObject);
			}
		}

		public static void AddCharacterInputEvent(char character)
		{
			Instance.eventQueue.Enqueue(new CharacterInputEvent(character));
		}

		public static void AddTextInputEvent(string text)
		{
			Instance.eventQueue.Enqueue(new TextInputEvent(text));
		}

		public static bool PopEvent(out InputEvent inputEvent)
		{
			if(Instance.eventQueue.Count == 0)
			{
				inputEvent = null;
				return false;
			}
			else
			{
				inputEvent = Instance.eventQueue.Dequeue();
				return true;
			}
		}

		public static void ClearEventQueue()
		{
			Instance.eventQueue.Clear();
		}
	}
}
