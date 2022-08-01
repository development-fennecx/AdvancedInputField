# AdvancedInputField

The opensource version of the Advanced Input Field plugin. The plugin is licensed under the MIT License.
The plugin won't be further supported by me, but feel free to fork this repository to do your own modifications. You are free to use this plugin in your projects. If you intent to resell a modified version of the plugin, please include some kind of attribution (can be a text file or a message in the log on boot). For example "Code based on the Advanced Input Field plugin by Jeroen van Pienbroek, link: https://github.com/development-fennecx/AdvancedInputField". While the attribution is not required, I would appreciate it.

This plugin provides a more Advanced Input Field that has a lot more features and properties than the official Unity InputField. It still inherits from the Selectable base class, so it can be used with the Unity EventSystem.
Also, it has it's own bindings for the native keyboards. This made it possible to provide a better user experience and to fix several issues that existed in the official Unity InputField.

Supported platforms: PC, Mac, Android, iOS & UWP (PC)

Features:

- Default features of the official Unity InputField
- More event callbacks (OnSelectionChanged, OnCaretPositionChanged, OnSpecialKeyPressed,...)
- Filters to process, decorate, block or allow text changes
- Validate characters when text changes in native code (using CustomCharacterValidators)
- Next Input Field option to control which InputField should be selected when done editing. (Tab key on Standalone platforms and Done/Next key on Mobile platforms).
- Show ActionBar with cut, copy, paste and select all options
- Touch Selection Cursors (Draws selection sprites for start and end of text selection to control the selected text more easily in large text blocks)
- Event for keyboard height changes in the new NativeKeyboard binding.
- KeyboardScroller component to scroll content when NativeKeyboard appears/hides.
- Support for TextMeshPro Text Renderers
- Support for full emoji range
- Support for rich text
- Multiple InputField Modes (indicates how to handle text bounds changes): SCROLL_TEXT, HORIZONTAL_RESIZE_FIT_TEXT and VERTICAL_RESIZE_FIT_TEXT
- And more...

NOTE: Asian IME are not supported on Standalone platforms, only on Android & iOS using the default system IME

Regarding emojis:
Due to licensing issues of emoji spritesheets there are no emoji assets included in the plugin. See the Documentation file of the plugin for details on how to create and configure your own emoji asset file.

## Building from Source

### Android:

- Open Android/NativeKeyboard with Android Studio
- It may fail the first time, you need to set a JDK version. In Android studio go to Android Studio -> Preferences -> Build, Execution, Deployment -> Build Tools -> Gradle. Select Gradle JDK -> coretto-1.8 (If you do not have it downloaded, click Download JDK, set Version to 1.8 (not 18) and Vendor to Amazon Coretto and download).
- Close and reopen the folder in Android Studio.
- Click Build.
- Copy NativeKeyboard.aar from Android/NativeKeyboard/NativeKeyboard/build/outputs/.aar to Assets/Plugins/Android.

### iOS:

- Open iOS/NativeKeyboard/NativeKeyboard.xcodeproj with Xcode.
- Ensure the folder Assets/Plugins/iOS exists.
- Select build target of "Any iOS Device".
- Build, this will build to Assets/Plugins/iOS.

## Updating RedFox's AdvancedInputField

- Ensure new builds for Android and iOS have been made if needed.
- Copy AdvancedInputField/Assets/AdvancedInputField to RedFox/Assets/BindyStreet.Kit.Unity/Plugins
