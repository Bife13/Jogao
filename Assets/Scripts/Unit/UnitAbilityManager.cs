using System.Collections.Generic;
using UnityEngine;

public class UnitAbilityManager : MonoBehaviour
{
	private Unit unit;

	[HideInInspector]
	public List<Ability> _abilities = new List<Ability>();

	public Dictionary<Ability, int> abilityCooldowns = new Dictionary<Ability, int>();

	public void Initialize(Unit owner)
	{
		unit = owner;
	}

	public void UseAbility(Ability ability, List<Unit> targets)
	{
		unit.BeforeAbility();
		unit.PerformAttackAnimation();
		
		foreach (var fx in ability.modules)
			fx.BeforeExecute(unit);

		foreach (Unit target in targets)
		{
			Debug.Log($"{gameObject.name} uses {ability.abilityName} on {target.unitName}!");
			if (!isTargetValid(ability, target))
			{
				Debug.LogWarning($"Invalid target for ability {ability.abilityName}");
				return;
			}

			foreach (var fx in ability.modules)
			{
				bool lastHit = fx.Execute(unit, target);
				if (!lastHit)
					return;
			}
		}

		foreach (var fx in ability.modules)
			fx.AfterExecute(unit, ability);

		ApplyAbilityCooldown(ability);
	}

	private void ApplyAbilityCooldown(Ability ability)
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
				case TargetType.Enemy:
					if (target.unitType != UnitType.ENEMY)
						return false;
					break;
				case TargetType.Ally:
					if (target.unitType != UnitType.PLAYER)
						return false;
					break;
				case TargetType.Self:
					if (target != this.GetComponent<Unit>())
						return false;
					break;
				case TargetType.AllEnemies:
					if (target.unitType != UnitType.ENEMY)
						return false;
					break;
				case TargetType.AllAllies:
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
				case TargetType.Enemy:
					if (target.unitType != UnitType.PLAYER)
						return false;
					break;
				case TargetType.Ally:
					if (target.unitType != UnitType.ENEMY)
						return false;
					break;
				case TargetType.Self:
					if (target != this.GetComponent<Unit>())
						return false;
					break;
				case TargetType.AllEnemies:
					if (target.unitType != UnitType.PLAYER)
						return false;
					break;
				case TargetType.AllAllies:
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