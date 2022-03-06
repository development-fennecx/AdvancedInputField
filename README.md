# AdvancedInputField
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
