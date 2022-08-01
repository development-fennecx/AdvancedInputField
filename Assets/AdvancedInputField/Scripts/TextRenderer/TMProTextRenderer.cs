#if ADVANCEDINPUTFIELD_TEXTMESHPRO
using System.Collections.Generic;
using TMPro;
using UnityEngine;
#endif

namespace AdvancedInputFieldPlugin
{
#if ADVANCEDINPUTFIELD_TEXTMESHPRO
	[RequireComponent(typeof(TextMeshProUGUI))]
#endif
	public class TMProTextRenderer: TextRenderer
	{
#if ADVANCEDINPUTFIELD_TEXTMESHPRO
		/// <summary>Spacing multiplier to add some more spacing based on the line spacing to avoid rounding/scroll issues when calculating text content height</summary>
		private const int EXTRA_LINE_SPACING_MULTIPLIER = 3;

		private new TextMeshProUGUI renderer;
		private List<CharacterInfo> characters;
		private List<LineInfo> lines;
		private int characterCount;
		private int lineCount;
		private bool fixNewline;
		private float minX;
		private float maxX;

		public TextMeshProUGUI Renderer
		{
			get
			{
				if(renderer == null)
				{
					renderer = GetComponent<TextMeshProUGUI>();
				}

				return renderer;
			}
		}

		public List<CharacterInfo> Characters
		{
			get
			{
				if(characters == null)
				{
					characters = new List<CharacterInfo>();
				}

				return characters;
			}
		}

		public List<LineInfo> Lines
		{
			get
			{
				if(lines == null)
				{
					lines = new List<LineInfo>();
				}

				return lines;
			}
		}

		public override string Text
		{
			get
			{
				return Renderer.text;
			}
			set
			{
				Renderer.text = value;
				fixNewline = (value.EndsWith("\n"));
			}
		}

		public override bool Visible { get { return Renderer.enabled; } }

		public override bool Multiline
		{
			get { return multiline; }
			set
			{
				multiline = value;
				Renderer.enableWordWrapping = multiline;
			}
		}

		public override bool RichTextEnabled
		{
			get { return richTextEditingEnabled; }
			set
			{
				richTextEditingEnabled = value;
				Renderer.richText = value;
			}
		}

		public override TextAlignment TextAlignment
		{
			get
			{
				switch(renderer.alignment)
				{
					case TextAlignmentOptions.TopLeft: return TextAlignment.TOP_LEFT;
					case TextAlignmentOptions.Top: return TextAlignment.TOP;
					case TextAlignmentOptions.TopRight: return TextAlignment.TOP_RIGHT;
					case TextAlignmentOptions.Left: return TextAlignment.LEFT;
					case TextAlignmentOptions.Center: return TextAlignment.CENTER;
					case TextAlignmentOptions.Right: return TextAlignment.RIGHT;
					case TextAlignmentOptions.BottomLeft: return TextAlignment.BOTTOM_LEFT;
					case TextAlignmentOptions.Bottom: return TextAlignment.BOTTOM;
					case TextAlignmentOptions.BottomRight: return TextAlignment.BOTTOM_RIGHT;
					default: return TextAlignment.TOP_LEFT;
				}
			}
		}

		public override int LineCount
		{
			get
			{
				return Mathf.Max(1, lineCount);
			}
		}

		public override int CharacterCount
		{
			get
			{
				if(string.IsNullOrEmpty(Text))
				{
					return 1;
				}
				else
				{
					return characterCount + 1;
				}
			}
		}

		public override int CharacterCountVisible
		{
			get
			{
				if(string.IsNullOrEmpty(Text))
				{
					return 0;
				}
				else
				{
					return characterCount;
				}
			}
		}

		public override Color Color
		{
			get
			{
				return Renderer.color;
			}
			set
			{
				Renderer.color = value;
			}
		}

		public override float FontSize
		{
			get
			{
				return Renderer.fontSize;
			}
			set
			{
				Renderer.fontSize = value;
			}
		}

		public override bool ResizeTextForBestFit
		{
			get
			{
				return Renderer.enableAutoSizing;
			}
		}

		public override float FontSizeUsedForBestFit
		{
			get
			{
				return Renderer.fontSize;
			}
		}

		protected override void Awake()
		{
			base.Awake();

			renderer = GetComponent<TextMeshProUGUI>();
		}

		public override void Show()
		{
			Renderer.enabled = true;
		}

		public override void Hide()
		{
			Renderer.enabled = false;
		}

		public override bool UpdateImmediately()
		{
			if(string.IsNullOrEmpty(Text))
			{
				TMP_CharacterInfo charInfo = new TMP_CharacterInfo();
				if(renderer.textInfo != null)
				{
					renderer.textInfo.characterInfo = new TMP_CharacterInfo[] { charInfo };
					renderer.textInfo.characterCount = 1;
				}
			}
			else
			{
				renderer.ForceMeshUpdate();
			}

			if(renderer.textInfo != null)
			{
				RefreshData();
				return true;
			}

			return false;
		}

		public void RefreshData()
		{
			RefreshCharacterData();
			RefreshLineData();
			Characters.Add(CreateCharacterInfoForTextEnd());
		}

		public void RefreshCharacterData()
		{
			TMP_TextInfo textInfo = renderer.textInfo;

			lineCount = textInfo.lineCount;
			if(!multiline) { lineCount = 1; } //Workaround for incorrect lineCount

			TMP_CharacterInfo[] characterInfos = textInfo.characterInfo;
			int charIndex = 0;
			Vector2 rectSize = renderer.rectTransform.rect.size;
			float textWidth = 0;
			minX = int.MaxValue;
			maxX = int.MinValue;
			TMP_CharacterInfo lastCharacterInfo = new TMP_CharacterInfo();
			bool indexCharacters = true;
			Characters.Clear();

			int lastIndex = -1;
			int length = characterInfos.Length;
			for(int i = 0; i < length; i++)
			{
				TMP_CharacterInfo tmpCharacterInfo = characterInfos[i];
				charIndex = tmpCharacterInfo.index;
				if(charIndex - lastIndex > 1 && i <= textInfo.characterCount)
				{
					if(i == 0)
					{
						lastCharacterInfo = tmpCharacterInfo;
					}

					float widthOffset = 0;
					if(Characters.Count > 0)
					{
						widthOffset = Characters[Characters.Count - 1].width;
					}

					int dummyAmount = (charIndex - lastIndex) - 1;
					for(int di = 0; di < dummyAmount; di++)
					{
						CharacterInfo dummyCharacterInfo = CreateCharacterInfo(lastCharacterInfo);
						if(widthOffset > 0)
						{
							dummyCharacterInfo.position.x += widthOffset;
						}
						dummyCharacterInfo.width = 0;
						dummyCharacterInfo.index = lastIndex + 1 + di;
						Characters.Add(dummyCharacterInfo);
					}
				}

				if(i >= textInfo.characterCount)
				{
					characterInfos[i].index = -1; //Not used
					continue;
				}

				if(tmpCharacterInfo.character == 0)
				{
					indexCharacters = false;
					characterInfos[i].index = -1; //Not used
					continue;
				}

				if(!indexCharacters)
				{
					break;
				}

				characterInfos[i].index = charIndex;

				CharacterInfo characterInfo = CreateCharacterInfo(tmpCharacterInfo);
				characterInfo.index = charIndex;
				Characters.Add(characterInfo);
				lastIndex = charIndex;

				if(!Multiline || viewportTransform == null)
				{
					float width = tmpCharacterInfo.xAdvance - tmpCharacterInfo.origin;
					textWidth += width;
				}

				minX = Mathf.Min(tmpCharacterInfo.origin, minX);
				maxX = Mathf.Max(tmpCharacterInfo.xAdvance, maxX);
				lastCharacterInfo = tmpCharacterInfo;
			}

			if(lastIndex >= 0)
			{
				characterCount = lastIndex + 1;
			}

			if(!Multiline || viewportTransform == null)
			{
				preferredSize.x = textWidth + CaretWidth;
			}
			else
			{
				preferredSize.x = viewportTransform.rect.width;
			}

			if(fixNewline && Characters.Count == 0) //Workaround for bug with only newline character as text (TextMeshPro 2.1.0)
			{
				TMP_CharacterInfo tmpCharacterInfo = characterInfos[0];
				tmpCharacterInfo.character = '\n';
				tmpCharacterInfo.index = 0;
				CharacterInfo characterInfo = CreateCharacterInfo(tmpCharacterInfo);
				characterInfo.index = 0;
				Characters.Add(characterInfo);
			}

			if(fixNewline && Text.Length > 1)
			{
				if(Text.Length != characterInfos.Length) { return; } //Not sure why this can happen sometimes
				TMP_CharacterInfo previousCharInfoValue = characterInfos[Text.Length - 2];
				Vector2 position = new Vector2(previousCharInfoValue.origin, previousCharInfoValue.ascender);
				float width = previousCharInfoValue.xAdvance - previousCharInfoValue.origin;

				CharacterInfo characterInfo = Characters[Text.Length - 1];
				characterInfo.position.x = position.x + width + (rectSize.x * 0.5f);
				characterInfo.position.y = previousCharInfoValue.ascender - (rectSize.y * 0.5f);
				characterInfo.width = 0;
				Characters[Text.Length - 1] = characterInfo;
			}
		}

		public CharacterInfo CreateCharacterInfo(TMP_CharacterInfo tmpCharacterInfo)
		{
			Vector2 rectSize = renderer.rectTransform.rect.size;
			CharacterInfo characterInfo = new CharacterInfo
			{
				position = new Vector2(tmpCharacterInfo.origin + (rectSize.x * 0.5f), tmpCharacterInfo.ascender - (rectSize.y * 0.5f)),
				width = tmpCharacterInfo.xAdvance - tmpCharacterInfo.origin
			};

			return characterInfo;
		}

		public CharacterInfo CreateCharacterInfo(CharacterInfo sourceCharacterInfo)
		{
			CharacterInfo characterInfo = new CharacterInfo
			{
				position = sourceCharacterInfo.position,
				width = sourceCharacterInfo.width
			};

			return characterInfo;
		}

		public CharacterInfo CreateCharacterInfoForTextEnd()
		{
			if(Characters.Count == 0 || Lines.Count == 0) { return default(CharacterInfo); }

			Vector2 rectSize = renderer.rectTransform.rect.size;
			CharacterInfo lastCharacterInfo = Characters[Characters.Count - 1];
			LineInfo lastLineInfo = Lines[Lines.Count - 1];

			if(fixNewline)
			{
				float x = 0;

				switch(TextAlignment)
				{
					case TextAlignment.TOP_LEFT: x = minX; break;
					case TextAlignment.TOP: x = 0; break;
					case TextAlignment.TOP_RIGHT: x = maxX; break;
					case TextAlignment.LEFT: x = minX; break;
					case TextAlignment.CENTER: x = 0; break;
					case TextAlignment.RIGHT: x = maxX; break;
					case TextAlignment.BOTTOM_LEFT: x = minX; break;
					case TextAlignment.BOTTOM: x = 0; break;
					case TextAlignment.BOTTOM_RIGHT: x = maxX; break;
				}

				CharacterInfo charInfo = CreateCharacterInfo(lastCharacterInfo);
				charInfo.position.x = x + (rectSize.x * 0.5f);
				charInfo.position.y -= lastLineInfo.height;
				charInfo.width = 0;
				charInfo.index = Text.Length;
				return charInfo;
			}
			else
			{
				CharacterInfo charInfo = CreateCharacterInfo(lastCharacterInfo);
				charInfo.position.x += lastCharacterInfo.width;
				charInfo.width = 0;
				charInfo.index = Text.Length;
				return charInfo;
			}
		}

		public void RefreshLineData()
		{
			Vector2 rectSize = renderer.rectTransform.rect.size;
			TMP_TextInfo textInfo = renderer.textInfo;
			float topAscender = 0;
			float bottomDescender = 0;
			float lineSpacing = renderer.lineSpacing;

			TMP_LineInfo[] lines = textInfo.lineInfo;
			TMP_LineInfo lastTMPLineInfo = new TMP_LineInfo();
			float textHeight = 0;
			Lines.Clear();

			int length = lines.Length;
			for(int i = 0; i < length; i++)
			{
				if(i >= lineCount)
				{
					break;
				}

				TMP_LineInfo tmpLineInfo = lines[i];
				if(i == 0)
				{
					topAscender = tmpLineInfo.ascender;
				}
				if(i == lineCount - 1)
				{
					bottomDescender = tmpLineInfo.descender;
				}

				if(tmpLineInfo.firstCharacterIndex < textInfo.characterCount)
				{
					TMP_CharacterInfo firstTMPCharInfo = textInfo.characterInfo[tmpLineInfo.firstCharacterIndex];
					if(firstTMPCharInfo.index != -1)
					{
						tmpLineInfo.firstCharacterIndex = firstTMPCharInfo.index; //This index is recalculated at this point
					}
				}

				if(tmpLineInfo.firstCharacterIndex < Text.Length)
				{
					char firstCharacter = Text[tmpLineInfo.firstCharacterIndex];
					if(firstCharacter == '\n')
					{
						float x = 0;

						switch(TextAlignment)
						{
							case TextAlignment.TOP_LEFT: x = minX; break;
							case TextAlignment.TOP: x = 0; break;
							case TextAlignment.TOP_RIGHT: x = maxX; break;
							case TextAlignment.LEFT: x = minX; break;
							case TextAlignment.CENTER: x = 0; break;
							case TextAlignment.RIGHT: x = maxX; break;
							case TextAlignment.BOTTOM_LEFT: x = minX; break;
							case TextAlignment.BOTTOM: x = 0; break;
							case TextAlignment.BOTTOM_RIGHT: x = maxX; break;
						}

						CharacterInfo characterInfo = Characters[tmpLineInfo.firstCharacterIndex];
						characterInfo.position.x = x + (rectSize.x * 0.5f);
						Characters[tmpLineInfo.firstCharacterIndex] = characterInfo;
					}
				}

				LineInfo lineInfo = CreateLineInfo(tmpLineInfo);
				Lines.Add(lineInfo);

				textHeight += tmpLineInfo.lineHeight;

				lastTMPLineInfo = tmpLineInfo;

				if(i == 0 && !Multiline)
				{
					break;
				}
			}

			textHeight = Mathf.Abs(topAscender - bottomDescender) + (EXTRA_LINE_SPACING_MULTIPLIER * lineSpacing);

			if(fixNewline)
			{
				LineInfo lineInfo = CreateLineInfo(lastTMPLineInfo);
				lineInfo.topY -= lastTMPLineInfo.lineHeight;
				lineInfo.startCharIdx = Text.Length;
				Lines.Add(lineInfo);

				textHeight += lines[0].lineHeight;
				lineCount++;
			}

			preferredSize.y = textHeight;
		}

		public LineInfo CreateLineInfo(TMP_LineInfo tmpLineInfo)
		{
			Vector2 rectSize = renderer.rectTransform.rect.size;
			LineInfo lineInfo = new LineInfo
			{
				topY = tmpLineInfo.ascender - (rectSize.y * 0.5f),
				height = tmpLineInfo.lineHeight,
				startCharIdx = tmpLineInfo.firstCharacterIndex
			};

			return lineInfo;
		}

		public override bool FontHasCharacter(char c)
		{
			return renderer.font.HasCharacter(c);
		}

		public override bool IsReady()
		{
			return true;
		}

		public override CharacterInfo GetCharacterInfo(int index)
		{
			if(index < Characters.Count)
			{
				return Characters[index];
			}

			return default(CharacterInfo);
		}

		public override LineInfo GetLineInfo(int index)
		{
			if(index < Lines.Count)
			{
				return Lines[index];
			}

			return default(LineInfo);
		}

		public override int GetLineEndCharIndex(int line)
		{
			line = Mathf.Max(line, 0);

			if(line + 1 < LineCount)
			{
				return Lines[line + 1].startCharIdx - 1;
			}
			else
			{
				return CharacterCountVisible;
			}
		}

		public override float DeterminePreferredWidth(int fontSize)
		{
			renderer.fontSize = fontSize;
			renderer.ForceMeshUpdate();

			return renderer.preferredWidth;
		}

		public override float DeterminePreferredHeight(int fontSize)
		{
			renderer.fontSize = fontSize;
			renderer.ForceMeshUpdate();

			return renderer.preferredHeight;
		}

#endif
	}
}