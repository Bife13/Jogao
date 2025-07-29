using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class UnitConditions : MonoBehaviour
{
	private Unit unit;
	public List<ActiveCondition> activeConditions = new List<ActiveCondition>();
	public WeaponCoating activeCoating = null;
	public int coatingDuration = 0;

	public void Initialize(Unit owner)
	{
		unit = owner;
	}

	public void ApplyCondition(Condition condition)
	{
		if (condition != null)
		{
			Debug.Log($"{unit.unitName} gains {condition.conditionName}");
			GameManager.Instance.combatUIManager.AddLog(
				$"{unit.unitName} gains {condition.conditionName}");

			ActiveCondition existing = activeConditions.Find(e =>
				e.condition.effects.OfType<StatusEffect>().FirstOrDefault() ==
				condition.effects.OfType<StatusEffect>().FirstOrDefault());
			if (existing != null)
			{
				if (condition.isStackable)
				{
					existing.remainingDuration += condition.turnDuration; // Increase duration
					unit.unitUI.RefreshStatusIcons();
					return;
				}

				if (existing.remainingDuration < condition.turnDuration)
					existing.remainingDuration = condition.turnDuration; // Refresh duration
				unit.unitUI.RefreshStatusIcons();
				return;
			}

			activeConditions.Add(new ActiveCondition(condition, GameManager.Instance.turnManager.turnCounter));

			foreach (var fx in condition.effects)
				fx.OnApply(unit);

			unit.unitUI.RefreshStatusIcons();
		}
	}

	public void CheckAndApplyAbilityConditions(List<Condition> conditions, bool applyAllConditions, List<int> chances,
		Unit target)
	{
		if (conditions.Count > 0)
			if (applyAllConditions)
				for (int i = 0; i < conditions.Count; i++)
				{
					TryApplyCondition(conditions[i], chances[i], target);
				}
			else
			{
				int randomIndex = Random.Range(0, conditions.Count);
				TryApplyCondition(conditions[randomIndex], chances[randomIndex], target);
			}
	}

	public void ApplyAbilityConditions(List<Condition> conditions, bool applyAllConditions, Unit target)
	{
		if (conditions.Count > 0)
			if (applyAllConditions)
				for (int i = 0; i < conditions.Count; i++)
				{
					target.unitConditions.ApplyCondition(conditions[i]);
				}
			else
			{
				int randomIndex = Random.Range(0, conditions.Count);
				target.unitConditions.ApplyCondition(conditions[randomIndex]);
			}
	}


	public void CheckAndApplyItemCondition(ItemConditionModule itemCondition, Unit target)
	{
		Condition condition = itemCondition.conditionToApply;
		if (condition == null) return;

		int conditionRoll = Random.Range(1, 101);

		if (conditionRoll <= itemCondition.conditionChance)
			target.unitConditions.ApplyCondition(condition);
	}

	private void TryApplyCondition(Condition condition, int chance, Unit target)
	{
		if (condition == null) return;

		int conditionRoll = Random.Range(1, 101);
		int finalChance = CalculateFinalChance(condition, chance, target);

		if (conditionRoll <= finalChance)
		{
			target.unitConditions.ApplyCondition(condition);
			unit.unitInventory.HandleItemTriggers(ItemTriggerType.OnStatusApply, unit, target);
		}
	}

	public int CalculateFinalChance(Condition condition, int baseChance, Unit target)
	{
		switch (condition.conditionType)
		{
			case ConditionType.Boon:
				return baseChance;
			case ConditionType.Jinx:
				if (target.unitType == UnitType.ENEMY)
					return Mathf.Max(0,
						baseChance + unit.unitInventory.HandleJinxPassiveItem() - target.unitTenacy.jinx);

				return baseChance;
			case ConditionType.Status:
				StatusType statusType = condition.effects.OfType<StatusEffect>().FirstOrDefault().status;
				int targetTenacity = statusType switch
				{
					StatusType.Wound => unit.unitTenacy.wound,
					StatusType.Toxin => unit.unitTenacy.toxin,
					StatusType.Ignite => unit.unitTenacy.ignite,
					StatusType.Shock => unit.unitTenacy.shock,
					StatusType.Stun => unit.unitTenacy.stun,
					_ => 0
				};
				if (target.unitType == UnitType.ENEMY)
					return Mathf.Max(0,
						baseChance + unit.unitInventory.HandleStatusPassiveItem(statusType) - targetTenacity);
				return baseChance;
			default:
				return 0;
		}
	}


	public bool CheckForActiveConditions(Unit target, List<StatusType> boostingStatus)
	{
		if (target.unitConditions.activeConditions.Count > 0)
		{
			foreach (StatusType condition in boostingStatus)
			{
				ActiveCondition existing =
					target.unitConditions.activeConditions.Find(e =>
						e.condition.effects.OfType<StatusEffect>().FirstOrDefault().status ==
						condition);
				if (existing == null)
					return false;
			}

			return true;
		}

		return false;
	}

	public void ProcessConditionsPerTurn(ConditionTiming conditionTiming, int currentTurn)
	{
		if (activeConditions.Count > 0)
		{
			for (int i = activeConditions.Count - 1; i >= 0; i--)
			{
				ActiveCondition appliedCondition = activeConditions[i];

				if (appliedCondition.condition.conditionTiming == conditionTiming)
				{
					foreach (var fx in appliedCondition.condition.effects)
						fx.OnTick(unit, appliedCondition.remainingDuration);

					appliedCondition.TickCondition(currentTurn);

					unit.unitUI.RefreshStatusIcons();

					if (appliedCondition.IsExpired())
					{
						foreach (var fx in appliedCondition.condition.effects)
							fx.OnRemove(unit);
						Debug.Log($"{appliedCondition.condition.conditionName} expired on {unit.unitName}");
						GameManager.Instance.combatUIManager.AddLog(
							$"{appliedCondition.condition.conditionName} expired on {unit.unitName}");
						activeConditions.Remove(appliedCondition);
						unit.unitUI.RefreshStatusIcons();
					}
				}
			}
		}
	}

	// public virtual bool PerformConditionApplication(Ability ability, Unit target)
	// {
	// 	if (ability.abilityEffectType == AbilityEffectType.Debuff && target != this)
	// 	{
	// 		if (!unit.unitCombatCalculator.PerformAccuracyDodgeCheck(ability.accuracy, target))
	// 			return false;
	// 	}
	//
	// 	CheckAndApplyAbilityConditions(ability, target);
	// 	return true;
	// }

	public virtual void CleanseTarget(int cleanseAmount, ConditionType conditionType, StatusType statusType,
		Unit target)
	{
		int amount = cleanseAmount <= 0 ? int.MaxValue : cleanseAmount;
		target.unitConditions.Cleanse(amount, conditionType, statusType);
		Debug.Log($"{target.unitName} is cleansed of {amount} debuffs!");
		GameManager.Instance.combatUIManager.AddLog(($"{target.unitName} is cleansed of {amount} debuffs!"));
	}

	public void Cleanse(int amount, ConditionType conditionTypeToCleanse, StatusType statusTypeToCleanse)
	{
		int removed = 0;
		if (activeConditions.Count > 0)
			for (int i = activeConditions.Count - 1; i >= 0; i--)
			{
				if (activeConditions[i].condition.conditionType == conditionTypeToCleanse)
				{
					switch (conditionTypeToCleanse)
					{
						case ConditionType.Boon:
						case ConditionType.Jinx:
							Debug.Log($"{unit.unitName} had {activeConditions[i].condition.name} cleansed!");
							GameManager.Instance.combatUIManager.AddLog(
								$"{unit.unitName} had {activeConditions[i].condition.name} cleansed!");

							activeConditions.RemoveAt(i);
							unit.unitUI.RefreshStatusIcons();
							removed++;
							break;
						case ConditionType.Status:
							if (statusTypeToCleanse == StatusType.Any)
							{
								int index = Random.Range(0, activeConditions.Count);
								activeConditions.RemoveAt(index);
								Debug.Log($"{unit.unitName} had {activeConditions[index].condition.name} cleansed!");
								GameManager.Instance.combatUIManager.AddLog(
									$"{unit.unitName} had {activeConditions[i].condition.name} cleansed!");
								unit.unitUI.RefreshStatusIcons();
								removed++;
							}

							if (activeConditions[i].condition.effects.OfType<StatusEffect>().FirstOrDefault().status ==
							    statusTypeToCleanse)
							{
								activeConditions.RemoveAt(i);
								Debug.Log($"{unit.unitName} had {activeConditions[i].condition.name} cleansed!");
								GameManager.Instance.combatUIManager.AddLog(
									$"{unit.unitName} had {activeConditions[i].condition.name} cleansed!");
								unit.unitUI.RefreshStatusIcons();
								removed++;
							}

							break;
					}

					if (removed >= amount)
					{
						unit.unitUI.RefreshStatusIcons();
						break;
					}
				}
			}

		unit.unitUI.RefreshStatusIcons();
	}


	public bool HasCounterBuff()
	{
		foreach (var activeCondition in activeConditions)
		{
			if (activeCondition.grantsCounter)
				return true;
		}

		return false;
	}

	public bool HasCoatingBuff()
	{
		foreach (var activeCondition in activeConditions)
		{
			if (activeCondition.isCoatingBuff)
				return true;
		}

		return false;
	}

	public bool HasShockedDebuff()
	{
		foreach (var activeCondition in activeConditions)
		{
			StatusEffect statusEffect = activeCondition.condition.effects.OfType<StatusEffect>().FirstOrDefault();
			if (statusEffect != null)
				if (statusEffect.status == StatusType.Shock)
					return true;
		}

		return false;
	}

	public bool HasStunDebuff()
	{
		foreach (var activeCondition in activeConditions)
		{
			StatusEffect statusEffect = activeCondition.condition.effects.OfType<StatusEffect>().FirstOrDefault();
			if (statusEffect != null)
				if (statusEffect.status == StatusType.Stun)
					return true;
		}

		return false;
	}

	public int CoatingBuffMultiplier()
	{
		foreach (var activeCondition in activeConditions)
		{
			if (activeCondition.isCoatingBuff)
				return activeCondition.condition.effects.OfType<CoatingEffect>().FirstOrDefault().coatingBuffAmount;
		}

		return 1;
	}

	public ActiveCondition GetCounterBuff()
	{
		return activeConditions.FirstOrDefault(e => e.grantsCounter);
	}

	public virtual void HandleWeaponCoating()
	{
		if (activeCoating == null) return;

		coatingDuration--;
		unit.unitUI.RefreshCoatingUI(true);
		Debug.Log($"{unit.unitName}'s {activeCoating.coatingName}  has: " + coatingDuration);
		GameManager.Instance.combatUIManager.AddLog($"{unit.unitName}'s {activeCoating.coatingName}  has: " +
		                                            coatingDuration);


		if (coatingDuration > 0) return;

		Debug.Log($"{unit.unitName}'s {activeCoating.coatingName} coating has worn off.");
		GameManager.Instance.combatUIManager.AddLog(
			$"{unit.unitName}'s {activeCoating.coatingName} coating has worn off.");
		activeCoating = null;
		unit.unitUI.RefreshCoatingUI(false);
	}

	public void ApplyWeaponCoating(WeaponCoating coating)
	{
		activeCoating = coating;
		coatingDuration = activeCoating.coatDuration;
		Debug.Log($"{unit.unitName} applied {coating.coatingName} coating to their weapon!");
		GameManager.Instance.combatUIManager.AddLog(
			$"{unit.unitName} applied {coating.coatingName} coating to their weapon!");
		unit.unitUI.RefreshCoatingUI(true);
	}
}