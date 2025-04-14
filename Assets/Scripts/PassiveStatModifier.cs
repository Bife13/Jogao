using UnityEngine;

[System.Serializable]
public class PassiveStatModifier
{
	public StatType statType;
	public int amount;
}

public class StatusStatModifier : PassiveStatModifier
{
	public StatusType statusType;
}