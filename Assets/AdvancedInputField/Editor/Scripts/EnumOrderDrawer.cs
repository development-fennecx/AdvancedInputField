using UnityEditor;
using UnityEngine;

namespace AdvancedInputFieldPlugin.Editor
{
	/// <summary>
	/// PropertyDrawer to show custom enum order, source: https://forum.unity.com/threads/enum-inspector-sorting-attribute.357558/
	/// </summary>
	[CustomPropertyDrawer(typeof(EnumOrder))]
	public class EnumOrderDrawer: PropertyDrawer
	{
		public EnumOrder enumOrder { get { return ((EnumOrder)attribute); } }

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			EditorGUI.BeginProperty(position, label, property);

			// Store array of indexes based on ascending value
			int[] indexArray = GetIndexArray(enumOrder.order);

			// Store resorted string array for the popup item names
			string[] items = new string[property.enumNames.Length];
			items[0] = property.enumNames[0];
			for(int i = 0; i < property.enumNames.Length; i++)
			{
				items[i] = property.enumNames[indexArray[i]];
			}

			// Get selected enum based on position
			int index = -1;
			for(int i = 0; i < indexArray.Length; i++)
			{
				if(indexArray[i] == property.enumValueIndex)
				{
					index = i;
					break;
				}
			}
			if((index == -1) && (property.enumValueIndex != -1)) { SortingError(position, property, label); return; }

			// Display popup
			index = EditorGUI.Popup(
				position,
				label.text,
				index,
				items);
			property.enumValueIndex = indexArray[index];

			// Default
			//EditorGUI.PropertyField(position, property, new GUIContent("*" + label.text));

			EditorGUI.EndProperty();

		}

		private int[] GetIndexArray(int[] order)
		{

			int[] indexArray = new int[order.Length];

			for(int i = 0; i < order.Length; i++)
			{
				int index = 0;
				for(int j = 0; j < order.Length; j++)
				{
					if(order[i] > order[j])
					{
						index++;
					}
				}

				indexArray[i] = index;

			}

			return (indexArray);

		}

		/// Use default enum popup, but flag label to aware user
		private void SortingError(Rect position, SerializedProperty property, GUIContent label)
		{
			EditorGUI.PropertyField(position, property, new GUIContent(label.text + " (sorting error)"));
			EditorGUI.EndProperty();
		}
	}
}