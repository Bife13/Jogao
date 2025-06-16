using System;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[CustomEditor(typeof(Ability))]
public class AbilityEditor : Editor
{
    ReorderableList modulesList;
    SerializedProperty modulesProp;
    Type[] moduleTypes;

    void OnEnable()
    {
        modulesProp = serializedObject.FindProperty("modules");
        moduleTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t => typeof(AbilityModule).IsAssignableFrom(t) && !t.IsAbstract)
            .ToArray();

        modulesList = new ReorderableList(
            serializedObject,
            modulesProp,
            draggable: true,
            displayHeader: true,
            displayAddButton: true,
            displayRemoveButton: true
        );

        modulesList.drawHeaderCallback = rect =>
            EditorGUI.LabelField(rect, "Modules");

        modulesList.onAddDropdownCallback = (buttonRect, _) =>
        {
            var menu = new GenericMenu();
            foreach (var t in moduleTypes)
            {
                menu.AddItem(new GUIContent(t.Name), false, () =>
                {
                    modulesProp.arraySize++;
                    var elem = modulesProp.GetArrayElementAtIndex(modulesProp.arraySize - 1);
                    elem.managedReferenceValue = Activator.CreateInstance(t);
                    serializedObject.ApplyModifiedProperties();
                });
            }
            menu.DropDown(buttonRect);
        };

        // always expanded, header shows the module's type name
        modulesList.drawElementCallback = (rect, index, active, focused) =>
        {
            var elem = modulesProp.GetArrayElementAtIndex(index);
            elem.isExpanded = true;

            string fullName = elem.managedReferenceFullTypename;
            string typeName = !string.IsNullOrEmpty(fullName)
                ? fullName.Split(' ')[1]
                : $"Element {index}";

            float height = EditorGUI.GetPropertyHeight(elem, true);
            var r = new Rect(rect.x, rect.y, rect.width, height);
            EditorGUI.PropertyField(r, elem, new GUIContent(typeName), true);
        };

        modulesList.elementHeightCallback = index =>
        {
            var elem = modulesProp.GetArrayElementAtIndex(index);
            return EditorGUI.GetPropertyHeight(elem, true)
                 + EditorGUIUtility.standardVerticalSpacing;
        };
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(
            serializedObject.FindProperty(nameof(Ability.abilityName)),
            new GUIContent("Ability Name"));
        EditorGUILayout.PropertyField(
            serializedObject.FindProperty(nameof(Ability.icon)),
            new GUIContent("Icon"));
        EditorGUILayout.PropertyField(
            serializedObject.FindProperty(nameof(Ability.description)),
            new GUIContent("Description"));
        EditorGUILayout.PropertyField(
            serializedObject.FindProperty(nameof(Ability.targetType)),
            new GUIContent("Target Type"));
        EditorGUILayout.PropertyField(
            serializedObject.FindProperty(nameof(Ability.cooldown)),
            new GUIContent("Cooldown"));

        EditorGUILayout.Space();
        modulesList.DoLayoutList();

        serializedObject.ApplyModifiedProperties();
    }
}
