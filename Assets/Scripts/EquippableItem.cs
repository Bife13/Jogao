using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EquippableItem", menuName = "Game/Items/EquippableItem")]
public class EquippableItem : ScriptableObject
{
	public string itemName;
	public string description;
	public Sprite icon;

	public List<PassiveStatModifier> baseStatBonuses;
	public List<ItemEffect> itemEffects;
}