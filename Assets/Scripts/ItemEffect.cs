using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[System.Serializable]
public class ItemEffect
{
	[FormerlySerializedAs("triggerType")]
	public ItemTriggerType itemTriggerType;

	public ItemEffectType itemEffectType;

	[Header("Effects")]
	public bool applyEffect = false;

	[ShowIf("applyEffect")]
	public Effect effectToApply;

	[ShowIf("applyEffect")]
	public float effectChance;

	[ShowIf("applyEffect")]
	public TargetType targetType;

	[Header("Healing")]
	public bool isHealing = false;

	[ShowIf("applyEffect")]
	public int healingAmount;

	[Header("Cleanse")]
	public bool isCleanse = false;

	[ShowIf("isCleanse")]
	public EffectType effectToCleanse;

	[ShowIf("isCleanse")]
	public StatusType statusTypeToCleanse;

	[ShowIf("isCleanse")]
	public int cleanseAmount;
}