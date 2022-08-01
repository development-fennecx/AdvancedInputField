using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AdvancedInputFieldPlugin
{
	public class CustomNameAttribute: PropertyAttribute
	{
		public string label;

		public CustomNameAttribute(string label)
		{
			this.label = label;
		}

#if UNITY_EDITOR
		[CustomPropertyDrawer(typeof(CustomNameAttribute))]
		public class CustomNameDrawer: PropertyDrawer
		{
			public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
			{
				var propertyAttribute = this.attribute as CustomNameAttribute;
				label.text = propertyAttribute.label;
				EditorGUI.PropertyField(position, property, label);
			}
		}
#endif
	}
}