// Copyright (c) Jeroen van Pienbroek. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using UnityEngine;
using UnityEngine.EventSystems;

namespace AdvancedInputFieldPlugin
{
	public class TouchTextSelectionHandler: TextSelectionHandler
	{
		/// <summary>The thumb size multiplier used for selection cursors size calculations</summary>
		private const float THUMB_SIZE_RATIO = 0.5f;

		public TouchCursor CurrentCursor { get; private set; }
		public TouchCursor StartCursor { get; private set; }
		public TouchCursor EndCursor { get; private set; }

		private int startCaretPosition;

		public bool ShouldCurrentCursorFrontBeVisible
		{
			get
			{
				if(CurrentCursor.Selected) { return true; }

				return CursorWithinBounds(TextNavigator.TextAreaTransform, CurrentCursor.RectTransform);
			}
		}

		public bool ShouldStartCursorFrontBeVisible
		{
			get
			{
				if(StartCursor.Selected) { return true; }

				return CursorWithinBounds(TextNavigator.TextAreaTransform, StartCursor.RectTransform);
			}
		}

		public bool ShouldEndCursorFrontBeVisible
		{
			get
			{
				if(EndCursor.Selected) { return true; }

				return CursorWithinBounds(TextNavigator.TextAreaTransform, EndCursor.RectTransform);
			}
		}

		public override void Initialize()
		{
			base.Initialize();

			CurrentCursor = transform.Find("CurrentCursor").GetComponent<TouchCursor>();
			StartCursor = transform.Find("StartCursor").GetComponent<TouchCursor>();
			EndCursor = transform.Find("EndCursor").GetComponent<TouchCursor>();

			CurrentCursor.TouchCursorSelected += OnCurrentCursorSelected;
			CurrentCursor.TouchCursorMoved += OnCurrentCursorMoved;
			CurrentCursor.TouchCursorMoveFinished += OnCurrentCursorMoveFinished;
			StartCursor.TouchCursorMoved += OnCursorMoved;
			StartCursor.TouchCursorMoveFinished += OnCursorMoveFinished;
			EndCursor.TouchCursorMoved += OnCursorMoved;
			EndCursor.TouchCursorMoveFinished += OnCursorMoveFinished;
		}

		public override void OnSelectionUpdate(int selectionStartPosition, int selectionEndPosition)
		{
			if(InputField.CanUseTouchSelectionCursors)
			{
				UpdateMobileSelectionCursors(Engine.VisibleSelectionStartPosition, Engine.VisibleSelectionEndPosition);
			}
			else
			{
				DisableStartCursor();
				DisableEndCursor();
			}

			if(InputField.CanUseActionBar)
			{
				UpdateMobileCurrentCursor();
			}
			else
			{
				DisableCurrentCursor();
			}
		}

		internal void UpdateMobileSelectionCursors(int selectionStartPosition, int selectionEndPosition)
		{
			bool resetMobileCursorPosition = (!StartCursor.Selected && !EndCursor.Selected);

			TextRenderer activeTextRenderer = Engine.GetActiveTextRenderer();
			if(selectionEndPosition > selectionStartPosition || StartCursor.Selected || EndCursor.Selected)
			{
				if(selectionStartPosition >= 0)
				{
					if(resetMobileCursorPosition)
					{
						int charIndex = Mathf.Clamp(Engine.VisibleSelectionStartPosition, 0, activeTextRenderer.CharacterCount - 1);
						CharacterInfo charInfo = activeTextRenderer.GetCharacterInfo(charIndex);
						int lineIndex = activeTextRenderer.DetermineCharacterLine(charIndex);
						LineInfo lineInfo = activeTextRenderer.GetLineInfo(lineIndex);

						Vector2 startCursorPosition = new Vector2(charInfo.position.x, lineInfo.topY - lineInfo.height);
						StartCursor.UpdatePosition(startCursorPosition, true);
					}

					EnableStartCursor();
					if(ShouldStartCursorFrontBeVisible)
					{
						ShowStartCursorFront();
					}
					else
					{
						HideStartCursorFront();
					}
				}
				else
				{
					DisableStartCursor();
				}

				if(selectionEndPosition >= 0)
				{
					if(resetMobileCursorPosition)
					{
						int charIndex = Mathf.Clamp(Engine.VisibleSelectionEndPosition, 0, activeTextRenderer.CharacterCount - 1);
						CharacterInfo charInfo = activeTextRenderer.GetCharacterInfo(charIndex);
						int lineIndex = activeTextRenderer.DetermineCharacterLine(charIndex);
						LineInfo lineInfo = activeTextRenderer.GetLineInfo(lineIndex);

						Vector2 endCursorPosition = new Vector2(charInfo.position.x, lineInfo.topY - lineInfo.height);
						EndCursor.UpdatePosition(endCursorPosition, true);
					}

					EnableEndCursor();
					if(ShouldEndCursorFrontBeVisible)
					{
						ShowEndCursorFront();
					}
					else
					{
						HideEndCursorFront();
					}
				}
				else
				{
					DisableEndCursor();
				}

				DisableCurrentCursor();
			}
			else
			{
				DisableStartCursor();
				DisableEndCursor();
			}
		}

		internal void UpdateMobileCurrentCursor()
		{
			TextRenderer activeTextRenderer = Engine.GetActiveTextRenderer();
			if(StartCursor.Selected || EndCursor.Selected)
			{
				DisableCurrentCursor();
				return;
			}

			int charIndex = Mathf.Clamp(Engine.VisibleCaretPosition, 0, activeTextRenderer.CharacterCount - 1);
			if(!CurrentCursor.Selected)
			{
				CharacterInfo charInfo = activeTextRenderer.GetCharacterInfo(charIndex);
				int lineIndex = activeTextRenderer.DetermineCharacterLine(charIndex);
				LineInfo lineInfo = activeTextRenderer.GetLineInfo(lineIndex);

				Vector2 currentCursorPosition = new Vector2(charInfo.position.x, lineInfo.topY - lineInfo.height);
				if(Engine.CaretPosition >= activeTextRenderer.CharacterCountVisible)
				{
					currentCursorPosition.x += charInfo.width;
				}

				CurrentCursor.UpdatePosition(currentCursorPosition, true);
			}

			if(Engine.HasSelection)
			{
				DisableCurrentCursor();
			}
			else if(!Engine.HasModifiedTextAfterClick && InputField.CanUseActionBar)
			{
				EnableCurrentCursor();
			}
			else
			{
				DisableCurrentCursor();
			}

			if(InputField.CanUseActionBar && !Engine.HasSelection)
			{
				TextNavigator.ActionBar.transform.SetParent(InputField.transform);
				TextNavigator.ActionBar.transform.localScale = Vector3.one;

				TextNavigator.UpdateActionBarPosition();
			}
		}

		public override void Setup(Transform parent, TextNavigator textNavigator)
		{
			base.Setup(parent, textNavigator);

			ScrollArea scrollArea = TextNavigator.TextAreaTransform.GetComponent<ScrollArea>();
			scrollArea.OnValueChanged.RemoveListener(OnTextScrollChanged); //Remove old listener if any
			scrollArea.OnValueChanged.AddListener(OnTextScrollChanged);

			CurrentCursor.Type = MobileCursorType.CURRENT_CURSOR;
			StartCursor.Type = MobileCursorType.START_CURSOR;
			EndCursor.Type = MobileCursorType.END_CURSOR;
		}

		public void OnTextScrollChanged(Vector2 scroll)
		{
			CurrentCursor.OnTextScrollChanged(scroll);
			StartCursor.OnTextScrollChanged(scroll);
			EndCursor.OnTextScrollChanged(scroll);

			if(ShouldCurrentCursorFrontBeVisible)
			{
				ShowCurrentCursorFront();
			}
			else
			{
				HideCurrentCursorFront();
			}

			if(ShouldStartCursorFrontBeVisible)
			{
				ShowStartCursorFront();
			}
			else
			{
				HideStartCursorFront();
			}

			if(ShouldEndCursorFrontBeVisible)
			{
				ShowEndCursorFront();
			}
			else
			{
				HideEndCursorFront();
			}
		}

		public void EnableCurrentCursor()
		{
			EnsureInitialization();
			CurrentCursor.enabled = true;
		}

		public void EnableStartCursor()
		{
			EnsureInitialization();
			StartCursor.enabled = true;
		}

		public void EnableEndCursor()
		{
			EnsureInitialization();
			EndCursor.enabled = true;
		}

		public void DisableCurrentCursor()
		{
			EnsureInitialization();
			CurrentCursor.enabled = false;
		}

		public void DisableStartCursor()
		{
			EnsureInitialization();
			StartCursor.enabled = false;
		}

		public void DisableEndCursor()
		{
			EnsureInitialization();
			EndCursor.enabled = false;
		}

		public void ShowCurrentCursorFront()
		{
			EnsureInitialization();
			CurrentCursor.ShowFront();
		}

		public void ShowStartCursorFront()
		{
			EnsureInitialization();
			StartCursor.ShowFront();
		}

		public void ShowEndCursorFront()
		{
			EnsureInitialization();
			EndCursor.ShowFront();
		}

		public void HideCursors()
		{
			EnsureInitialization();
			CurrentCursor.enabled = false;
			StartCursor.enabled = false;
			EndCursor.enabled = false;
		}

		public void HideCurrentCursorFront()
		{
			EnsureInitialization();
			CurrentCursor.HideFront();
		}

		public void HideStartCursorFront()
		{
			EnsureInitialization();
			StartCursor.HideFront();
		}

		public void HideEndCursorFront()
		{
			EnsureInitialization();
			EndCursor.HideFront();
		}

		public bool CursorWithinBounds(RectTransform boundsTransform, RectTransform cursorTransform)
		{
			Vector3[] boundsCorners = new Vector3[4]; //BottomLeft, TopLeft, TopRight, BottomRight
			boundsTransform.GetWorldCorners(boundsCorners);

			Vector3[] cursorBounds = new Vector3[4]; //BottomLeft, TopLeft, TopRight, BottomRight
			cursorTransform.GetWorldCorners(cursorBounds);
			Vector2 cursorCenter = (cursorBounds[2] + cursorBounds[0]) * 0.5f;
			Vector2 cursorSize = cursorBounds[2] - cursorBounds[0];
			cursorSize *= 1.1f; //Make size slighly bigger to avoid accuracy problems
			cursorBounds[0] = cursorCenter + new Vector2(-(cursorSize.x * 0.5f), -(cursorSize.y * 0.5f));
			cursorBounds[1] = cursorCenter + new Vector2(-(cursorSize.x * 0.5f), (cursorSize.y * 0.5f));
			cursorBounds[2] = cursorCenter + new Vector2((cursorSize.x * 0.5f), (cursorSize.y * 0.5f));
			cursorBounds[3] = cursorCenter + new Vector2((cursorSize.x * 0.5f), -(cursorSize.y * 0.5f));

			Vector2 min = boundsCorners[0];
			Vector2 max = boundsCorners[2];

			int length = cursorBounds.Length;
			int outOfBoundsCount = 0;
			int yOutOfBoundsCount = 0;
			for(int i = 0; i < length; i++)
			{
				Vector2 point = cursorBounds[i];

				if(point.x < min.x || point.x > max.x)
				{
					if(i != 0 && i != 3 && (point.y < min.y || point.y > max.y)) //Don't check bottom points
					{
						yOutOfBoundsCount++;
					}

					outOfBoundsCount++;
				}
				else if(i != 0 && i != 3 && (point.y < min.y || point.y > max.y)) //Don't check bottom points
				{
					yOutOfBoundsCount++;
					outOfBoundsCount++;
				}
			}

			if(outOfBoundsCount == length)
			{
				return false;
			}
			else
			{
				if(yOutOfBoundsCount > 0)
				{
					return false;
				}
				return true;
			}
		}

		public override void OnCanvasScaleChanged(float canvasScaleFactor)
		{
			UpdateCursorSize(canvasScaleFactor);
		}

		internal void UpdateCursorSize(float canvasScaleFactor)
		{
#if UNITY_EDITOR
			int thumbSize = -1;
#else
			int thumbSize = Util.DetermineThumbSize();
#endif
			float cursorSize;
			if(thumbSize <= 0) //Unknown DPI
			{
				if(InputField.TextRenderer.ResizeTextForBestFit)
				{
					cursorSize = InputField.TextRenderer.FontSizeUsedForBestFit * 1.5f;
				}
				else
				{
					cursorSize = InputField.TextRenderer.FontSize * 1.5f;
				}
			}
			else
			{
				Canvas canvas = GetComponentInParent<Canvas>();
				if(canvas.renderMode == RenderMode.WorldSpace)
				{
					RectTransform canvasTransform = canvas.GetComponent<RectTransform>();
					Vector3[] canvasCorners = new Vector3[4];
					canvasTransform.GetWorldCorners(canvasCorners);
					Vector2 canvasWorldSize = new Vector2(Mathf.Abs(canvasCorners[3].x - canvasCorners[1].x), Mathf.Abs(canvasCorners[3].y - canvasCorners[1].y));
					float ratioY = canvasWorldSize.y / (canvas.worldCamera.orthographicSize * 2);
					cursorSize = ((thumbSize * THUMB_SIZE_RATIO) / Screen.height) * (canvasTransform.rect.height / ratioY / canvas.scaleFactor);
				}
				else
				{
					cursorSize = (thumbSize * THUMB_SIZE_RATIO) / canvasScaleFactor;
				}
			}

			cursorSize *= Settings.TouchSelectionCursorsScale;

			CurrentCursor.UpdateSize(new Vector2(cursorSize, cursorSize));
			StartCursor.UpdateSize(new Vector2(cursorSize, cursorSize));
			EndCursor.UpdateSize(new Vector2(cursorSize, cursorSize));
		}

		public void OnCursorMoved(TouchCursor mobileCursor, PointerEventData eventData)
		{
			mobileCursor.OutOfBounds = PositionOutOfBounds(eventData);

			Vector2 localMousePosition;
			RectTransformUtility.ScreenPointToLocalPointInRectangle(TextNavigator.TextAreaTransform, eventData.position, eventData.pressEventCamera, out localMousePosition);
			localMousePosition.x += (TextNavigator.TextAreaTransform.rect.width * 0.5f);
			localMousePosition.y -= (TextNavigator.TextAreaTransform.rect.height * 0.5f);
			localMousePosition += mobileCursor.Offset;

			if(mobileCursor.Type == MobileCursorType.START_CURSOR)
			{
				bool switchToEnd;
				Vector2 cursorPosition;
				TextNavigator.UpdateSelectionStart(localMousePosition, out cursorPosition, out switchToEnd);
				mobileCursor.TargetPosition = cursorPosition;

				if(switchToEnd)
				{
					mobileCursor.Type = MobileCursorType.END_CURSOR;
				}
			}
			else if(mobileCursor.Type == MobileCursorType.END_CURSOR)
			{
				bool switchToStart;
				Vector2 cursorPosition;
				TextNavigator.UpdateSelectionEnd(localMousePosition, out cursorPosition, out switchToStart);
				mobileCursor.TargetPosition = cursorPosition;

				if(switchToStart)
				{
					mobileCursor.Type = MobileCursorType.START_CURSOR;
				}
			}

			TextAlignment alignment = TextNavigator.Engine.GetActiveTextRenderer().TextAlignment;
			if(alignment == TextAlignment.BOTTOM || alignment == TextAlignment.CENTER || alignment == TextAlignment.TOP)
			{
				localMousePosition.x -= (TextNavigator.TextAreaTransform.rect.width * 0.5f);
				localMousePosition.x += (TextNavigator.TextContentTransform.rect.width * 0.5f);
			}
			else if(alignment == TextAlignment.BOTTOM_RIGHT || alignment == TextAlignment.RIGHT || alignment == TextAlignment.TOP_RIGHT)
			{
				localMousePosition.x -= TextNavigator.TextAreaTransform.rect.width;
				localMousePosition.x += TextNavigator.TextContentTransform.rect.width;
			}

			if(alignment == TextAlignment.LEFT || alignment == TextAlignment.CENTER || alignment == TextAlignment.RIGHT)
			{
				localMousePosition.y += (TextNavigator.TextAreaTransform.rect.height * 0.5f);
				localMousePosition.y -= (TextNavigator.TextContentTransform.rect.height * 0.5f);
			}
			else if(alignment == TextAlignment.BOTTOM_LEFT || alignment == TextAlignment.BOTTOM || alignment == TextAlignment.BOTTOM_RIGHT)
			{
				localMousePosition.y += (TextNavigator.TextAreaTransform.rect.height * 1f);
				localMousePosition.y -= (TextNavigator.TextContentTransform.rect.height * 1f);
			}

			Vector2 anchoredPosition = localMousePosition;
			anchoredPosition -= TextNavigator.TextContentTransform.anchoredPosition;
			CursorClampMode cursorClampMode = TextNavigator.InputField.CursorClampMode;
			if(cursorClampMode == CursorClampMode.TEXT_BOUNDS)
			{
				anchoredPosition = ClampPositionTextBounds(anchoredPosition);
			}
			else if(cursorClampMode == CursorClampMode.INPUTFIELD_BOUNDS)
			{
				anchoredPosition = ClampPositionInputFieldBounds(anchoredPosition);
			}
			mobileCursor.UpdatePosition(anchoredPosition);
		}

		public Vector2 ClampPositionTextBounds(Vector2 anchoredPosition)
		{
			Vector2 textContentSize = TextNavigator.TextContentTransform.rect.size;

			float minX = 0;
			float maxX = textContentSize.x;
			if(InputField.Multiline)
			{
				TextRenderer textRenderer = Engine.GetActiveTextRenderer();
				if(textRenderer.TextAlignment == TextAlignment.BOTTOM_LEFT || textRenderer.TextAlignment == TextAlignment.LEFT || textRenderer.TextAlignment == TextAlignment.TOP_LEFT)
				{
					maxX = textRenderer.MultilineMaxWidth;
				}
				else if(textRenderer.TextAlignment == TextAlignment.BOTTOM || textRenderer.TextAlignment == TextAlignment.CENTER || textRenderer.TextAlignment == TextAlignment.TOP)
				{
					float centerX = (maxX - minX) * 0.5f;
					minX = centerX - (textRenderer.MultilineMaxWidth * 0.5f);
					maxX = centerX + (textRenderer.MultilineMaxWidth * 0.5f);
				}
				if(textRenderer.TextAlignment == TextAlignment.BOTTOM_RIGHT || textRenderer.TextAlignment == TextAlignment.RIGHT || textRenderer.TextAlignment == TextAlignment.TOP_RIGHT)
				{
					minX = maxX - textRenderer.MultilineMaxWidth;
				}
			}
			anchoredPosition.x = Mathf.Clamp(anchoredPosition.x, minX, maxX);

			float minY = -textContentSize.y;
			float maxY = 0;
			anchoredPosition.y = Mathf.Clamp(anchoredPosition.y, minY, maxY);

			return anchoredPosition;
		}

		public Vector2 ClampPositionInputFieldBounds(Vector2 anchoredPosition)
		{
			RectTransform canvasRectTransform = TextNavigator.Canvas.GetComponent<RectTransform>();

			Vector3[] inputFieldCorners = new Vector3[4];
			TextNavigator.InputField.RectTransform.GetWorldCorners(inputFieldCorners);
			for(int i = 0; i < 4; i++)
			{
				inputFieldCorners[i] = canvasRectTransform.InverseTransformPoint(inputFieldCorners[i]);
			}

			Vector3[] contentCorners = new Vector3[4];
			TextNavigator.TextContentTransform.GetWorldCorners(contentCorners);
			for(int i = 0; i < 4; i++)
			{
				contentCorners[i] = canvasRectTransform.InverseTransformPoint(contentCorners[i]);
			}
			Vector2 contentSize = contentCorners[2] - contentCorners[0];

			float leftMargin = contentCorners[0].x - inputFieldCorners[0].x;
			float rightMargin = inputFieldCorners[3].x - contentCorners[3].x;
			float minX = -leftMargin;
			float maxX = contentSize.x + rightMargin;
			anchoredPosition.x = Mathf.Clamp(anchoredPosition.x, minX, maxX);

			float bottomMargin = contentCorners[0].y - inputFieldCorners[0].y;
			float topMargin = inputFieldCorners[1].y - contentCorners[1].y;
			float minY = -(contentSize.y + bottomMargin);
			float maxY = topMargin;
			anchoredPosition.y = Mathf.Clamp(anchoredPosition.y, minY, maxY);

			return anchoredPosition;
		}

		public bool PositionOutOfBounds(PointerEventData eventData)
		{
			return !RectTransformUtility.RectangleContainsScreenPoint(TextNavigator.TextAreaTransform, eventData.position, eventData.pressEventCamera);
		}

		public bool RectTransformWithinBounds(RectTransform rectTransform)
		{
			RectTransform boundsTransform = TextNavigator.TextAreaTransform;
			Vector3[] boundsCorners = new Vector3[4]; //BottomLeft, TopLeft, TopRight, BottomRight
			boundsTransform.GetWorldCorners(boundsCorners);

			Vector3[] corners = new Vector3[4]; //BottomLeft, TopLeft, TopRight, BottomRight
			rectTransform.GetWorldCorners(boundsCorners);

			int length = corners.Length;
			for(int i = 0; i < length; i++)
			{
				Vector3 corner = corners[i];
				if(corner.x >= boundsCorners[0].x && corner.x <= boundsCorners[2].x && corner.y >= boundsCorners[0].y && corner.y <= boundsCorners[2].y)
				{
					return true;
				}
			}

			return false;
		}

		public void OnCursorMoveFinished()
		{
			UpdateMobileSelectionCursors(Engine.VisibleSelectionStartPosition, Engine.VisibleSelectionEndPosition);
			StartCursor.Type = MobileCursorType.START_CURSOR;
			EndCursor.Type = MobileCursorType.END_CURSOR;
		}

		public void OnCurrentCursorSelected()
		{
			startCaretPosition = TextNavigator.Engine.CaretPosition;
		}

		public void OnCurrentCursorMoved(TouchCursor mobileCursor, PointerEventData eventData)
		{
			mobileCursor.OutOfBounds = PositionOutOfBounds(eventData);

			Vector2 localMousePosition;
			RectTransformUtility.ScreenPointToLocalPointInRectangle(TextNavigator.TextAreaTransform, eventData.position, eventData.pressEventCamera, out localMousePosition);
			localMousePosition.x += (TextNavigator.TextAreaTransform.rect.width * 0.5f);
			localMousePosition.y -= (TextNavigator.TextAreaTransform.rect.height * 0.5f);
			localMousePosition += mobileCursor.Offset;

			Vector2 cursorPosition;
			TextNavigator.UpdateCurrentCursor(localMousePosition, out cursorPosition);
			mobileCursor.TargetPosition = cursorPosition;

			TextAlignment alignment = TextNavigator.Engine.GetActiveTextRenderer().TextAlignment;
			if(alignment == TextAlignment.BOTTOM || alignment == TextAlignment.CENTER || alignment == TextAlignment.TOP)
			{
				localMousePosition.x -= (TextNavigator.TextAreaTransform.rect.width * 0.5f);
				localMousePosition.x += (TextNavigator.TextContentTransform.rect.width * 0.5f);
			}
			else if(alignment == TextAlignment.BOTTOM_RIGHT || alignment == TextAlignment.RIGHT || alignment == TextAlignment.TOP_RIGHT)
			{
				localMousePosition.x -= TextNavigator.TextAreaTransform.rect.width;
				localMousePosition.x += TextNavigator.TextContentTransform.rect.width;
			}

			if(alignment == TextAlignment.LEFT || alignment == TextAlignment.CENTER || alignment == TextAlignment.RIGHT)
			{
				localMousePosition.y += (TextNavigator.TextAreaTransform.rect.height * 0.5f);
				localMousePosition.y -= (TextNavigator.TextContentTransform.rect.height * 0.5f);
			}
			else if(alignment == TextAlignment.BOTTOM_LEFT || alignment == TextAlignment.BOTTOM || alignment == TextAlignment.BOTTOM_RIGHT)
			{
				localMousePosition.y += (TextNavigator.TextAreaTransform.rect.height * 1f);
				localMousePosition.y -= (TextNavigator.TextContentTransform.rect.height * 1f);
			}

			Vector2 anchoredPosition = localMousePosition;
			anchoredPosition -= TextNavigator.TextContentTransform.anchoredPosition;
			CursorClampMode cursorClampMode = TextNavigator.InputField.CursorClampMode;
			if(cursorClampMode == CursorClampMode.TEXT_BOUNDS)
			{
				anchoredPosition = ClampPositionTextBounds(anchoredPosition);
			}
			else if(cursorClampMode == CursorClampMode.INPUTFIELD_BOUNDS)
			{
				anchoredPosition = ClampPositionInputFieldBounds(anchoredPosition);
			}
			mobileCursor.UpdatePosition(anchoredPosition);
		}

		public void OnCurrentCursorMoveFinished()
		{
			if(TextNavigator.Engine.CaretPosition == startCaretPosition)
			{
				TextNavigator.ToggleActionBar();
			}
		}
	}
}
