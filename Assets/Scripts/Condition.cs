using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewCondition", menuName = "Game/Conditions/Condition")]
public class Condition : ScriptableObject
{
	public string conditionName;
	public Sprite conditionIcon;

	[Tooltip("Type of Condition")]
	public ConditionType conditionType;

	[Tooltip("Duration of Condition")]
	public int turnDuration;

	[Tooltip("Can condition stack  durations?")]
	public bool isStackable;

	[Tooltip("Condition timing")]
	public ConditionTiming conditionTiming;

	[Header("What effects this condition applies")]
	[SerializeReference]
	public List<ConditionEffect> effects = new();
}

[Serializable]
public abstract class ConditionEffect
{
	/// <summary>Runs when this condition is applied (OnApply timing).</summary>
	public virtual void OnApply(Unit target)
	{
	}

	/// <summary>Runs each turn (either start or end, depending on the SO).</summary>
	public virtual void OnTick(Unit target)
	{
	}

	/// <summary>Runs each turn (either start or end, depending on the SO).</summary>
	public virtual void OnTick(Unit target, int remainingDuration)
	{
	}

	/// <summary>Runs when the condition is removed (OnRemove timing).</summary>
	public virtual void OnRemove(Unit target)
	{
	}
}

[Serializable]
public class StatModifyEffect : ConditionEffect
{
	[Tooltip("Affected Stat")]
	public StatType statAffected;

	[Tooltip("Amount")]
	public int amount;
}

[Serializable]
public class StatusEffect : ConditionEffect
{
	[Tooltip("Status to apply")]
	public StatusType status;

	[Tooltip("Damage per turn")]
	public int statusDamagePerTurn;

	public override void OnTick(Unit target, int remainingDuration)
	{
		switch (status)
		{
			case StatusType.Toxin:
				target.unitHealth.TakeDoTDamage(remainingDuration, DamageType.DoT);
				Debug.Log(
					$"{target.unitName} takes {remainingDuration} damage from {"Toxin"}");
				break;
			case StatusType.Wound:
				target.unitHealth.TakeDoTDamage(remainingDuration, DamageType.DoT);
				Debug.Log(
					$"{target.unitName} takes {remainingDuration} damage from {"Wound"}");
				break;
			case StatusType.Ignite:
				target.unitHealth.TakeDoTDamage(
					Mathf.CeilToInt(target.maxHP * (statusDamagePerTurn / 100f)),
					DamageType.DoT);
				Debug.Log(
					$"{target.unitName} takes {Mathf.CeilToInt(target.maxHP * (statusDamagePerTurn / 100f))} damage from {"Ignite"}");
				break;
		}
	}
}

[Serializable]
public class CounterattackEffect : ConditionEffect
{
	[Tooltip("Counter ability")]
	public Ability counterAbility;
}


[Serializable]
public class CoatingEffect : ConditionEffect
{
	[Tooltip("Coating to apply")]
	public WeaponCoating weaponCoating;

	[Tooltip("Coating damage bonus")]
	public int coatingBuffAmount;

	public override void OnApply(Unit target)
	{
		Debug.Log("TEST GOT HERE");
		target.unitConditions.ApplyWeaponCoating(weaponCoating);
	}
}