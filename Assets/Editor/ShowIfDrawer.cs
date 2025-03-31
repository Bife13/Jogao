using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(ShowIfAttribute))]
public class ShowIfDrawer : PropertyDrawer
{
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		ShowIfAttribute showIf = (ShowIfAttribute)attribute;
		SerializedProperty conditionProperty = property.serializedObject.FindProperty(showIf.conditionField);

		// If the condition field exists and is false, skip drawing this property (i.e., hide it)
		if (conditionProperty != null && !conditionProperty.boolValue)
		{
			return;
		}

		// Draw the property if the condition is met
		EditorGUI.PropertyField(position, property, label, true);
	}

	public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
	{
		ShowIfAttribute showIf = (ShowIfAttribute)attribute;
		SerializedProperty conditionProperty = property.serializedObject.FindProperty(showIf.conditionField);

		// If the condition is false, return height of 0 to hide the property
		if (conditionProperty != null && !conditionProperty.boolValue)
		{
			return 0;
		}

		return EditorGUI.GetPropertyHeight(property, label, true);
	}
}