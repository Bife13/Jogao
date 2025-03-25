using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "NewEffect", menuName = "Game/Effects/Effect")]
public class Effect : ScriptableObject
{
	public string effectName;
	public EffectType effectType;
	public StatType statAffected;
	public EffectTiming effectTiming;

	public int amount;
	public int turnDuration;
	public bool isStackable;

	public Sprite effectIcon;

	[Header("ONLY MATTERS IF IT HAS A STATUS EFFECT")]
	public bool isStatusEffect;

	public StatusType statusEffect;

	[FormerlySerializedAs("statusDamageTiming")]
	public int statusDamagePerTurn;

	[Header("Only matters if it has counterattack")]
	public bool grantsCounter;
	public Ability counterAbility;
}

public enum EffectType
{
	Buff,
	Debuff,
	Status
}

public enum StatusType
{
	Bleed,
	Poison,
	Burn,
	None,
}

public enum EffectTiming
{
	StartTurn,
	EndTurn
}

public enum StatType
{
	Health,
	Attack,
	Defense,
	Speed,
	Dodge,
	Crit,
	Accuracy
}