using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Serialization;

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

[CreateAssetMenu(fileName = "NewAbility", menuName = "Game/Abilities/Ability")]
public class Ability : ScriptableObject
{
	public string abilityName;

	[TextArea]
	public string description;

	public Sprite icon;

	public AbilityTargetType targetType;
	public AbilityEffectType abilityEffectType;

	public int
		basePower; // for damage its % of damage, for healing its either a flat value or %hp, for defense its %of defense, for buff its %of buff

	public int maxPower;
	public int cooldown;
	public int accuracy; // % to hit
	public int bonusCritical;
	public bool isWeaponAttack;

	[Header("Status Boosts")]
	public bool isStatusBoosted;

	[ShowIf("isStatusBoosted")]
	public int statusBoost;

	[ShowIf("isStatusBoosted")]
	public List<Effect> boostingEffects;

	[Header("Effects")]
	public bool hasEffects;

	[ShowIf("hasEffects")]
	public bool applyAllEffects;

	[ShowIf("hasEffects")]
	public List<Effect> effects;

	[ShowIf("hasEffects")]
	public List<int> effectChances;

	[Header("Cleanse")]
	public bool canCleanse;

	[ShowIf("canCleanse")]
	public int cleanseAmount = 1;

	[ShowIf("canCleanse")]
	public EffectType effectTypeToCleanse;

	[ShowIf("canCleanse")]
	public StatusType statusTypeToCleanse;

	[Header("Coating")]
	public bool canCoat;

	[ShowIf("canCoat")]
	public WeaponCoating coating;


	[Header("Swapping")]
	public bool canSwap;

	[ShowIf("canSwap")]
	public List<int> abilityIndexes;
	
	[Header("Hits Self")]
	public bool hitsSelf;
	public bool debuffSelf;
	
}