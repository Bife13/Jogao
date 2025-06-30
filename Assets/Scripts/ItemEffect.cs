using System;
using UnityEngine;


[Serializable]
public abstract class ItemEffectModule
{
	public ItemTriggerType itemTriggerType;
	public TargetType targetType;
	public abstract void Apply(Unit target);
}

[Serializable]
public class ItemConditionModule : ItemEffectModule
{
	public Condition conditionToApply;

	[Range(0, 100)]
	public int conditionChance;


	public override void Apply(Unit target)
	{
		target.unitConditions.CheckAndApplyItemCondition(this, target);
	}
}

[Serializable]
public class ItemCleanseModule : ItemEffectModule
{
	public ConditionType conditionToCleanse;

	public StatusType statusTypeToCleanse;

	public int cleanseAmount;

	public override void Apply(Unit target)
	{
		target.unitConditions.Cleanse(cleanseAmount, conditionToCleanse,
			statusTypeToCleanse);
	}
}

[Serializable]
public class ItemHealModule : ItemEffectModule
{
	public int healingAmount;

	public override void Apply(Unit target)
	{
		target.unitHealth.Heal(healingAmount);
	}
}