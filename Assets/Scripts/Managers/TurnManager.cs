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

	public void CalculatePace()
	{
		int randomMax = unitManager.allUnits.Count;

		List<(Unit unit, int paceRoll)> unitPaces = new List<(Unit, int)>();

		foreach (Unit unit in unitManager.allUnits)
		{
			int randomRoll = Random.Range(1, 6);
			int pace = (int)unit.unitStatCalculator.GetTotalModifiedStat(StatType.Speed) + randomRoll;

			unitPaces.Add((unit, pace));

			Debug.Log(
				$"{unit.unitName} has speed {(int)unit.unitStatCalculator.GetTotalModifiedStat(StatType.Speed)} + roll {randomRoll} = pace {pace}");
		}

		unitPaces.Sort((a, b) => b.paceRoll.CompareTo(a.paceRoll));
		turnOrder = unitPaces.Select(t => t.unit).ToList();

		string logString = null;

		for (int i = 0; i < unitPaces.Count; i++)
		{
			logString += $"{unitPaces[i].unit.unitName}";
			if (i < unitPaces.Count - 1)
				logString += " | ";
		}

		GameManager.Instance.combatUIManager.AddLog($"{logString}");
	}
}