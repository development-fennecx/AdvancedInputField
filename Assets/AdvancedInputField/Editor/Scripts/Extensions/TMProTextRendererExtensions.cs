#if ADVANCEDINPUTFIELD_TEXTMESHPRO
using TMPro;
using UnityEditor;
using UnityEngine;

namespace AdvancedInputFieldPlugin.Editor
{
	public static class TMProTextRendererExtensions
	{
		public static TextRendererData ExtractTextRendererData(this TextMeshProUGUI textRenderer)
		{
			TextRendererData data = new TextRendererData();
			data.text = textRenderer.text;
			data.color = textRenderer.color;
			data.tmpFont = textRenderer.font;
			data.fontStyle = textRenderer.ExtractFontStyle();
			data.fontSize = textRenderer.fontSize;
			data.lineSpacing = textRenderer.lineSpacing;
			data.textAlignment = textRenderer.ExtractTextAlignment();
			data.autoSize = textRenderer.enableAutoSizing;
			data.minFontSize = textRenderer.fontSizeMin;
			data.maxFontSize = textRenderer.fontSizeMax;

			return data;
		}

		public static void ApplyTextRendererData(this TextMeshProUGUI textRenderer, TextRendererData data)
		{
			Undo.RecordObject(textRenderer, "Undo " + textRenderer.GetInstanceID());
			textRenderer.text = data.text;
			textRenderer.color = data.color;

			if(data.tmpFont != null)
			{
				textRenderer.font = data.tmpFont;
			}

			textRenderer.ApplyFontStyle(data);
			textRenderer.fontSize = (int)data.fontSize.GetValueOrDefault();
			textRenderer.lineSpacing = data.lineSpacing.GetValueOrDefault();
			textRenderer.ApplyTextAlignment(data);
			textRenderer.enableAutoSizing = data.autoSize.GetValueOrDefault();
			textRenderer.fontSizeMin = (int)data.minFontSize.GetValueOrDefault();
			textRenderer.fontSizeMax = (int)data.maxFontSize.GetValueOrDefault();
		}

		public static FontStyle ExtractFontStyle(this TextMeshProUGUI textRenderer)
		{
			switch(textRenderer.fontStyle)
			{
				case FontStyles.Bold: return FontStyle.Bold;
				case FontStyles.Italic: return FontStyle.Italic;
				default: return FontStyle.Normal;
			}
		}

		public static void ApplyFontStyle(this TextMeshProUGUI textRenderer, TextRendererData data)
		{
			FontStyle fontStyle = data.fontStyle.GetValueOrDefault();

			switch(fontStyle)
			{
				case FontStyle.Bold: textRenderer.fontStyle = FontStyles.Bold; break;
				case FontStyle.BoldAndItalic: textRenderer.fontStyle = FontStyles.Bold; break;
				case FontStyle.Italic: textRenderer.fontStyle = FontStyles.Italic; break;
				case FontStyle.Normal: textRenderer.fontStyle = FontStyles.Normal; break;
			}
		}

		public static TextAnchor ExtractTextAlignment(this TextMeshProUGUI textRenderer)
		{
			switch(textRenderer.alignment)
			{
				case TextAlignmentOptions.TopLeft: return TextAnchor.UpperLeft;
				case TextAlignmentOptions.Top: return TextAnchor.UpperCenter;
				case TextAlignmentOptions.TopRight: return TextAnchor.UpperRight;
				case TextAlignmentOptions.Left: return TextAnchor.MiddleLeft;
				case TextAlignmentOptions.Center: return TextAnchor.MiddleCenter;
				case TextAlignmentOptions.Right: return TextAnchor.MiddleRight;
				case TextAlignmentOptions.BottomLeft: return TextAnchor.LowerLeft;
				case TextAlignmentOptions.Bottom: return TextAnchor.LowerCenter;
				case TextAlignmentOptions.BottomRight: return TextAnchor.LowerRight;
				default: return TextAnchor.UpperLeft;
			}
		}

		public static void ApplyTextAlignment(this TextMeshProUGUI textRenderer, TextRendererData data)
		{
			TextAnchor textAlignment = data.textAlignment.GetValueOrDefault();

			switch(textAlignment)
			{
				case TextAnchor.UpperLeft: textRenderer.alignment = TextAlignmentOptions.TopLeft; break;
				case TextAnchor.UpperCenter: textRenderer.alignment = TextAlignmentOptions.Top; break;
				case TextAnchor.UpperRight: textRenderer.alignment = TextAlignmentOptions.TopRight; break;
				case TextAnchor.MiddleLeft: textRenderer.alignment = TextAlignmentOptions.Left; break;
				case TextAnchor.MiddleCenter: textRenderer.alignment = TextAlignmentOptions.Center; break;
				case TextAnchor.MiddleRight: textRenderer.alignment = TextAlignmentOptions.Right; break;
				case TextAnchor.LowerLeft: textRenderer.alignment = TextAlignmentOptions.BottomLeft; break;
				case TextAnchor.LowerCenter: textRenderer.alignment = TextAlignmentOptions.Bottom; break;
				case TextAnchor.LowerRight: textRenderer.alignment = TextAlignmentOptions.BottomRight; break;
			}
		}
	}
}
#endif