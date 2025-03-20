using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
	private static GameManager _instance;

	public static GameManager Instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = FindFirstObjectByType<GameManager>();
				if (_instance == null)
				{
					GameObject singletonObject = new GameObject("CombatManager");
					_instance = singletonObject.AddComponent<GameManager>();
				}
			}

			return _instance;
		}
	}

	[SerializeField]
	private List<Unit> allUnits = new List<Unit>();


	private List<Unit> playerUnits = new List<Unit>();
	private List<Unit> enemyUnits = new List<Unit>();

	[SerializeField]
	private GameObject skillPanel;

	[SerializeField]
	private List<ButtonHandler> _buttonHandlers = new List<ButtonHandler>();


	private List<Unit> turnOrder = new List<Unit>();

	private int currentUnitIndex = 0;

	// private bool isPlayerTurnActive = false;

	public int roundCounter = 1;

	public bool combatEnded = false;

	private Unit currentUnit;


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
		OrganizeUnits();
		CalculateInitiative();
		StartCoroutine(HandleTurnLoop());
	}

	private void Update()
	{
		CheckEndRound();
	}

	void CalculateInitiative()
	{
		int randomMax = allUnits.Count;

		List<(Unit unit, int initiativeRoll)> unitInitiatives = new List<(Unit, int)>();

		foreach (Unit unit in allUnits)
		{
			int randomRoll = Random.Range(1, randomMax + 1);
			int initiative = unit.ModifiedSpeed + randomRoll;

			unitInitiatives.Add((unit, initiative));

			Debug.Log($"{unit.unitName} has speed {unit.ModifiedSpeed} + roll {randomRoll} = initiative {initiative}");
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

		while (!combatEnded)
		{
			if (CheckEndRound())
			{
				Debug.Log("Combat Ended!");
				combatEnded = true;
				yield break;
			}

			currentUnit = turnOrder[currentUnitIndex];
			if (currentUnit.isAlive())
			{
				Debug.Log($"It's {currentUnit.unitName}'s turn!");
				currentUnit.ProcessEffectsPerTurn(EffectTiming.StartTurn);

				yield return new WaitForSeconds(0.5f);

				switch (currentUnit.unitType)
				{
					case UnitType.PLAYER:
						HandlePanel(true);
						yield return StartCoroutine(PlayerTurn((PlayerUnit)currentUnit));
						break;
					case UnitType.ENEMY:
						yield return StartCoroutine(EnemyTurn((EnemyUnit)currentUnit));
						break;
				}
			}

			currentUnit.ProcessEffectsPerTurn(EffectTiming.EndTurn);

			currentUnitIndex++;
			if (currentUnitIndex >= turnOrder.Count)
			{
				currentUnitIndex = 0;
				roundCounter++;
				foreach (EnemyUnit unit in enemyUnits)
				{
					unit.DecideNextIntent();
				}

				Debug.Log("NEW TURN");
				yield return new WaitForSeconds(2f);
				CalculateInitiative();
			}

			yield return new WaitForSeconds(1f);
		}
	}

	IEnumerator PlayerTurn(PlayerUnit playerUnit)
	{
		Debug.Log($"{currentUnit.unitName} is choosing an action...");

		TargetSelectionUI.Instance.isSelecting = true;

		while (TargetSelectionUI.Instance.isSelecting)
		{
			yield return null;
		}

		HandlePanel(false);

		yield return new WaitForSeconds(1.0f); // Simulate the attack animation/delay
	}

	public PlayerUnit GetCurrentPlayerUnit()
	{
		// However you track the active unit in turn order
		return currentUnit as PlayerUnit;
	}

	IEnumerator EnemyTurn(EnemyUnit enemyUnit)
	{
		enemyUnit.PerformIntent();
		yield return new WaitForSeconds(1.0f); // Simulate the attack animation/delay
	}

	public void OnPlayerActionChosen(string action)
	{
		// isPlayerTurnActive = false;

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
		else if (action == "Miss")
		{
			Debug.Log("Player missed!");
			// Perform item usage logic here
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
					break;
				case UnitType.ENEMY:
					enemyUnits.Add(unit);
					break;
			}
		}
	}

	public bool CheckEndRound()
	{
		int alivePlayers = playerUnits.Count(u => u.isAlive());
		int aliveEnemies = enemyUnits.Count(u => u.isAlive());

		if (alivePlayers <= 0 || aliveEnemies <= 0)
		{
			combatEnded = true;
			Debug.Log("Combat Ended!");
			StopAllCoroutines();
			return true;
		}

		return false;
	}

	public List<Unit> GetValidTargets(Ability ability)
	{
		List<Unit> validTargets = new List<Unit>();

		switch (ability.targetType)
		{
			case AbilityTargetType.Enemy:
				validTargets = enemyUnits.FindAll(u => u.isAlive());
				break;
			case AbilityTargetType.Ally:
				validTargets = playerUnits.FindAll(u => u.isAlive());
				break;
			case AbilityTargetType.AllEnemies:
				validTargets = enemyUnits.FindAll(u => u.isAlive());
				break;
			case AbilityTargetType.AllAllies:
				validTargets = playerUnits.FindAll(u => u.isAlive());
				break;
			case AbilityTargetType.Self:
				validTargets.Add(currentUnit);
				break;

			default:
				Debug.LogWarning("Unknown AbilityTargetType!");
				break;
		}

		return validTargets;
	}

	public List<Unit> GetValidTargetsForEnemy(Ability ability)
	{
		List<Unit> validTargets = new List<Unit>();

		switch (ability.targetType)
		{
			case AbilityTargetType.Enemy:
				validTargets = playerUnits.FindAll(u => u.isAlive());
				if (validTargets.Count > 0)
				{
					Unit randomTarget = validTargets[Random.Range(0, validTargets.Count)];
					validTargets.Clear();
					validTargets.Add(randomTarget);
				}

				break;
			case AbilityTargetType.Ally:
				validTargets = enemyUnits.FindAll(u => u.isAlive());
				if (validTargets.Count > 0)
				{
					Unit randomTarget = validTargets[Random.Range(0, validTargets.Count)];
					validTargets.Clear();
					validTargets.Add(randomTarget);
				}

				break;
			case AbilityTargetType.AllEnemies:
				validTargets = playerUnits.FindAll(u => u.isAlive());
				break;
			case AbilityTargetType.AllAllies:
				validTargets = enemyUnits.FindAll(u => u.isAlive());
				break;
			case AbilityTargetType.Self:
				validTargets.Add(currentUnit);
				break;

			default:
				Debug.LogWarning("Unknown AbilityTargetType!");
				break;
		}

		return validTargets;
	}

	public void HandlePanel(bool change)
	{
		List<Ability> playerAbilities = currentUnit.GetAbilities();
		if (change)
		{
			skillPanel.SetActive(true);
			for (int i = 0; i < playerAbilities.Count; i++)
			{
				_buttonHandlers[i].assignedAbility = playerAbilities[i];
			}
		}
		else
		{
			foreach (ButtonHandler buttonHandler in _buttonHandlers)
			{
				buttonHandler.assignedAbility = null;
			}

			skillPanel.SetActive(false);
		}
	}
}