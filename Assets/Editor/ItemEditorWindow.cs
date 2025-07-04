using System;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

public class ItemEditorWindow : EditorWindow
{
	private EquippableItem currentItem;
	private SerializedObject serializedItem;
	private ReorderableList moduleList;
	private ReorderableList statBonusList;

	[MenuItem("Tools/Item Creator & Editor")]
	public static void ShowWindow() =>
		GetWindow<ItemEditorWindow>("Item Creator & Editor");

	void OnEnable()
	{
		SetupLists();
	}

	void OnSelectionChange()
	{
		var sel = Selection.activeObject as EquippableItem;
		if (sel != null)
		{
			LoadItem(sel);
			Repaint();
		}
	}

	void SetupLists()
	{
		if (serializedItem == null)
			return;

		// Modules list
		moduleList = new ReorderableList(
			serializedItem,
			serializedItem.FindProperty(nameof(EquippableItem.itemEffectModules)),
			true, true, true, true
		);
		moduleList.drawHeaderCallback = rect =>
			EditorGUI.LabelField(rect, "Effect Modules");
		moduleList.onAddDropdownCallback = (buttonRect, _) =>
		{
			var menu = new GenericMenu();
			var types = new Type[]
			{
				typeof(ItemConditionModule),
				typeof(ItemHealModule),
				typeof(ItemCleanseModule)
			};
			foreach (var t in types)
				menu.AddItem(new GUIContent(t.Name), false, () => AddModule(t));
			menu.DropDown(buttonRect);
		};
		moduleList.drawElementCallback = (rect, index, active, focused) =>
		{
			var prop = serializedItem.FindProperty(nameof(EquippableItem.itemEffectModules))
				.GetArrayElementAtIndex(index);
			prop.isExpanded = true;
			string fullName = prop.managedReferenceFullTypename;
			string label = !string.IsNullOrEmpty(fullName)
				? fullName.Split(' ')[1]
				: $"Module {index}";
			float h = EditorGUI.GetPropertyHeight(prop, true);
			var r = new Rect(rect.x, rect.y, rect.width, h);
			EditorGUI.PropertyField(r, prop, new GUIContent(label), true);
		};
		moduleList.elementHeightCallback = index =>
		{
			var prop = serializedItem.FindProperty(nameof(EquippableItem.itemEffectModules))
				.GetArrayElementAtIndex(index);
			return EditorGUI.GetPropertyHeight(prop, true)
			       + EditorGUIUtility.standardVerticalSpacing;
		};

		// Stat Bonuses list
		statBonusList = new ReorderableList(
			serializedItem,
			serializedItem.FindProperty(nameof(EquippableItem.baseStatBonuses)),
			true, true, true, true
		);
		statBonusList.drawHeaderCallback = rect =>
			EditorGUI.LabelField(rect, "Stat Modifiers");
		statBonusList.drawElementCallback = (rect, index, active, focused) =>
		{
			var prop = serializedItem.FindProperty(nameof(EquippableItem.baseStatBonuses))
				.GetArrayElementAtIndex(index);
			prop.isExpanded = true;
			float h = EditorGUI.GetPropertyHeight(prop, true);
			var r = new Rect(rect.x, rect.y, rect.width, h);
			EditorGUI.PropertyField(r, prop, new GUIContent($"Modifier {index + 1}"), true);
		};
		statBonusList.elementHeightCallback = i =>
			EditorGUI.GetPropertyHeight(
				serializedItem.FindProperty(nameof(EquippableItem.baseStatBonuses))
					.GetArrayElementAtIndex(i), true)
			+ EditorGUIUtility.standardVerticalSpacing;
	}

	void AddModule(Type moduleType)
	{
		var listProp = serializedItem.FindProperty(nameof(EquippableItem.itemEffectModules));
		listProp.arraySize++;
		var elem = listProp.GetArrayElementAtIndex(listProp.arraySize - 1);
		elem.managedReferenceValue = Activator.CreateInstance(moduleType);
		serializedItem.ApplyModifiedProperties();
	}

	void LoadItem(EquippableItem item)
	{
		currentItem = item;
		serializedItem = new SerializedObject(currentItem);
		SetupLists();
	}

	void OnGUI()
	{
		EditorGUILayout.Space();
		// Selection or New
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("Equippable Item", GUILayout.Width(120));
		EditorGUI.BeginChangeCheck();
		var sel = (EquippableItem)EditorGUILayout.ObjectField(
			currentItem, typeof(EquippableItem), false);
		if (EditorGUI.EndChangeCheck())
		{
			if (sel != null) LoadItem(sel);
			else
			{
				currentItem = null;
				serializedItem = null;
			}
		}

		if (currentItem == null)
		{
			if (GUILayout.Button("New", GUILayout.Width(40)))
			{
				var inst = CreateInstance<EquippableItem>();
				inst.itemName = "New Item";
				LoadItem(inst);
			}
		}

		EditorGUILayout.EndHorizontal();

		if (serializedItem == null)
		{
			EditorGUILayout.HelpBox("Select or create an EquippableItem to begin.", MessageType.Info);
			return;
		}

		serializedItem.Update();

		// Basic fields
		EditorGUILayout.PropertyField(
			serializedItem.FindProperty(nameof(EquippableItem.itemName)),
			new GUIContent("Item Name")
		);
		EditorGUILayout.PropertyField(
			serializedItem.FindProperty(nameof(EquippableItem.description)),
			new GUIContent("Description")
		);
		EditorGUILayout.PropertyField(
			serializedItem.FindProperty(nameof(EquippableItem.icon)),
			new GUIContent("Icon")
		);

		EditorGUILayout.Space();
		moduleList.DoLayoutList();
		EditorGUILayout.Space();
		statBonusList.DoLayoutList();

		EditorGUILayout.Space();
		// Save or Save As
		var path = AssetDatabase.GetAssetPath(currentItem);
		if (string.IsNullOrEmpty(path))
		{
			if (GUILayout.Button("Save As New Asset"))
			{
				var savePath = EditorUtility.SaveFilePanelInProject(
					"Save Item", currentItem.itemName, "asset", "Choose save location");
				if (!string.IsNullOrEmpty(savePath))
				{
					AssetDatabase.CreateAsset(currentItem, savePath);
					serializedItem.Update();
					AssetDatabase.SaveAssets();
					EditorUtility.DisplayDialog("Saved", "Item created!", "OK");
				}
			}
		}
		else
		{
			if (GUILayout.Button("Save Changes"))
			{
				serializedItem.ApplyModifiedProperties();
				EditorUtility.SetDirty(currentItem);
				AssetDatabase.SaveAssets();
				EditorUtility.DisplayDialog("Saved", "Item updated!", "OK");
			}
		}

		// Always apply property modifications so edits persist immediately
		serializedItem.ApplyModifiedProperties();
	}
}