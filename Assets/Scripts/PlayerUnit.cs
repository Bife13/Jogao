using UnityEngine;
using UnityEngine.Rendering;

public class PlayerUnit : Unit
{
	public void UseAbility(Ability ability, Unit target)
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

				if (PerformCriticalHitCheck(critChance))
					damage = (1.5f * maxDamage);

				damage += damage * ability.minPower;
				if (PerformAccuracyDodgeCheck(ability.accuracy, target))
				{
					target.TakeDamage(Mathf.RoundToInt(damage));
					TurnManager.Instance.OnPlayerActionChosen("Attack");
				}
				else
				{
					TurnManager.Instance.OnPlayerActionChosen("Miss");
				}

				break;
			case AbilityEffectType.Heal:
				float healAmount = Random.Range(ability.minPower, ability.maxPower);
				if (PerformCriticalHitCheck(15))
					healAmount *= 2;
				target.Heal(Mathf.RoundToInt(healAmount));
				break;
			case AbilityEffectType.Buff:
				break;
			case AbilityEffectType.Debuff:
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
			case AbilityTargetType.AllAllies:
				return false;
			default:
				return false;
		}

		return true;
	}

	public void PerformAbility1(Unit target)
	{
		UseAbility(_abilities[0], target);
	}
}