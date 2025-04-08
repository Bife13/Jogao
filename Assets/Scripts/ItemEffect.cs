using System;
using UnityEngine;

[Serializable]
public class ItemEffect
{
	public TriggerType triggerType;
	public EffectType effectType;

	public StatType affectedStatType;
	public StatusType statusType;

	public int value; // flat stat increase or stacks
	public float percentValue; // 
	public int duration; // effect duration
	public string coatingOverride;
	public string notes;
}