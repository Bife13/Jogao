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
				if (itemTriggerType == itemEffect.itemTriggerType)
					ApplyItemEffect(itemEffect, self, target);
			}
		}
	}

	public void ApplyItemEffect(ItemEffect itemEffect, Unit self, Unit target)
	{
		switch (itemEffect.itemEffectType)
		{
			case ItemEffectType.ApplyStatus:
			case ItemEffectType.ApplyDebuff:
			case ItemEffectType.ApplyBuff:
				if (itemEffect.targetType == TargetType.Self)
					target.unitEffects.CheckAndApplyItemEffect(itemEffect, self);
				if (itemEffect.targetType == TargetType.Enemy)
					target.unitEffects.CheckAndApplyItemEffect(itemEffect, target);
				break;
			case ItemEffectType.Heal:
				target.unitHealth.Heal(itemEffect.healingAmount);
				break;
			case ItemEffectType.Cleanse:
				target.unitEffects.Cleanse(itemEffect.cleanseAmount, itemEffect.effectToCleanse,
					itemEffect.statusTypeToCleanse);
				break;
		}
	}
}