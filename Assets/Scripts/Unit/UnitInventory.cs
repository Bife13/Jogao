using System.Collections.Generic;
using UnityEngine;

public class UnitInventory : MonoBehaviour
{
	private Unit unit;
	public List<EquippableItem> equippedItems;

	public void Initialize(Unit owner)
	{
		unit = owner;
	}

	public float HandlePassiveItem(StatType statType)
	{
		float finalValue = 0;
		foreach (EquippableItem item in equippedItems)
		{
			foreach (PassiveStatModifier statModifier in item.baseStatBonuses)
			{
				if (statModifier.statType == statType)
				{
					finalValue += statModifier.amount;
				}
			}
		}

		return finalValue;
	}

	public float HandlePassiveItem(StatusType statusType)
	{
		float finalValue = 0;
		foreach (EquippableItem item in equippedItems)
		{
			foreach (StatusStatModifier statModifier in item.baseStatBonuses)
			{
				if (statModifier.statusType == statusType)
				{
					finalValue += statModifier.amount;
				}
			}
		}

		return finalValue;
	}

	public void HandleItemTriggers(ItemTriggerType itemTriggerType, Unit self, Unit target = null)
	{
		foreach (EquippableItem item in equippedItems)
		{
			foreach (ItemEffect itemEffect in item.itemEffects)
			{
				ApplyItemEffect(itemEffect.itemEffectType, self, target);
			}
		}
	}

	public void ApplyItemEffect(ItemEffectType itemEffectType, Unit self, Unit target)
	{
		switch (itemEffectType)
		{
			case ItemEffectType.ApplyStatus:
				break;
			case ItemEffectType.ApplyDebuff:
				break;
			case ItemEffectType.ApplyBuff:
				break;
			case ItemEffectType.Heal:
				break;
			case ItemEffectType.Cleanse:
				break;
		}
	}
}