using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class TurnManager : MonoBehaviour
{
	private static TurnManager _instance;

	public static TurnManager Instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = FindFirstObjectByType<TurnManager>();
				if (_instance == null)
				{
					GameObject singletonObject = new GameObject("CombatManager");
					_instance = singletonObject.AddComponent<TurnManager>();
				}
			}

			return _instance;
		}
	}

	[SerializeField]
	private List<Unit> allUnits = new List<Unit>();

	[SerializeField]
	private List<Unit> playerUnits = new List<Unit>();

	private List<Unit> turnOrder = new List<Unit>();

	private int currentUnitIndex = 0;

	private bool isPlayerTurnActive = false;

	private void Awake()
	{
		if (_instance == null)
		{
			_instance = this;
			DontDestroyOnLoad(gameObject); // Optionally preserve this object between scenes
		}
		else
		{
			Destroy(gameObject); // If a duplicate exists, destroy this instance
		}
	}

	private void Start()
	{
		StartCombat();
	}

	void StartCombat()
	{
		NewRound();
		StartCoroutine(HandleTurnLoop());
	}

	void NewRound()
	{
		int randomMax = allUnits.Count;

		List<(Unit unit, int initiativeRoll)> unitInitiatives = new List<(Unit, int)>();

		foreach (Unit unit in allUnits)
		{
			int randomRoll = Random.Range(1, randomMax + 1);
			int initiative = unit.speed + randomRoll;

			unitInitiatives.Add((unit, initiative));

			Debug.Log($"{unit.unitName} has speed {unit.speed} + roll {randomRoll} = initiative {initiative}");
		}

		unitInitiatives.Sort((a, b) => b.initiativeRoll.CompareTo(a.initiativeRoll));

		turnOrder = unitInitiatives.Select(t => t.unit).ToList();

		Debug.Log("Turn Order:");
		foreach (Unit u in turnOrder)
		{
			Debug.Log(u.unitName);
		}
	}

	IEnumerator HandleTurnLoop()
	{
		yield return new WaitForSeconds(1f);

		while (true)
		{
			// if (turnOrder.Count(u => u.isAlive) == 0)
			// {
			// 	Debug.Log("Combat Ended!");
			// 	yield break;
			// }

			Unit currentUnit = turnOrder[currentUnitIndex];
			if (currentUnit.isAlive())
			{
				Debug.Log($"It's {currentUnit.unitName}'s turn!");

				switch (currentUnit.unitType)
				{
					case UnitType.PLAYER:
						yield return StartCoroutine(PlayerTurn((PlayerUnit)currentUnit));
						break;
					case UnitType.ENEMY:
						yield return StartCoroutine(EnemyTurn((EnemyUnit)currentUnit));
						break;
				}
			}

			currentUnitIndex++;
			if (currentUnitIndex >= turnOrder.Count)
				currentUnitIndex = 0;

			yield return new WaitForSeconds(1f);
		}
	}

	IEnumerator PlayerTurn(PlayerUnit playerUnit)
	{
		Debug.Log($"{turnOrder[currentUnitIndex].unitName} is choosing an action...");

		isPlayerTurnActive = true;

		while (isPlayerTurnActive)
		{
			yield return null;
		}

		yield return new WaitForSeconds(1.0f); // Simulate the attack animation/delay
	}

	IEnumerator EnemyTurn(EnemyUnit enemyUnit)
	{
		Unit target = playerUnits[Random.Range(0, playerUnits.Count)];
		enemyUnit.PerformIntent(target);
		yield return new WaitForSeconds(1.0f); // Simulate the attack animation/delay
	}

	public void OnPlayerActionChosen(string action)
	{
		isPlayerTurnActive = false;

		// Based on the player's action choice, you can handle the action here
		if (action == "Attack")
		{
			Debug.Log("Player chose to attack!");
			// Perform attack logic here, like damage calculation, etc.
		}
		else if (action == "Defend")
		{
			Debug.Log("Player chose to defend!");
			// Perform defense logic here
		}
		else if (action == "Use Item")
		{
			Debug.Log("Player chose to use an item!");
			// Perform item usage logic here
		}
	}
}