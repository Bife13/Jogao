using UnityEngine;

public enum AbilityTargetType
{
	Enemy,
	Ally,
	Self,
	AllEnemies,
	AllAllies
}

public enum AbilityEffectType
{
	Damage,
	Heal,
	Buff,
	Debuff,
	StatusEffect,
}

public enum EffectType
{
	Buff,
	Debuff,
	Status,
	StatusImprovement,
	ModifyCoating,
	CooldownReduction,
	DamageModifier,
	None
}

public enum StatusType
{
	Bleed,
	Poison,
	Burn,
	Stun,
	Shock,
	None,
	Any,
}

public enum EffectTiming
{
	StartTurn,
	EndTurn,
	OnHit,
	SkipAction
}

public enum StatType
{
	Health,
	Attack,
	Defense,
	Speed,
	Dodge,
	Crit,
	Accuracy,
	None
}

public enum UnitType
{
	PLAYER,
	ENEMY
}

public enum DamageType
{
	Direct,
	DoT
}

public enum StanceType
{
	Neutral = 0,
	Offensive = 1,
	Defensive = 2
}

public enum ItemType
{
	Offensive,
	Defensive,
	Utility,
	General
}

public enum TriggerType
{
	Passive,
	OnHit,
	OnCoat,
	OnTurnStart,
	OnCrit,
	OnAllyHit,
	OnStatusApply,
	OnDefend
}

