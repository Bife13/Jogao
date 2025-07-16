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
	private TMP_Text healText;

	[SerializeField]
	private TMP_Text healCritChanceText;

	[SerializeField]
	private GameObject coatingGameObject;

	[SerializeField]
	private TMP_Text selfdamageText;


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
			ShowPlayerUnitItems(unitManager.currentUnit);
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

		playerTenacityTexts[0].text = (selectedUnit.unitTenacy.wound).ToString();
		playerTenacityTexts[1].text = (selectedUnit.unitTenacy.toxin).ToString();
		playerTenacityTexts[2].text = (selectedUnit.unitTenacy.ignite).ToString();
		playerTenacityTexts[3].text = (selectedUnit.unitTenacy.shock).ToString();
		playerTenacityTexts[4].text = (selectedUnit.unitTenacy.shock).ToString();
		playerTenacityTexts[5].text = (selectedUnit.unitTenacy.jinx).ToString();
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
		enemyStatsTexts[0].text = selectedUnit.unitStatCalculator.GetTotalModifiedStat(StatType.Health).ToString();
		enemyStatsTexts[1].text = selectedUnit.unitStatCalculator.GetTotalModifiedAttackStat()[0] + "-" +
		                          selectedUnit.unitStatCalculator.GetTotalModifiedAttackStat()[1];
		enemyStatsTexts[2].text = selectedUnit.unitStatCalculator.GetTotalModifiedStat(StatType.Defense) + " %";
		enemyStatsTexts[3].text = selectedUnit.unitStatCalculator.GetTotalModifiedStat(StatType.Speed).ToString();
		enemyStatsTexts[4].text = selectedUnit.unitStatCalculator.GetTotalModifiedStat(StatType.Crit) + " %";
		enemyStatsTexts[5].text = selectedUnit.unitStatCalculator.GetTotalModifiedStat(StatType.Accuracy) + " %";
		enemyStatsTexts[6].text = selectedUnit.unitStatCalculator.GetTotalModifiedStat(StatType.Dodge) + " %";

		enemyTenacityTexts[0].text = (selectedUnit.unitTenacy.wound).ToString();
		enemyTenacityTexts[1].text = (selectedUnit.unitTenacy.toxin).ToString();
		enemyTenacityTexts[2].text = (selectedUnit.unitTenacy.ignite).ToString();
		enemyTenacityTexts[3].text = (selectedUnit.unitTenacy.shock).ToString();
		enemyTenacityTexts[4].text = (selectedUnit.unitTenacy.shock).ToString();
		enemyTenacityTexts[5].text = (selectedUnit.unitTenacy.jinx).ToString();
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

			HealModule healModule = assignedAbility.modules.OfType<HealModule>().FirstOrDefault();
			if (healModule != null)
			{
				int minHeal = healModule.minHealAmount;
				int maxHeal = healModule.maxHealAmount;
				int critChance = currentUnit.healCritChance + healModule.bonusCriticalChance;

				if (healText != null) healText.text = $"{minHeal}-{maxHeal}";
				if (healCritChanceText != null) healCritChanceText.text = $"{critChance}";
			}

			CoatModule coatModule = assignedAbility.modules.OfType<CoatModule>().FirstOrDefault();
			if (coatModule != null)
			{
				coatingGameObject.SetActive(true);
				coatingGameObject.GetComponentInChildren<Image>().sprite = coatModule.coating.coatingSprite;
				coatingGameObject.GetComponentInChildren<TMP_Text>().text = coatModule.coating.coatDuration.ToString();
			}

			SelfHitModule selfHitModule = assignedAbility.modules.OfType<SelfHitModule>().FirstOrDefault();
			if (selfHitModule != null)
			{
				if (selfdamageText != null) selfdamageText.text = $"{selfHitModule.damagePercentage}";
			}
		}
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

		if (healText != null) healText.text = "";
		if (healCritChanceText != null) healCritChanceText.text = "";
		coatingGameObject.SetActive(false);
		if (selfdamageText != null) selfdamageText.text = "";
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