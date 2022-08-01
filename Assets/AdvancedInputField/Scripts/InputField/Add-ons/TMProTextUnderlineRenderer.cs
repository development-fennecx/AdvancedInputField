//-----------------------------------------
//			Advanced Input Field
// Copyright (c) 2017 Jeroen van Pienbroek
//------------------------------------------

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#if ADVANCEDINPUTFIELD_TEXTMESHPRO
using TMPro;
#endif

namespace AdvancedInputFieldPlugin
{
	/// <summary>Renderer for TextMeshPro text underlining (based on TextMeshPro example: http://digitalnativestudios.com/forum/index.php?topic=1034.msg8056#msg8056) </summary>
	public class TMProTextUnderlineRenderer: MaskableGraphic
	{
		[SerializeField]
		public float highlightSize = 10;

		[SerializeField]
		public float highlightHeight = 10;

		private Texture highlightTexture;
		private List<TextRange> textRanges;

#if ADVANCEDINPUTFIELD_TEXTMESHPRO
		private TMP_Text textRenderer;

		protected override void Awake()
		{
			base.Awake();
			textRenderer = GetComponentInParent<TMP_Text>();
			highlightTexture = materialForRendering.mainTexture;
		}

		public override Texture mainTexture
		{
			get
			{
				return highlightTexture;
			}
		}
#endif
		/// <summary>Updates the text ranges that need to be underlined</summary>
		/// <param name="textRanges">The text ranges that need to be underlined</param>
		public void UpdateTextRanges(List<TextRange> textRanges)
		{
			this.textRanges = textRanges;
			SetVerticesDirty();
			SetMaterialDirty();
		}

#if ADVANCEDINPUTFIELD_TEXTMESHPRO
		protected override void OnPopulateMesh(VertexHelper vertexHelper)
		{
			vertexHelper.Clear();

			if(textRanges == null || textRanges.Count == 0) { return; }

			int length = textRanges.Count;
			for(int i = 0; i < length; i++)
			{
				TextRange range = textRanges[i];

				TMP_CharacterInfo startCharInfo;
				TMP_CharacterInfo endCharInfo;
				if(TryGetCharacterInfo(range.start, out startCharInfo) && TryGetCharacterInfo(range.end, out endCharInfo))
				{
					if(startCharInfo.lineNumber == endCharInfo.lineNumber)
					{
						UnderlineSingleLineWord(vertexHelper, startCharInfo, endCharInfo);
					}
					else
					{
						UnderlineMultilineWord(vertexHelper, startCharInfo, endCharInfo);
					}
				}
			}
		}

		internal bool TryGetCharacterInfo(int index, out TMP_CharacterInfo charInfo)
		{
			if(textRenderer == null)
			{
				charInfo = default(TMP_CharacterInfo);
				return false;
			}

			TMP_TextInfo textInfo = textRenderer.textInfo;
			if(textInfo == null)
			{
				charInfo = default(TMP_CharacterInfo);
				return false;
			}

			if(index < textInfo.characterCount)
			{
				charInfo = textInfo.characterInfo[index];
				return true;
			}

			charInfo = default(TMP_CharacterInfo);
			return false;
		}

		internal bool TryGetLineInfo(int index, out TMP_LineInfo lineInfo)
		{
			if(textRenderer == null)
			{
				lineInfo = default(TMP_LineInfo);
				return false;
			}

			TMP_TextInfo textInfo = textRenderer.textInfo;
			if(textInfo == null)
			{
				lineInfo = default(TMP_LineInfo);
				return false;
			}

			if(index < textInfo.lineCount)
			{
				lineInfo = textInfo.lineInfo[index];
				return true;
			}

			lineInfo = default(TMP_LineInfo);
			return false;
		}

		internal void UnderlineSingleLineWord(VertexHelper vertexHelper, TMP_CharacterInfo startCharInfo, TMP_CharacterInfo endCharInfo)
		{
			float wordScale = startCharInfo.scale;

			Vector3 bottomLeft = new Vector3(startCharInfo.bottomLeft.x, endCharInfo.baseLine - highlightHeight * wordScale, 0);
			Vector3 topLeft = new Vector3(bottomLeft.x, startCharInfo.baseLine, 0);
			Vector3 topRight = new Vector3(endCharInfo.topRight.x, topLeft.y, 0);
			Vector3 bottomRight = new Vector3(topRight.x, bottomLeft.y, 0);

			float xLength = Mathf.Abs(topRight.x - bottomLeft.x) / wordScale * highlightSize;
			float tiling = xLength / (highlightTexture == null ? 1 : highlightTexture.width);

			UIVertex vertex = UIVertex.simpleVert;
			vertex.color = color;

			vertex.position = new Vector2(bottomLeft.x, bottomLeft.y);
			vertex.uv0 = new Vector2(0, 0);
			vertexHelper.AddVert(vertex);

			vertex.position = new Vector2(bottomLeft.x, topRight.y);
			vertex.uv0 = new Vector2(0, 1);
			vertexHelper.AddVert(vertex);

			vertex.position = new Vector2(topRight.x, topRight.y);
			vertex.uv0 = new Vector2(tiling, 1);
			vertexHelper.AddVert(vertex);

			vertex.position = new Vector2(topRight.x, bottomLeft.y);
			vertex.uv0 = new Vector2(tiling, 0);
			vertexHelper.AddVert(vertex);

			int vertexCount = vertexHelper.currentVertCount;
			vertexHelper.AddTriangle(vertexCount - 4, vertexCount - 3, vertexCount - 2);
			vertexHelper.AddTriangle(vertexCount - 2, vertexCount - 1, vertexCount - 4);
		}

		internal void UnderlineMultilineWord(VertexHelper vertexHelper, TMP_CharacterInfo startCharInfo, TMP_CharacterInfo endCharInfo)
		{
			int startLineNr = startCharInfo.lineNumber;
			int endLineNr = endCharInfo.lineNumber;

			TMP_LineInfo startLineInfo;
			TMP_CharacterInfo startLineEndCharInfo;
			if(TryGetLineInfo(startLineNr, out startLineInfo) && TryGetCharacterInfo(startLineInfo.lastCharacterIndex, out startLineEndCharInfo))
			{
				UnderlineSingleLineWord(vertexHelper, startCharInfo, startLineEndCharInfo);
			}

			for(int lineNr = startLineNr + 1; lineNr < endLineNr; lineNr++)
			{
				TMP_LineInfo lineInfo;
				if(TryGetLineInfo(lineNr, out lineInfo))
				{
					TMP_CharacterInfo lineStartCharInfo;
					TMP_CharacterInfo lineEndCharInfo;
					if(TryGetCharacterInfo(lineInfo.firstCharacterIndex, out lineStartCharInfo) && TryGetCharacterInfo(lineInfo.lastCharacterIndex, out lineEndCharInfo))
					{
						UnderlineSingleLineWord(vertexHelper, lineStartCharInfo, lineEndCharInfo);
					}
				}
			}

			TMP_LineInfo endLineInfo;
			TMP_CharacterInfo endLineStartCharInfo;
			if(TryGetLineInfo(endLineNr, out endLineInfo) && TryGetCharacterInfo(endLineInfo.firstCharacterIndex, out endLineStartCharInfo))
			{
				UnderlineSingleLineWord(vertexHelper, endLineStartCharInfo, endCharInfo);
			}
		}
#endif
	}
}