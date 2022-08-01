using UnityEngine;

namespace AdvancedInputFieldPlugin
{
	public class KeyboardClient: MonoBehaviour
	{
		public NativeKeyboard Keyboard { get { return NativeKeyboardManager.Keyboard; } }

		protected virtual void Awake()
		{
			enabled = false;
		}

		public void ShowKeyboard(string text, int selectionStartPosition, int selectionEndPosition, NativeKeyboardConfiguration configuration)
		{
			Keyboard.ShowKeyboard(text, selectionStartPosition, selectionEndPosition, configuration);
		}

		public void HideKeyboard()
		{
			Keyboard.HideKeyboard();
		}

		public virtual void UpdateTextEdit(string text, int selectionStartPosition, int selectionEndPosition)
		{
			Keyboard.UpdateTextEdit(text, selectionStartPosition, selectionEndPosition);
		}

		public virtual void Activate()
		{
			enabled = true;
			Keyboard.UpdateActiveState();
		}

		public void Deactivate()
		{
			enabled = false;
			Keyboard.UpdateActiveState();
		}

		public void ClearEventQueue()
		{
			Keyboard.ClearEventQueue();
		}

		private void Update()
		{
			NativeKeyboardEvent keyboardEvent;
			while(Keyboard.PopEvent(out keyboardEvent))
			{
				switch(keyboardEvent.type)
				{
					case NativeKeyboardEventType.TEXT_EDIT_UPDATE: OnTextEditUpdate(keyboardEvent); break;
					case NativeKeyboardEventType.SHOW: OnShow(keyboardEvent); break;
					case NativeKeyboardEventType.HIDE: OnHide(keyboardEvent); break;
					case NativeKeyboardEventType.DONE: OnDone(keyboardEvent); break;
					case NativeKeyboardEventType.NEXT: OnNext(keyboardEvent); break;
					case NativeKeyboardEventType.CANCEL: OnCancel(keyboardEvent); break;
					case NativeKeyboardEventType.SPECIAL_KEY_PRESSED: OnSpecialKeyPressed(keyboardEvent); break;
					case NativeKeyboardEventType.MOVE_LEFT: OnMoveLeft(keyboardEvent); break;
					case NativeKeyboardEventType.MOVE_RIGHT: OnMoveRight(keyboardEvent); break;
					case NativeKeyboardEventType.MOVE_UP: OnMoveUp(keyboardEvent); break;
					case NativeKeyboardEventType.MOVE_DOWN: OnMoveDown(keyboardEvent); break;
				}
			}
		}

		public virtual void OnTextEditUpdate(NativeKeyboardEvent keyboardEvent)
		{
		}

		/// <summary>Processes keyboard show event</summary>
		public virtual void OnShow(NativeKeyboardEvent keyboardEvent)
		{
		}

		/// <summary>Processes keyboard hide event</summary>
		public virtual void OnHide(NativeKeyboardEvent keyboardEvent)
		{
		}

		/// <summary>Processes keyboard done event</summary>
		public virtual void OnDone(NativeKeyboardEvent keyboardEvent)
		{
		}

		public virtual void OnNext(NativeKeyboardEvent keyboardEvent)
		{
		}

		/// <summary>Processes keyboard cancel event</summary>
		public virtual void OnCancel(NativeKeyboardEvent keyboardEvent)
		{
		}

		/// <summary>Processes special key pressed event</summary>
		public virtual void OnSpecialKeyPressed(NativeKeyboardEvent keyboardEvent)
		{
		}

		public virtual void OnMoveLeft(NativeKeyboardEvent keyboardEvent)
		{
		}

		public virtual void OnMoveRight(NativeKeyboardEvent keyboardEvent)
		{
		}

		public virtual void OnMoveUp(NativeKeyboardEvent keyboardEvent)
		{
		}

		public virtual void OnMoveDown(NativeKeyboardEvent keyboardEvent)
		{
		}
	}
}
