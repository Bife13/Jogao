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

	public virtual float CalculateDamage(int basePower, int bonusCritical, List<StatusType> boostingStatus,
		float statusBoost, Unit target, DamageWindow damageWindow)
	{
		float damage = 0;
		switch (damageWindow)
		{
			case DamageWindow.Minimum:
				damage = unit.unitStatCalculator.GetTotalModifiedAttackStat()[0];

				break;
			case DamageWindow.Maximum:
				damage = unit.unitStatCalculator.GetTotalModifiedAttackStat()[1];

				break;
		}

		float baseDamage = damage;
		damage += damage * (basePower / 100f);

		if (target.unitConditions.CheckForActiveConditions(target, boostingStatus))
		{
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

		if (accuracyRoll <= CalculateAccuracy(abilityAccuracy, target))
		{
			return true; // Hit
		}

		return false; // Miss
	}

	public int CalculateAccuracy(int abilityAccuracy, Unit target)
	{
		int dodgeThreshold = Mathf.CeilToInt(target.unitStatCalculator.GetTotalModifiedStat(StatType.Dodge));
		int currentAccuracy = Mathf.CeilToInt(unit.unitStatCalculator.GetTotalModifiedStat(StatType.Accuracy));

		return abilityAccuracy + currentAccuracy - dodgeThreshold;
	}

	public bool PerformCriticalHitCheck(int finalCritChance)
	{
		int critRoll = Random.Range(1, 101);
		if (critRoll <= finalCritChance)
			return true; //CRIT

		return false; //NORMAL HIT
	}

	public virtual bool ApplyDamageOrMiss(int accuracy, bool isWeaponAttack, Unit target, int damage)
	{
		if (PerformAccuracyDodgeCheck(accuracy, target) || target == this)
		{
			if (isWeaponAttack && unit.unitConditions.activeCoating != null)
			{
				damage += AddCoatingDamage();
				target.unitConditions.ApplyCondition(unit.unitConditions.activeCoating.condition);
			}

			target.unitHealth.TakeDirectDamage(damage, DamageType.Direct, unit);
			unit.unitInventory.HandleItemTriggers(ItemTriggerType.OnHit, unit, target);
			return true;
		}

		GameManager.Instance.combatUIManager.ActionChosen("Miss");
		return false;
	}

	public int AddCoatingDamage()
	{
		if (unit.unitConditions.activeCoating != null)
		{
			int coatDamage = unit.unitConditions.activeCoating.bonusDamage;
			if (unit.unitConditions.HasCoatingBuff())
			{
				coatDamage *= unit.unitConditions.CoatingBuffMultiplier();
			}

			return coatDamage;
		}

		return 0;
	}

	public virtual void ApplyHealing(int minAmount, int maxAmount, int bonusCriticalChance, Unit target)
	{
		float healAmount = Random.Range(minAmount, maxAmount + 1);
		if (PerformCriticalHitCheck(unit.healCritChance + bonusCriticalChance))
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

public enum DamageWindow
{
	Minimum,
	Maximum
}