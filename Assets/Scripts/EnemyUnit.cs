using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class EnemyUnit : Unit
{
	private Ability nextAbility;

	[SerializeField]
	private Image intentImage;

	void Start()
	{
		DecideNextIntent();
	}

	public void DecideNextIntent()
	{
		intentImage.enabled = true;
		nextAbility = _abilities[Random.Range(0, _abilities.Count)];
		intentImage.sprite = nextAbility.icon;
	}

	public override void UseAbility(Ability ability, List<Unit> targets)
	{
		foreach (Unit target in targets)
		{
			Debug.Log($"{gameObject.name} uses {ability.abilityName} on {target.unitName}!");
			if (!isTargetValid(ability, target))
			{
				Debug.LogWarning($"Invalid target for ability {ability.abilityName}");
				return;
			}

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
				RefreshCoatingUI(true);
			}


			switch (ability.abilityEffectType)
			{
				case AbilityEffectType.Damage:
					AttackAnimation(-1);

					float damage = Random.Range(minDamage, maxDamage + 1);

					if (PerformCriticalHitCheck((int)GetTotalModifiedStat(StatType.Crit,
						    critChance + ability.bonusCritical)))
						damage = (1.5f * maxDamage);

					float baseDamage = damage;

					damage += (GetTotalModifiedStat(StatType.Attack, baseDamage) - baseDamage);
					damage += baseDamage * (ability.basePower / 100f);

					if (target.CheckForActiveEffects(target, ability.boostingEffects))
						damage += baseDamage * (ability.statusBoost / 100f);

					if (target.HasShockedDebuff())
					{
						damage += baseDamage * (shockAmount / 100f);
						target.ProcessEffectsPerTurn(EffectTiming.OnHit, 500);
					}


					if (activeCoating != null)
					{
						int coatDamage = activeCoating.bonusDamage;
						if (HasCoatingBuff())
						{
							coatDamage *= CoatingBuffMultiplier();
						}

						damage += coatDamage;
						target.ApplyEffect(activeCoating.effect);
					}

					if (target != this)
					{
						if (PerformAccuracyDodgeCheck(ability.accuracy, target))
						{
							target.TakeDirectDamage(Mathf.CeilToInt(damage), DamageType.Direct, this);
							CheckAndApplyEffects(ability, target);
						}
						else
						{
							Debug.Log("ERROU");
							// DO MISS LOGIC HERE EVENTUALLY
						}
					}
					else
					{
						target.TakeDirectDamage(Mathf.CeilToInt(damage), DamageType.Direct, this);
						CheckAndApplyEffects(ability, target);
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
			intentImage.enabled = false;
		}
	}

	private bool isTargetValid(Ability ability, Unit target)
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
				if (target.unitType != UnitType.PLAYER)
					return false;
				break;
			case AbilityTargetType.Ally:
				if (target.unitType != UnitType.ENEMY)
					return false;
				break;
			case AbilityTargetType.Self:
				if (target != this.GetComponent<Unit>())
					return false;
				break;
			case AbilityTargetType.AllEnemies:
				if (target.unitType != UnitType.PLAYER)
				{
					if (!ability.hitsSelf)
						return false;
				}

				break;
			case AbilityTargetType.AllAllies:
				if (target.unitType != UnitType.ENEMY)
					return false;
				break;
			default:
				return false;
		}

		return true;
	}

	public void PerformIntent()
	{
		List<Unit> targets = GameManager.Instance.GetValidTargetsForEnemy(nextAbility);
		UseAbility(nextAbility, targets);
	}
}