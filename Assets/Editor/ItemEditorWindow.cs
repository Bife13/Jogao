using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEditor;

public class ItemEditorWindow : EditorWindow
{
	private EquippableItem newItem;

	[MenuItem("Tools/Item Creator")]
	public static void ShowWindow()
	{
		GetWindow<ItemEditorWindow>("Item Creator");
	}

	void OnGUI()
	{
		if (newItem == null)
			newItem = CreateInstance<EquippableItem>();

		newItem.itemName = EditorGUILayout.TextField("Item Name", newItem.itemName);
		newItem.description = EditorGUILayout.TextField("Item Description", newItem.itemName);
		newItem.icon = (Sprite)EditorGUILayout.ObjectField("Item Icon", newItem.icon, typeof(Sprite), false);

		if (GUILayout.Button("Add Effect"))
		{
			newItem.itemEffects.Add(new ItemEffect());
		}

		for (int i = 0; i < newItem.itemEffects.Count; i++)
		{
			EditorGUILayout.Space();
			EditorGUILayout.LabelField($"Effect {i + 1}", EditorStyles.boldLabel);
			var effect = newItem.itemEffects[i];

			effect.itemTriggerType = (ItemTriggerType)EditorGUILayout.EnumPopup("Item Trigger", effect.itemTriggerType);
			effect.itemEffectType = (ItemEffectType)EditorGUILayout.EnumPopup("Item Effect", effect.itemEffectType);

			switch (effect.itemEffectType)
			{
				case ItemEffectType.ApplyStatus:
				case ItemEffectType.ApplyBuff:
				case ItemEffectType.ApplyDebuff:
					effect.conditionToApply =
						(Condition)EditorGUILayout.ObjectField("Condition", effect.conditionToApply, typeof(Condition),
							false);
					effect.conditionChance = EditorGUILayout.IntField("Condition Chance", effect.conditionChance);
					effect.targetType = (TargetType)EditorGUILayout.EnumPopup("Target Type", effect.targetType);
					break;
				case ItemEffectType.Heal:
					effect.healingAmount = EditorGUILayout.IntField("Healing Amount", effect.healingAmount);
					break;
				case ItemEffectType.Cleanse:
					effect.conditionToCleanse =
						(ConditionType)EditorGUILayout.EnumPopup("Condition to Cleanse", effect.conditionToCleanse);
					effect.statusTypeToCleanse =
						(StatusType)EditorGUILayout.EnumPopup("Status to Cleanse", effect.statusTypeToCleanse);
					effect.cleanseAmount = EditorGUILayout.IntField("Cleanse Amount", effect.cleanseAmount);
					break;
			}

			if (GUILayout.Button("Remove"))
			{
				newItem.itemEffects.RemoveAt(i);
				break;
			}
		}
		
		EditorGUILayout.Space();


		if (GUILayout.Button("Add Passive Stat Modifier"))
		{
			newItem.baseStatBonuses.Add(new PassiveStatModifier());
		}

		for (int i = 0; i < newItem.baseStatBonuses.Count; i++)
		{
			EditorGUILayout.Space();
			EditorGUILayout.LabelField($"Effect {i + 1}", EditorStyles.boldLabel);
			var effect = newItem.baseStatBonuses[i];

			effect.statType = (StatType)EditorGUILayout.EnumPopup("Stat Type", effect.statType);
			effect.amount = EditorGUILayout.IntField("Stat Amount", effect.amount);

			if (GUILayout.Button("Remove"))
			{
				newItem.baseStatBonuses.RemoveAt(i);
				break;
			}
		}

		EditorGUILayout.Space();

		if (GUILayout.Button("Save Item"))
		{
			string path =
				EditorUtility.SaveFilePanelInProject("Save Item", newItem.itemName, "asset", "Save item to folder");
			if (!string.IsNullOrEmpty(path))
			{
				AssetDatabase.CreateAsset(newItem, path);
				AssetDatabase.SaveAssets();
				newItem = null;
				EditorUtility.DisplayDialog("Saved", "Item created!", "OK");
			}
		}
	}
}