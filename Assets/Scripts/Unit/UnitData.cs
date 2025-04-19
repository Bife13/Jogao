using System;
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

	public Tenacity tenacity;

	public UnitType unitType;

	public List<Ability> abilities;
}

[Serializable]
public class Tenacity
{
	public int wound;
	public int toxin;
	public int ignite;
	public int shock;
	public int stun;
	public int jinx;
}