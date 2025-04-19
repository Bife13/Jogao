using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class CombatUIManager : MonoBehaviour
{
	public GameManager gameManager;
	private UnitManager unitManager;

	[SerializeField]
	private GameObject skillPanel;

	[SerializeField]
	private List<TMP_Text> playerStatsTexts;

	[SerializeField]
	private List<TMP_Text> enemyStatsTexts;

	[SerializeField]
	private List<ButtonHandler> _buttonHandlers = new List<ButtonHandler>();

	[SerializeField]
	private GameObject playerStats;

	[SerializeField]
	private GameObject enemyStats;

	public void Initialize()
	{
		gameManager = GameManager.Instance;
		unitManager = gameManager.unitManager;
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

	public void HandlePanel(bool change)
	{
		List<Ability> playerAbilities = unitManager.currentUnit.unitAbilityManager.GetAbilities();
		Dictionary<Ability, int> cooldowns = unitManager.currentUnit.unitAbilityManager.abilityCooldowns;

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

			ShowPlayerUnitStats(unitManager.currentUnit);
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

	public void ShowPlayerUnitStats(Unit selectedUnit)
	{
		playerStats.SetActive(true);
		playerStatsTexts[0].text = selectedUnit.unitStatCalculator.GetTotalModifiedStat(StatType.Health).ToString();
		playerStatsTexts[1].text = selectedUnit.unitStatCalculator.GetTotalModifiedAttackStat()[0] + "-" +
		                           selectedUnit.unitStatCalculator.GetTotalModifiedAttackStat()[1];
		playerStatsTexts[2].text = selectedUnit.unitStatCalculator.GetTotalModifiedStat(StatType.Defense) + " %";
		playerStatsTexts[3].text = selectedUnit.unitStatCalculator.GetTotalModifiedStat(StatType.Speed).ToString();
		playerStatsTexts[4].text = selectedUnit.unitStatCalculator.GetTotalModifiedStat(StatType.Crit) + " %";
		playerStatsTexts[5].text = selectedUnit.unitStatCalculator.GetTotalModifiedStat(StatType.Accuracy) + " %";
		playerStatsTexts[6].text = selectedUnit.unitStatCalculator.GetTotalModifiedStat(StatType.Dodge) + " %";
	}

	public void ShowEnemyUnitStats(Unit selectedUnit)
	{
		enemyStats.SetActive(true);
		enemyStatsTexts[0].text = selectedUnit.unitStatCalculator.GetTotalModifiedStat(StatType.Health).ToString();
		enemyStatsTexts[1].text = selectedUnit.unitStatCalculator.GetTotalModifiedAttackStat()[0] + "-" +
		                          selectedUnit.unitStatCalculator.GetTotalModifiedAttackStat()[1];
		enemyStatsTexts[2].text = selectedUnit.unitStatCalculator.GetTotalModifiedStat(StatType.Defense) + " %";
		enemyStatsTexts[3].text = selectedUnit.unitStatCalculator.GetTotalModifiedStat(StatType.Speed).ToString();
		enemyStatsTexts[4].text = selectedUnit.unitStatCalculator.GetTotalModifiedStat(StatType.Crit) + " %";
		enemyStatsTexts[5].text = selectedUnit.unitStatCalculator.GetTotalModifiedStat(StatType.Accuracy) + " %";
		enemyStatsTexts[6].text = selectedUnit.unitStatCalculator.GetTotalModifiedStat(StatType.Dodge) + " %";
	}

	public void HidePlayerUnitStats()
	{
		playerStats.SetActive(false);
	}

	public void TrySwitchStance()
	{
		if (unitManager.currentUnit.unitStance.hasComittedStance)
		{
			Debug.Log("Stance already committed. Cannot preview/change anymore.");
			return;
		}

		int stanceCount = Enum.GetValues(typeof(StanceType)).Length;
		unitManager.currentUnit.unitStance.previewStance =
			(StanceType)(((int)unitManager.currentUnit.unitStance.previewStance + 1) % stanceCount);

		Debug.Log(
			$"{unitManager.currentUnit.unitName} switched to {unitManager.currentUnit.unitStance.previewStance} stance!");
	}
}