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
	public StatusType status;
	public int statusDamagePerTurn;

	[Header("Counterattack")]
	public bool grantsCounter;

	public Ability counterAbility;

	[Header("Coating")]
	public bool isCoatingBuff;
	public WeaponCoating weaponCoating;
}

