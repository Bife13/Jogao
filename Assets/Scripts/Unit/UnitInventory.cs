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

	public int HandleStatPassiveItem(StatType statType)
	{
		int finalValue = 0;
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

	public int HandleStatusPassiveItem(StatusType statusType)
	{
		int finalValue = 0;
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

	public int HandleJinxPassiveItem()
	{
		int finalValue = 0;
		foreach (EquippableItem item in equippedItems)
		{
			foreach (JinxStatModifier statModifier in item.baseStatBonuses)
			{
				if (statModifier.conditionType == ConditionType.Jinx)
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
			foreach (ItemEffectModule itemEffectModule in item.itemEffectModules)
			{
				if (itemTriggerType == itemEffectModule.itemTriggerType)
					ApplyItemEffect(itemEffectModule, self, target);
			}
		}
	}

	public void ApplyItemEffect(ItemEffectModule itemEffectModule, Unit self, Unit target)
	{
		Unit actualTarget = itemEffectModule.targetType switch
		{
			TargetType.Self => self,
			TargetType.Enemy => target ?? self,
			_ => self
		};

		itemEffectModule.Apply(actualTarget);
	}
}