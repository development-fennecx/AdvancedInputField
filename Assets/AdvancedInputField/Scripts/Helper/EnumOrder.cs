using UnityEngine;

namespace AdvancedInputFieldPlugin
{
	/// <summary>
	/// PropertyAttribute to set custom enum order, source: https://forum.unity.com/threads/enum-inspector-sorting-attribute.357558/
	/// </summary>
	[System.AttributeUsage(System.AttributeTargets.Field)]
	public class EnumOrder: PropertyAttribute
	{
		public readonly new int[] order;

		public EnumOrder(string _orderStr)
		{
			this.order = StringToInts(_orderStr);
		}

		public EnumOrder(int[] _order)
		{
			this.order = _order;
		}

		private int[] StringToInts(string str)
		{
			string[] stringArray = str.Split(',');
			int[] intArray = new int[stringArray.Length];
			for(int i = 0; i < stringArray.Length; i++)
			{
				intArray[i] = System.Int32.Parse(stringArray[i]);
			}

			return (intArray);
		}
	}
}
