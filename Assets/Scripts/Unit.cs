using System;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public enum UnitType
{
	PLAYER,
	ENEMY
}

public class Unit : MonoBehaviour
{
	[SerializeField]
	private UnitData unitData;

	[Header("Runtime Stats")] public string unitName { get; private set; }

	protected int maxHP;
	public int currentHP;
	protected int baseDefense;
	protected int dodge;
	protected int accuracy;

	public int speed { get; private set; }

	protected int minDamage;
	protected int maxDamage;
	protected int critChance;
	public UnitType unitType { get; private set; }

	public Image healthBar;

	public void Awake()
	{
		if (unitData != null)
			LoadFromData();
		else
			Debug.LogWarning($"{gameObject.name} has no UnitData assigned");
	}

	private void LoadFromData()
	{
		unitName = unitData.name;
		maxHP = unitData.maxHP;
		currentHP = maxHP;
		baseDefense = unitData.baseDefense;
		dodge = unitData.dodge;
		accuracy = unitData.accuracy;
		speed = unitData.speed;
		minDamage = unitData.minDamage;
		maxDamage = unitData.maxDamage;
		critChance = unitData.critChance;
		unitType = unitData.unitType;
	}

	public void TakeDamage(int damage)
	{
		float reducedDamage = damage - damage * (baseDefense / 100);
		int finalDamage = Mathf.Max(0, Mathf.RoundToInt(reducedDamage));
		currentHP -= finalDamage;
		if (currentHP < 0)
			currentHP = 0;

		DOTween.To(() => healthBar.fillAmount, x => healthBar.fillAmount = x, (float)currentHP / maxHP, 0.5f);
		Debug.Log($"{unitName} took {finalDamage} damage! Remaining HP: {currentHP}");
	}

	public void Heal(int amount)
	{
		currentHP += amount;
		if (currentHP > unitData.maxHP)
			currentHP = unitData.maxHP;
		DOTween.To(() => healthBar.fillAmount, x => healthBar.fillAmount = x, (float)currentHP / maxHP, 0.5f);
	}

	public bool isAlive()
	{
		return currentHP > 0;
	}
}