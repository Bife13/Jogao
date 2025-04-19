using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[System.Serializable]
public class ItemEffect
{
	public ItemTriggerType itemTriggerType;

	public ItemEffectType itemEffectType;

	[Header("Conditions")]
	public bool applyCondition = false;

	[ShowIf("applyCondition")]
	public Condition conditionToApply;

	[ShowIf("applyCondition")]
	public int conditionChance;

	[ShowIf("applyCondition")]
	public TargetType targetType;

	[Header("Healing")]
	public bool isHealing = false;

	[ShowIf("isHealing")]
	public int healingAmount;

	[Header("Cleanse")]
	public bool isCleanse = false;

	[ShowIf("isCleanse")]
	public ConditionType conditionToCleanse;

	[ShowIf("isCleanse")]
	public StatusType statusTypeToCleanse;

	[ShowIf("isCleanse")]
	public int cleanseAmount;
}