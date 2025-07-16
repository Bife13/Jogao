using System;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[CustomEditor(typeof(Condition))]
public class ConditionEditor : Editor
{
	ReorderableList effectsList;
	SerializedProperty effectsProp;
	Type[] effectTypes;

	void OnEnable()
	{
		// 1) Grab the serialized "effects" property
		effectsProp = serializedObject.FindProperty("effects");

		// 2) Find all non-abstract subclasses of ConditionEffect
		effectTypes = AppDomain.CurrentDomain.GetAssemblies()
			.SelectMany(a => a.GetTypes())
			.Where(t => typeof(ConditionEffect).IsAssignableFrom(t) && !t.IsAbstract)
			.ToArray();

		// 3) Create the reorderable list
		effectsList = new ReorderableList(
			serializedObject,
			effectsProp,
			draggable: true,
			displayHeader: true,
			displayAddButton: true,
			displayRemoveButton: true
		);

		// 4) Draw the header
		effectsList.drawHeaderCallback = rect =>
			EditorGUI.LabelField(rect, "Effects");

		// 5) On Add: show dropdown of effect types
		effectsList.onAddDropdownCallback = (buttonRect, _) =>
		{
			var menu = new GenericMenu();
			foreach (var t in effectTypes)
			{
				menu.AddItem(new GUIContent(t.Name), false, () =>
				{
					effectsProp.arraySize++;
					var elem = effectsProp.GetArrayElementAtIndex(effectsProp.arraySize - 1);
					elem.managedReferenceValue = Activator.CreateInstance(t);
					serializedObject.ApplyModifiedProperties();
				});
			}

			menu.DropDown(buttonRect);
		};

		// 6) Draw each element fully expanded with type name
		effectsList.drawElementCallback = (rect, index, active, focused) =>
		{
			var elem = effectsProp.GetArrayElementAtIndex(index);
			elem.isExpanded = true; // always expanded

			string fullName = elem.managedReferenceFullTypename;
			string typeName = !string.IsNullOrEmpty(fullName) ? fullName.Split(' ')[1] : $"Element {index}";

			// Calculate height
			float height = EditorGUI.GetPropertyHeight(elem, true);
			var r = new Rect(rect.x, rect.y, rect.width, height);
			EditorGUI.PropertyField(r, elem, new GUIContent(typeName), true);
		};

		// 7) Reserve correct height per element
		effectsList.elementHeightCallback = index =>
		{
			var elem = effectsProp.GetArrayElementAtIndex(index);
			return EditorGUI.GetPropertyHeight(elem, true)
			       + EditorGUIUtility.standardVerticalSpacing;
		};
	}

	public override void OnInspectorGUI()
	{
		serializedObject.Update();

		// Draw core Condition fields
		EditorGUILayout.PropertyField(
			serializedObject.FindProperty(nameof(Condition.conditionName)),
			new GUIContent("Name")
		);
		EditorGUILayout.PropertyField(
			serializedObject.FindProperty(nameof(Condition.conditionIcon)),
			new GUIContent("Icon")
		);
		EditorGUILayout.PropertyField(
			serializedObject.FindProperty(nameof(Condition.conditionType)),
			new GUIContent("Condition Type")
		);
		EditorGUILayout.PropertyField(
			serializedObject.FindProperty(nameof(Condition.turnDuration)),
			new GUIContent("Duration")
		);
		EditorGUILayout.PropertyField(
			serializedObject.FindProperty(nameof(Condition.isStackable)),
			new GUIContent("Stackable")
		);
		EditorGUILayout.PropertyField(
			serializedObject.FindProperty(nameof(Condition.conditionTiming)),
			new GUIContent("Timing")
		);

		EditorGUILayout.Space();
		// Draw the Effects list
		effectsList.DoLayoutList();

		serializedObject.ApplyModifiedProperties();
	}
}