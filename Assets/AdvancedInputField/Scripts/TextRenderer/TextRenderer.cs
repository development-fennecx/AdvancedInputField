// Copyright (c) Jeroen van Pienbroek. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System;
using UnityEngine;

namespace AdvancedInputFieldPlugin
{
	public struct CharacterInfo
	{
		public Vector2 position;
		public float width;
		public int index;
	}

	public struct LineInfo
	{
		public float topY;
		public float height;
		public int startCharIdx;
	}

	public struct RichTextTagInfo
	{
		public RichTextTagType type;
		public string startTag;
		public string endTag;
		public string startTagStart;

		public RichTextTagInfo(RichTextTagType type, string startTag, string endTag)
		{
			this.type = type;
			this.startTag = startTag;
			this.endTag = endTag;

			if(type == RichTextTagType.SINGLE_PARAMETER_TAG_PAIR || type == RichTextTagType.SINGLE_PARAMETER_SINGLE_TAG)
			{
				this.startTagStart = startTag.Substring(0, startTag.IndexOf("{0}"));
			}
			else
			{
				this.startTagStart = null;
			}
		}
	}

	public enum RichTextTagType { BASIC_TAG_PAIR, SINGLE_PARAMETER_TAG_PAIR, BASIC_SINGLE_TAG, SINGLE_PARAMETER_SINGLE_TAG }
	public enum TextAlignment { TOP_LEFT, TOP, TOP_RIGHT, LEFT, CENTER, RIGHT, BOTTOM_LEFT, BOTTOM, BOTTOM_RIGHT };

	[ExecuteInEditMode]
	[RequireComponent(typeof(RectTransform))]
	public abstract class TextRenderer: MonoBehaviour
	{
		private const string NOT_CONFIGURED_ERROR = "This type of Text Renderer is not configured, please check the Documentation for instructions.";

		[SerializeField]
		[Tooltip("Automatically change anchor settings to fit parent. Don't disable this, unless you want to use this component outside of the AdvancedInputField")]
		private bool autoAnchor = true;

		protected RectTransform rectTransform;
		protected Vector2 preferredSize;
		protected RectTransform viewportTransform;
		protected bool multiline;
		protected bool richTextEditingEnabled;
		private DrivenRectTransformTracker transformTracker;

		public RectTransform RectTransform
		{
			get
			{
				if(rectTransform == null)
				{
					rectTransform = GetComponent<RectTransform>();
				}

				return rectTransform;
			}
		}

		public virtual bool Visible { get { throw new NotImplementedException(NOT_CONFIGURED_ERROR); } }
		public float CaretWidth { get; set; }
		public virtual bool Multiline { get { throw new NotImplementedException(NOT_CONFIGURED_ERROR); } set { throw new System.NotImplementedException(NOT_CONFIGURED_ERROR); } }
		public float MultilineMaxWidth { get; protected set; }
		public virtual bool RichTextEnabled { get { throw new NotImplementedException(NOT_CONFIGURED_ERROR); } set { throw new NotImplementedException(NOT_CONFIGURED_ERROR); } }
		public virtual TextAlignment TextAlignment { get { throw new NotImplementedException(NOT_CONFIGURED_ERROR); } }

		public virtual string Text { get { throw new NotImplementedException(NOT_CONFIGURED_ERROR); } set { throw new System.NotImplementedException(NOT_CONFIGURED_ERROR); } }

		public Vector2 PreferredSize
		{
			get
			{
				if(ResizeTextForBestFit)
				{
					ScrollArea scrollArea = GetComponentInParent<ScrollArea>();
					if(scrollArea != null)
					{
						return scrollArea.Viewport.rect.size;
					}
				}

				return preferredSize;
			}
		}

		public RectTransform ViewportTransform
		{
			get { return viewportTransform; }
			set { viewportTransform = value; }
		}

		public virtual int LineCount { get { throw new NotImplementedException(NOT_CONFIGURED_ERROR); } }
		public virtual int CharacterCount { get { throw new NotImplementedException(NOT_CONFIGURED_ERROR); } }
		public virtual int CharacterCountVisible { get { throw new NotImplementedException(NOT_CONFIGURED_ERROR); } }
		public virtual Color Color { get { throw new NotImplementedException(NOT_CONFIGURED_ERROR); } set { throw new System.NotImplementedException(NOT_CONFIGURED_ERROR); } }
		public virtual float FontSize { get { throw new NotImplementedException(NOT_CONFIGURED_ERROR); } set { throw new System.NotImplementedException(NOT_CONFIGURED_ERROR); } }
		public virtual bool ResizeTextForBestFit { get { throw new NotImplementedException(NOT_CONFIGURED_ERROR); } }
		public virtual float FontSizeUsedForBestFit { get { throw new NotImplementedException(NOT_CONFIGURED_ERROR); } }

		protected virtual void Awake()
		{
			rectTransform = GetComponent<RectTransform>();
		}

		protected virtual void OnEnable()
		{
			if(autoAnchor)
			{
				rectTransform.anchorMin = new Vector2(0, 0);
				rectTransform.anchorMax = new Vector2(1, 1);
				rectTransform.pivot = new Vector2(0.5f, 0.5f);
				rectTransform.offsetMin = new Vector2(0, 0);
				rectTransform.offsetMax = new Vector2(0, 0);
				rectTransform.localScale = Vector3.one;
				rectTransform.localRotation = Quaternion.identity;

				transformTracker.Add(this, rectTransform, DrivenTransformProperties.All);
			}
		}

		protected virtual void OnDisable()
		{
			transformTracker.Clear();
		}

		/// <summary>Determines the character line of character position</summary>
		/// <param name="textGenerator">The TextGenerator to use</param>
		/// <param name="charPosition">The character position to check</param>
		/// <returns>The character line</returns>
		internal int DetermineCharacterLine(int charPosition)
		{
			for(int i = 0; i < LineCount - 1; ++i)
			{
				if(GetLineInfo(i + 1).startCharIdx > charPosition)
				{
					return i;
				}
			}

			return LineCount - 1;
		}

		/// <summary>Finds the character position of next line</summary>
		/// <param name="position">The character position to start checking from</param>
		/// <returns>The character position of next line</returns>
		internal int LineDownPosition(int position, bool multiline)
		{
			int visiblePosition = position;
			if(!multiline)
			{
				return Text.Length;
			}

			CharacterInfo originChar = GetCharacterInfo(visiblePosition);
			int originLine = DetermineCharacterLine(visiblePosition);

			if(originLine + 1 >= LineCount) // We are on the last line return last character
			{
				return (CharacterCount - 1);
			}

			int endCharIdx = GetLineEndCharIndex(originLine + 1); // Need to determine end line for next line.

			float lastDiffX = float.MaxValue;
			for(int i = GetLineInfo(originLine + 1).startCharIdx; i < endCharIdx; ++i)
			{
				CharacterInfo characterInfo = GetCharacterInfo(i);
				float diffX = Mathf.Abs(characterInfo.position.x - originChar.position.x);
				if(characterInfo.position.x >= originChar.position.x)
				{
					if(diffX < lastDiffX)
					{
						return i;
					}
					else
					{
						return (i - 1);
					}
				}

				lastDiffX = diffX;
			}
			return endCharIdx;
		}

		/// <summary>Finds the character position of previous line</summary>
		/// <param name="position">The character position to start checking from</param>
		/// <returns>The character position of previous line</returns>
		internal int LineUpPosition(int position, bool multiline)
		{
			int visiblePosition = position;
			if(!multiline)
			{
				return 0;
			}

			CharacterInfo originChar = GetCharacterInfo(visiblePosition);
			int originLine = DetermineCharacterLine(visiblePosition);

			if(originLine <= 0) // We are on the first line return first character
			{
				return 0;
			}

			int endCharIdx = GetLineInfo(originLine).startCharIdx - 1;

			float lastDiffX = float.MaxValue;
			for(int i = GetLineInfo(originLine - 1).startCharIdx; i < endCharIdx; ++i)
			{
				CharacterInfo characterInfo = GetCharacterInfo(i);
				float diffX = Mathf.Abs(characterInfo.position.x - originChar.position.x);
				if(characterInfo.position.x >= originChar.position.x)
				{
					if(diffX < lastDiffX)
					{
						return i;
					}
					else
					{
						return (i - 1);
					}
				}

				lastDiffX = diffX;
			}
			return endCharIdx;
		}

		public virtual void Show() { throw new System.NotImplementedException(NOT_CONFIGURED_ERROR); }
		public virtual void Hide() { throw new System.NotImplementedException(NOT_CONFIGURED_ERROR); }

		public virtual bool UpdateImmediately() { throw new System.NotImplementedException(NOT_CONFIGURED_ERROR); }
		public virtual float DeterminePreferredWidth(int fontSize) { throw new System.NotImplementedException(NOT_CONFIGURED_ERROR); }
		public virtual float DeterminePreferredHeight(int fontSize) { throw new System.NotImplementedException(NOT_CONFIGURED_ERROR); }
		public virtual bool FontHasCharacter(char c) { throw new System.NotImplementedException(NOT_CONFIGURED_ERROR); }
		public virtual bool IsReady() { throw new System.NotImplementedException(NOT_CONFIGURED_ERROR); }
		public virtual CharacterInfo GetCharacterInfo(int index) { throw new System.NotImplementedException(NOT_CONFIGURED_ERROR); }
		public virtual LineInfo GetLineInfo(int index) { throw new System.NotImplementedException(NOT_CONFIGURED_ERROR); }

		/// <summary>Gets the character index  of the line end</summary>
		/// <param name="line">The line to check</param>
		public virtual int GetLineEndCharIndex(int line) { throw new System.NotImplementedException(NOT_CONFIGURED_ERROR); }
	}
}
