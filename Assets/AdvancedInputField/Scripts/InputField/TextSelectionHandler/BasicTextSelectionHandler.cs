using UnityEngine;
using UnityEngine.UI;

namespace AdvancedInputFieldPlugin
{
	public class BasicTextSelectionHandler: TextSelectionHandler
	{
		private Image caretRenderer;
		private TextSelectionRenderer selectionRenderer;
		private float caretBlinkTime;

		public RectTransform CaretTransform
		{
			get
			{
				if(caretRenderer != null)
				{
					return caretRenderer.rectTransform;
				}

				return null;
			}
		}

		public override void Initialize()
		{
			base.Initialize();

			caretRenderer = transform.Find("Caret").GetComponent<Image>();
			selectionRenderer = transform.Find("Selection").GetComponent<TextSelectionRenderer>();
		}

		public override void Setup(Transform parent, TextNavigator textNavigator)
		{
			base.Setup(parent, textNavigator);
			selectionRenderer.Initialize(Engine);
			RefreshAppearance();
		}

		public void RefreshAppearance()
		{
			caretRenderer.rectTransform.sizeDelta = new Vector2(InputField.CaretWidth, InputField.TextRenderer.FontSize);
			caretRenderer.color = InputField.CaretColor;
			selectionRenderer.color = InputField.SelectionColor;
		}

		public override void OnUpdate()
		{
			caretBlinkTime += Time.deltaTime;
			caretBlinkTime = Mathf.Min(caretBlinkTime, InputField.CaretBlinkRate);
			float progress = caretBlinkTime / InputField.CaretBlinkRate;
			if(progress <= 0.5f)
			{
				caretRenderer.enabled = true;
			}
			else
			{
				caretRenderer.enabled = false;
			}

			if(progress == 1)
			{
				caretBlinkTime = 0;
			}
		}

		public override void OnSelectionUpdate(int selectionStartPosition, int selectionEndPosition)
		{
			selectionRenderer.UpdateSelection(Engine.VisibleSelectionStartPosition, Engine.VisibleSelectionEndPosition);
			UpdateCaret();
		}

		internal virtual void UpdateCaret(bool endingEditMode = false)
		{
			TextRenderer activeTextRenderer = Engine.GetActiveTextRenderer();

			int charIndex = Mathf.Clamp(Engine.VisibleCaretPosition, 0, activeTextRenderer.CharacterCount - 1);
			CharacterInfo charInfo = activeTextRenderer.GetCharacterInfo(charIndex);
			int lineIndex = activeTextRenderer.DetermineCharacterLine(charIndex);
			LineInfo lineInfo = activeTextRenderer.GetLineInfo(lineIndex);

			Vector2 cursorPosition = new Vector2(charInfo.position.x, lineInfo.topY);
			if(Engine.VisibleCaretPosition >= activeTextRenderer.CharacterCountVisible)
			{
				cursorPosition.x += charInfo.width;
			}

			caretRenderer.rectTransform.anchoredPosition = cursorPosition;
			caretRenderer.rectTransform.sizeDelta = new Vector2(InputField.CaretWidth, lineInfo.height);

			if(Engine.EditMode || endingEditMode)
			{
				if(InputField.Multiline)
				{
					UpdateVerticalScrollPosition(activeTextRenderer, 1, endingEditMode);
				}
				else
				{
					UpdateHorizontalScrollPosition(activeTextRenderer, 1, endingEditMode);
				}
			}
		}

		/// <summary>Updates the horizontal scroll position for given TextRenderer</summary>
		internal void UpdateHorizontalScrollPosition(TextRenderer textRenderer, float scrollSensitivity = 1, bool immediately = false)
		{
			RectTransform textAreaTransform = InputField.TextAreaTransform;
			RectTransform textContentTransform = InputField.TextContentTransform;
			Vector2 areaSize = textAreaTransform.rect.size;
			Vector2 contentSize = textContentTransform.rect.size;
			Vector2 contentPosition = textContentTransform.anchoredPosition;
			Vector2 caretSize = caretRenderer.rectTransform.rect.size;
			Vector2 caretPosition = caretRenderer.rectTransform.anchoredPosition;
			TextAlignment alignment = textRenderer.TextAlignment;
			float minX;
			float maxX;
			if(alignment == TextAlignment.BOTTOM || alignment == TextAlignment.CENTER || alignment == TextAlignment.TOP)
			{
				minX = -(contentSize.x * 0.5f) + (areaSize.x * 0.5f);
				maxX = minX + contentSize.x - areaSize.x;
			}
			else if(alignment == TextAlignment.BOTTOM_RIGHT || alignment == TextAlignment.RIGHT || alignment == TextAlignment.TOP_RIGHT)
			{
				minX = 0;
				maxX = Mathf.Max(minX + (contentSize.x - areaSize.x), minX);
			}
			else
			{
				maxX = 0;
				minX = Mathf.Min(maxX - (contentSize.x - areaSize.x), maxX);
			}
			caretPosition.x = maxX - caretPosition.x;

			float maxVisibleX = contentPosition.x;
			float minVisibleX = contentPosition.x - areaSize.x;

			if(caretPosition.x < minVisibleX)
			{
				float diffX = minVisibleX - caretPosition.x;
				contentPosition.x = Mathf.Clamp(contentPosition.x - diffX, minX, maxX);
				ScrollArea scrollArea = textAreaTransform.GetComponent<ScrollArea>();
				if(immediately)
				{
					scrollArea.MoveContentImmediately(contentPosition);
				}
				else
				{
					scrollArea.MoveContentHorizontally(contentPosition, scrollSensitivity * InputField.ScrollSpeed, InputField.MaxScrollTransitionTime);
				}
			}
			else if(caretPosition.x - caretSize.x > maxVisibleX)
			{
				float diffX = maxVisibleX - (caretPosition.x - caretSize.x);
				contentPosition.x = Mathf.Clamp(contentPosition.x - diffX, minX, maxX);
				ScrollArea scrollArea = textAreaTransform.GetComponent<ScrollArea>();
				if(immediately)
				{
					scrollArea.MoveContentImmediately(contentPosition);
				}
				else
				{
					scrollArea.MoveContentHorizontally(contentPosition, scrollSensitivity * InputField.ScrollSpeed, InputField.MaxScrollTransitionTime);
				}
			}
			else
			{
				textAreaTransform.GetComponent<ScrollArea>().StopMoveContent();
			}
		}

		/// <summary>Updates the vertical scroll position for given TextRenderer</summary>
		internal void UpdateVerticalScrollPosition(TextRenderer textRenderer, float scrollSensitivity = 1, bool immediately = false)
		{
			RectTransform textAreaTransform = InputField.TextAreaTransform;
			RectTransform textContentTransform = InputField.TextContentTransform;
			Vector2 areaSize = textAreaTransform.rect.size;
			Vector2 contentSize = textContentTransform.rect.size;
			Vector2 contentPosition = textContentTransform.anchoredPosition;
			Vector2 caretSize = caretRenderer.rectTransform.rect.size;
			Vector2 caretPosition = caretRenderer.rectTransform.anchoredPosition;
			TextAlignment alignment = textRenderer.TextAlignment;
			float minY;
			float maxY;
			if(alignment == TextAlignment.LEFT || alignment == TextAlignment.CENTER || alignment == TextAlignment.RIGHT)
			{
				minY = -(contentSize.y * 0.5f) + (areaSize.y * 0.5f);
				maxY = minY + contentSize.y - areaSize.y;
			}
			else if(alignment == TextAlignment.BOTTOM_LEFT || alignment == TextAlignment.BOTTOM || alignment == TextAlignment.BOTTOM_RIGHT)
			{
				maxY = 0;
				minY = Mathf.Min(maxY - (contentSize.y - areaSize.y), maxY);
			}
			else
			{
				minY = 0;
				maxY = Mathf.Max(minY + contentSize.y - areaSize.y, minY);
			}
			caretPosition.y = minY - caretPosition.y;

			float minVisibleY = contentPosition.y;
			float maxVisibleY = contentPosition.y + areaSize.y;

			if(caretPosition.y < minVisibleY)
			{
				float diffY = minVisibleY - caretPosition.y;
				contentPosition.y = Mathf.Clamp(contentPosition.y - diffY, minY, maxY);
				ScrollArea scrollArea = textAreaTransform.GetComponent<ScrollArea>();
				if(immediately)
				{
					scrollArea.MoveContentImmediately(contentPosition);
				}
				else
				{
					scrollArea.MoveContentVertically(contentPosition, scrollSensitivity * InputField.ScrollSpeed, InputField.MaxScrollTransitionTime);
				}
			}
			else if(caretPosition.y + caretSize.y > maxVisibleY)
			{
				float diffY = (caretPosition.y + caretSize.y) - maxVisibleY;
				contentPosition.y = Mathf.Clamp(contentPosition.y + diffY, minY, maxY);
				ScrollArea scrollArea = textAreaTransform.GetComponent<ScrollArea>();
				if(immediately)
				{
					scrollArea.MoveContentImmediately(contentPosition);
				}
				else
				{
					scrollArea.MoveContentVertically(contentPosition, scrollSensitivity * InputField.ScrollSpeed, InputField.MaxScrollTransitionTime);
				}
			}
			else
			{
				textAreaTransform.GetComponent<ScrollArea>().StopMoveContent();
			}
		}
	}
}
