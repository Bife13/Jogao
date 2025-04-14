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
			inCombat = true;
			unitManager.SpawnEnemies();
			unitManager.OrganizeUnits();
			yield return new WaitForSeconds(0.5f);
			gameManager.turnManager.CalculateInitiative();
			StartCoroutine(HandleTurnLoop());
		}
		else
			Debug.Log("No more rounds!");
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
				Debug.Log($"It's {currentUnit.unitName}'s turn!");
				foreach (Unit unit in unitManager.allUnits)
				{
					unit.unitUI.SetHighlight(false);
				}

				gameManager.turnManager.turnCounter++;
				currentUnit.unitUI.SetHighlight(true);
				currentUnit.unitEffects.ProcessEffectsPerTurn(EffectTiming.StartTurn,
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

			currentUnit.unitEffects.ProcessEffectsPerTurn(EffectTiming.EndTurn, gameManager.turnManager.turnCounter);
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
				gameManager.turnManager.CalculateInitiative();
			}

			yield return new WaitForSeconds(1f);
		}
	}

	IEnumerator PlayerTurn(PlayerUnit playerUnit)
	{
		if (!playerUnit.unitEffects.HasStunDebuff())
		{
			Debug.Log($"{unitManager.currentUnit.unitName} is choosing an action...");

			playerUnit.unitAbilityManager.StartTurn();

			TargetSelectionUI.Instance.isSelecting = true;

			while (TargetSelectionUI.Instance.isSelecting)
			{
				yield return null;
			}
		}
		else
		{
			playerUnit.unitEffects.ProcessEffectsPerTurn(EffectTiming.SkipAction, gameManager.turnManager.turnCounter);
			Debug.Log($"{unitManager.currentUnit.unitName} is stunned");
		}

		gameManager.combatUIManager.HandlePanel(false);
		unitManager.currentUnit.unitUI.SetHighlight(false);

		yield return new WaitForSeconds(1.0f); // Simulate the attack animation/delay
	}

	IEnumerator EnemyTurn(EnemyUnit enemyUnit)
	{
		yield return new WaitForSeconds(1.0f);
		if (!enemyUnit.unitEffects.HasStunDebuff())
		{
			enemyUnit.PerformIntent();
			unitManager.currentUnit.unitUI.SetHighlight(false);
		}
		else
		{
			enemyUnit.unitEffects.ProcessEffectsPerTurn(EffectTiming.SkipAction, gameManager.turnManager.turnCounter);
			Debug.Log($"{enemyUnit.unitName} is stunned");
		}

		yield return new WaitForSeconds(1.0f); // Simulate the attack animation/delay
	}

	public bool CheckEndRound()
	{
		int alivePlayers = unitManager.playerUnits.Count(u => u.unitHealth.isAlive());
		int aliveEnemies = unitManager.enemyUnits.Count(u => u.unitHealth.isAlive());

		Debug.Log("Players:" + alivePlayers);
		Debug.Log("Enemies:" + aliveEnemies);

		if ((alivePlayers <= 0 || aliveEnemies <= 0) && inCombat)
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
			unit.unitEffects.activeEffects.Clear();
			unit.unitAbilityManager.abilityCooldowns.Clear();
			unit.unitUI.RefreshStatusIcons();
			unit.unitUI.RefreshCoatingUI(false);
		}

		unitManager.playerUnits.Clear();
		unitManager.enemyUnits.Clear();
	}

	IEnumerator ShowEndFightScreen(string winner)
	{
		Debug.Log(winner);
		yield return new WaitForSeconds(3f);
		if (winner == "Player")
			StartCoroutine(StartCombat());
	}
}