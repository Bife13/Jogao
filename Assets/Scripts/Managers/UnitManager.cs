using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class UnitManager : MonoBehaviour
{
	[SerializeField]
	public List<Unit> allUnits = new List<Unit>();

	public List<Unit> playerUnits = new List<Unit>();
	public List<Unit> enemyUnits = new List<Unit>();
	public List<Wave> _waves;

	public Unit currentUnit;

	public int currentUnitIndex = 0;


	public Transform waveSpawningLocation;
	public float unitSpacing = 2.5f;
	public float yOffset = 0.25f;

	public void SpawnEnemies()
	{
		int count = _waves[GameManager.Instance.turnManager.roundCounter].waveUnits.Count;
		Vector2 center = waveSpawningLocation.position;
		float startX = center.x - (unitSpacing * (count - 1) / 2);

		for (int i = 0; i < count; i++)
		{
			float x = startX + (i * unitSpacing);
			int randomSign = (Random.value < 0.5f) ? -1 : 1;
			float y = center.y + (i * yOffset * randomSign);

			Vector2 spawnPos = new Vector2(x, y);

			GameObject newEnemy = Instantiate(_waves[GameManager.Instance.turnManager.roundCounter].waveUnits[i],
				spawnPos, Quaternion.identity);
			allUnits.Add(newEnemy.GetComponent<Unit>());
		}
	}


	public void OrganizeUnits()
	{
		foreach (Unit unit in allUnits)
		{
			switch (unit.unitType)
			{
				case UnitType.PLAYER:
					playerUnits.Add(unit);
					unit.unitStance.hasComittedStance = false;
					break;
				case UnitType.ENEMY:
					enemyUnits.Add(unit);
					break;
			}
		}
	}

	public void RemoveUnit(Unit unit)
	{
		switch (unit)
		{
			case PlayerUnit:
				allUnits.Remove(unit);
				playerUnits.Remove(unit);
				break;
			case EnemyUnit:
				allUnits.Remove(unit);
				enemyUnits.Remove(unit);
				break;
		}
	}

	public PlayerUnit GetCurrentPlayerUnit()
	{
		// However you track the active unit in turn order
		return currentUnit as PlayerUnit;
	}
	
	public List<Unit> GetValidTargets(Ability ability)
	{
		List<Unit> validTargets = new List<Unit>();

		switch (ability.targetType)
		{
			case TargetType.Enemy:
				validTargets = enemyUnits.FindAll(u => u.unitHealth.isAlive());
				break;
			case TargetType.Ally:
				validTargets = playerUnits.FindAll(u => u.unitHealth.isAlive());
				break;
			case TargetType.AllEnemies:
				validTargets = enemyUnits.FindAll(u => u.unitHealth.isAlive());
				break;
			case TargetType.AllAllies:
				validTargets = playerUnits.FindAll(u => u.unitHealth.isAlive());
				break;
			case TargetType.Self:
				validTargets.Add(currentUnit);
				break;

			default:
				Debug.LogWarning("Unknown AbilityTargetType!");
				break;
		}

		if (ability.hitsSelf)
			validTargets.Add(currentUnit);

		return validTargets;
	}

	public List<Unit> GetValidTargetsForEnemy(Ability ability)
	{
		List<Unit> validTargets = new List<Unit>();

		switch (ability.targetType)
		{
			case TargetType.Enemy:
				validTargets = playerUnits.FindAll(u => u.unitHealth.isAlive());
				if (validTargets.Count > 0)
				{
					Unit randomTarget = validTargets[Random.Range(0, validTargets.Count)];
					validTargets.Clear();
					validTargets.Add(randomTarget);
				}

				break;
			case TargetType.Ally:
				validTargets = enemyUnits.FindAll(u => u.unitHealth.isAlive());
				if (validTargets.Count > 0)
				{
					Unit randomTarget = validTargets[Random.Range(0, validTargets.Count)];
					validTargets.Clear();
					validTargets.Add(randomTarget);
				}

				break;
			case TargetType.AllEnemies:
				validTargets = playerUnits.FindAll(u => u.unitHealth.isAlive());
				break;
			case TargetType.AllAllies:
				validTargets = enemyUnits.FindAll(u => u.unitHealth.isAlive());
				break;
			case TargetType.Self:
				validTargets.Add(currentUnit);
				break;

			default:
				Debug.LogWarning("Unknown AbilityTargetType!");
				break;
		}


		if (ability.hitsSelf)
			validTargets.Add(currentUnit);

		return validTargets;
	}
}