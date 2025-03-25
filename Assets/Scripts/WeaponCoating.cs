using UnityEngine;

[CreateAssetMenu(fileName = "NewCoating", menuName = "Game/Coating")]
public class WeaponCoating : ScriptableObject
{
	public string coatingName;
	public Effect effect;
	public int bonusDamage;
	public int coatDuration;
	public Sprite coatingSprite;
}
