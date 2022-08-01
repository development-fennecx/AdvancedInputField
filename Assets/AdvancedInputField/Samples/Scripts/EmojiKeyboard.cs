using AdvancedInputFieldPlugin;
#if ADVANCEDINPUTFIELD_TEXTMESHPRO
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;
using System.IO;
using System.Text;
using System.Globalization;
#endif
using UnityEngine;

namespace AdvancedInputFieldSamples
{
	public class EmojiKeyboard: MonoBehaviour
	{
		[SerializeField]
		private InputFieldButton emojiButtonPrefab;

		[SerializeField]
		private int amount = 24;

#if ADVANCEDINPUTFIELD_TEXTMESHPRO
		private TMProEmojiAsset emojiAsset;
		private List<InputFieldButton> buttons;

		private void Start()
		{
			emojiAsset = TMP_Settings.defaultSpriteAsset as TMProEmojiAsset;
			if(emojiAsset != null)
			{
				InitializeButtons(emojiAsset);
			}
			else
			{
				Debug.LogWarning("No emoji asset assigned as default sprite asset in TextMeshPro settings. See the Documentation on how to create an emoji asset file.");
			}
		}

		private void InitializeButtons(TMProEmojiAsset emojiAsset)
		{
			buttons = new List<InputFieldButton>();
			List<TMP_Sprite> sprites = emojiAsset.spriteInfoList;
			int length = Mathf.Min(sprites.Count, amount);

			RectTransform rectTransform = GetComponent<RectTransform>();
			Vector2 rectSize = rectTransform.rect.size;
			Vector2 emojiButtonSize = emojiButtonPrefab.GetComponent<RectTransform>().rect.size;
			int columns = Mathf.FloorToInt(rectSize.x / emojiButtonSize.x);
			int rows = Mathf.CeilToInt((float)length / (float)columns);

			for(int y = 0; y < rows; y++)
			{
				for(int x = 0; x < columns; x++)
				{
					int index = (y * columns) + x;
					if(index >= length) { continue; }

					TMP_Sprite sprite = sprites[index];

					InputFieldButton emojiButton = CreateEmojiButton();
					emojiButton.onClick.AddListener(() => OnClick(index));
					RectTransform buttonTransform = emojiButton.GetComponent<RectTransform>();
					Vector2 position = new Vector2(x * emojiButtonSize.x, -(y * emojiButtonSize.y));
					buttonTransform.anchoredPosition = position;

					RawImage iconRenderer = emojiButton.GetComponentInChildren<RawImage>();
					Texture emojiTexture = emojiAsset.spriteSheet;
					iconRenderer.texture = emojiTexture;
					float width = (float)sprite.width / (float)emojiTexture.width;
					float height = (float)sprite.height / (float)emojiTexture.height;
					float xOffset = (float)sprite.x / (float)emojiTexture.width;
					float yOffset = (float)(emojiTexture.height - sprite.y - sprite.height) / (float)emojiTexture.height;
					iconRenderer.uvRect = new Rect(xOffset, 1 - yOffset - height, width, height);

					buttons.Add(emojiButton);
				}
			}

			float totalHeight = rows * emojiButtonSize.y;
			Vector2 sizeDelta = rectTransform.sizeDelta;
			sizeDelta.y = totalHeight;
			rectTransform.sizeDelta = sizeDelta;
		}

		private InputFieldButton CreateEmojiButton()
		{
			InputFieldButton emojiButton = Instantiate(emojiButtonPrefab);
			RectTransform buttonTransform = emojiButton.GetComponent<RectTransform>();
			Vector2 size = buttonTransform.sizeDelta;
			buttonTransform.SetParent(transform);
			buttonTransform.localScale = Vector3.one;
			buttonTransform.localRotation = Quaternion.identity;
			buttonTransform.localPosition = Vector3.zero;
			buttonTransform.sizeDelta = size;

			return emojiButton;
		}

		public void OnClick(int index)
		{
			TMP_Sprite sprite = emojiAsset.spriteInfoList[index];
			string emojiName = Path.GetFileNameWithoutExtension(sprite.name);
			string[] values = emojiName.Split('-');
			StringBuilder stringBuilder = new StringBuilder();
			int length = values.Length;
			for(int i = 0; i < length; i++)
			{
				string part = values[i].Trim();
				string utf16String = char.ConvertFromUtf32(int.Parse(part, NumberStyles.HexNumber));
				stringBuilder.Append(utf16String);
			}
			string text = stringBuilder.ToString();

			InputMethodManager.AddTextInputEvent(text);
		}
#endif
	}
}
