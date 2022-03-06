// Copyright (c) Jeroen van Pienbroek. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace AdvancedInputFieldPlugin
{
	/// <summary>Base class for processing input events</summary>
	public class TextInputHandler
	{
		public InputFieldEngine Engine { get; private set; }

		/// <summary>The InputField</summary>
		public AdvancedInputField InputField { get { return Engine.InputField; } }

		/// <summary>The Canvas</summary>
		public Canvas Canvas { get { return InputField.Canvas; } }

		/// <summary>The TextNavigator</summary>
		public TextNavigator TextNavigator { get; protected set; }

		/// <summary>The TextManipulator</summary>
		public virtual TextManipulator TextManipulator { get; protected set; }

		/// <summary>The character position when press started</summary>
		protected int pressCharPosition;

		/// <summary>The time of last tap</summary>
		protected float lastTapTime;

		/// <summary>The time input is currently holding</summary>
		protected float holdTime;

		/// <summary>Indicates whether input is currently holding</summary>
		protected bool holding;

		/// <summary>Character position of last event</summary>
		protected Vector2 lastPosition;

		/// <summary>The ActionBar</summary>
		protected ActionBar actionBar;

		/// <summary>The amount of taps (rapidly after each other)</summary>
		private int tapCount;

		/// <summary>The character position when press started</summary>
		public int PressCharPosition { get { return pressCharPosition; } }

		/// <summary>Character position of last event</summary>
		public Vector2 LastPosition { get { return lastPosition; } set { lastPosition = value; } }

		public TextInputHandler()
		{
		}

		/// <summary>Initializes this class</summary>
		internal virtual void Initialize(InputFieldEngine engine, TextNavigator textNavigator, TextManipulator textManipulator)
		{
			Engine = engine;
			TextNavigator = textNavigator;
			TextManipulator = textManipulator;

			TextNavigator.ActionBar = actionBar;
		}

		/// <summary>Initializes the ActionBar</summary>
		/// <param name="textRenderer">The text renderer to attach the ActionBar to</param>
		internal void InitActionBar(InputFieldEngine engine)
		{
			Engine = engine;
			actionBar = Object.Instantiate(Settings.ActionBarPrefab);

			if(TextNavigator != null && TextNavigator.ActionBar == null)
			{
				TextNavigator.ActionBar = actionBar;
			}

			actionBar.transform.SetParent(InputField.transform);
			actionBar.transform.localScale = Vector3.one;
			actionBar.CheckInputFieldScale();
			actionBar.transform.localPosition = Vector3.zero;

			actionBar.Initialize(InputField, this, TextNavigator);
		}

		internal virtual void OnCanvasScaleChanged(float canvasScaleFactor)
		{
		}

		/// <summary>Processes input events</summary>
		internal virtual void Process()
		{
			UpdateHold();
		}

		internal void BreakHold()
		{
			holding = false;
		}

		internal virtual void UpdateHold()
		{
			if(holding && holdTime < Settings.HoldThreshold)
			{
				holdTime += Time.deltaTime;
				if(holdTime >= Settings.HoldThreshold)
				{
					OnHold(lastPosition);
				}
			}
		}

		internal virtual void BeginEditMode()
		{

		}

		/// <summary>Event callback when selection started</summary>
		internal virtual void OnSelect()
		{
		}

		/// <summary>Event callback for input press</summary>
		/// <param name="position">The position of the event</param>
		internal virtual void OnPress(Vector2 position)
		{
			if(InputField.LiveDecoration)
			{
				pressCharPosition = TextNavigator.GetCharacterIndexFromPosition(InputField.ProcessedTextRenderer, position);
			}
			else
			{
				pressCharPosition = TextNavigator.GetCharacterIndexFromPosition(InputField.TextRenderer, position);
			}
			holding = true;
			holdTime = 0;
			lastPosition = position;
		}

		/// <summary>Event callback for input drag</summary>
		/// <param name="position">The position of the event</param>
		internal virtual void OnDrag(Vector2 position)
		{
			int holdCharPosition;
			if(InputField.LiveDecoration)
			{
				holdCharPosition = TextNavigator.GetCharacterIndexFromPosition(InputField.ProcessedTextRenderer, position);
			}
			else
			{
				holdCharPosition = TextNavigator.GetCharacterIndexFromPosition(InputField.TextRenderer, position);
			}

			if(holdCharPosition != pressCharPosition)
			{
				holding = false;
			}

			lastPosition = position;
		}

		/// <summary>Event callback for input release</summary>
		/// <param name="position">The position of the event</param>
		internal virtual void OnRelease(Vector2 position)
		{
			int releaseCharPosition;
			if(InputField.LiveDecoration)
			{
				releaseCharPosition = TextNavigator.GetCharacterIndexFromPosition(InputField.ProcessedTextRenderer, position);
			}
			else
			{
				releaseCharPosition = TextNavigator.GetCharacterIndexFromPosition(InputField.TextRenderer, position);
			}

			if(pressCharPosition == releaseCharPosition)
			{
				if(holdTime < Settings.HoldThreshold)
				{
					OnTap(position);
				}
			}
			else
			{
				lastTapTime = 0;
			}

			holding = false;
			lastPosition = position;
		}

		/// <summary>Event callback for input tap</summary>
		/// <param name="position">The position of th event</param>
		internal virtual void OnTap(Vector2 position)
		{
#if UNITY_STANDALONE || UNITY_WSA
			if(StandaloneTap(position)) { return; }
#endif

			if(lastTapTime > 0 && Time.realtimeSinceStartup - lastTapTime <= Settings.DoubleTapThreshold)
			{
				tapCount++;
				if(tapCount == 2)
				{
					OnDoubleTap(position);
				}
			}
			else
			{
				tapCount = 1;
				OnSingleTap(position);
			}

			Engine.EventHandler.InvokeTextTap(tapCount, position);
			lastTapTime = Time.realtimeSinceStartup;
		}

#if UNITY_STANDALONE || UNITY_WSA
		private bool StandaloneTap(Vector2 position)
		{
			if(actionBar != null)
			{
#if ENABLE_INPUT_SYSTEM
				if(Mouse.current.leftButton.wasReleasedThisFrame) //Left mouse button was clicked
#else
				if(Input.GetMouseButtonUp(0)) //Left mouse button was clicked
#endif
				{
					if(lastTapTime > 0 && Time.realtimeSinceStartup - lastTapTime <= Settings.DoubleTapThreshold)
					{
						tapCount++;
						if(tapCount == 2)
						{
							OnDoubleTap(position);
						}
					}
					else
					{
						tapCount = 1;
						OnSingleTap(position);
					}

					Engine.EventHandler.InvokeTextTap(tapCount, position);
					lastTapTime = Time.realtimeSinceStartup;
					TextNavigator.HideActionBar();
				}
#if ENABLE_INPUT_SYSTEM
				else if(Mouse.current.rightButton.wasReleasedThisFrame) //Right mouse button was clicked
#else
				else if(Input.GetMouseButtonUp(1)) //Right mouse button was clicked
#endif
				{
					if(Engine.HasSelection)
					{
						int visibleCaretPosition = TextNavigator.DetermineVisibleCaretPosition(position);
						if(visibleCaretPosition < Engine.VisibleSelectionStartPosition || visibleCaretPosition >= Engine.VisibleSelectionEndPosition) //Only change caret if clicked outside of selection
						{
							TextNavigator.ResetCaret(position);
						}
					}
					else
					{
						TextNavigator.ResetCaret(position);
					}
					TextNavigator.ShowActionBar();
				}

				return true;
			}

			return false;
		}
#endif

		/// <summary>Event callback for single tap</summary>
		/// <param name="position">The position of th event</param>
		internal virtual void OnSingleTap(Vector2 position)
		{
			TextNavigator.ResetCaret(position);
			Engine.HasModifiedTextAfterClick = false;
		}

		/// <summary>Event callback for double tap</summary>
		/// <param name="position">The position of th event</param>
		internal virtual void OnDoubleTap(Vector2 position)
		{
			TextNavigator.SelectCurrentWord();
		}

		/// <summary>Event callback for input hold</summary>
		/// <param name="position">The position of the event</param>
		internal virtual void OnHold(Vector2 position)
		{
			if(InputField.Text.Length > 0)
			{
				TextNavigator.SelectCurrentWord();
				if(!Engine.HasSelection && InputField.CanUseActionBar) //Just show the ActionBar when no word was selected
				{
					if(TextNavigator.ActionBar == null)
					{
						InitActionBar(Engine);
					}
					TextNavigator.ShowActionBar();
				}
			}
			else if(InputField.CanUseActionBar)
			{
				if(TextNavigator.ActionBar == null)
				{
					InitActionBar(Engine);
				}
				TextNavigator.ShowActionBar();
				if(!Engine.EditMode)
				{
					TextNavigator.KeepActionBarVisible = true; //Workaround to keep the ActionBar visible when beginning edit mode
				}
			}
		}

		/// <summary>Cancels the input</summary>
		internal virtual void CancelInput()
		{
			NativeKeyboard keyboard = Engine.KeyboardClient.Keyboard;
			if(keyboard != null)
			{
				if(EventSystem.current == null) { return; }

				GameObject currentSelectedGameObject = EventSystem.current.currentSelectedGameObject;
				if(currentSelectedGameObject != null)
				{
					AdvancedInputField currentSelectedInputField = currentSelectedGameObject.GetComponentInParent<AdvancedInputField>();
					if(currentSelectedInputField != null && !currentSelectedInputField.ReadOnly)
					{
						if(currentSelectedInputField != InputField.NextInputField)
						{
							BeginEditReason beginEditReason = BeginEditReason.PROGRAMMATIC_SELECT;
							if(currentSelectedInputField.UserPressing)
							{
								beginEditReason = BeginEditReason.USER_SELECT;
							}

							currentSelectedInputField.ManualSelect(beginEditReason);
						}

						return; //Don't hide keyboard, next inputfield is selected
					}
				}

#if !UNITY_EDITOR
				keyboard.State = KeyboardState.PENDING_HIDE;
#endif
				keyboard.HideKeyboard();
				keyboard.ClearEventQueue(); //Should be last event to process, so clear queue
			}
		}

		/// <summary>Event callback when cut button has been clicked</summary>
		public virtual void OnCut()
		{
			TextManipulator.Cut();
		}

		/// <summary>Event callback when copy button has been clicked</summary>
		public virtual void OnCopy()
		{
			TextManipulator.Copy();
		}

		/// <summary>Event callback when paste button has been clicked</summary>
		public virtual void OnPaste()
		{
			TextManipulator.Paste();
		}

		/// <summary>Event callback when select all button has been clicked</summary>
		public virtual void OnSelectAll()
		{
			TextNavigator.SelectAll();
		}

		/// <summary>Event callback when replace button has been clicked</summary>
		public virtual void OnReplace(string text)
		{
			TextManipulator.Replace(text);
		}
	}
}
