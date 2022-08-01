using UnityEditor;
using UnityEngine.UI;

namespace AdvancedInputFieldPlugin.Editor
{
	public static class UnityTextRendererExtensions
	{
		public static TextRendererData ExtractTextRendererData(this Text textRenderer)
		{
			TextRendererData data = new TextRendererData();
			data.text = textRenderer.text;
			data.color = textRenderer.color;
			data.font = textRenderer.font;
			data.fontStyle = textRenderer.fontStyle;
			data.fontSize = textRenderer.fontSize;
			data.lineSpacing = textRenderer.lineSpacing;
			data.textAlignment = textRenderer.alignment;
			data.autoSize = textRenderer.resizeTextForBestFit;
			data.minFontSize = textRenderer.resizeTextMinSize;
			data.maxFontSize = textRenderer.resizeTextMaxSize;

			return data;
		}

		public static void ApplyTextRendererData(this Text textRenderer, TextRendererData data)
		{
			Undo.RecordObject(textRenderer, "Undo " + textRenderer.GetInstanceID());
			textRenderer.text = data.text;
			textRenderer.color = data.color;

			if(data.font != null)
			{
				textRenderer.font = data.font;
			}

			textRenderer.fontStyle = data.fontStyle.GetValueOrDefault();
			textRenderer.fontSize = (int)data.fontSize.GetValueOrDefault();
			textRenderer.lineSpacing = data.lineSpacing.GetValueOrDefault();
			textRenderer.alignment = data.textAlignment.GetValueOrDefault();
			textRenderer.resizeTextForBestFit = data.autoSize.GetValueOrDefault();
			textRenderer.resizeTextMinSize = (int)data.minFontSize.GetValueOrDefault();
			textRenderer.resizeTextMaxSize = (int)data.maxFontSize.GetValueOrDefault();
		}
	}
}
