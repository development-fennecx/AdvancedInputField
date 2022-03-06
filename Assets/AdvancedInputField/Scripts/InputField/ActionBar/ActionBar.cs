// Copyright (c) Jeroen van Pienbroek. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AdvancedInputFieldPlugin
{
	public enum ActionBarActionType { CUT, COPY, PASTE, SELECT_ALL, SHOW_REPLACE, REPLACE, CUSTOM }

	/// <summary>The onscreen control for cut, copy, paste and select all operations</summary>
	[RequireComponent(typeof(RectTransform))]
	public class ActionBar: MonoBehaviour
	{
		/// <summary>The multiplier for thumb height to calculate action bar height</summary>
		private const float THUMB_SIZE_RATIO = 0.4f;

		/// <summary>The multiplier of canvas min size (width or height) to calculate action bar width</summary>
		private const float CANVAS_MIN_SIZE_RATIO = 0.9f;

		/// <summary>The maximum amount of buttons that can active at one time (cut, copy, paste, select all)</summary>
		private const int MAX_ACTIONS_PER_PAGE = 3;

		private const int MAX_ACTIONS_IN_BAR = 4;

		/// <summary>The RectTransform for the ActionBar that will be rendered last in the Canvas</summary>
		[SerializeField]
		private CanvasFrontRenderer actionBarRenderer;

		[SerializeField]
		private Button previousPageButton;

		[SerializeField]
		private Button nextPageButton;

		[Tooltip("The padding of the icon on the buttons with an icon")]
		[SerializeField]
		private float iconButtonPadding = 28;

		[Tooltip("Indicates whether to try to find the font size that will make everything fit")]
		[SerializeField]
		private bool optimizeItemSizes = true;

		[Tooltip("The preferred font size, will try this font size first when trying to make everything fit")]
		[SerializeField]
		private int preferredFontSize = 44;

		[Tooltip("The minimal item width")]
		[SerializeField]
		private int minItemWidth = 120;

		[Tooltip("The horizontal text padding when trying to make everything fit")]
		[SerializeField]
		private float horizontalTextPadding = 12;

		[Tooltip("The vertical text padding when trying to make everything fit")]
		[SerializeField]
		private float verticalTextPadding = 4;

		[SerializeField]
		private bool showDividers = true;

		[SerializeField]
		private bool defaultActionsFirst = true;

		[SerializeField]
		private bool showSoloReplaceActionImmediately = false;

		private ActionBarItem[] itemPool;
		private Image[] dividerPool;

		/// <summary>The RectTransform</summary>
		public RectTransform RectTransform { get; private set; }

		/// <summary>The max size of the ActionBar when all buttons are enabled</summary>
		private Vector2 fullSize;

		/// <summary>The size of a button</summary>
		private Vector2 buttonSize;

		/// <summary>The InputField</summary>
		public AdvancedInputField InputField { get; private set; }

		/// <summary>The TextInputHandler</summary>
		public TextInputHandler TextInputHandler { get; private set; }

		/// <summary>The TextNavigator</summary>
		public TextNavigator TextNavigator { get; private set; }

		/// <summary>The Canvas</summary>
		public Canvas Canvas { get { return InputField.Canvas; } }

		/// <summary>Indicates if the ActionBar is visible</summary>
		public bool Visible { get { return gameObject.activeInHierarchy; } }

		/// <summary>Indicates if the cut operation is enabled</summary>
		private bool cut;

		/// <summary>Indicates if the copy operation is enabled</summary>
		private bool copy;

		/// <summary>Indicates if the paste operation is enabled</summary>
		private bool paste;

		/// <summary>Indicates if the select all operation is enabled</summary>
		private bool selectAll;

		/// <summary>All active actions</summary>
		private List<ActionBarAction> actions;

		/// <summary>The default actions: cut, copy, paste, select all</summary>
		private List<ActionBarAction> defaultActions;

		/// <summary>The replace actions</summary>
		private List<ActionBarAction> replaceActions;

		/// <summary>The custom actions</summary>
		private List<ActionBarAction> customActions;

		/// <summary>Current action index used for pages</summary>
		private int actionIndex;

		/// <summary>The text used for the cut action</summary>
		public string CutText { get; set; }

		/// <summary>The text used for the copy action</summary>
		public string CopyText { get; set; }

		/// <summary>The text used for the paste action</summary>
		public string PasteText { get; set; }

		/// <summary>The text used for the select all action</summary>
		public string SelectAllText { get; set; }

		/// <summary>The text used for the replace action</summary>
		public string ReplaceText { get; set; }

		/// <summary>Indicates if the default actions should be placed before the custom actions</summary>
		public bool DefaultActionsFirst
		{
			get { return defaultActionsFirst; }
			set
			{
				if(defaultActionsFirst != value)
				{
					defaultActionsFirst = value;
					RefreshActions();
					LoadPage();
				}
			}
		}

		/// <summary>Show the replace action immediately (not in the "Replace..." submenu) if there is only one</summary>
		public bool ShowSoloReplaceActionImmediately
		{
			get { return showSoloReplaceActionImmediately; }
			set
			{
				if(showSoloReplaceActionImmediately != value)
				{
					showSoloReplaceActionImmediately = value;
					RefreshActions();
					LoadPage();
				}
			}
		}

		/// <summary>Initializes this class</summary>
		internal void Initialize(AdvancedInputField inputField, TextInputHandler textInputHandler, TextNavigator textNavigator)
		{
			InputField = inputField;
			TextInputHandler = textInputHandler;
			TextNavigator = textNavigator;

			if(Canvas != null)
			{
				UpdateSize(Canvas.scaleFactor);
				actionBarRenderer.Initialize();
			}
		}

		#region UNITY
		private void Awake()
		{
			RectTransform = GetComponent<RectTransform>();
			itemPool = actionBarRenderer.transform.Find("Items").GetComponentsInChildren<ActionBarItem>();
			dividerPool = actionBarRenderer.transform.Find("Dividers").GetComponentsInChildren<Image>();
			actions = new List<ActionBarAction>();
			defaultActions = new List<ActionBarAction>();
			replaceActions = new List<ActionBarAction>();
			customActions = new List<ActionBarAction>();
			gameObject.SetActive(false);
		}

		private void OnEnable()
		{
			actionBarRenderer.Show();

			int length = itemPool.Length;
			for(int i = 0; i < length; i++)
			{
				itemPool[i].Click += OnItemClick;
			}
		}

		private void OnDisable()
		{
			actionBarRenderer.Hide();

			int length = itemPool.Length;
			for(int i = 0; i < length; i++)
			{
				itemPool[i].Click -= OnItemClick;
			}
		}

		private void OnDestroy()
		{
			if(actionBarRenderer != null)
			{
				Destroy(actionBarRenderer.gameObject);
			}
		}

		private void Update()
		{
			actionBarRenderer.SyncTransform(RectTransform);
		}
		#endregion

		private void ConfigureIconButtonAnchors()
		{
			float scaleFactor = fullSize.y / 100;
			float padding = iconButtonPadding * scaleFactor;

			RectTransform rectTransform = previousPageButton.transform.Find("Icon").GetComponent<RectTransform>();
			rectTransform.anchorMin = new Vector2(0, 0);
			rectTransform.anchorMax = new Vector2(1, 1);
			rectTransform.pivot = new Vector2(0.5f, 0.5f);
			rectTransform.offsetMin = new Vector2(padding, padding);
			rectTransform.offsetMax = new Vector2(-padding, -padding);
			rectTransform.localScale = Vector3.one;

			rectTransform = nextPageButton.transform.Find("Icon").GetComponent<RectTransform>();
			rectTransform.anchorMin = new Vector2(0, 0);
			rectTransform.anchorMax = new Vector2(1, 1);
			rectTransform.pivot = new Vector2(0.5f, 0.5f);
			rectTransform.offsetMin = new Vector2(padding, padding);
			rectTransform.offsetMax = new Vector2(-padding, -padding);
			rectTransform.localScale = Vector3.one;
		}

		/// <summary>Updates the replace actions with given actions.</summary>
		public void UpdateReplaceActions(List<ActionBarAction> replaceActions)
		{
			this.replaceActions = replaceActions;
			RefreshActions();
			LoadPage();
		}

		/// <summary>Updates the custom actions with given actions.</summary>
		public void UpdateCustomActions(List<ActionBarAction> customActions)
		{
			this.customActions = customActions;
			RefreshActions();
			LoadPage();
		}

		public void OnNextPageClick()
		{
			actionIndex += MAX_ACTIONS_PER_PAGE;
			LoadPage();
		}

		public void OnPreviousPageClick()
		{
			actionIndex -= MAX_ACTIONS_PER_PAGE;
			LoadPage();
		}

		public void OnItemClick(ActionBarItem item)
		{
			ActionBarAction action = item.Action;
			switch(item.Action.type)
			{
				case ActionBarActionType.COPY: TextInputHandler.OnCopy(); break;
				case ActionBarActionType.CUT: TextInputHandler.OnCut(); break;
				case ActionBarActionType.PASTE: TextInputHandler.OnPaste(); break;
				case ActionBarActionType.SELECT_ALL: TextInputHandler.OnSelectAll(); break;
				case ActionBarActionType.SHOW_REPLACE: ShowReplaceActions(); break;
				case ActionBarActionType.REPLACE: TextInputHandler.OnReplace(action.text); break;
				case ActionBarActionType.CUSTOM:
					if(action.onClick != null)
					{
						action.onClick(action);
					}
					else
					{
						Debug.LogWarningFormat("Custom action {0} clicked without event listener, please specify an event listener in ActionBarAction constructor", action.text);
					}
					break;
			}

			InputField.Engine.EventHandler.InvokeActionBarAction(action);
		}

		/// <summary>Determines fullSize and buttonSize</summary>
		internal void UpdateSize(float canvasScaleFactor)
		{
			if(RectTransform == null)
			{
				RectTransform = GetComponent<RectTransform>();
			}

#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WSA
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
				if(Canvas.renderMode == RenderMode.WorldSpace)
				{
					RectTransform canvasTransform = Canvas.GetComponent<RectTransform>();
					Vector3[] canvasCorners = new Vector3[4];
					canvasTransform.GetWorldCorners(canvasCorners);
					Vector2 canvasWorldSize = new Vector2(Mathf.Abs(canvasCorners[3].x - canvasCorners[1].x), Mathf.Abs(canvasCorners[3].y - canvasCorners[1].y));
					float ratioY = canvasWorldSize.y / (Canvas.worldCamera.orthographicSize * 2);
					cursorSize = ((thumbSize * THUMB_SIZE_RATIO) / Screen.height) * (canvasTransform.rect.height / ratioY / Canvas.scaleFactor);
				}
				else
				{
					cursorSize = (thumbSize * THUMB_SIZE_RATIO) / canvasScaleFactor;
				}
			}

			float canvasMinSize = Mathf.Min(Canvas.pixelRect.width, Canvas.pixelRect.height);
			if(Canvas.renderMode == RenderMode.WorldSpace)
			{
				RectTransform canvasTransform = Canvas.GetComponent<RectTransform>();
				Vector3[] canvasCorners = new Vector3[4];
				canvasTransform.GetWorldCorners(canvasCorners);
				Vector2 canvasWorldSize = new Vector2(Mathf.Abs(canvasCorners[3].x - canvasCorners[1].x), Mathf.Abs(canvasCorners[3].y - canvasCorners[1].y));
				float ratioX = canvasWorldSize.x / ((Canvas.worldCamera.orthographicSize * 2) / Screen.height * Screen.width);
				float ratioY = canvasWorldSize.y / (Canvas.worldCamera.orthographicSize * 2);
				canvasMinSize = Mathf.Min(Canvas.pixelRect.width / ratioX, Canvas.pixelRect.height / ratioY);
			}
			fullSize = new Vector2((canvasMinSize * CANVAS_MIN_SIZE_RATIO) / canvasScaleFactor, cursorSize);
			RectTransform.sizeDelta = fullSize;

			buttonSize = new Vector2(fullSize.x / MAX_ACTIONS_IN_BAR, 0);

			actionBarRenderer.RefreshCanvas(Canvas);
			actionBarRenderer.SyncTransform(RectTransform);

			ConfigureIconButtonAnchors();
			CheckInputFieldScale();
		}

		/// <summary>Adjust the scale when the inputfield itself is scaled</summary>
		public void CheckInputFieldScale()
		{
			if(InputField != null)
			{
				Vector3 inputFieldScale = InputField.transform.localScale;
				float threshold = 0.001f;
				if(Mathf.Abs(1 - inputFieldScale.x) > threshold && Mathf.Abs(1 - inputFieldScale.x) > threshold && Mathf.Abs(1 - inputFieldScale.x) > threshold)
				{
					transform.localScale = new Vector3(1 / inputFieldScale.x, 1 / inputFieldScale.y, 1 / inputFieldScale.z);
				}
			}
		}

		/// <summary>Shows the ActionBar</summary>
		/// <param name="cut">Indicates if the cut button should be enabled</param>
		/// <param name="copy">Indicates if the copy button should be enabled</param>
		/// <param name="paste">Indicates if the paste button should be enabled</param>
		/// <param name="selectAll">Indicates if the select all button should be enabled</param>
		public void Show(bool cut, bool copy, bool paste, bool selectAll)
		{
			if(Visible && this.cut == cut && this.copy == copy && this.paste == paste && this.selectAll == selectAll)
			{
				return;
			}
			else
			{
				this.cut = cut;
				this.copy = copy;
				this.paste = paste;
				this.selectAll = selectAll;
			}

			gameObject.SetActive(true);

			if(Canvas != null)
			{
				UpdateSize(Canvas.scaleFactor);
			}

			UpdateButtons();
		}

		internal void RefreshActions()
		{
			actions.Clear();

			if(defaultActionsFirst)
			{
				AddDefaultActions();
				AddReplaceActions();
				actions.AddRange(customActions);
			}
			else
			{
				actions.AddRange(customActions);
				AddDefaultActions();
				AddReplaceActions();
			}

			actionIndex = 0;
		}

		internal void AddDefaultActions()
		{
			if(cut) { actions.Add(new ActionBarAction(ActionBarActionType.CUT, CutText)); }
			if(copy) { actions.Add(new ActionBarAction(ActionBarActionType.COPY, CopyText)); }
			if(paste) { actions.Add(new ActionBarAction(ActionBarActionType.PASTE, PasteText)); }
			if(selectAll) { actions.Add(new ActionBarAction(ActionBarActionType.SELECT_ALL, SelectAllText)); }
		}

		internal void AddReplaceActions()
		{
			if(replaceActions.Count > 0)
			{
				if(replaceActions.Count == 1 && showSoloReplaceActionImmediately)
				{
					actions.AddRange(replaceActions);
				}
				else
				{
					actions.Add(new ActionBarAction(ActionBarActionType.SHOW_REPLACE, ReplaceText));
				}
			}
		}

		internal void ShowReplaceActions()
		{
			actions.Clear();
			actions.AddRange(replaceActions);
			actionIndex = 0;

			LoadPage();
		}

		internal void LoadPage()
		{
			bool exactFit = (actions.Count == MAX_ACTIONS_IN_BAR);
			bool hasPreviousPage = (!exactFit && actionIndex > 0);
			bool hasNextPage = (!exactFit && (actionIndex + MAX_ACTIONS_PER_PAGE) < actions.Count);
			int maxActionsOnPage = MAX_ACTIONS_PER_PAGE;
			if(exactFit) { maxActionsOnPage = MAX_ACTIONS_IN_BAR; }
			float x = 0;

			previousPageButton.gameObject.SetActive(hasPreviousPage);
			if(hasPreviousPage)
			{
				RectTransform buttonTransform = previousPageButton.GetComponent<RectTransform>();
				Vector2 sizeDelta = buttonTransform.sizeDelta;
				sizeDelta.x = (buttonSize.x * 0.5f);
				buttonTransform.sizeDelta = sizeDelta;
				buttonTransform.anchoredPosition = new Vector2(x, 0);

				x += buttonTransform.rect.width;
			}

			int itemsLength = itemPool.Length;
			for(int i = 0; i < itemsLength; i++)
			{
				itemPool[i].gameObject.SetActive(false);
			}

			int dividersLength = dividerPool.Length;
			for(int i = 0; i < dividersLength; i++)
			{
				dividerPool[i].gameObject.SetActive(false);
			}

			int dividerIndex = 0;

			int actionsLength = actions.Count;
			for(int i = 0; i < maxActionsOnPage; i++)
			{
				ActionBarItem item = itemPool[i];

				int currentActionIndex = actionIndex + i;
				if(currentActionIndex >= actionsLength)
				{
					break;
				}

				if(showDividers && (hasPreviousPage || i > 0))
				{
					Image divider = dividerPool[dividerIndex];
					divider.gameObject.SetActive(true);
					divider.rectTransform.anchoredPosition = new Vector2(x, 0);
					dividerIndex++;
				}

				item.gameObject.SetActive(true);
				item.ConfigureUI(actions[currentActionIndex]);
				RectTransform buttonTransform = item.GetComponent<RectTransform>();
				Vector2 sizeDelta = buttonTransform.sizeDelta;
				sizeDelta.x = buttonSize.x;
				buttonTransform.sizeDelta = sizeDelta;
				buttonTransform.anchoredPosition = new Vector2(x, 0);

				x += buttonTransform.rect.width;
			}

			nextPageButton.gameObject.SetActive(hasNextPage);
			if(hasNextPage)
			{
				if(showDividers)
				{
					Image divider = dividerPool[dividerIndex];
					divider.gameObject.SetActive(true);
					divider.rectTransform.anchoredPosition = new Vector2(x, 0);
				}

				RectTransform buttonTransform = nextPageButton.GetComponent<RectTransform>();
				Vector2 sizeDelta = buttonTransform.sizeDelta;
				sizeDelta.x = (buttonSize.x * 0.5f);
				buttonTransform.sizeDelta = sizeDelta;
				buttonTransform.anchoredPosition = new Vector2(x, 0);

				x += buttonTransform.rect.width;
			}

			RectTransform.sizeDelta = new Vector2(Mathf.Abs(x), fullSize.y);

			if(optimizeItemSizes)
			{
				OptimizeActionBarItemSizes();
			}

			if(TextNavigator != null)
			{
				TextNavigator.KeepActionBarWithinBounds();
			}
		}

		internal void OptimizeActionBarItemSizes()
		{
			float[] preferredWidths = new float[itemPool.Length];

			RectTransform nextPageButtonTransform = null;
			if(nextPageButton.gameObject.activeInHierarchy)
			{
				nextPageButtonTransform = nextPageButton.GetComponent<RectTransform>();
			}

			RectTransform previousPageButtonTransform = null;
			if(previousPageButton.gameObject.activeInHierarchy)
			{
				previousPageButtonTransform = previousPageButton.GetComponent<RectTransform>();
			}

			int bestFontSize = TryDetermineBestFontSize(ref preferredWidths, out int minFontSizeToFitHeight, previousPageButtonTransform, nextPageButtonTransform);
			if(bestFontSize < minFontSizeToFitHeight)
			{
				bool success = TryTrimItemTexts(ref preferredWidths, minFontSizeToFitHeight, previousPageButtonTransform, nextPageButtonTransform);
				if(!success)
				{
					Debug.LogWarning("Couldn't fit all ActionBarItems with current settings. Check the optimize settings on the Action prefabs." +
						"\nCurrent configured ActionBar prefabs can be found in the Global Settings (TopBar: Tools => Advanced Input Field => Global Settings)");
					return;
				}
				bestFontSize = minFontSizeToFitHeight;
			}

			ApplyOptimalItemSizes(bestFontSize, ref preferredWidths, previousPageButtonTransform, nextPageButtonTransform);
		}

		internal int TryDetermineBestFontSize(ref float[] preferredWidths, out int minFontSizeToFitHeight, RectTransform previousPageButtonTransform, RectTransform nextPageButtonTransform)
		{
			float scaleFactor = fullSize.y / 100;
			int bestFontSize = preferredFontSize;
			minFontSizeToFitHeight = 0;
			int itemsLength = itemPool.Length;

			for(int fontSize = Mathf.RoundToInt(preferredFontSize * scaleFactor); fontSize > 0; fontSize--)
			{
				float totalWidth = 0;
				float maxHeight = 0;

				for(int i = 0; i < itemsLength; i++)
				{
					ActionBarItem item = itemPool[i];
					if(item.gameObject.activeInHierarchy)
					{
						item.TextTrimAmount = 0;
						float preferredWidth = Mathf.Max(item.DeterminePreferredWidth(fontSize) + (horizontalTextPadding * scaleFactor * 2), minItemWidth);
						preferredWidths[i] = preferredWidth;
						totalWidth += preferredWidth;

						maxHeight = Mathf.Max(item.DeterminePreferredHeight(fontSize) + (verticalTextPadding * scaleFactor * 2), maxHeight);
						if(maxHeight <= fullSize.y && fontSize > minFontSizeToFitHeight)
						{
							minFontSizeToFitHeight = fontSize;
						}
					}
				}

				if(previousPageButton.gameObject.activeInHierarchy)
				{
					totalWidth += previousPageButtonTransform.rect.width;
				}

				if(nextPageButton.gameObject.activeInHierarchy)
				{
					totalWidth += nextPageButtonTransform.rect.width;
				}

				if(totalWidth <= fullSize.x && maxHeight <= fullSize.y)
				{
					bestFontSize = fontSize;
					break;
				}
			}

			return bestFontSize;
		}

		internal bool TryTrimItemTexts(ref float[] preferredWidths, int minFontSizeToFitHeight, RectTransform previousPageButtonTransform, RectTransform nextPageButtonTransform)
		{
			float scaleFactor = fullSize.y / 100;
			int itemsLength = itemPool.Length;

			for(int tries = 1; tries < 100; tries++)
			{
				float totalWidth = 0;
				float maxItemHeight = 0;

				int longestTextIndex = -1;
				int longestTextLength = 0;

				for(int i = 0; i < itemsLength; i++)
				{
					ActionBarItem item = itemPool[i];
					if(item.gameObject.activeInHierarchy && item.Label.text.Length > longestTextLength)
					{
						longestTextIndex = i;
						longestTextLength = item.Label.text.Length;
					}
				}

				ActionBarItem longestTextItem = itemPool[longestTextIndex];
				longestTextItem.TextTrimAmount++;

				for(int i = 0; i < itemsLength; i++)
				{
					ActionBarItem item = itemPool[i];
					if(item.gameObject.activeInHierarchy)
					{
						float preferredWidth = Mathf.Max(item.DeterminePreferredWidth(minFontSizeToFitHeight) + (horizontalTextPadding * scaleFactor * 2), minItemWidth);
						preferredWidths[i] = preferredWidth;
						totalWidth += preferredWidth;

						maxItemHeight = Mathf.Max(item.DeterminePreferredHeight(minFontSizeToFitHeight) + (verticalTextPadding * scaleFactor * 2), maxItemHeight);
					}
				}


				if(previousPageButton.gameObject.activeInHierarchy)
				{
					totalWidth += previousPageButtonTransform.rect.width;
				}

				if(nextPageButton.gameObject.activeInHierarchy)
				{
					totalWidth += nextPageButtonTransform.rect.width;
				}

				if(totalWidth <= fullSize.x && maxItemHeight <= fullSize.y)
				{
					return true;
				}
			}

			for(int i = 0; i < itemsLength; i++) //Restore trim amount when failure
			{
				ActionBarItem item = itemPool[i];
				if(item.gameObject.activeInHierarchy)
				{
					item.TextTrimAmount = 0;
				}
			}

			return false;
		}

		internal void ApplyOptimalItemSizes(int bestFontSize, ref float[] preferredWidths, RectTransform previousPageButtonTransform, RectTransform nextPageButtonTransform)
		{
			int itemsLength = itemPool.Length;
			int dividersLength = dividerPool.Length;
			for(int i = 0; i < dividersLength; i++)
			{
				dividerPool[i].gameObject.SetActive(false);
			}

			float x = 0;
			int dividerIndex = 0;

			if(previousPageButton.gameObject.activeInHierarchy)
			{
				previousPageButtonTransform.anchoredPosition = new Vector2(x, 0);
				x += previousPageButtonTransform.rect.width;

				Image divider = dividerPool[dividerIndex];
				divider.gameObject.SetActive(true);
				divider.rectTransform.anchoredPosition = new Vector2(x, 0);
				dividerIndex++;
			}

			for(int i = 0; i < itemsLength; i++)
			{
				ActionBarItem item = itemPool[i];
				if(item.gameObject.activeInHierarchy)
				{
					if(i > 0)
					{
						Image divider = dividerPool[dividerIndex];
						divider.gameObject.SetActive(true);
						divider.rectTransform.anchoredPosition = new Vector2(x, 0);
						dividerIndex++;
					}

					item.Label.resizeTextForBestFit = false;
					item.Label.fontSize = bestFontSize;
					Vector2 sizeDelta = item.RectTransform.sizeDelta;
					sizeDelta.x = preferredWidths[i];
					item.RectTransform.sizeDelta = sizeDelta;
					item.RectTransform.anchoredPosition = new Vector2(x, 0);
					x += preferredWidths[i];
				}
			}

			if(nextPageButton.gameObject.activeInHierarchy)
			{
				if(showDividers)
				{
					Image divider = dividerPool[dividerIndex];
					divider.gameObject.SetActive(true);
					divider.rectTransform.anchoredPosition = new Vector2(x, 0);
				}

				nextPageButtonTransform.anchoredPosition = new Vector2(x, 0);
				x += nextPageButtonTransform.rect.width;
			}

			RectTransform.sizeDelta = new Vector2(Mathf.Abs(x), fullSize.y);
		}

		internal void UpdateButtons()
		{
			if(defaultActions == null) { return; }

			RefreshActions();
			LoadPage();

			if(Canvas != null)
			{
				actionBarRenderer.RefreshCanvas(Canvas);
				actionBarRenderer.SyncTransform(RectTransform);
			}
		}

		/// <summary>Changes the position of the ActionBar</summary>
		/// <param name="position">The new position of the ActionBar</param>
		internal void UpdatePosition(Vector2 position)
		{
			if(Canvas != null)
			{
				RectTransform.anchoredPosition = position;
				RectTransform.SetAsLastSibling();

				actionBarRenderer.RefreshCanvas(Canvas);
				actionBarRenderer.SyncTransform(RectTransform);
			}
		}

		/// <summary>Hides the ActionBar</summary>
		public void Hide()
		{
			gameObject.SetActive(false);
			actionBarRenderer.gameObject.SetActive(false);
		}
	}

	public struct ActionBarAction
	{
		public ActionBarActionType type;
		public string text;
		public Action<ActionBarAction> onClick;

		public ActionBarAction(ActionBarActionType type, string text, Action<ActionBarAction> onClick = null)
		{
			this.type = type;
			this.text = text;
			this.onClick = onClick;
		}
	}
}