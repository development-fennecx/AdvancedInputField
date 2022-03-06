using BetterJSON;
using System;
using Windows.Devices.Input;
using Windows.Foundation.Metadata;
using Windows.Graphics.Display;
using Windows.System;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace NativeKeyboardUWP
{
	public enum KeyboardState
	{
		HIDDEN,
		PENDING_SHOW,
		VISIBLE,
		PENDING_HIDE
	}

	public enum KeyboardType
	{
		DEFAULT,
		ASCII_CAPABLE,
		NUMBERS_AND_PUNCTUATION,
		URL,
		NUMBER_PAD,
		PHONE_PAD,
		EMAIL_ADDRESS
	}

	public enum CharacterValidation
	{
		NONE,
		INTEGER,
		DECIMAL,
		ALPHANUMERIC,
		NAME,
		EMAIL_ADDRESS,
		IP_ADDRESS,
		SENTENCE,
		CUSTOM
	}

	public enum LineType
	{
		SINGLE_LINE,
		MULTI_LINE_SUBMIT,
		MULTI_LINE_NEWLINE
	}

	public enum AutocapitalizationType
	{
		NONE,
		CHARACTERS,
		WORDS,
		SENTENCES
	}

	public enum ReturnKeyType
	{
		DEFAULT,
		GO,
		SEND,
		SEARCH
	}

	public enum EventType
	{
		TEXT_CHANGE,
		TEXT_EDIT_UPDATE,
		KEYBOARD_SHOW,
		KEYBOARD_HIDE
	}

	public enum DeviceType
	{
		PHONE,
		TABLET,
		DESKTOP,
		XBOX,
		IOT,
		CONTINUUM
	};

	public class NativeKeyboard
	{
		private const int UPDATE_FREQUENCY = 100;
		private const int HARDWARE_KEYBOARD_FREQUENCY = 3000;
		private const float MIN_VISIBLE_TIME_FOR_CANCEL = 1.5f;
		private const float MAX_PENDING_TIME = 1.5f;

		private ThreadsafeQueue<IUnityEvent> unityEventQueue;
		private IUnityEvent currentEvent;
		private INativeKeyboardCallback unityCallback;
		private bool updatesEnabled;
		private bool cancelUpdateWhenDone;
		private bool hardwareKeyboardUpdatesEnabled;
		private bool cancelHardwareKeyboardUpdateWhenDone;

		private DummyView dummyView;
		private CharacterValidation characterValidation;
		private bool emojisAllowed;
		private KeyboardState state;
		private int lastKeyboardHeight;
		private TextValidator textValidator;

		private DispatcherTimer updateTimer;
		private DispatcherTimer hardwareKeyboardUpdateTimer;
		private Panel panel;
		private Canvas canvas;
		private Button dummyButton; //Needed to block unfocus of TextBox
		private InputPane inputPane;
		private double lastKnownActiveKeyboardHeight;
		private bool manualUnfocus;
		private bool keyboardVisible;
		private bool ignoreTextChange;
		private bool hardwareKeyboardConnected;
		private DeviceType deviceType;
		private DateTimeOffset visibleStartTime;
		private DateTimeOffset pendingStartTime;

		private String lastText;
		private int lastSelectionStartPosition;
		private int lastSelectionEndPosition;

		#region LIFECYCLE
		public NativeKeyboard()
		{
			unityEventQueue = new ThreadsafeQueue<IUnityEvent>();
			textValidator = new TextValidator();
			state = KeyboardState.HIDDEN;
		}

		public void Initialize(INativeKeyboardCallback unityCallback, Panel panel)
		{
			this.unityCallback = unityCallback;
			this.panel = panel;

			dummyView = new DummyView();
			dummyView.Width = 480;
			dummyView.Height = 80;
			dummyView.HorizontalAlignment = HorizontalAlignment.Stretch;
			dummyView.VerticalAlignment = VerticalAlignment.Top;
			dummyView.AllowFocusOnInteraction = false;

			canvas = new Canvas();
			Canvas.SetLeft(dummyView, -dummyView.Width * 2);
			Canvas.SetTop(dummyView, -dummyView.Height * 2);
			canvas.Children.Add(dummyView);
			panel.Children.Add(canvas);

			dummyButton = new Button();
			dummyButton.HorizontalAlignment = HorizontalAlignment.Stretch;
			dummyButton.VerticalAlignment = VerticalAlignment.Stretch;
			dummyButton.AllowFocusOnInteraction = false;
			dummyButton.Opacity = 0;
			dummyButton.IsEnabled = true;
			panel.Children.Add(dummyButton);

			dummyView.TextChanging += OnTextChanging;
			dummyView.TextChanged += OnTextChanged;
			dummyView.KeyDown += OnKeyDown;
			dummyView.LostFocus += OnFocusLost;
			dummyView.GotFocus += OnGotFocus;

			inputPane = InputPane.GetForCurrentView();
			inputPane.Showing += OnKeyboardShowing;
			inputPane.Hiding += OnKeyboardHiding;

			updateTimer = new DispatcherTimer();
			updateTimer.Interval = TimeSpan.FromMilliseconds(UPDATE_FREQUENCY);
			updateTimer.Tick += Update;

			hardwareKeyboardUpdateTimer = new DispatcherTimer();
			hardwareKeyboardUpdateTimer.Interval = TimeSpan.FromMilliseconds(HARDWARE_KEYBOARD_FREQUENCY);
			hardwareKeyboardUpdateTimer.Tick += UpdateHardwareKeyboardConnectivity;

			UpdateDeviceType();
			UpdateHardwareKeyboardConnectivity(null, null);
		}

		public void Destroy()
		{
			Util.RunOnUIThread(
			() =>
			{
				panel.Children.Remove(canvas);
				updateTimer.Stop();
				hardwareKeyboardUpdateTimer.Stop();
			});
		}
		#endregion

		#region PROCESS
		private void Update(object sender, object e)
		{
			if(currentEvent != null && currentEvent.Type == EventType.KEYBOARD_SHOW && state == KeyboardState.VISIBLE)
			{
				if(manualUnfocus) //Wait until unfocus event is processed
				{
					return;
				}
				else
				{
					FinishKeyboardShowEvent((KeyboardShowEvent)currentEvent);
				}
			}

			if(state == KeyboardState.PENDING_SHOW)
			{
				if(!keyboardVisible && (DateTimeOffset.Now - pendingStartTime).TotalSeconds <= MAX_PENDING_TIME)
				{
					dummyView.Focus(FocusState.Programmatic);
					inputPane.TryShow();
					return;
				}

				state = KeyboardState.VISIBLE;
				visibleStartTime = DateTimeOffset.Now;
				unityCallback.OnKeyboardShow();
			}
			else if(state == KeyboardState.PENDING_HIDE)
			{
				if(keyboardVisible && (DateTimeOffset.Now - pendingStartTime).TotalSeconds <= MAX_PENDING_TIME)
				{
					Unfocus(dummyView);
					inputPane.TryHide();
					return;
				}

				state = KeyboardState.HIDDEN;
				unityCallback.OnKeyboardHide();
			}

			currentEvent = null;
			IUnityEvent unityEvent = null;
			while(PopEvent(out unityEvent))
			{
				currentEvent = unityEvent;
				switch(currentEvent.Type)
				{
					case EventType.TEXT_EDIT_UPDATE: ProcessTextEditUpdateEvent((TextEditUpdateEvent)currentEvent); break;
					case EventType.KEYBOARD_SHOW: ProcessKeyboardShowEvent((KeyboardShowEvent)currentEvent); break;
					case EventType.KEYBOARD_HIDE: ProcessKeyboardHideEvent((KeyboardHideEvent)currentEvent); break;
				}
			}

			UpdateKeyboardHeight();

			if(cancelUpdateWhenDone && unityEventQueue.Count == 0 && currentEvent == null)
			{
				cancelUpdateWhenDone = false;
				updateTimer.Stop();
			}
		}

		private bool PopEvent(out IUnityEvent unityEvent)
		{
			if(unityEventQueue.Count == 0)
			{
				unityEvent = null;
				return false;
			}

			unityEvent = unityEventQueue.Dequeue();
			return true;
		}

		private void ProcessTextEditUpdateEvent(TextEditUpdateEvent textEditUpdateEvent)
		{
			string text = textEditUpdateEvent.text;
			int selectionStartPosition = textEditUpdateEvent.selectionStartPosition;
			int selectionEndPosition = textEditUpdateEvent.selectionEndPosition;
			ApplyTextEditUpdate(text, selectionStartPosition, selectionEndPosition);
		}

		private void ApplyTextEditUpdate(string text, int selectionStartPosition, int selectionEndPosition)
		{
			try
			{
				if(text != lastText)
				{
					dummyView.Text = text;
					lastText = text;

					dummyView.Select(selectionStartPosition, selectionEndPosition - selectionStartPosition); //Always update selection after text change
					lastSelectionStartPosition = selectionStartPosition;
					lastSelectionEndPosition = selectionEndPosition;
				}
				else if(selectionStartPosition != lastSelectionStartPosition || selectionEndPosition != lastSelectionEndPosition)
				{
					dummyView.Select(selectionStartPosition, selectionEndPosition - selectionStartPosition);
					lastSelectionStartPosition = selectionStartPosition;
					lastSelectionEndPosition = selectionEndPosition;
				}
			}
			catch { }
		}

		private void ProcessKeyboardShowEvent(KeyboardShowEvent keyboardShowEvent)
		{
			NativeKeyboardConfiguration configuration = keyboardShowEvent.configuration;
			characterValidation = configuration.characterValidation;
			emojisAllowed = configuration.emojisAllowed;
			LineType lineType = configuration.lineType;
			CharacterValidator characterValidator = configuration.characterValidator;
			textValidator.Validation = characterValidation;
			textValidator.LineType = lineType;
			textValidator.Validator = characterValidator;

			if(state == KeyboardState.VISIBLE)
			{
				pendingStartTime = DateTimeOffset.Now;
				visibleStartTime = DateTimeOffset.Now;
				Unfocus(dummyView); //Will perform final step after unfocus event is processed
			}
			else
			{
				FinishKeyboardShowEvent(keyboardShowEvent);
			}
		}

		private void Unfocus(Control control)
		{
			manualUnfocus = true;
			bool isTabStop = control.IsTabStop;
			control.IsTabStop = false;
			control.IsEnabled = false;
			control.IsEnabled = true;
			control.IsTabStop = isTabStop;
		}

		private void FinishKeyboardShowEvent(KeyboardShowEvent keyboardShowEvent)
		{
			String text = keyboardShowEvent.text;
			int selectionStartPosition = keyboardShowEvent.selectionStartPosition;
			int selectionEndPosition = keyboardShowEvent.selectionEndPosition;
			ApplyTextEditUpdate(text, selectionStartPosition, selectionEndPosition);

			NativeKeyboardConfiguration configuration = keyboardShowEvent.configuration;
			dummyView.MaxLength = configuration.characterLimit;
			ConfigureKeyboardType(keyboardShowEvent);

			state = KeyboardState.PENDING_SHOW;
			pendingStartTime = DateTimeOffset.Now;
			visibleStartTime = DateTimeOffset.Now;
			dummyView.Focus(FocusState.Programmatic);
			inputPane.TryShow();
		}

		private void ConfigureKeyboardType(KeyboardShowEvent keyboardShowEvent)
		{
			NativeKeyboardConfiguration configuration = keyboardShowEvent.configuration;
			KeyboardType keyboardType = configuration.keyboardType;
			LineType lineType = configuration.lineType;
			AutocapitalizationType autocapitalizationType = configuration.autocapitalizationType;
			bool secure = configuration.secure;
			bool autocorrection = configuration.autocorrection;

			InputScope inputScope = new InputScope();
			InputScopeName scopeName = new InputScopeName();
			if(lineType == LineType.MULTI_LINE_NEWLINE)
			{
				scopeName.NameValue = InputScopeNameValue.Default;
				inputScope.Names.Add(scopeName);
			}
			else if(secure)
			{
				switch(keyboardType)
				{
					case KeyboardType.DEFAULT:
						scopeName.NameValue = InputScopeNameValue.Password;
						break;
					case KeyboardType.ASCII_CAPABLE:
						scopeName.NameValue = InputScopeNameValue.Password;
						break;
					case KeyboardType.NUMBERS_AND_PUNCTUATION:
						scopeName.NameValue = InputScopeNameValue.NumericPin;
						break;
					case KeyboardType.URL:
						scopeName.NameValue = InputScopeNameValue.Password;
						break;
					case KeyboardType.NUMBER_PAD:
						scopeName.NameValue = InputScopeNameValue.NumericPin;
						break;
					case KeyboardType.PHONE_PAD:
						scopeName.NameValue = InputScopeNameValue.NumericPin;
						break;
					case KeyboardType.EMAIL_ADDRESS:
						scopeName.NameValue = InputScopeNameValue.Password;
						break;
				}
				inputScope.Names.Add(scopeName);
			}
			else
			{
				switch(keyboardType)
				{
					case KeyboardType.DEFAULT:
						scopeName.NameValue = InputScopeNameValue.Default;
						break;
					case KeyboardType.ASCII_CAPABLE:
						scopeName.NameValue = InputScopeNameValue.Text;
						break;
					case KeyboardType.NUMBERS_AND_PUNCTUATION:
						scopeName.NameValue = InputScopeNameValue.CurrencyAmount;
						break;
					case KeyboardType.URL:
						scopeName.NameValue = InputScopeNameValue.Url;
						break;
					case KeyboardType.NUMBER_PAD:
						scopeName.NameValue = InputScopeNameValue.Number;
						break;
					case KeyboardType.PHONE_PAD:
						scopeName.NameValue = InputScopeNameValue.TelephoneNumber;
						break;
					case KeyboardType.EMAIL_ADDRESS:
						scopeName.NameValue = InputScopeNameValue.EmailNameOrAddress;
						break;
				}

				inputScope.Names.Add(scopeName);
			}
			dummyView.InputScope = inputScope;

			if(autocorrection && !secure && !(keyboardType == KeyboardType.NUMBERS_AND_PUNCTUATION || keyboardType == KeyboardType.NUMBER_PAD))
			{
				dummyView.IsSpellCheckEnabled = true;
				dummyView.IsTextPredictionEnabled = true;
			}
			else
			{
				dummyView.IsSpellCheckEnabled = false;
				dummyView.IsTextPredictionEnabled = false;
			}

			if(autocapitalizationType != AutocapitalizationType.NONE)
			{
				dummyView.IsSpellCheckEnabled = true; //This also controls autocapitalization, no specific types are available on UWP
			}

			dummyView.AcceptsReturn = (lineType == LineType.MULTI_LINE_NEWLINE);
		}

		private void ProcessKeyboardHideEvent(KeyboardHideEvent keyboardHideEvent)
		{
			Unfocus(dummyView);
			state = KeyboardState.PENDING_HIDE;
			pendingStartTime = DateTimeOffset.Now;
			inputPane.TryHide();
		}

		private void UpdateKeyboardHeight()
		{
			double height = inputPane.OccludedRect.Height;
			if(height > 0)
			{
				lastKnownActiveKeyboardHeight = height;
			}
			else if(height == 0 && inputPane.OccludedRect.Top > 0)
			{
				height = lastKnownActiveKeyboardHeight; //Attempt to correct non full width keyboard
			}

			int keyboardHeight = (int)Math.Round(height * DisplayInformation.GetForCurrentView().LogicalDpi / 96);
			if(keyboardHeight != lastKeyboardHeight)
			{
				if(keyboardHeight == 0 && (state == KeyboardState.PENDING_SHOW || state == KeyboardState.VISIBLE))
				{
					return;
				}
				else if(keyboardHeight > 0 && (state == KeyboardState.PENDING_HIDE || state == KeyboardState.HIDDEN))
				{
					return;
				}

				unityCallback.OnKeyboardHeightChanged(keyboardHeight);
			}

			lastKeyboardHeight = keyboardHeight;
		}

		private void UpdateHardwareKeyboardConnectivity(object sender, object e)
		{
			bool connected = IsHardwareKeyboardConnected();

			if(hardwareKeyboardConnected != connected)
			{
				hardwareKeyboardConnected = connected;
				unityCallback.OnHardwareKeyboardChanged(hardwareKeyboardConnected);

				if(hardwareKeyboardConnected && state != KeyboardState.HIDDEN)
				{
					HideKeyboard();
				}
				dummyButton.IsEnabled = !hardwareKeyboardConnected;
				dummyView.IsEnabled = !hardwareKeyboardConnected;
			}

			if(cancelHardwareKeyboardUpdateWhenDone)
			{
				cancelHardwareKeyboardUpdateWhenDone = false;
				hardwareKeyboardUpdateTimer.Stop();
			}
		}

		public bool IsHardwareKeyboardConnected()
		{
			if(deviceType == DeviceType.DESKTOP) //KeyboardCapabilities is unreliable
			{
				UserInteractionMode userInteractionMode = UIViewSettings.GetForCurrentView().UserInteractionMode;
				if(userInteractionMode == UserInteractionMode.Mouse)
				{
					return true;
				}
				else if(userInteractionMode == UserInteractionMode.Touch)
				{
					return false;
				}
			}
			else
			{
				KeyboardCapabilities keyboardCapabilities = new KeyboardCapabilities();
				return (keyboardCapabilities.KeyboardPresent != 0);
			}
			return false;
		}

		public void UpdateDeviceType()
		{
			if(Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile")
			{
				if(ApiInformation.IsApiContractPresent("Windows.Phone.PhoneContract", 1))
				{
					Windows.Devices.Input.KeyboardCapabilities keyboard = new Windows.Devices.Input.KeyboardCapabilities();
					if(keyboard.KeyboardPresent > 0)
					{
						deviceType = DeviceType.CONTINUUM;
					}
					else
					{
						deviceType = DeviceType.PHONE;
					}
				}
				else
				{
					deviceType = DeviceType.TABLET;
				}
			}
			else if(Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Desktop")
			{
				Windows.Devices.Input.KeyboardCapabilities keyboard = new Windows.Devices.Input.KeyboardCapabilities();
				if(keyboard.KeyboardPresent > 0)
				{
					deviceType = DeviceType.DESKTOP;
				}
				else
				{
					deviceType = DeviceType.TABLET;
				}
			}
			else if(Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Xbox")
			{
				deviceType = DeviceType.XBOX;
			}
			else if(Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.IoT")
			{
				deviceType = DeviceType.IOT;
			}
			else
			{
				deviceType = DeviceType.DESKTOP; //Default to desktop
			}

		}
		#endregion

		#region INPUTFIELD_EVENTS
		public void OnTextChanging(object sender, TextBoxTextChangingEventArgs arg)
		{
		}

		public void OnTextChanged(object sender, TextChangedEventArgs arg)
		{
			if(currentEvent != null || ignoreTextChange)
			{
				return;
			}

			if(characterValidation == CharacterValidation.NONE)
			{
				string currentText = dummyView.Text;
				if(!emojisAllowed && textValidator.RemoveEmoji(ref currentText))
				{
					int difference = currentText.Length - dummyView.Text.Length;
					int start = dummyView.SelectionStart + difference;
					start = Math.Max(start, 0);
					start = Math.Min(start, currentText.Length);

					ignoreTextChange = true;
					dummyView.Text = currentText;
					ignoreTextChange = false;

					try
					{
						dummyView.Select(start, 0);
					}
					catch { }
				}
			}
			else
			{
				string lastText = dummyView.Text;
				string text = "";

				int caretPosition = dummyView.SelectionStart;
				int selectionStartPosition = -1;
				if(dummyView.SelectionLength > 0)
				{
					selectionStartPosition = dummyView.SelectionStart;
				}

				textValidator.Validate(text, lastText, caretPosition, selectionStartPosition);
				text = textValidator.ResultText;

				if(!lastText.Equals(text))
				{
					int lastSelectionStart = dummyView.SelectionStart;

					ignoreTextChange = true;
					dummyView.Text = text;
					ignoreTextChange = false;

					int amountChanged = text.Length - lastText.Length;
					try
					{
						caretPosition = lastSelectionStart + amountChanged;
						dummyView.Select(caretPosition, 0);
					}
					catch { };
				}
			}

			string resultText = dummyView.Text;
			int resultSelectionStartPosition = dummyView.SelectionStart;
			int resultSelectionEndPosition = dummyView.SelectionStart + dummyView.SelectionLength;
			if(dummyView.AcceptsReturn)
			{
				resultText = resultText.Replace('\r', '\n');
			}
			unityCallback.OnTextEditUpdate(resultText, resultSelectionStartPosition, resultSelectionEndPosition);

			lastText = resultText;
			lastSelectionStartPosition = resultSelectionStartPosition;
			lastSelectionEndPosition = resultSelectionEndPosition;
		}

		private void OnKeyDown(object sender, KeyRoutedEventArgs e)
		{
			if(!dummyView.AcceptsReturn && e.Key == VirtualKey.Enter)
			{
				e.Handled = true;
				unityCallback.OnKeyboardNext();
			}
		}

		private void OnFocusLost(object sender, RoutedEventArgs e)
		{
			if(manualUnfocus)
			{
				manualUnfocus = false;
				return;
			}

			if(state == KeyboardState.PENDING_SHOW || state == KeyboardState.VISIBLE)
			{
				dummyView.Focus(FocusState.Programmatic);
			}
		}

		private void OnGotFocus(object sender, RoutedEventArgs e)
		{
		}

		private void OnKeyboardShowing(InputPane sender, InputPaneVisibilityEventArgs args)
		{
			keyboardVisible = true;

			if(state == KeyboardState.PENDING_HIDE || state == KeyboardState.HIDDEN)
			{
				inputPane.TryHide();
			}
		}

		private void OnKeyboardHiding(InputPane sender, InputPaneVisibilityEventArgs args)
		{
			keyboardVisible = false;

			if(state == KeyboardState.PENDING_SHOW)
			{
				inputPane.TryShow();
			}
			else if(state == KeyboardState.VISIBLE)
			{
				TimeSpan timeElapsed = DateTimeOffset.Now - visibleStartTime;
				if(!manualUnfocus && timeElapsed.TotalSeconds >= MIN_VISIBLE_TIME_FOR_CANCEL)
				{
					unityCallback.OnKeyboardCancel();
				}
				else
				{
					inputPane.TryShow();
				}
			}
		}
		#endregion

		#region PUBLIC_METHODS
		public void EnableUpdates()
		{
			if(!updatesEnabled)
			{
				updatesEnabled = true;

				if(cancelUpdateWhenDone) //Thread action haven't been cancelled yet
				{
					cancelUpdateWhenDone = false;
				}
				else
				{
					Util.RunOnUIThread(() => { updateTimer.Start(); });
				}
			}
		}

		public void DisableUpdates()
		{
			if(updatesEnabled)
			{
				updatesEnabled = false;
				cancelUpdateWhenDone = true;
			}
		}

		public void EnableHardwareKeyboardUpdates()
		{
			if(!hardwareKeyboardUpdatesEnabled)
			{
				hardwareKeyboardUpdatesEnabled = true;

				if(cancelHardwareKeyboardUpdateWhenDone) //Thread action haven't been cancelled yet
				{
					cancelHardwareKeyboardUpdateWhenDone = false;
				}
				else
				{
					Util.RunOnUIThread(() => { hardwareKeyboardUpdateTimer.Start(); });
				}
			}
		}

		public void DisableHardwareKeyboardUpdates()
		{
			if(hardwareKeyboardUpdatesEnabled)
			{
				hardwareKeyboardUpdatesEnabled = false;
				cancelHardwareKeyboardUpdateWhenDone = true;
			}
		}

		public void UpdateTextEdit(string text, int selectionStartPosition, int selectionEndPosition)
		{
			TextEditUpdateEvent textEditUpdateEvent = new TextEditUpdateEvent(text, selectionStartPosition, selectionEndPosition);
			unityEventQueue.Enqueue(textEditUpdateEvent);
		}

		public void ShowKeyboard(string text, int selectionStartPosition, int selectionEndPosition, string configurationJSON)
		{
			NativeKeyboardConfiguration configuration = null;
			if(!string.IsNullOrEmpty(configurationJSON))
			{
				JSONObject jsonObject = new JSONObject(configurationJSON);
				configuration = new NativeKeyboardConfiguration(jsonObject);
			}

			KeyboardShowEvent keyboardShowEvent = new KeyboardShowEvent(text, selectionStartPosition, selectionEndPosition, configuration);
			unityEventQueue.Enqueue(keyboardShowEvent);
		}

		public void HideKeyboard()
		{
			KeyboardHideEvent keyboardHideEvent = new KeyboardHideEvent();
			unityEventQueue.Enqueue(keyboardHideEvent);
		}
	}
	#endregion
}
