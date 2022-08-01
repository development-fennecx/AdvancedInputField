using System.Collections.Generic;
using UnityEngine;

namespace AdvancedInputFieldPlugin
{
	public class TexturePackerData
	{
		[System.Serializable]
		public struct SpriteFrame
		{
			public float x;
			public float y;
			public float w;
			public float h;

			public override string ToString()
			{
				string s = "x: " + x.ToString("f2") + " y: " + y.ToString("f2") + " h: " + h.ToString("f2") + " w: " + w.ToString("f2");
				return s;
			}
		}

		[System.Serializable]
		public struct SpriteSize
		{
			public float w;
			public float h;

			public override string ToString()
			{
				string s = "w: " + w.ToString("f2") + " h: " + h.ToString("f2");
				return s;
			}
		}

		[System.Serializable]
		public struct Frame
		{
			public string filename;
			public SpriteFrame frame;
			public bool rotated;
			public bool trimmed;
			public SpriteFrame spriteSourceSize;
			public SpriteSize sourceSize;
			public Vector2 pivot;
		}

		[System.Serializable]
		public class SpriteDataObject
		{
			public List<Frame> frames;
		}
	}
}