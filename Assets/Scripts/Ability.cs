using System.Collections.Generic;
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
	StatusEffect
}

[CreateAssetMenu(fileName = "NewAbility", menuName = "Game/Abilities/Ability")]
public class Ability : ScriptableObject
{
	public string abilityName;

	[TextArea]
	public string description;

	public Sprite icon;

	public AbilityTargetType targetType;
	public AbilityEffectType effectType;

	public bool isPercentage = true;

	public int
		minPower; // for damage its % of damage, for healing its either a flat value or %hp, for defense its %of defense, for buff its %of buff

	public int maxPower;
	public int cost; // might not use this
	public int cooldown;
	public int accuracy; // % to hit

	[Tooltip("For Cleanse Abilities")]
	public bool canCleanse;
	public int cleanseAmount = 1;
	public EffectType effectTypeToCleanse;
	public StatusType statusTypeToCleanse;
	// VER dos paineis de attack??

	// FOR LATER DOING STATUS EFFECTS
	public List<Effect> effects;
	public List<int> statusEffectChances;
}