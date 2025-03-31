using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewUnitData", menuName = "Game/Unit Data")]
public class UnitData : ScriptableObject
{
	public string unitName;
	
	public int maxHP;
	public int speed;
	public int baseDefense;
	public int minDamage;
	public int maxDamage;
	public int critChance;
	public int accuracy;
	public int dodge;
	public UnitType unitType;

	public List<Ability> abilities;
}