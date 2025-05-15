using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class UnitCombatCalculator : MonoBehaviour
{
	private Unit unit;


	private float shockAmount = 25;


	public void Initialize(Unit owner)
	{
		unit = GetComponent<Unit>();
	}

	public virtual float CalculateDamage(Ability ability, Unit target)
	{
		float damage = Random.Range(unit.unitStatCalculator.GetTotalModifiedAttackStat()[0],
			unit.unitStatCalculator.GetTotalModifiedAttackStat()[1] + 1);

		if (PerformCriticalHitCheck((int)unit.unitStatCalculator.GetTotalModifiedStat(StatType.Crit) +
		                            ability.bonusCritical))
		{
			damage = (1.5f * unit.unitStatCalculator.GetTotalModifiedAttackStat()[1]);
			unit.unitInventory.HandleItemTriggers(ItemTriggerType.OnCrit, unit, target);
			Debug.Log($"{unit.unitName} hit a Critical Strike");
		}

		float baseDamage = damage;
		damage += damage * (ability.basePower / 100f);

		if (target.unitConditions.CheckForActiveConditions(target, ability.boostingConditions))
			damage += baseDamage * (ability.statusBoost / 100f);

		if (target.unitConditions.HasShockedDebuff())
		{
			damage += baseDamage * (shockAmount / 100f);
			target.unitConditions.ProcessConditionsPerTurn(ConditionTiming.OnHit, 500);
		}

		return damage;
	}

	public bool PerformAccuracyDodgeCheck(int abilityAccuracy, Unit target)
	{
		int accuracyRoll = Random.Range(1, 101);
		float dodgeThreshold = target.unitStatCalculator.GetTotalModifiedStat(StatType.Dodge);
		float currentAccuracy = unit.unitStatCalculator.GetTotalModifiedStat(StatType.Accuracy);
		if (accuracyRoll <= abilityAccuracy + currentAccuracy - dodgeThreshold)
		{
			return true; // Hit
		}

		return false; // Miss
	}

	public bool PerformCriticalHitCheck(int finalCritChance)
	{
		int critRoll = Random.Range(1, 101);
		if (critRoll <= finalCritChance)
			return true; //CRIT

		return false; //NORMAL HIT
	}

	public virtual bool ApplyDamageOrMiss(Ability ability, Unit target, float damage)
	{
		if (PerformAccuracyDodgeCheck(ability.accuracy, target) || target == this)
		{
			if (unit.unitConditions.activeCoating != null && ability.isWeaponAttack)
			{
				int coatDamage = unit.unitConditions.activeCoating.bonusDamage;
				if (unit.unitConditions.HasCoatingBuff())
				{
					coatDamage *= unit.unitConditions.CoatingBuffMultiplier();
				}

				damage += coatDamage;
				target.unitConditions.ApplyCondition(unit.unitConditions.activeCoating.condition);
			}

			target.unitHealth.TakeDirectDamage(Mathf.CeilToInt(damage), DamageType.Direct, unit);
			unit.unitInventory.HandleItemTriggers(ItemTriggerType.OnHit, unit, target);
			unit.unitConditions.CheckAndApplyAbilityConditions(ability, target);
			return true;
		}

		GameManager.Instance.combatUIManager.ActionChosen("Miss");
		return false;
	}

	public virtual void ApplyHealing(Ability ability, Unit target)
	{
		float healAmount = Random.Range(ability.basePower, ability.maxPower + 1);
		if (PerformCriticalHitCheck(15 + ability.bonusCritical))
			healAmount *= 2;

		target.unitHealth.Heal(Mathf.CeilToInt(healAmount));
		unit.unitConditions.CheckAndApplyAbilityConditions(ability, target);
	}

	public IEnumerator ExecuteCounterAttack(Unit originTarget)
	{
		yield return new WaitForSeconds(0.15f);

		Ability counterAbility = unit.unitConditions.GetCounterBuff().condition.counterAbility;
		List<Unit> originTargets = new List<Unit>();
		originTargets.Add(originTarget);
		unit.unitAbilityManager.UseAbility(counterAbility, originTargets);
	}
}