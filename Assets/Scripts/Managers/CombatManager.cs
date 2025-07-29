using System.Collections;
using System.Linq;
using UnityEngine;

public class CombatManager : MonoBehaviour
{
	public GameManager gameManager;
	private UnitManager unitManager;
	public bool inCombat = false;

	public void Initialize()
	{
		gameManager = GameManager.Instance;
		unitManager = gameManager.unitManager;
		StartCoroutine(StartCombat());
	}

	IEnumerator StartCombat()
	{
		yield return new WaitForSeconds(1f);
		if (gameManager.turnManager.roundCounter < unitManager._waves.Count)
		{
			GameManager.Instance.combatUIManager.AddLog($"Round {GameManager.Instance.turnManager.roundCounter + 1}");
			inCombat = true;
			unitManager.SpawnEnemies();
			unitManager.OrganizeUnits();
			yield return new WaitForSeconds(0.5f);
			gameManager.turnManager.CalculatePace();
			StartCoroutine(HandleTurnLoop());
			// GameManager.Instance.combatUIManager.AddLog("<color=red>Combat Start!</color>");
		}
		else
		{
			Debug.Log("No more rounds!");
			GameManager.Instance.combatUIManager.AddLog("No more rounds!");
		}
	}

	IEnumerator HandleTurnLoop()
	{
		yield return new WaitForSeconds(2f);

		while (inCombat)
		{
			if (CheckEndRound())
				break;


			Unit currentUnit = unitManager.currentUnit =
				gameManager.turnManager.turnOrder[unitManager.currentUnitIndex];
			if (currentUnit.unitHealth.isAlive())
			{
				GameManager.Instance.combatUIManager.AddLog(
					($"<color=yellow>It's {currentUnit.unitName}'s turn!</color>"));
				foreach (Unit unit in unitManager.allUnits)
				{
					unit.unitUI.SetHighlight(false);
				}

				gameManager.turnManager.turnCounter++;
				currentUnit.unitUI.SetHighlight(true);
				currentUnit.unitConditions.ProcessConditionsPerTurn(ConditionTiming.StartTurn,
					gameManager.turnManager.turnCounter);

				yield return new WaitForSeconds(1f);

				switch (currentUnit.unitType)
				{
					case UnitType.PLAYER:
						gameManager.combatUIManager.HandlePanel(true);
						yield return StartCoroutine(PlayerTurn((PlayerUnit)currentUnit));
						break;
					case UnitType.ENEMY:
						yield return StartCoroutine(EnemyTurn((EnemyUnit)currentUnit));
						break;
				}
			}

			currentUnit.unitConditions.ProcessConditionsPerTurn(ConditionTiming.EndTurn,
				gameManager.turnManager.turnCounter);
			yield return new WaitForSeconds(0.5f);
			CheckEndRound();
			unitManager.currentUnitIndex++;
			if (unitManager.currentUnitIndex >= gameManager.turnManager.turnOrder.Count)
			{
				unitManager.currentUnitIndex = 0;
				if (CheckEndRound())
					break;
				foreach (EnemyUnit unit in unitManager.enemyUnits)
				{
					unit.DecideNextIntent();
				}

				Debug.Log("NEW TURN");

				yield return new WaitForSeconds(2f);
				gameManager.turnManager.CalculatePace();
			}

			yield return new WaitForSeconds(1f);
		}
	}

	IEnumerator PlayerTurn(PlayerUnit playerUnit)
	{
		if (!playerUnit.unitConditions.HasStunDebuff())
		{
			Debug.Log($"{unitManager.currentUnit.unitName} is choosing an action...");
			GameManager.Instance.combatUIManager.AddLog($"{unitManager.currentUnit.unitName} is choosing an action...");

			playerUnit.unitAbilityManager.StartTurn();

			playerUnit.unitInventory.HandleItemTriggers(ItemTriggerType.OnTurnStart, playerUnit);

			TargetSelectionUI.Instance.isSelecting = true;

			while (TargetSelectionUI.Instance.isSelecting)
			{
				yield return null;
			}
		}
		else
		{
			playerUnit.unitConditions.ProcessConditionsPerTurn(ConditionTiming.SkipAction,
				gameManager.turnManager.turnCounter);
			Debug.Log($"{unitManager.currentUnit.unitName} is stunned");
			GameManager.Instance.combatUIManager.AddLog($"{unitManager.currentUnit.unitName} is stunned");
		}

		gameManager.combatUIManager.HandlePanel(false);
		unitManager.currentUnit.unitUI.SetHighlight(false);

		yield return new WaitForSeconds(1.0f); // Simulate the attack animation/delay

		playerUnit.unitInventory.HandleItemTriggers(ItemTriggerType.OnTurnEnd, playerUnit);
	}

	IEnumerator EnemyTurn(EnemyUnit enemyUnit)
	{
		yield return new WaitForSeconds(1.0f);
		if (enemyUnit != null)
			if (!enemyUnit.unitConditions.HasStunDebuff())
			{
				// GameManager.Instance.combatUIManager.AddLog($"{enemyUnit.unitName} attacked!");
				enemyUnit.PerformIntent();
				unitManager.currentUnit.unitUI.SetHighlight(false);

			}
			else
			{
				GameManager.Instance.combatUIManager.AddLog($"{enemyUnit.unitName} is stunned");
				Debug.Log($"{enemyUnit.unitName} is stunned");
				enemyUnit.unitConditions.ProcessConditionsPerTurn(ConditionTiming.SkipAction,
					gameManager.turnManager.turnCounter);
			}

		yield return new WaitForSeconds(1.0f); // Simulate the attack animation/delay
	}

	public bool CheckEndRound()
	{
		int alivePlayers = unitManager.playerUnits.Count(u => u.unitHealth.isAlive());
		int aliveEnemies = unitManager.enemyUnits.Count(u => u.unitHealth.isAlive());

		Debug.Log("Players:" + alivePlayers);
		Debug.Log("Enemies:" + aliveEnemies);

		if ((alivePlayers <= 1 || aliveEnemies <= 0) && inCombat)
		{
			inCombat = false;
			gameManager.turnManager.roundCounter++;
			unitManager.currentUnitIndex = 0;
			gameManager.combatUIManager.HandlePanel(false);
			StopAllCoroutines();
			ResetCombat();
			StartCoroutine(ShowEndFightScreen(aliveEnemies == 0 ? "Player" : "Enemies"));
			return true;
		}

		return false;
	}

	public void ResetCombat()
	{
		foreach (Unit unit in unitManager.playerUnits)
		{
			unit.unitHealth.Heal(100);
			unit.unitConditions.activeConditions.Clear();
			unit.unitConditions.activeCoating = null;
			unit.unitConditions.coatingDuration = 0;
			unit.unitAbilityManager.abilityCooldowns.Clear();
			unit.unitUI.RefreshStatusIcons();
			unit.unitUI.RefreshCoatingUI(false);
		}

		unitManager.playerUnits.Clear();
		unitManager.enemyUnits.Clear();
	}

	IEnumerator ShowEndFightScreen(string winner)
	{
		// NEED TO WORK ON THE ITEM SELECTION SCREEN
		Debug.Log(winner);
		yield return new WaitForSeconds(3f);
		if (winner == "Player" && gameManager.turnManager.roundCounter < unitManager._waves.Count - 1)
			GameManager.Instance.itemSelection.ActivateItemSelection();
	}

	public void ContinueCombat()
	{
		StartCoroutine(StartCombat());
	}
}