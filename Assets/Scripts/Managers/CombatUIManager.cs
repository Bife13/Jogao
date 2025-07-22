using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class CombatUIManager : MonoBehaviour
{
	public GameManager gameManager;
	private UnitManager unitManager;

	[SerializeField]
	private GameObject skillPanel;

	[SerializeField]
	private GameObject playerStats;

	[SerializeField]
	private List<TMP_Text> playerStatsTexts;

	[SerializeField]
	private List<TMP_Text> playerTenacityTexts;

	[SerializeField]
	private GameObject enemyStats;

	[SerializeField]
	private List<TMP_Text> enemyStatsTexts;

	[SerializeField]
	private List<TMP_Text> enemyTenacityTexts;

	[SerializeField]
	private List<ButtonHandler> _buttonHandlers = new List<ButtonHandler>();


	[SerializeField]
	private GameObject playerItems;

	[SerializeField]
	private List<Image> itemImages;

	[SerializeField]
	private TMP_Text damageText;

	[SerializeField]
	private TMP_Text accuracyText;

	[SerializeField]
	private TMP_Text critchanceText;

	[SerializeField]
	private List<GameObject> conditionsFromAbility;

	[SerializeField]
	private GameObject combatInfoPanel;

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

	public void HandlePanel(bool shouldShow)
	{
		if (shouldShow)
		{
			List<Ability> playerAbilities = unitManager.currentUnit.unitAbilityManager.GetAbilities();
			Dictionary<Ability, int> cooldowns = unitManager.currentUnit.unitAbilityManager.abilityCooldowns;

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
			ShowPlayerUnitItems(unitManager.currentUnit);
		}
		else
		{
			foreach (ButtonHandler buttonHandler in _buttonHandlers)
			{
				buttonHandler.ResetAbility();
			}

			skillPanel.SetActive(false);
			playerStats.SetActive(false);
			playerItems.SetActive(false);
			HideEnemyStats();
		}
	}

	public void ShowPlayerUnitStats(Unit selectedUnit)
	{
		playerStats.SetActive(true);
		var calc = selectedUnit.unitStatCalculator;
		var attackRange = calc.GetTotalModifiedAttackStat();

		playerStatsTexts[(int)PlayerStatTextIndex.Attack].text =
			$"{attackRange[0]}-{attackRange[1]}";
		playerStatsTexts[(int)PlayerStatTextIndex.Accuracy].text =
			$"{calc.GetTotalModifiedStat(StatType.Accuracy)} %";
		playerStatsTexts[(int)PlayerStatTextIndex.Crit].text =
			$"{calc.GetTotalModifiedStat(StatType.Crit)} %";
		playerStatsTexts[(int)PlayerStatTextIndex.Speed].text =
			$"{calc.GetTotalModifiedStat(StatType.Speed)}";
		playerStatsTexts[(int)PlayerStatTextIndex.Defense].text =
			$"{calc.GetTotalModifiedStat(StatType.Defense)} %";
		playerStatsTexts[(int)PlayerStatTextIndex.Dodge].text =
			$"{calc.GetTotalModifiedStat(StatType.Dodge)} %";


		var tenacity = selectedUnit.unitTenacy;
		int[] tenacityValues =
		{
			tenacity.wound,
			tenacity.toxin,
			tenacity.ignite,
			tenacity.shock,
			tenacity.stun,
			tenacity.jinx
		};


		for (int i = 0; i < playerTenacityTexts.Count && i < tenacityValues.Length; i++)
		{
			playerTenacityTexts[i].text = $"{tenacityValues[i]}%";
		}
	}

	public void ShowPlayerUnitItems(Unit selectedUnit)
	{
		playerItems.SetActive(true);
		int counter = 0;
		foreach (EquippableItem item in selectedUnit.unitInventory.equippedItems)
		{
			if (!item) continue;
			itemImages[counter].sprite = item.icon;
			counter++;
		}
	}

	public void ShowEnemyUnitStats(Unit selectedUnit)
	{
		enemyStats.SetActive(true);

		enemyStatsTexts[(int)EnemyStatTextIndex.Defense].text =
			$"DEF: {selectedUnit.unitStatCalculator.GetTotalModifiedStat(StatType.Defense)}%";
		enemyStatsTexts[(int)EnemyStatTextIndex.Dodge].text =
			$"DGE: {selectedUnit.unitStatCalculator.GetTotalModifiedStat(StatType.Dodge)}%";
		enemyStatsTexts[(int)EnemyStatTextIndex.Speed].text =
			$"SPD: {selectedUnit.unitStatCalculator.GetTotalModifiedStat(StatType.Speed)}";

		var tenacity = selectedUnit.unitTenacy;
		int[] tenacityValues =
		{
			tenacity.wound,
			tenacity.toxin,
			tenacity.ignite,
			tenacity.shock,
			tenacity.stun,
			tenacity.jinx
		};


		for (int i = 0; i < enemyTenacityTexts.Count && i < tenacityValues.Length; i++)
		{
			enemyTenacityTexts[i].text = $"{tenacityValues[i]}%";
		}
	}

	public void HideEnemyStats()
	{
		enemyStats.SetActive(false);
	}

	public void UpdateAbilityTooltip(Ability assignedAbility, Unit target)
	{
		Unit currentUnit = GameManager.Instance.unitManager.currentUnit;
		if (assignedAbility)
		{
			DamageModule damageModule = assignedAbility.modules.OfType<DamageModule>().FirstOrDefault();
			if (damageModule != null)
			{
				List<int> damageWindow = damageModule.CalculateDamage(currentUnit, target);
				int extraDamage = currentUnit.unitCombatCalculator.AddCoatingDamage();
				int accuracy = damageModule.CalculateAccuracy(currentUnit, target);
				int critChance = (currentUnit.critChance + damageModule.bonusCriticalChance);

				string damageDisplay = extraDamage != 0
					? $"{damageWindow[0]}-{damageWindow[1]} + {extraDamage}"
					: $"{damageWindow[0]}-{damageWindow[1]}";

				if (damageText != null) damageText.text = damageDisplay;
				if (accuracyText != null) accuracyText.text = $"{accuracy}%";
				if (critchanceText != null) critchanceText.text = $"{critChance}%";
			}

			ConditionModule conditionModule = assignedAbility.modules.OfType<ConditionModule>().FirstOrDefault();
			if (conditionModule != null)
			{
				for (int i = 0; i < conditionModule.conditions.Count; i++)
				{
					conditionsFromAbility[i].SetActive(true);
					conditionsFromAbility[i].GetComponentInChildren<Image>().sprite =
						conditionModule.conditions[i].conditionIcon;

					int finalChance = target.unitConditions.CalculateFinalChance(conditionModule.conditions[i],
						conditionModule.conditionChances[i], target);
					conditionsFromAbility[i].GetComponentInChildren<TMP_Text>().text = $"{finalChance}%";
				}
			}
		}

		combatInfoPanel.SetActive(true);
	}

	public void ResetAbilityTooltip()
	{
		if (damageText != null) damageText.text = "";
		if (accuracyText != null) accuracyText.text = "";
		if (critchanceText != null) critchanceText.text = "";
		foreach (GameObject gameObject in conditionsFromAbility)
		{
			gameObject.SetActive(false);
		}

		combatInfoPanel.SetActive(false);
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

	private enum EnemyStatTextIndex
	{
		Defense = 0,
		Dodge = 1,
		Speed = 2
	}

	private enum TenacityTextIndex
	{
		Wound = 0,
		Toxin = 1,
		Ignite = 2,
		Shock = 3,
		Stun = 4,
		Jinx = 5
	}

	private enum PlayerStatTextIndex
	{
		Attack = 0,
		Accuracy = 1,
		Crit = 2,
		Speed = 3,
		Defense = 4,
		Dodge = 5
	}
}