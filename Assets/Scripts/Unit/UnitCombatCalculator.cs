using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

	public virtual float CalculateDamage(int basePower, int bonusCritical, List<StatusType> boostingStatus,
		float statusBoost, Unit target)
	{
		float damage = Random.Range(unit.unitStatCalculator.GetTotalModifiedAttackStat()[0],
			unit.unitStatCalculator.GetTotalModifiedAttackStat()[1] + 1);

		if (PerformCriticalHitCheck((int)unit.unitStatCalculator.GetTotalModifiedStat(StatType.Crit) + bonusCritical))
		{
			damage = (1.5f * unit.unitStatCalculator.GetTotalModifiedAttackStat()[1]);
			unit.unitInventory.HandleItemTriggers(ItemTriggerType.OnCrit, unit, target);
			Debug.Log($"{unit.unitName} hit a Critical Strike");
		}

		float baseDamage = damage;
		damage += damage * (basePower / 100f);

		if (target.unitConditions.CheckForActiveConditions(target, boostingStatus))
		{
			Debug.Log("GOT HERE");
			damage += baseDamage * (statusBoost / 100f);
		}

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

	public virtual bool ApplyDamageOrMiss(int accuracy, bool isWeaponAttack, Unit target, float damage)
	{
		if (PerformAccuracyDodgeCheck(accuracy, target) || target == this)
		{
			if (unit.unitConditions.activeCoating != null && isWeaponAttack)
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
			return true;
		}

		GameManager.Instance.combatUIManager.ActionChosen("Miss");
		return false;
	}

	public virtual void ApplyHealing(int minAmount, int maxAmount, int bonusCriticalChance, Unit target)
	{
		float healAmount = Random.Range(minAmount, maxAmount + 1);
		if (PerformCriticalHitCheck(15 + bonusCriticalChance))
			healAmount *= 2;

		target.unitHealth.Heal(Mathf.CeilToInt(healAmount));
	}

	public IEnumerator ExecuteCounterAttack(Unit originTarget)
	{
		yield return new WaitForSeconds(0.15f);

		Ability counterAbility = unit.unitConditions.GetCounterBuff().condition.effects.OfType<CounterattackEffect>()
			.FirstOrDefault().counterAbility;
		List<Unit> originTargets = new List<Unit>();
		originTargets.Add(originTarget);
		unit.unitAbilityManager.UseAbility(counterAbility, originTargets);
	}
}