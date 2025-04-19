using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "NewCondition", menuName = "Game/Conditions/Condition")]
public class Condition : ScriptableObject
{
	public string conditionName;
	public ConditionType conditionType;
	public StatType statAffected;
	public ConditionTiming conditionTiming;

	public int amount;
	public int turnDuration;
	public bool isStackable;

	public Sprite conditionIcon;

	[Header("Status")]
	public bool isStatus;

	[ShowIf("isStatus")]
	public StatusType status;

	[ShowIf("isStatus")]
	public int statusDamagePerTurn;

	[Header("Counterattack")]
	public bool grantsCounter;

	[ShowIf("grantsCounter")]
	public Ability counterAbility;

	[Header("Coating")]
	public bool isCoatingBuff;
	[ShowIf("isCoatingBuff")]
	public WeaponCoating weaponCoating;
}

