using UnityEngine;
using UnityEngine.UI;

namespace AdvancedInputFieldPlugin
{
	public delegate void OnActionBarItemClick(ActionBarItem item);

	public class ActionBarItem: MonoBehaviour
	{
		private Text label;
		private TextRenderer textRenderer;
		private event OnActionBarItemClick onClick;
		private int textTrimAmount;
		private bool initialized;

		public Text Label { get { return label; } }
		public ActionBarAction Action { get; private set; }

		public event OnActionBarItemClick Click
		{
			add { onClick += value; }
			remove { onClick -= value; }
		}

		public int TextTrimAmount
		{
			get { return textTrimAmount; }
			set
			{
				textTrimAmount = value;
				if(!string.IsNullOrEmpty(Action.text) && textTrimAmount < Action.text.Length)
				{
					if(textTrimAmount == 0)
					{
						label.text = Action.text;
					}
					else
					{
						label.text = Action.text.Substring(0, Action.text.Length - textTrimAmount) + "...";
					}
				}
			}
		}

		public RectTransform RectTransform { get; private set; }

		private void Awake()
		{
			if(!initialized) { Initialize(); }
		}

		private void Initialize()
		{
			RectTransform = GetComponent<RectTransform>();
			label = GetComponentInChildren<Text>();
			textRenderer = label.GetComponent<TextRenderer>();
			initialized = true;
		}

		public void ConfigureUI(ActionBarAction action)
		{
			if(!initialized) { Initialize(); }

			Action = action;
			label.text = action.text;
		}

		public void OnClick()
		{
			onClick?.Invoke(this);
		}

		public float DeterminePreferredWidth(int fontSize)
		{
			return textRenderer.DeterminePreferredWidth(fontSize);
		}

		public float DeterminePreferredHeight(int fontSize)
		{
			return textRenderer.DeterminePreferredHeight(fontSize);
		}
	}
}
