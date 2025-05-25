using System;
using System.Collections.Generic;
using UnityEngine;

public class UnitStatCalculator : MonoBehaviour
{
	private Unit unit;

	public void Initialize(Unit owner)
	{
		unit = owner;
	}

	public float GetTotalModifiedStat(StatType statType)
	{
		float modifier = CalculateModifier(statType);
		float finalValue = CalculateStatValue(statType, modifier);
		return finalValue;
	}

	public List<int> GetTotalModifiedAttackStat()
	{
		List<int> finalValues = new List<int>();
		float modifier = CalculateModifier(StatType.Attack) + 100;
		modifier /= 100;
		Debug.Log("MODIFIER IS:" + modifier);
		finalValues.Add(Mathf.CeilToInt(unit.minDamage * modifier));
		finalValues.Add(Mathf.CeilToInt(unit.maxDamage * modifier));
		return finalValues;
	}

	public float CalculateModifier(StatType statType)
	{
		float modifier = 0;
		foreach (ActiveCondition activeCondition in unit.unitConditions.activeConditions)
		{
			if (activeCondition.condition.statAffected == statType)
			{
				modifier += activeCondition.condition.amount;
			}
		}

		modifier += unit.unitInventory.HandleStatPassiveItem(statType) +
		            unit.unitStance.CalculateStanceBonusAttackAmount();

		return modifier;
	}

	public float CalculateStatValue(StatType statType, float modifier)
	{
		float resultValue = 0;
		switch (statType)
		{
			case StatType.Health:
				resultValue = MathF.Max(0f, unit.unitHealth.currentHP);
				break;
			case StatType.Speed:
				resultValue = MathF.Max(0f, unit.speed + modifier);
				break;
			case StatType.Accuracy:
				resultValue = MathF.Max(0f, unit.accuracy + modifier);
				break;
			case StatType.Defense:
				resultValue = MathF.Max(0f,
					unit.baseDefense + modifier + unit.unitStance.CalculateStanceBonusDefense());
				break;
			case StatType.Crit:
				resultValue = Mathf.Max(0f, unit.critChance + modifier);
				break;
			case StatType.Dodge:
				resultValue = Mathf.Max(0f, unit.dodge + modifier);
				break;
		}

		return resultValue;
	}
}