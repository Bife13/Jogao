using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class UnitCombatCalculator : MonoBehaviour
{
	private Unit unit;
	private UnitStatCalculator unitStatCalculator;
	private UnitEffects unitEffects;
	private UnitAbilityManager unitAbilityManager;

	private float shockAmount = 25;


	private void Awake()
	{
		unit = GetComponent<Unit>();
		unitStatCalculator = GetComponent<UnitStatCalculator>();
		unitEffects = GetComponent<UnitEffects>();
		unitAbilityManager = GetComponent<UnitAbilityManager>();
	}

	public virtual float CalculateDamage(Ability ability, Unit target)
	{
		float damage = Random.Range(unitStatCalculator.GetTotalModifiedAttackStat()[0],
			unitStatCalculator.GetTotalModifiedAttackStat()[1]);

		if (PerformCriticalHitCheck((int)unitStatCalculator.GetTotalModifiedStat(StatType.Crit) +
		                            ability.bonusCritical))
		{
			damage = (1.5f * unitStatCalculator.GetTotalModifiedAttackStat()[1]);
			Debug.Log($"{target.unitName} hit a Critical Strike");
		}

		float baseDamage = damage;
		damage += damage * (ability.basePower / 100f);

		if (target.unitEffects.CheckForActiveEffects(target, ability.boostingEffects))
			damage += baseDamage * (ability.statusBoost / 100f);

		if (target.unitEffects.HasShockedDebuff())
		{
			damage += baseDamage * (shockAmount / 100f);
			target.unitEffects.ProcessEffectsPerTurn(EffectTiming.OnHit, 500);
		}

		return damage;
	}

	public bool PerformAccuracyDodgeCheck(int abilityAccuracy, Unit target)
	{
		float accuracyRoll = Random.Range(0f, 100f);
		float dodgeThreshold = target.unitStatCalculator.GetTotalModifiedStat(StatType.Dodge);
		float currentAccuracy = unitStatCalculator.GetTotalModifiedStat(StatType.Accuracy);
		if (accuracyRoll <= abilityAccuracy + currentAccuracy - dodgeThreshold)
		{
			return true; // Hit
		}

		return false; // Miss
	}

	public bool PerformCriticalHitCheck(int finalCritChance)
	{
		float critRoll = Random.Range(0f, 100f);
		if (critRoll <= finalCritChance)
			return true; //CRIT

		return false; //NORMAL HIT
	}

	public virtual bool ApplyDamageOrMiss(Ability ability, Unit target, float damage)
	{
		if (PerformAccuracyDodgeCheck(ability.accuracy, target) || target == this)
		{
			if (unitEffects.activeCoating != null && ability.isWeaponAttack)
			{
				int coatDamage = unitEffects.activeCoating.bonusDamage;
				if (unitEffects.HasCoatingBuff())
				{
					coatDamage *= unitEffects.CoatingBuffMultiplier();
				}

				damage += coatDamage;
				target.unitEffects.ApplyEffect(unitEffects.activeCoating.effect);
			}

			target.unitHealth.TakeDirectDamage(Mathf.CeilToInt(damage), DamageType.Direct, unit);
			unitEffects.CheckAndApplyEffects(ability, target);
			return true;
		}

		GameManager.Instance.ActionChosen("Miss");
		return false;
	}

	public virtual void ApplyHealing(Ability ability, Unit target)
	{
		float healAmount = Random.Range(ability.basePower, ability.maxPower);
		if (PerformCriticalHitCheck(15 + ability.bonusCritical))
			healAmount *= 2;

		target.unitHealth.Heal(Mathf.CeilToInt(healAmount));
		unitEffects.CheckAndApplyEffects(ability, target);
	}

	public IEnumerator ExecuteCounterAttack(Unit originTarget)
	{
		yield return new WaitForSeconds(0.15f);

		Ability counterAbility = unitEffects.GetCounterBuff().effect.counterAbility;
		List<Unit> originTargets = new List<Unit>();
		originTargets.Add(originTarget);
		unitAbilityManager.UseAbility(counterAbility, originTargets);
	}
}