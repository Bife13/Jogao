using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering;

public class PlayerUnit : Unit
{
	public void UseAbility(Ability ability, List<Unit> targets)
	{
		foreach (Unit target in targets)
		{
			Debug.Log($"{gameObject.name} uses {ability.abilityName} on {target.unitName}!");
			if (!isTargetValid(ability, target))
			{
				Debug.LogWarning($"Invalid target for ability {ability.abilityName}");
				return;
			}

			Debug.Log(ability.effectType);
			switch (ability.effectType)
			{
				case AbilityEffectType.Damage:
					float damage = Random.Range(minDamage, maxDamage + 1);

					if (PerformCriticalHitCheck((int)GetTotalModifiedStat(StatType.Crit, critChance)))
						damage = (1.5f * maxDamage);

					damage += GetTotalModifiedStat(StatType.Attack, damage);
					damage += damage * ability.minPower;
					AttackAnimation(1);
					if (PerformAccuracyDodgeCheck(ability.accuracy, target))
					{
						target.TakeDamage(Mathf.CeilToInt(damage), DamageType.Direct);
						CheckAndApplyEffects(ability, target);
						GameManager.Instance.OnPlayerActionChosen("Attack");
					}
					else
					{
						GameManager.Instance.OnPlayerActionChosen("Miss");
					}

					break;
				case AbilityEffectType.Heal:
					float healAmount = Random.Range(ability.minPower, ability.maxPower);
					if (PerformCriticalHitCheck((int)GetTotalModifiedStat(StatType.Crit, 15)))
						healAmount *= 2;
					target.Heal(Mathf.CeilToInt(healAmount));
					CheckAndApplyEffects(ability, target);
					break;
				case AbilityEffectType.Buff:
					CheckAndApplyEffects(ability, target);
					break;
				case AbilityEffectType.Debuff:
					CheckAndApplyEffects(ability, target);
					break;
			}

			// if (ability.statusEffect != null)
			// {
			// 	int roll = Random.Range(0, 100);
			// 	if (roll < ability.statusEffectChance)
			// 		target.applyStatusEffect(ability.statusEffect);
			// }

			// DO VFX AND SHIT
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
}