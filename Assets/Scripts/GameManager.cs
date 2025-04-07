using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Unity.VisualScripting;
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
	public List<Unit> allUnits = new List<Unit>();


	public List<Unit> playerUnits = new List<Unit>();
	public List<Unit> enemyUnits = new List<Unit>();

	[SerializeField]
	private GameObject skillPanel;

	[SerializeField]
	private List<ButtonHandler> _buttonHandlers = new List<ButtonHandler>();


	private List<Unit> turnOrder = new List<Unit>();

	public int currentUnitIndex = 0;

	// private bool isPlayerTurnActive = false;

	public int roundCounter = 0;
	public int turnCounter = 0;

	public bool combatStarted = false;
	public bool combatEnded = false;

	private Unit currentUnit;

	[SerializeField]
	private List<Wave> _waves;

	public Transform waveSpawningLocation;
	public float unitSpacing = 2.5f;
	public float yOffset = 0.25f;

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
		StartCoroutine(StartCombat());
	}

	IEnumerator StartCombat()
	{
		yield return new WaitForSeconds(1f);
		if (roundCounter < _waves.Count)
		{
			combatEnded = false;
			SpawnEnemies();
			OrganizeUnits();
			CalculateInitiative();
			StartCoroutine(HandleTurnLoop());
		}
		else
			Debug.Log("No more rounds!");
	}


	private void Update()
	{
		// CheckEndRound();
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
		yield return new WaitForSeconds(2f);

		while (!combatEnded)
		{
			if (CheckEndRound())
				break;

			currentUnit = turnOrder[currentUnitIndex];
			if (currentUnit.isAlive())
			{
				Debug.Log($"It's {currentUnit.unitName}'s turn!");
				foreach (Unit unit in allUnits)
				{
					unit.SetHighlight(false);
				}

				turnCounter++;
				currentUnit.SetHighlight(true);
				currentUnit.ProcessEffectsPerTurn(EffectTiming.StartTurn, turnCounter);

				yield return new WaitForSeconds(1f);

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

			currentUnit.ProcessEffectsPerTurn(EffectTiming.EndTurn, turnCounter);

			currentUnitIndex++;
			if (currentUnitIndex >= turnOrder.Count)
			{
				currentUnitIndex = 0;
				if (CheckEndRound())
					break;
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
		if (!playerUnit.HasStunDebuff())
		{
			Debug.Log($"{currentUnit.unitName} is choosing an action...");

			playerUnit.StartTurn();

			TargetSelectionUI.Instance.isSelecting = true;

			while (TargetSelectionUI.Instance.isSelecting)
			{
				yield return null;
			}
		}
		else
		{
			playerUnit.ProcessEffectsPerTurn(EffectTiming.SkipAction, turnCounter);
			Debug.Log($"{currentUnit.unitName} is stunned");
		}

		HandlePanel(false);
		currentUnit.SetHighlight(false);

		yield return new WaitForSeconds(1.0f); // Simulate the attack animation/delay
	}

	public PlayerUnit GetCurrentPlayerUnit()
	{
		// However you track the active unit in turn order
		return currentUnit as PlayerUnit;
	}

	IEnumerator EnemyTurn(EnemyUnit enemyUnit)
	{
		yield return new WaitForSeconds(1.0f);
		if (!enemyUnit.HasStunDebuff())
		{
			enemyUnit.PerformIntent();
			currentUnit.SetHighlight(false);
		}
		else
		{
			enemyUnit.ProcessEffectsPerTurn(EffectTiming.SkipAction, turnCounter);
			Debug.Log($"{enemyUnit.unitName} is stunned");
		}

		yield return new WaitForSeconds(1.0f); // Simulate the attack animation/delay
	}

	public void ActionChosen(string action)
	{
		// isPlayerTurnActive = false;

		// Based on the player's action choice, you can handle the action here
		if (action == "Attack")
		{
			Debug.Log("Unit chose to attack!");
			// Perform attack logic here, like damage calculation, etc.
		}
		else if (action == "Defend")
		{
			Debug.Log("Unit chose to defend!");
			// Perform defense logic here
		}
		else if (action == "Use Item")
		{
			Debug.Log("Unit chose to use an item!");
			// Perform item usage logic here
		}
		else if (action == "Miss")
		{
			Debug.Log("Unit missed!");
			// Perform item usage logic here
		}
	}

	private void SpawnEnemies()
	{
		int count = _waves[roundCounter].waveUnits.Count;
		Vector2 center = waveSpawningLocation.position;
		float startX = center.x - (unitSpacing * (count - 1) / 2);

		for (int i = 0; i < count; i++)
		{
			float x = startX + (i * unitSpacing);
			int randomSign = (Random.value < 0.5f) ? -1 : 1;
			float y = center.y + (i * yOffset * randomSign);

			Vector2 spawnPos = new Vector2(x, y);

			GameObject newEnemy = Instantiate(_waves[roundCounter].waveUnits[i], spawnPos, Quaternion.identity);
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
					unit.hasComittedStance = false;
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

		if ((alivePlayers <= 0 || aliveEnemies <= 0) && combatStarted)
		{
			combatEnded = true;
			roundCounter++;
			currentUnitIndex = 0;
			HandlePanel(false);
			StopAllCoroutines();
			ResetCombat();
			StartCoroutine(ShowEndFightScreen(aliveEnemies == 0 ? "Player" : "Enemies"));
			return true;
		}

		return false;
	}

	public void ResetCombat()
	{
		foreach (Unit unit in playerUnits)
		{
			unit.Heal(100);
			unit.activeEffects.Clear();
			unit.abilityCooldowns.Clear();
			unit.RefreshStatusIcons();
			unit.RefreshCoatingUI(false);
		}

		playerUnits.Clear();
		enemyUnits.Clear();
	}

	IEnumerator ShowEndFightScreen(string winner)
	{
		Debug.Log(winner);
		yield return new WaitForSeconds(3f);
		if (winner == "Player")
			StartCoroutine(StartCombat());
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

		if (ability.hitsSelf)
			validTargets.Add(currentUnit);

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


		if (ability.hitsSelf)
			validTargets.Add(currentUnit);

		return validTargets;
	}

	public void HandlePanel(bool change)
	{
		List<Ability> playerAbilities = currentUnit.GetAbilities();
		Dictionary<Ability, int> cooldowns = currentUnit.abilityCooldowns;

		if (change)
		{
			skillPanel.SetActive(true);

			int count = Math.Min(_buttonHandlers.Count, playerAbilities.Count);
			for (int i = 0; i < count; i++)
			{
				if (cooldowns.ContainsKey(playerAbilities[i]))
					_buttonHandlers[i].SetAbility(playerAbilities[i], cooldowns[playerAbilities[i]]);
				else
					_buttonHandlers[i].SetAbility(playerAbilities[i], 0);
			}
		}
		else
		{
			foreach (ButtonHandler buttonHandler in _buttonHandlers)
			{
				buttonHandler.ResetAbility();
			}

			skillPanel.SetActive(false);
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

	public void TrySwitchStance()
	{
		if (currentUnit.hasComittedStance)
		{
			Debug.Log("Stance already committed. Cannot preview/change anymore.");
			return;
		}

		int stanceCount = Enum.GetValues(typeof(StanceType)).Length;
		currentUnit.previewStance = (StanceType)(((int)currentUnit.previewStance + 1) % stanceCount);

		Debug.Log($"{currentUnit.unitName} switched to {currentUnit.previewStance} stance!");
	}
}