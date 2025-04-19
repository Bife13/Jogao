using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "NewCoating", menuName = "Game/Coating")]
public class WeaponCoating : ScriptableObject
{
	public string coatingName;
	public Condition condition;
	public int bonusDamage;
	public int coatDuration;
	public Sprite coatingSprite;
}
