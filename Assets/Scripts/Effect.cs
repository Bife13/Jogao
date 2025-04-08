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

	[Header("Status Effects")]
	public bool isStatusEffect;

	[ShowIf("isStatusEffect")]
	public StatusType statusEffect;

	[ShowIf("isStatusEffect")]
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

