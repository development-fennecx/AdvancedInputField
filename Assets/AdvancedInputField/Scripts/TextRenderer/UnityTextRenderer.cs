using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AdvancedInputFieldPlugin
{
	[RequireComponent(typeof(Text))]
	public class UnityTextRenderer: TextRenderer
	{
		/// <summary>Spacing multiplier to add some more spacing based on the line spacing to avoid rounding/scroll issues when calculating text content height</summary>
		private const int EXTRA_LINE_SPACING_MULTIPLIER = 3;

		private new Text renderer;
		private Canvas canvas;

		public Text Renderer
		{
			get
			{
				if(renderer == null)
				{
					renderer = GetComponent<Text>();
				}

				return renderer;
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
			}
		}

		public override bool Visible { get { return Renderer.enabled; } }

		public Canvas Canvas
		{
			get
			{
				if(canvas == null)
				{
					canvas = GetComponentInParent<Canvas>();
				}

				return canvas;
			}
		}

		public float CanvasScaleFactor
		{
			get
			{
				if(Canvas != null)
				{
					return Canvas.scaleFactor;
				}

				return 1;
			}
		}

		public override bool Multiline
		{
			get { return multiline; }
			set
			{
				multiline = value;
			}
		}

		public override bool RichTextEnabled
		{
			get { return richTextEditingEnabled; }
			set
			{
				richTextEditingEnabled = value;
				Renderer.supportRichText = value;
			}
		}

		public override TextAlignment TextAlignment
		{
			get
			{
				switch(renderer.alignment)
				{
					case TextAnchor.UpperLeft: return TextAlignment.TOP_LEFT;
					case TextAnchor.UpperCenter: return TextAlignment.TOP;
					case TextAnchor.UpperRight: return TextAlignment.TOP_RIGHT;
					case TextAnchor.MiddleLeft: return TextAlignment.LEFT;
					case TextAnchor.MiddleCenter: return TextAlignment.CENTER;
					case TextAnchor.MiddleRight: return TextAlignment.RIGHT;
					case TextAnchor.LowerLeft: return TextAlignment.BOTTOM_LEFT;
					case TextAnchor.LowerCenter: return TextAlignment.BOTTOM;
					case TextAnchor.LowerRight: return TextAlignment.BOTTOM_RIGHT;
					default: return TextAlignment.TOP_LEFT;
				}
			}
		}

		public override int LineCount
		{
			get
			{
				return Renderer.cachedTextGenerator.lineCount;
			}
		}

		public override int CharacterCount
		{
			get
			{
				return Mathf.Max(1, Renderer.cachedTextGenerator.characterCount);
			}
		}

		public override int CharacterCountVisible
		{
			get
			{
				return Renderer.cachedTextGenerator.characterCountVisible;
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
				Renderer.fontSize = Mathf.RoundToInt(value);
			}
		}

		public override bool ResizeTextForBestFit
		{
			get
			{
				return Renderer.resizeTextForBestFit;
			}
		}

		public override float FontSizeUsedForBestFit
		{
			get
			{
				return Renderer.cachedTextGenerator.fontSizeUsedForBestFit;
			}
		}

		protected override void Awake()
		{
			base.Awake();

			renderer = GetComponent<Text>();
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
				return false;
			}
			else
			{
				renderer.UpdateImmediately(true);
				RefreshCharacterData();
			}

			if(renderer.cachedTextGenerator != null)
			{
				RefreshCharacterData();
				return true;
			}

			return false;
		}

		public void RefreshCharacterData()
		{
			TextGenerator textGenerator = renderer.cachedTextGenerator;
			IList<UICharInfo> characters = textGenerator.characters;
			float textWidth = 0;

			int length = characters.Count;
			for(int i = 0; i < length; i++)
			{
				UICharInfo charInfo = characters[i];

				if(!Multiline || viewportTransform == null)
				{
					textWidth += (charInfo.charWidth / CanvasScaleFactor);
				}
			}

			if(!Multiline || viewportTransform == null)
			{
				preferredSize.x = textWidth + CaretWidth;
			}
			else
			{
				preferredSize.x = viewportTransform.rect.width;
			}

			preferredSize.y = renderer.preferredHeight + (EXTRA_LINE_SPACING_MULTIPLIER * renderer.lineSpacing);
		}

		public override bool FontHasCharacter(char c)
		{
			return renderer.font.HasCharacter(c);
		}

		public override bool IsReady()
		{
			return renderer.IsReady();
		}

		public override CharacterInfo GetCharacterInfo(int index)
		{
			//this.Log("GetCharacterInfo: " + index);
			TextGenerator textGenerator = renderer.cachedTextGenerator;
			if(textGenerator.characterCount == 0)
			{
				renderer.text = " ";
				renderer.UpdateImmediately();
				renderer.text = string.Empty;
				renderer.UpdateImmediately();
			}

			UICharInfo charInfo = textGenerator.characters[index];
			Vector2 rectSize = renderer.rectTransform.rect.size;
			CharacterInfo characterInfo = new CharacterInfo
			{
				position = charInfo.cursorPos / CanvasScaleFactor,
				width = charInfo.charWidth / CanvasScaleFactor
			};
			characterInfo.position.x += (rectSize.x * 0.5f);
			characterInfo.position.y -= (rectSize.y * 0.5f);

			return characterInfo;
		}

		public override LineInfo GetLineInfo(int index)
		{
			TextGenerator textGenerator = renderer.cachedTextGenerator;
			UILineInfo info = textGenerator.lines[index];
			Vector2 rectSize = renderer.rectTransform.rect.size;
			LineInfo lineInfo = new LineInfo
			{
				topY = (info.topY / CanvasScaleFactor) - (rectSize.y * 0.5f),
				height = info.height / CanvasScaleFactor,
				startCharIdx = info.startCharIdx
			};

			return lineInfo;
		}

		public override int GetLineEndCharIndex(int line)
		{
			TextGenerator textGenerator = renderer.cachedTextGenerator;
			line = Mathf.Max(line, 0);
			if(line + 1 < textGenerator.lineCount)
			{
				return textGenerator.lines[line + 1].startCharIdx - 1;
			}

			return textGenerator.characterCountVisible;
		}

		public override float DeterminePreferredWidth(int fontSize)
		{
			TextGenerationSettings settings = renderer.GetGenerationSettings(rectTransform.rect.size);
			settings.fontSize = fontSize;
			settings.generateOutOfBounds = true;
			settings.horizontalOverflow = HorizontalWrapMode.Overflow;
			settings.verticalOverflow = VerticalWrapMode.Overflow;
			settings.scaleFactor = 1;

			return renderer.cachedTextGenerator.GetPreferredWidth(Text, settings);
		}

		public override float DeterminePreferredHeight(int fontSize)
		{
			TextGenerationSettings settings = renderer.GetGenerationSettings(rectTransform.rect.size);
			settings.fontSize = fontSize;
			settings.generateOutOfBounds = true;
			settings.horizontalOverflow = HorizontalWrapMode.Overflow;
			settings.verticalOverflow = VerticalWrapMode.Overflow;
			settings.scaleFactor = 1;

			return renderer.cachedTextGenerator.GetPreferredHeight(Text, settings);
		}
	}
}
