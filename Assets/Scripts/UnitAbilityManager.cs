using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class UnitAbilityManager : MonoBehaviour
{
	private Unit unit;
	private UnitCombatCalculator unitCombatCalculator;
	private UnitEffects unitEffects;
	[HideInInspector]
	public List<Ability> _abilities = new List<Ability>();
	public Dictionary<Ability, int> abilityCooldowns = new Dictionary<Ability, int>();

	private void Awake()
	{
		unit = GetComponent<Unit>();
		unitCombatCalculator = GetComponent<UnitCombatCalculator>();
		unitEffects = GetComponent<UnitEffects>();
	}

	public virtual void UseAbility(Ability ability, List<Unit> targets)
	{
		unit.BeforeAbility();

		unit.PerformAttackAnimation();

		bool hit = false;

		foreach (Unit target in targets)
		{
			Debug.Log($"{gameObject.name} uses {ability.abilityName} on {target.unitName}!");
			if (!isTargetValid(ability, target))
			{
				Debug.LogWarning($"Invalid target for ability {ability.abilityName}");
				return;
			}

			unitEffects.HandleWeaponCoating(ability);

			switch (ability.abilityEffectType)
			{
				case AbilityEffectType.Damage:

					float damage = unitCombatCalculator.CalculateDamage(ability, target);
					hit = unitCombatCalculator.ApplyDamageOrMiss(ability, target, damage);
					break;

				case AbilityEffectType.Heal:
					unitCombatCalculator.ApplyHealing(ability, target);
					break;
				case AbilityEffectType.Buff:
				case AbilityEffectType.Debuff:
				case AbilityEffectType.StatusEffect:
					hit = unitEffects.PerformEffectApplication(ability, target);
					break;
			}

			if (ability.canCleanse)
			{
				unitEffects.CleanseTarget(target, ability);
			}

			if (ability.canSwap)
			{
				foreach (int index in ability.abilityIndexes)
				{
					SwapAbilities(index, 4 + index);
				}
			}
		}

		ApplyAbilityCooldown(ability);
		unit.AfterAbilityUse(ability, hit);
	}

	protected virtual void ApplyAbilityCooldown(Ability ability)
	{
		abilityCooldowns[ability] = ability.cooldown;
	}

	public bool isTargetValid(Ability ability, Unit target)
	{
		if (target == null)
		{
			Debug.LogWarning("No target selected!");
			return false;
		}

		if (!target.unitHealth.isAlive())
		{
			Debug.LogWarning($"{target.unitName} is already dead!");
			return false;
		}

		if (unit is PlayerUnit)
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
		else if (unit is EnemyUnit)
		{
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
						if (!ability.hitsSelf)
							return false;
					break;
				case AbilityTargetType.AllAllies:
					if (target.unitType != UnitType.ENEMY)
						return false;
					break;
				default:
					return false;
			}
		}

		return true;
	}

	public bool CanUseAbility(Ability ability)
	{
		return !abilityCooldowns.ContainsKey(ability);
	}

	public List<Ability> GetAbilities()
	{
		return _abilities;
	}

	public void SwapAbilities(int firstIndex, int secondIndex)
	{
		(_abilities[firstIndex], _abilities[secondIndex]) = (_abilities[secondIndex], _abilities[firstIndex]);
	}

	public void StartTurn()
	{
		List<Ability> keys = new List<Ability>(abilityCooldowns.Keys);
		foreach (var ability in keys)
		{
			abilityCooldowns[ability]--;
			if (abilityCooldowns[ability] <= 0)
				abilityCooldowns.Remove(ability);
		}
	}
}