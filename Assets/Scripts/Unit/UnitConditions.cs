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

			ActiveCondition existing = activeConditions.Find(e => e.condition.status == condition.status);
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

			if (condition.isCoatingBuff)
			{
				Debug.Log("TEST1");
				coatingDuration--;
				if (activeCoating == null)
				{
					ApplyWeaponCoating(condition.weaponCoating);
					Debug.Log("Applied Coating");
				}
			}

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


	public void CheckAndApplyItemCondition(ItemEffect itemEffect, Unit target)
	{
		Condition condition = itemEffect.conditionToApply;
		if (condition == null) return;

		int conditionRoll = Random.Range(1, 101);

		if (conditionRoll <= itemEffect.conditionChance)
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

	private int CalculateFinalChance(Condition condition, int baseChance, Unit target)
	{
		switch (condition.conditionType)
		{
			case ConditionType.Boon:
				return baseChance;
			case ConditionType.Jinx:
				return Mathf.Max(0, baseChance + unit.unitInventory.HandleJinxPassiveItem() - target.unitTenacy.jinx);
			case ConditionType.Status:
				int targetTenacity = condition.status switch
				{
					StatusType.Wound => unit.unitTenacy.wound,
					StatusType.Toxin => unit.unitTenacy.toxin,
					StatusType.Ignite => unit.unitTenacy.ignite,
					StatusType.Shock => unit.unitTenacy.shock,
					StatusType.Stun => unit.unitTenacy.stun,
					_ => 0
				};
				return Mathf.Max(0,
					baseChance + unit.unitInventory.HandleStatusPassiveItem(condition.status) - targetTenacity);
			default:
				return 0;
		}
	}


	public bool CheckForActiveConditions(Unit target, List<Condition> conditions)
	{
		if (target.unitConditions.activeConditions.Count > 0)
		{
			foreach (Condition condition in conditions)
			{
				ActiveCondition existing =
					target.unitConditions.activeConditions.Find(e => e.condition.status == condition.status);
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
					switch (appliedCondition.condition.status)
					{
						case StatusType.Toxin:

							unit.unitHealth.TakeDoTDamage(appliedCondition.remainingDuration, DamageType.DoT);
							Debug.Log(
								$"{unit.unitName} takes {appliedCondition.remainingDuration} damage from {appliedCondition.condition.conditionName}");
							break;
						case StatusType.Wound:
							unit.unitHealth.TakeDoTDamage(appliedCondition.remainingDuration, DamageType.DoT);
							Debug.Log(
								$"{unit.unitName} takes {appliedCondition.remainingDuration} damage from {appliedCondition.condition.conditionName}");
							break;
						case StatusType.Ignite:
							unit.unitHealth.TakeDoTDamage(
								Mathf.CeilToInt(unit.maxHP * (appliedCondition.condition.statusDamagePerTurn / 100f)),
								DamageType.DoT);
							Debug.Log(
								$"{unit.unitName} takes {Mathf.CeilToInt(unit.maxHP * (appliedCondition.condition.statusDamagePerTurn / 100f))} damage from {appliedCondition.condition.conditionName}");
							break;
					}


					appliedCondition.TickCondition(currentTurn);
					unit.unitUI.RefreshStatusIcons();

					if (appliedCondition.IsExpired())
					{
						Debug.Log($"{appliedCondition.condition.conditionName} expired on {unit.unitName}");
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
							activeConditions.RemoveAt(i);
							unit.unitUI.RefreshStatusIcons();
							removed++;
							break;
						case ConditionType.Status:
							if (activeConditions[i].condition.status == statusTypeToCleanse)
							{
								Debug.Log($"{unit.unitName} had {activeConditions[i].condition.name} cleansed!");
								activeConditions.RemoveAt(i);
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
			if (activeCondition.condition.status == StatusType.Shock)
				return true;
		}

		return false;
	}

	public bool HasStunDebuff()
	{
		foreach (var activeCondition in activeConditions)
		{
			if (activeCondition.condition.status == StatusType.Stun)
				return true;
		}

		return false;
	}

	public int CoatingBuffMultiplier()
	{
		foreach (var activeCondition in activeConditions)
		{
			if (activeCondition.isCoatingBuff)
				return activeCondition.condition.amount;
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

		if (coatingDuration > 0) return;

		Debug.Log($"{unit.unitName}'s {activeCoating.coatingName} coating has worn off.");
		activeCoating = null;
		unit.unitUI.RefreshCoatingUI(false);
	}

	public void ApplyWeaponCoating(WeaponCoating coating)
	{
		activeCoating = coating;
		coatingDuration = activeCoating.coatDuration;
		Debug.Log($"{unit.unitName} applied {coating.coatingName} coating to their weapon!");
		unit.unitUI.RefreshCoatingUI(true);
	}
}