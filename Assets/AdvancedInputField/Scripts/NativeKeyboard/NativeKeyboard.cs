// Copyright (c) Jeroen van Pienbroek. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using UnityEngine;
using UnityEngine.EventSystems;

namespace AdvancedInputFieldPlugin
{
	/// <summary>The delegate for Keyboard Height Changed event</summary>
	public delegate void OnKeyboardHeightChangedHandler(int keyboardHeight);

	/// <summary>The delegate for Hardware Keyboard Changed event</summary>
	public delegate void OnHardwareKeyboardChangedHandler(bool connected);

	public enum KeyboardState
	{
		HIDDEN, PENDING_SHOW, VISIBLE, PENDING_HIDE
	}

	/// <summary>Event type of the keyboard callbacks</summary>
	public enum NativeKeyboardEventType
	{
		TEXT_EDIT_UPDATE,
		SHOW,
		HIDE,
		DONE,
		NEXT,
		CANCEL,
		SPECIAL_KEY_PRESSED,
		MOVE_LEFT,
		MOVE_RIGHT,
		MOVE_UP,
		MOVE_DOWN
	}

	/// <summary>Event for keyboard callbacks</summary>
	public struct NativeKeyboardEvent
	{
		public NativeKeyboardEventType type;
		public TextEditFrame textEditFrame;
		public SpecialKeyCode specialKeyCode;
		public bool shift;
		public bool ctrl;

		public NativeKeyboardEvent(NativeKeyboardEventType type, TextEditFrame textEditFrame = default(TextEditFrame), SpecialKeyCode specialKeyCode = default(SpecialKeyCode), bool shift = false, bool ctrl = false)
		{
			this.type = type;
			this.textEditFrame = textEditFrame;
			this.specialKeyCode = specialKeyCode;
			this.shift = shift;
			this.ctrl = ctrl;
		}
	}

	/// <summary>Base class that acts as a bridge for the Native Keyboard for a specific platform</summary>
	public abstract class NativeKeyboard: MonoBehaviour
	{
		/// <summary>Queue with Keyboard events</summary>
		protected ThreadsafeQueue<NativeKeyboardEvent> nativeEventQueue;

		/// <summary>The name of the GameObject used for callbacks</summary>
		protected string gameObjectName;

		/// <summary>The event for Keyboard Height Changed</summary>
		protected event OnKeyboardHeightChangedHandler onKeyboardHeightChanged;

		/// <summary>The event for Hardware Keyboard Changed</summary>
		protected event OnHardwareKeyboardChangedHandler onHardwareKeyboardChanged;

		/// <summary>Indicates whether the state of the keyboard</summary>
		public KeyboardState State { get; set; }

		/// <summary>Indicates whether the native binding is active or not</summary>
		public bool Active { get; private set; }

		/// <summary>Indicates whether a hardware keyboard is connected</summary>
		public bool HardwareKeyboardConnected { get; protected set; }

		/// <summary>Initializes this class</summary>
		/// <param name="gameObjectName">The name of the GameObject to use for callbacks</param>
		internal void Init(string gameObjectName)
		{
			this.gameObjectName = gameObjectName;
			nativeEventQueue = new ThreadsafeQueue<NativeKeyboardEvent>(30);
			Setup();
		}

		/// <summary>Gets and removes next keyboard event</summary>
		/// <param name="keyboardEvent">The output keyboard event</param>
		internal bool PopEvent(out NativeKeyboardEvent keyboardEvent)
		{
			if(nativeEventQueue.Count == 0)
			{
				keyboardEvent = default(NativeKeyboardEvent);
				return false;
			}

			keyboardEvent = nativeEventQueue.Dequeue();
			return true;
		}

		/// <summary>Clears the keyboard event queue</summary>
		internal void ClearEventQueue()
		{
			nativeEventQueue.Clear();
		}

		/// <summary>Adds a KeyboardHeightChangedListener</summary>
		/// <param name="listener">The KeyboardHeightChangedListener to add</param>
		public void AddKeyboardHeightChangedListener(OnKeyboardHeightChangedHandler listener)
		{
			onKeyboardHeightChanged += listener;
		}

		/// <summary>Removes a KeyboardHeightChangedListener</summary>
		/// <param name="listener">The KeyboardHeightChangedListener to remove</param>
		public void RemoveKeyboardHeightChangedListener(OnKeyboardHeightChangedHandler listener)
		{
			onKeyboardHeightChanged -= listener;
		}

		/// <summary>Adds a KeyboardHeightChangedListener</summary>
		/// <param name="listener">The HardwareKeyboardChangedListener to add</param>
		public void AddHardwareKeyboardChangedListener(OnHardwareKeyboardChangedHandler listener)
		{
			onHardwareKeyboardChanged += listener;
		}

		/// <summary>Removes a KeyboardHeightChangedListener</summary>
		/// <param name="listener">The KeyboardHeightChangedListener to remove</param>
		public void RemoveHardwareKeyboardChangedListener(OnHardwareKeyboardChangedHandler listener)
		{
			onHardwareKeyboardChanged -= listener;
		}

		/// <summary>Setups the bridge to the Native Keyboard</summary>
		internal virtual void Setup() { }

		/// <summary>Checks whether the native binding should be active or not</summary>
		internal void UpdateActiveState()
		{
			if(EventSystem.current == null) { return; }

			bool inputFieldSelected = false;
			GameObject currentSelection = EventSystem.current.currentSelectedGameObject;
			if(currentSelection != null)
			{
				inputFieldSelected = currentSelection.GetComponentInParent<AdvancedInputField>() || currentSelection.GetComponentInParent<CanvasFrontRenderer>();
			}

			if(Active != inputFieldSelected)
			{
				Active = inputFieldSelected;
				if(Active)
				{
					EnableUpdates();
					if(Settings.MobileKeyboardBehaviour == MobileKeyboardBehaviour.USE_HARDWARE_KEYBOARD_WHEN_AVAILABLE)
					{
						EnableHardwareKeyboardUpdates();
					}
				}
				else
				{
					DisableUpdates();
					DisableHardwareKeyboardUpdates();
				}
			}
		}

		/// <summary>Enables updates in the native binding</summary>
		public virtual void EnableUpdates() { }

		/// <summary>Disables updates in the native binding</summary>
		public virtual void DisableUpdates() { }

		/// <summary>Enables hardware keyboard updates in the native binding</summary>
		public virtual void EnableHardwareKeyboardUpdates() { }

		/// <summary>Disables hardware keyboard updates in the native binding</summary>
		public virtual void DisableHardwareKeyboardUpdates() { }

		/// <summary>Updates the native text and selection</summary>
		public virtual void UpdateTextEdit(string text, int selectionStartPosition, int selectionEndPosition) { }

		/// <summary>Requests a text edit update (after OS autofills a value)</summary>
		public virtual void RequestTextEditUpdate() { }

		/// <summary>Shows the NativeKeyboard for current platform</summary>
		public virtual void ShowKeyboard(string text, int selectionStartPosition, int selectionEndPosition, NativeKeyboardConfiguration configuration) { }

		/// <summary>Shows the NativeKeyboard for current platform without changing settings</summary>
		public virtual void RestoreKeyboard() { }

		/// <summary>Hides the NativeKeyboard for current platform</summary>
		public virtual void HideKeyboard() { }

		/// <summary>Resets the autofill service for current platform (Android only)</summary>
		public virtual void ResetAutofill() { }

		/// <summary>Resets the autofill service for current platform (Android & iOS only)</summary>
		/// <param name="domainName">The domain name of your website, only needed for iOS</param>
		public virtual void SaveCredentials(string domainName) { }

		/// <summary>(Android only) Starts listening for sms messages with one time codes until timeout (5 minutes)</summary>
		public virtual void StartListeningForOneTimeCodes() { }

		public void OnTextEditUpdate(string text, int selectionStartPosition, int selectionEndPosition)
		{
			TextEditFrame textEditFrame = new TextEditFrame(text, selectionStartPosition, selectionEndPosition);
			nativeEventQueue.Enqueue(new NativeKeyboardEvent(NativeKeyboardEventType.TEXT_EDIT_UPDATE, textEditFrame));
		}

		/// <summary>Event callback when other inputfield got autofilled</summary>
		public void OnAutofillUpdate(string text, AutofillType autofillType)
		{
			AdvancedInputField[] inputfields = GameObject.FindObjectsOfType<AdvancedInputField>();
			int length = inputfields.Length;
			for(int i = 0; i < length; i++) //Find an enabled inputfield with given autofillType
			{
				if(inputfields[i].AutofillType == autofillType)
				{
					inputfields[i].Engine.SetText(text);
				}
			}
		}

		/// <summary>Event callback when the keyboard gets shown</summary>
		public void OnKeyboardShow()
		{
			nativeEventQueue.Enqueue(new NativeKeyboardEvent(NativeKeyboardEventType.SHOW));
			State = KeyboardState.VISIBLE;
		}

		/// <summary>Event callback when the keyboard gets hidden</summary>
		public void OnKeyboardHide()
		{
			nativeEventQueue.Enqueue(new NativeKeyboardEvent(NativeKeyboardEventType.HIDE));
			State = KeyboardState.HIDDEN;
		}

		/// <summary>Event callback when the done key of the keyboard gets pressed</summary>
		public void OnKeyboardDone()
		{
			nativeEventQueue.Enqueue(new NativeKeyboardEvent(NativeKeyboardEventType.DONE));
		}

		/// <summary>Event callback when the next key of the keyboard gets pressed</summary>
		public void OnKeyboardNext()
		{
			nativeEventQueue.Enqueue(new NativeKeyboardEvent(NativeKeyboardEventType.NEXT));
		}

		/// <summary>Event callback when the cancel key of the keyboard (back key on Android) gets pressed</summary>
		public void OnKeyboardCancel()
		{
			nativeEventQueue.Enqueue(new NativeKeyboardEvent(NativeKeyboardEventType.CANCEL));
		}

		/// <summary>Event callback when a special key of the keyboard gets pressed</summary>
		public void OnSpecialKeyPressed(SpecialKeyCode specialKeyCode)
		{
			nativeEventQueue.Enqueue(new NativeKeyboardEvent(NativeKeyboardEventType.SPECIAL_KEY_PRESSED, default(TextEditFrame), specialKeyCode));
		}

		public void OnMoveLeft(bool shift, bool ctrl)
		{
			nativeEventQueue.Enqueue(new NativeKeyboardEvent(NativeKeyboardEventType.MOVE_LEFT, default, default, shift, ctrl));
		}

		public void OnMoveRight(bool shift, bool ctrl)
		{
			nativeEventQueue.Enqueue(new NativeKeyboardEvent(NativeKeyboardEventType.MOVE_RIGHT, default, default, shift, ctrl));
		}

		public void OnMoveUp(bool shift, bool ctrl)
		{
			nativeEventQueue.Enqueue(new NativeKeyboardEvent(NativeKeyboardEventType.MOVE_UP, default, default, shift, ctrl));
		}

		public void OnMoveDown(bool shift, bool ctrl)
		{
			nativeEventQueue.Enqueue(new NativeKeyboardEvent(NativeKeyboardEventType.MOVE_DOWN, default, default, shift, ctrl));
		}

		/// <summary>Event callback when the height of the keyboard has changed</summary>
		public void OnKeyboardHeightChanged(int height)
		{
			if(onKeyboardHeightChanged != null)
			{
				onKeyboardHeightChanged.Invoke(height);
			}

			if(height == 0) //Safety check if something external caused the keyboard to hide
			{
				State = KeyboardState.HIDDEN;
			}
		}

		/// <summary>Event callback when the connectivity of the hardware keyboard has changed</summary>
		public void OnHardwareKeyboardChanged(bool connected)
		{
			HardwareKeyboardConnected = connected;

			if(onHardwareKeyboardChanged != null)
			{
				onHardwareKeyboardChanged.Invoke(connected);
			}
		}
	}
}
