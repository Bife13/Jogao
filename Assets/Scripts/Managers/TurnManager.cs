using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class TurnManager : MonoBehaviour
{
	public GameManager gameManager;
	public UnitManager unitManager;
	
	public List<Unit> turnOrder = new List<Unit>();

	public int roundCounter = 0;
	public int turnCounter = 0;

	public void Initialize()
	{
		gameManager = GameManager.Instance;
		unitManager = gameManager.unitManager;
	}
	
	public void CalculateInitiative()
	{
		int randomMax = unitManager.allUnits.Count;

		List<(Unit unit, int initiativeRoll)> unitInitiatives = new List<(Unit, int)>();

		foreach (Unit unit in unitManager.allUnits)
		{
			int randomRoll = Random.Range(1, 6);
			int initiative = (int)unit.unitStatCalculator.GetTotalModifiedStat(StatType.Speed) + randomRoll;

			unitInitiatives.Add((unit, initiative));

			Debug.Log(
				$"{unit.unitName} has speed {(int)unit.unitStatCalculator.GetTotalModifiedStat(StatType.Speed)} + roll {randomRoll} = initiative {initiative}");
		}

		unitInitiatives.Sort((a, b) => b.initiativeRoll.CompareTo(a.initiativeRoll));

		turnOrder = unitInitiatives.Select(t => t.unit).ToList();

		Debug.Log("Turn Order:");
		foreach (Unit u in turnOrder)
		{
			Debug.Log(u.unitName);
		}
	}
}