using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Serialization;





[CreateAssetMenu(fileName = "NewAbility", menuName = "Game/Abilities/Ability")]
public class Ability : ScriptableObject
{
	public string abilityName;

	[TextArea]
	public string description;

	public Sprite icon;

	public TargetType targetType;
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
	public List<Condition> boostingConditions;

	[Header("Conditions")]
	public bool hasConditions;

	[ShowIf("hasConditions")]
	public bool applyAllConditions;

	[ShowIf("hasConditions")]
	public List<Condition> conditions;

	[ShowIf("hasConditions")]
	public List<int> conditionChances;

	[Header("Cleanse")]
	public bool canCleanse;

	[ShowIf("canCleanse")]
	public int cleanseAmount = 1;

	[ShowIf("canCleanse")]
	public ConditionType conditionTypeToCleanse;

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