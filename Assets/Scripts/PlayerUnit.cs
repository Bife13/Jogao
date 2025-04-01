using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering;
using Random = UnityEngine.Random;

public class PlayerUnit : Unit
{
	public override void UseAbility(Ability ability, List<Unit> targets)
	{
		CommitStance();

		foreach (Unit target in targets)
		{
			if (!isTargetValid(ability, target))
			{
				Debug.LogWarning($"Invalid target for ability {ability.abilityName}");
			}

			Debug.Log($"{gameObject.name} uses {ability.abilityName} on {target.unitName}!");

			if (activeCoating != null && ability.isWeaponAttack)
			{
				coatingDuration--;
				RefreshCoatingUI(true);
				Debug.Log($"{unitName}'s {activeCoating.coatingName}  has: " + coatingDuration);

				if (coatingDuration <= 0)
				{
					Debug.Log($"{unitName}'s {activeCoating.coatingName} coating has worn off.");
					activeCoating = null;
					RefreshCoatingUI(false);
				}
			}

			if (ability.canCoat)
			{
				ApplyWeaponCoating(ability.coating);
			}

			switch (ability.abilityEffectType)
			{
				case AbilityEffectType.Damage:
					AttackAnimation(1);

					float damage = Random.Range(minDamage, maxDamage + 1);

					if (PerformCriticalHitCheck((int)GetTotalModifiedStat(StatType.Crit,
						    critChance + ability.bonusCritical)))
					{
						damage = (1.5f * maxDamage);
						Debug.Log($"{target.unitName} hit a Critical Strike");
					}

					float baseDamage = damage;
					damage += CalculateStanceBonusDamage(baseDamage);
					damage += (GetTotalModifiedStat(StatType.Attack, baseDamage) - baseDamage);

					damage += baseDamage * (ability.basePower / 100f);

					if (target.CheckForActiveEffects(target, ability.boostingEffects))
						damage += baseDamage * (ability.statusBoost / 100f);

					if (target.HasShockedDebuff())
					{
						damage += baseDamage * (shockAmount / 100f);
						target.ProcessEffectsPerTurn(EffectTiming.OnHit, 500);
					}

					if (target != this)
					{
						if (PerformAccuracyDodgeCheck(ability.accuracy, target))
						{
							if (activeCoating != null && ability.isWeaponAttack)
							{
								int coatDamage = activeCoating.bonusDamage;
								if (HasCoatingBuff())
								{
									coatDamage *= CoatingBuffMultiplier();
								}

								damage += coatDamage;
								target.ApplyEffect(activeCoating.effect);
							}

							target.TakeDirectDamage(Mathf.CeilToInt(damage), DamageType.Direct, this);

							CheckAndApplyEffects(ability, target);
							GameManager.Instance.OnPlayerActionChosen("Attack");
						}
						else
						{
							GameManager.Instance.OnPlayerActionChosen("Miss");
						}
					}
					else
					{
						if (activeCoating != null && ability.isWeaponAttack)
						{
							int coatDamage = activeCoating.bonusDamage;
							if (HasCoatingBuff())
							{
								coatDamage *= CoatingBuffMultiplier();
							}

							damage += coatDamage;
							target.ApplyEffect(activeCoating.effect);
						}

						target.TakeDirectDamage(Mathf.CeilToInt(damage), DamageType.Direct, this);

						CheckAndApplyEffects(ability, target);
						GameManager.Instance.OnPlayerActionChosen("Attack");
					}

					break;
				case AbilityEffectType.Heal:
					float healAmount = Random.Range(ability.basePower, ability.maxPower);
					if (PerformCriticalHitCheck((int)GetTotalModifiedStat(StatType.Crit, 15 + ability.bonusCritical)))
						healAmount *= 2;
					target.Heal(Mathf.CeilToInt(healAmount));
					CheckAndApplyEffects(ability, target);
					break;
				case AbilityEffectType.Buff:
					CheckAndApplyEffects(ability, target);
					break;
				case AbilityEffectType.Debuff:
					if (target != this)
					{
						if (PerformAccuracyDodgeCheck(ability.accuracy, target))
							CheckAndApplyEffects(ability, target);
					}
					else
					{
						CheckAndApplyEffects(ability, target);
					}

					break;
				case AbilityEffectType.StatusEffect:
					CheckAndApplyEffects(ability, target);
					break;
			}

			if (ability.canCleanse)
			{
				int amount = ability.cleanseAmount <= 0 ? int.MaxValue : ability.cleanseAmount;
				target.Cleanse(amount, ability.effectTypeToCleanse, ability.statusTypeToCleanse);
				Debug.Log($"{target.unitName} is cleansed of {amount} debuffs!");
			}


			abilityCooldowns[ability] = ability.cooldown;
			if (ability.abilityIndexes.Any())
			{
				foreach (var index in ability.abilityIndexes)
				{
					SwapAbilities(index, 4 + index);
				}
			}
		}
	}

	public bool isTargetValid(Ability ability, Unit target)
	{
		if (target == null)
		{
			Debug.LogWarning("No target selected!");
			return false;
		}

		if (!target.isAlive())
		{
			Debug.LogWarning($"{target.unitName} is already dead!");
			return false;
		}

		switch (ability.targetType)
		{
			case AbilityTargetType.Enemy:
				if (target.unitType != UnitType.ENEMY)
					return false;
				break;
			case AbilityTargetType.Ally:
				if (target.unitType != UnitType.PLAYER)
					return false;
				break;
			case AbilityTargetType.Self:
				if (target != this.GetComponent<Unit>())
					return false;
				break;
			case AbilityTargetType.AllEnemies:
				if (target.unitType != UnitType.ENEMY)
					if (!ability.hitsSelf)
						return false;
				break;
			case AbilityTargetType.AllAllies:
				if (target.unitType != UnitType.PLAYER)
					return false;
				break;
			default:
				return false;
		}

		return true;
	}

	public void CommitStance()
	{
		if (hasComittedStance)
		{
			Debug.Log("You've already committed to a stance!");
			return;
		}

		if (previewStance != StanceType.Neutral)
		{
			currentStance = previewStance;
			hasComittedStance = true;
			Debug.Log($"Stance committed: {currentStance}");
		}
	}
}