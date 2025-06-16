using UnityEngine;

public enum TargetType
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
	Status,
}

public enum ConditionType
{
	Boon,
	Jinx,
	Status,
	None,
	Any
}

public enum StatusType
{
	Wound,
	Toxin,
	Ignite,
	Stun,
	Shock,
	None,
	Any,
}

public enum ConditionTiming
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

public enum ItemTriggerType
{
	OnHit,
	OnCrit,
	OnGetHit,
	OnTurnStart,
	OnTurnEnd,
	OnLowHealth,
	OnStatusApply,
}

public enum ItemEffectType
{
	ApplyStatus,
	ApplyBuff,
	ApplyDebuff,
	Heal,
	Cleanse
}
