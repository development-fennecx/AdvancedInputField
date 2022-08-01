using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AdvancedInputFieldPlugin
{
	/// <summary>Custom Dropdown class to show suggestions for an AdvancedInputField instance</summary>
	public class InputFieldDropdown: Selectable
	{
		protected internal class DropdownItem: MonoBehaviour, ICancelHandler
		{
			[SerializeField]
			private Text m_Text;
			[SerializeField]
			private Image m_Image;
			[SerializeField]
			private RectTransform m_RectTransform;
			[SerializeField]
			private Toggle m_Toggle;

			public Text text { get { return m_Text; } set { m_Text = value; } }
			public Image image { get { return m_Image; } set { m_Image = value; } }
			public RectTransform rectTransform { get { return m_RectTransform; } set { m_RectTransform = value; } }
			public Toggle toggle { get { return m_Toggle; } set { m_Toggle = value; } }

			public virtual void OnCancel(BaseEventData eventData)
			{
				Dropdown dropdown = GetComponentInParent<Dropdown>();
				if(dropdown)
				{
					dropdown.Hide();
				}
			}
		}

		[Serializable]
		/// <summary>
		/// Class to store the text and/or image of a single option in the dropdown list.
		/// </summary>
		public class OptionData
		{
			[SerializeField]
			private string m_Text;
			[SerializeField]
			private Sprite m_Image;

			/// <summary>
			/// The text associated with the option.
			/// </summary>
			public string text { get { return m_Text; } set { m_Text = value; } }

			/// <summary>
			/// The image associated with the option.
			/// </summary>
			public Sprite image { get { return m_Image; } set { m_Image = value; } }

			public OptionData()
			{
			}

			public OptionData(string text)
			{
				this.text = text;
			}

			public OptionData(Sprite image)
			{
				this.image = image;
			}

			/// <summary>
			/// Create an object representing a single option for the dropdown list.
			/// </summary>
			/// <param name="text">Optional text for the option.</param>
			/// <param name="image">Optional image for the option.</param>
			public OptionData(string text, Sprite image)
			{
				this.text = text;
				this.image = image;
			}
		}

		[Serializable]
		/// <summary>
		/// Class used internally to store the list of options for the dropdown list.
		/// </summary>
		/// <remarks>
		/// The usage of this class is not exposed in the runtime API. It's only relevant for the PropertyDrawer drawing the list of options.
		/// </remarks>
		public class OptionDataList
		{
			[SerializeField]
			private List<OptionData> m_Options;

			/// <summary>
			/// The list of options for the dropdown list.
			/// </summary>
			public List<OptionData> options { get { return m_Options; } set { m_Options = value; } }


			public OptionDataList()
			{
				options = new List<OptionData>();
			}
		}

		[Serializable]
		/// <summary>
		/// UnityEvent callback for when a dropdown current option is changed.
		/// </summary>
		public class DropdownEvent: UnityEvent<int> { }

		// Template used to create the dropdown.
		[SerializeField]
		private RectTransform m_Template;

		/// <summary>
		/// The Rect Transform of the template for the dropdown list.
		/// </summary>
		public RectTransform template { get { return m_Template; } set { m_Template = value; } }

		[Space]

		[SerializeField]
		private Text m_ItemText;

		/// <summary>
		/// The Text component to hold the text of the item.
		/// </summary>
		public Text itemText { get { return m_ItemText; } set { m_ItemText = value; } }

		[SerializeField]
		private Image m_ItemImage;

		/// <summary>
		/// The Image component to hold the image of the item
		/// </summary>
		public Image itemImage { get { return m_ItemImage; } set { m_ItemImage = value; } }

		[Space]

		[SerializeField]
		private int m_Value;

		[Space]

		// Items that will be visible when the dropdown is shown.
		// We box this into its own class so we can use a Property Drawer for it.
		[SerializeField]
		private OptionDataList m_Options = new OptionDataList();

		public List<OptionData> options
		{
			get { return m_Options.options; }
			set
			{
				m_Options.options = value;
				RefreshOptions();
			}
		}

		[Space]

		// Notification triggered when the dropdown changes.
		[SerializeField]
		private DropdownEvent m_OnValueChanged = new DropdownEvent();

		public DropdownEvent onValueChanged { get { return m_OnValueChanged; } set { m_OnValueChanged = value; } }

		[SerializeField]
		private float m_AlphaFadeSpeed = 0.15f;

		/// <summary>
		/// The time interval at which a drop down will appear and disappear
		/// </summary>
		public float alphaFadeSpeed { get { return m_AlphaFadeSpeed; } set { m_AlphaFadeSpeed = value; } }

		private GameObject m_Dropdown;
		private List<DropdownItem> m_Items = new List<DropdownItem>();
		private bool validTemplate = false;

		private static OptionData s_NoOptionData = new OptionData();

		public int value
		{
			get
			{
				return m_Value;
			}
			set
			{
				Set(value);
			}
		}
		/// <summary>
		/// Set index number of the current selection in the Dropdown without invoking onValueChanged callback.
		/// </summary>
		/// <param name="input"> The new index for the current selection. </param>
		public void SetValueWithoutNotify(int input)
		{
			Set(input, false);
		}

		void Set(int value, bool sendCallback = true)
		{
			if(Application.isPlaying && (options.Count == 0))
				return;

			m_Value = Mathf.Clamp(value, 0, options.Count - 1);

			if(sendCallback)
			{
				// Notify all listeners
				UISystemProfilerApi.AddMarker("Dropdown.value", this);
				m_OnValueChanged.Invoke(m_Value);
			}
		}

		protected void Awake()
		{
#if UNITY_EDITOR
			if(!Application.isPlaying)
				return;
#endif

			if(m_Template)
				m_Template.gameObject.SetActive(false);
		}

		protected void OnDisable()
		{
			//Destroy dropdown and blocker in case user deactivates the dropdown when they click an option (case 935649)
			ImmediateDestroyDropdownList();
		}

		private void Update()
		{
			if(m_Dropdown != null && m_Dropdown.activeInHierarchy && !IsInputFieldSelected())
			{
				enabled = false;
			}
		}

		internal bool IsInputFieldSelected()
		{
			GameObject currentSelection = EventSystem.current.currentSelectedGameObject;
			if(currentSelection != null)
			{
				AdvancedInputField selectedInputField = currentSelection.GetComponentInParent<AdvancedInputField>();
				return (selectedInputField != null && selectedInputField == GetComponentInParent<AdvancedInputField>());
			}

			return false;
		}

		/// <summary>
		/// Add multiple options to the options of the Dropdown based on a list of OptionData objects.
		/// </summary>
		/// <param name="options">The list of OptionData to add.</param>
		/// /// <remarks>
		/// See AddOptions(List<string> options) for code example of usages.
		/// </remarks>
		public void AddOptions(List<OptionData> options)
		{
			this.options.AddRange(options);
		}

		public void AddOptions(List<string> options)
		{
			for(int i = 0; i < options.Count; i++)
				this.options.Add(new OptionData(options[i]));
		}

		/// <summary>
		/// Add multiple image-only options to the options of the Dropdown based on a list of Sprites.
		/// </summary>
		/// <param name="options">The list of Sprites to add.</param>
		/// <remarks>
		/// See AddOptions(List<string> options) for code example of usages.
		/// </remarks>
		public void AddOptions(List<Sprite> options)
		{
			for(int i = 0; i < options.Count; i++)
				this.options.Add(new OptionData(options[i]));
		}

		/// <summary>
		/// Clear the list of options in the Dropdown.
		/// </summary>
		public void ClearOptions()
		{
			options.Clear();
			m_Value = 0;
		}

		private void SetupTemplate()
		{
			validTemplate = false;

			if(!m_Template)
			{
				Debug.LogError("The dropdown template is not assigned. The template needs to be assigned and must have a child GameObject with a Toggle component serving as the item.", this);
				return;
			}

			GameObject templateGo = m_Template.gameObject;
			templateGo.SetActive(true);
			Toggle itemToggle = m_Template.GetComponentInChildren<Toggle>();

			validTemplate = true;
			if(!itemToggle || itemToggle.transform == template)
			{
				validTemplate = false;
				Debug.LogError("The dropdown template is not valid. The template must have a child GameObject with a Toggle component serving as the item.", template);
			}
			else if(!(itemToggle.transform.parent is RectTransform))
			{
				validTemplate = false;
				Debug.LogError("The dropdown template is not valid. The child GameObject with a Toggle component (the item) must have a RectTransform on its parent.", template);
			}
			else if(itemText != null && !itemText.transform.IsChildOf(itemToggle.transform))
			{
				validTemplate = false;
				Debug.LogError("The dropdown template is not valid. The Item Text must be on the item GameObject or children of it.", template);
			}
			else if(itemImage != null && !itemImage.transform.IsChildOf(itemToggle.transform))
			{
				validTemplate = false;
				Debug.LogError("The dropdown template is not valid. The Item Image must be on the item GameObject or children of it.", template);
			}

			if(!validTemplate)
			{
				templateGo.SetActive(false);
				return;
			}

			DropdownItem item = itemToggle.gameObject.AddComponent<DropdownItem>();
			item.text = m_ItemText;
			item.image = m_ItemImage;
			item.toggle = itemToggle;
			item.rectTransform = (RectTransform)itemToggle.transform;

			// Find the Canvas that this dropdown is a part of
			Canvas parentCanvas = null;
			Transform parentTransform = m_Template.parent;
			while(parentTransform != null)
			{
				parentCanvas = parentTransform.GetComponent<Canvas>();
				if(parentCanvas != null)
					break;

				parentTransform = parentTransform.parent;
			}

			Canvas popupCanvas = GetOrAddComponent<Canvas>(templateGo);
			popupCanvas.overrideSorting = true;
			popupCanvas.sortingOrder = 30000;

			// If we have a parent canvas, apply the same raycasters as the parent for consistency.
			if(parentCanvas != null)
			{
				Component[] components = parentCanvas.GetComponents<BaseRaycaster>();
				for(int i = 0; i < components.Length; i++)
				{
					Type raycasterType = components[i].GetType();
					if(templateGo.GetComponent(raycasterType) == null)
					{
						templateGo.AddComponent(raycasterType);
					}
				}
			}
			else
			{
				GetOrAddComponent<GraphicRaycaster>(templateGo);
			}

			GetOrAddComponent<CanvasGroup>(templateGo);
			templateGo.SetActive(false);

			validTemplate = true;
		}

		private static T GetOrAddComponent<T>(GameObject go) where T : Component
		{
			T comp = go.GetComponent<T>();
			if(!comp)
				comp = go.AddComponent<T>();
			return comp;
		}

		/// <summary>
		/// Show the dropdown.
		///
		/// Plan for dropdown scrolling to ensure dropdown is contained within screen.
		///
		/// We assume the Canvas is the screen that the dropdown must be kept inside.
		/// This is always valid for screen space canvas modes.
		/// For world space canvases we don't know how it's used, but it could be e.g. for an in-game monitor.
		/// We consider it a fair constraint that the canvas must be big enough to contain dropdowns.
		/// </summary>
		public void Show()
		{
			enabled = true;
			m_Dropdown.SetActive(true);
		}

		private void CreateOptions()
		{
			if(m_Dropdown != null)
				return;

			// Get root Canvas.
			var list = new List<Canvas>();
			gameObject.GetComponentsInParent(false, list);
			if(list.Count == 0)
				return;

			// case 1064466 rootCanvas should be last element returned by GetComponentsInParent()
			Canvas rootCanvas = list[list.Count - 1];
			for(int i = 0; i < list.Count; i++)
			{
				if(list[i].isRootCanvas)
				{
					rootCanvas = list[i];
					break;
				}
			}

			if(!validTemplate)
			{
				SetupTemplate();
				if(!validTemplate)
					return;
			}

			m_Template.gameObject.SetActive(true);

			// popupCanvas used to assume the root canvas had the default sorting Layer, next line fixes (case 958281 - [UI] Dropdown list does not copy the parent canvas layer when the panel is opened)
			m_Template.GetComponent<Canvas>().sortingLayerID = rootCanvas.sortingLayerID;

			// Instantiate the drop-down template
			m_Dropdown = CreateDropdownList(m_Template.gameObject);
			m_Dropdown.name = "Dropdown List";

			// Make drop-down RectTransform have same values as original.
			RectTransform dropdownRectTransform = m_Dropdown.transform as RectTransform;
			dropdownRectTransform.SetParent(m_Template.transform.parent, false);

			// Instantiate the drop-down list items

			// Find the dropdown item and disable it.
			DropdownItem itemTemplate = m_Dropdown.GetComponentInChildren<DropdownItem>();

			GameObject content = itemTemplate.rectTransform.parent.gameObject;
			RectTransform contentRectTransform = content.transform as RectTransform;
			itemTemplate.rectTransform.gameObject.SetActive(true);

			// Get the rects of the dropdown and item
			Rect dropdownContentRect = contentRectTransform.rect;
			Rect itemTemplateRect = itemTemplate.rectTransform.rect;

			// Calculate the visual offset between the item's edges and the background's edges
			Vector2 offsetMin = itemTemplateRect.min - dropdownContentRect.min + (Vector2)itemTemplate.rectTransform.localPosition;
			Vector2 offsetMax = itemTemplateRect.max - dropdownContentRect.max + (Vector2)itemTemplate.rectTransform.localPosition;
			Vector2 itemSize = itemTemplateRect.size;

			m_Items.Clear();

			Toggle prev = null;
			for(int i = 0; i < options.Count; ++i)
			{
				OptionData data = options[i];
				DropdownItem item = AddItem(data, value == i, itemTemplate, m_Items);
				if(item == null)
					continue;

				// Automatically set up a toggle state change listener
				item.toggle.isOn = value == i;
				item.toggle.onValueChanged.AddListener(x => OnSelectItem(item.toggle));

				// Select current option
				if(item.toggle.isOn)
					//item.toggle.Select();

					// Automatically set up explicit navigation
					if(prev != null)
					{
						Navigation prevNav = prev.navigation;
						Navigation toggleNav = item.toggle.navigation;
						prevNav.mode = Navigation.Mode.Explicit;
						toggleNav.mode = Navigation.Mode.Explicit;

						prevNav.selectOnDown = item.toggle;
						prevNav.selectOnRight = item.toggle;
						toggleNav.selectOnLeft = prev;
						toggleNav.selectOnUp = prev;

						prev.navigation = prevNav;
						item.toggle.navigation = toggleNav;
					}
				prev = item.toggle;
			}

			// Reposition all items now that all of them have been added
			Vector2 sizeDelta = contentRectTransform.sizeDelta;
			sizeDelta.y = itemSize.y * m_Items.Count + offsetMin.y - offsetMax.y;
			contentRectTransform.sizeDelta = sizeDelta;

			float extraSpace = dropdownRectTransform.rect.height - contentRectTransform.rect.height;
			if(extraSpace > 0)
				dropdownRectTransform.sizeDelta = new Vector2(dropdownRectTransform.sizeDelta.x, dropdownRectTransform.sizeDelta.y - extraSpace);

			// Invert anchoring and position if dropdown is partially or fully outside of canvas rect.
			// Typically this will have the effect of placing the dropdown above the button instead of below,
			// but it works as inversion regardless of initial setup.
			Vector3[] corners = new Vector3[4];
			dropdownRectTransform.GetWorldCorners(corners);

			RectTransform rootCanvasRectTransform = rootCanvas.transform as RectTransform;
			Rect rootCanvasRect = rootCanvasRectTransform.rect;
			for(int axis = 0; axis < 2; axis++)
			{
				bool outside = false;
				for(int i = 0; i < 4; i++)
				{
					Vector3 corner = rootCanvasRectTransform.InverseTransformPoint(corners[i]);
					if((corner[axis] < rootCanvasRect.min[axis] && !Mathf.Approximately(corner[axis], rootCanvasRect.min[axis])) ||
						(corner[axis] > rootCanvasRect.max[axis] && !Mathf.Approximately(corner[axis], rootCanvasRect.max[axis])))
					{
						outside = true;
						break;
					}
				}

				outside = true; //Force dropdown to render above inputfield

				if(outside)
					RectTransformUtility.FlipLayoutOnAxis(dropdownRectTransform, axis, false, false);
			}

			for(int i = 0; i < m_Items.Count; i++)
			{
				RectTransform itemRect = m_Items[i].rectTransform;
				itemRect.anchorMin = new Vector2(itemRect.anchorMin.x, 0);
				itemRect.anchorMax = new Vector2(itemRect.anchorMax.x, 0);
				itemRect.anchoredPosition = new Vector2(itemRect.anchoredPosition.x, offsetMin.y + itemSize.y * (m_Items.Count - 1 - i) + itemSize.y * itemRect.pivot.y);
				itemRect.sizeDelta = new Vector2(itemRect.sizeDelta.x, itemSize.y);
			}

			// Make drop-down template and item template inactive
			m_Template.gameObject.SetActive(false);
			itemTemplate.gameObject.SetActive(false);
		}

		/// <summary>
		/// Create the dropdown list to be shown when the dropdown is clicked. The dropdown list should correspond to the provided template GameObject, equivalent to instantiating a copy of it.
		/// </summary>
		/// <remarks>
		/// Override this method to implement a different way to obtain a dropdown list GameObject.
		/// </remarks>
		/// <param name="template">The template to create the dropdown list from.</param>
		/// <returns>The created drop down list gameobject.</returns>
		protected virtual GameObject CreateDropdownList(GameObject template)
		{
			return (GameObject)Instantiate(template);
		}

		/// <summary>
		/// Convenience method to explicitly destroy the previously generated dropdown list
		/// </summary>
		/// <remarks>
		/// Override this method to implement a different way to dispose of a dropdown list GameObject.
		/// </remarks>
		/// <param name="dropdownList">The dropdown list GameObject to destroy</param>
		protected virtual void DestroyDropdownList(GameObject dropdownList)
		{
			Destroy(dropdownList);
		}

		/// <summary>
		/// Create a dropdown item based upon the item template.
		/// </summary>
		/// <remarks>
		/// Override this method to implement a different way to obtain an option item.
		/// The option item should correspond to the provided template DropdownItem and its GameObject, equivalent to instantiating a copy of it.
		/// </remarks>
		/// <param name="itemTemplate">e template to create the option item from.</param>
		/// <returns>The created dropdown item component</returns>
		protected virtual DropdownItem CreateItem(DropdownItem itemTemplate)
		{
			return (DropdownItem)Instantiate(itemTemplate);
		}

		/// <summary>
		///  Convenience method to explicitly destroy the previously generated Items.
		/// </summary>
		/// <remarks>
		/// Override this method to implement a different way to dispose of an option item.
		/// Likely no action needed since destroying the dropdown list destroys all contained items as well.
		/// </remarks>
		/// <param name="item">The Item to destroy.</param>
		protected virtual void DestroyItem(DropdownItem item)
		{ }

		// Add a new drop-down list item with the specified values.
		private DropdownItem AddItem(OptionData data, bool selected, DropdownItem itemTemplate, List<DropdownItem> items)
		{
			// Add a new item to the dropdown.
			DropdownItem item = CreateItem(itemTemplate);
			item.rectTransform.SetParent(itemTemplate.rectTransform.parent, false);

			item.gameObject.SetActive(true);
			item.gameObject.name = "Item " + items.Count + (data.text != null ? ": " + data.text : "");

			if(item.toggle != null)
			{
				item.toggle.isOn = false;
			}

			// Set the item's data
			if(item.text)
				item.text.text = data.text;
			if(item.image)
			{
				item.image.sprite = data.image;
				item.image.enabled = (item.image.sprite != null);
			}

			items.Add(item);
			return item;
		}

		/// <summary>
		/// Hide the dropdown list. I.e. close it.
		/// </summary>
		public void Hide()
		{
			if(m_Dropdown != null)
			{
				// User could have disabled the dropdown during the OnValueChanged call.
				if(isActiveAndEnabled)
					StartCoroutine(DelayedDestroyDropdownList(m_AlphaFadeSpeed));
			}

			enabled = false;
		}

		public void RefreshOptions()
		{
			ImmediateDestroyDropdownList();
			CreateOptions();
		}

		private IEnumerator DelayedDestroyDropdownList(float delay)
		{
			yield return new WaitForSecondsRealtime(delay);
			ImmediateDestroyDropdownList();
		}

		private void ImmediateDestroyDropdownList()
		{
			for(int i = 0; i < m_Items.Count; i++)
			{
				if(m_Items[i] != null)
					DestroyItem(m_Items[i]);
			}
			m_Items.Clear();
			if(m_Dropdown != null)
				DestroyDropdownList(m_Dropdown);
			m_Dropdown = null;
		}

		// Change the value and hide the dropdown.
		private void OnSelectItem(Toggle toggle)
		{
			toggle.isOn = true;

			int selectedIndex = -1;
			Transform tr = toggle.transform;
			Transform parent = tr.parent;
			for(int i = 0; i < parent.childCount; i++)
			{
				if(parent.GetChild(i) == tr)
				{
					// Subtract one to account for template child.
					selectedIndex = i - 1;
					break;
				}
			}

			if(selectedIndex < 0)
			{
				return;
			}

			value = selectedIndex;
			Hide();
		}
	}
}
