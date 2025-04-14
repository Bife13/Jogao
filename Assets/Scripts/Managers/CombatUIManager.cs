using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CombatUIManager : MonoBehaviour
{
	public GameManager gameManager;
	private UnitManager unitManager;

	[SerializeField]
	private GameObject skillPanel;

	[SerializeField]
	private List<TMP_Text> statsTexts;

	[SerializeField]
	private List<ButtonHandler> _buttonHandlers = new List<ButtonHandler>();


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

			UpdateStats();
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

	public void UpdateStats()
	{
		Unit currentUnit = unitManager.currentUnit;
		statsTexts[0].text = currentUnit.unitStatCalculator.GetTotalModifiedStat(StatType.Health).ToString();
		statsTexts[1].text = currentUnit.unitStatCalculator.GetTotalModifiedAttackStat()[0] + "-" +
		                     currentUnit.unitStatCalculator.GetTotalModifiedAttackStat()[1];
		statsTexts[2].text = currentUnit.unitStatCalculator.GetTotalModifiedStat(StatType.Defense) + " %";
		statsTexts[3].text = currentUnit.unitStatCalculator.GetTotalModifiedStat(StatType.Speed).ToString();
		statsTexts[4].text = currentUnit.unitStatCalculator.GetTotalModifiedStat(StatType.Crit) + " %";
		statsTexts[5].text = currentUnit.unitStatCalculator.GetTotalModifiedStat(StatType.Accuracy) + " %";
		statsTexts[6].text = currentUnit.unitStatCalculator.GetTotalModifiedStat(StatType.Dodge) + " %";
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