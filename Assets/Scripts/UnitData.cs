using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewUnitData", menuName = "Game/Unit Data")]
public class UnitData : ScriptableObject
{
	public string unitName;
	
	public int maxHP;
	public int baseDefense;
	public int dodge;
	public int accuracy;
	public int speed;
	public int minDamage;
	public int maxDamage;
	public int critChance;
	public UnitType unitType;

	public List<Ability> abilities;
}