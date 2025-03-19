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
		nextAbility = _abilities[Random.Range(0, _abilities.Count)];
		intentImage.sprite = nextAbility.icon;
	}

	public void UseAbility(Ability ability, Unit target)
	{
		Debug.Log($"{gameObject.name} uses {ability.abilityName} on {target.unitName}!");
		if (!isTargetValid(ability, target))
		{
			Debug.LogWarning($"Invalid target for ability {ability.abilityName}");
			return;
		}

		switch (ability.effectType)
		{
			case AbilityEffectType.Damage:
				float damage = Random.Range(minDamage, maxDamage + 1);
				damage += damage * ability.minPower;
				AttackAnimation(-1);
				if (PerformAccuracyDodgeCheck(ability.accuracy, target))
				{
					if (PerformCriticalHitCheck(critChance))
						damage = ability.minPower * (1.5f * maxDamage);

					target.TakeDamage(Mathf.RoundToInt(damage));
				}
				else
				{
					Debug.LogWarning("ERROU");
					// DO MISS LOGIC HERE EVENTUALLY
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
			case AbilityTargetType.AllAllies:
				return false;
			default:
				return false;
		}

		return true;
	}

	public void PerformIntent()
	{
		List<Unit> targets = TurnManager.Instance.GetValidTargetsForEnemy(nextAbility);
		UseAbility(nextAbility, targets[Random.Range(0, targets.Count)]);
	}
}