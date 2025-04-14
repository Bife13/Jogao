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
		float modifier = CalculateModifier(StatType.Attack);
		modifier += unit.unitStance.CalculateStanceBonusAttackAmount() + 100;
		modifier /= 100;
		finalValues.Add(Mathf.CeilToInt(unit.minDamage * modifier));
		finalValues.Add(Mathf.CeilToInt(unit.maxDamage * modifier));
		return finalValues;
	}

	public float CalculateModifier(StatType statType)
	{
		float modifier = 0;
		foreach (var activeEffect in unit.unitEffects.activeEffects)
		{
			if (activeEffect.effect.statAffected == statType)
			{
				modifier += activeEffect.effect.amount;
			}
		}
		
		return modifier;
	}

	public float CalculateStatValue(StatType statType, float modifier)
	{
		float resultValue = 0;
		switch (statType)
		{
			case StatType.Health:
				resultValue = unit.unitHealth.currentHP;
				break;
			case StatType.Speed:
				resultValue = unit.speed + modifier;
				break;
			case StatType.Accuracy:
				resultValue = unit.accuracy + modifier;
				break;
			case StatType.Defense:
				resultValue = unit.baseDefense + modifier + unit.unitStance.CalculateStanceBonusDefense();
				break;
			case StatType.Crit:
				resultValue = unit.critChance + modifier;
				if (resultValue < 0)
					resultValue = 0;
				break;
			case StatType.Dodge:
				resultValue = unit.dodge + modifier;
				if (resultValue < 0)
					resultValue = 0;
				break;
		}

		return resultValue;
	}
	
}