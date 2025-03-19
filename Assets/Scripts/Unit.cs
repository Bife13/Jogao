using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Random = UnityEngine.Random;

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
	public int dodge { get; private set; }
	protected int accuracy;
	public int speed { get; private set; }
	protected int minDamage;
	protected int maxDamage;
	protected int critChance;
	public UnitType unitType { get; private set; }

	[Header("UI Stuff")]
	public Image healthBar;

	[Header("Ability")]
	protected List<Ability> _abilities = new List<Ability>();


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
		_abilities = unitData.abilities;
	}

	public void TakeDamage(int damage)
	{
		
		float reducedDamage = damage - damage * (baseDefense / 100);
		Debug.Log(reducedDamage + "DANO REDUZIDO");
		Debug.Log($"{unitName} took {reducedDamage} damage!");
		int finalDamage = Mathf.Max(0, Mathf.RoundToInt(reducedDamage));
		currentHP -= finalDamage;
		if (currentHP < 0)
		{
			currentHP = 0;
		}

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

	protected bool PerformAccuracyDodgeCheck(int abilityAccuracy, Unit target)
	{
		float accuracyRoll = Random.Range(0f, 100f);
		float dodgeThreshold = target.dodge;
		if (accuracyRoll <= abilityAccuracy + accuracy - dodgeThreshold)
		{
			return true; // Hit
		}

		return false; // Miss
	}

	protected bool PerformCriticalHitCheck(int finalCritChance)
	{
		float critRoll = Random.Range(0f, 100f);

		if (critRoll <= finalCritChance)
			return true; //CRIT

		return false; //NORMAL HIT
	}

	public List<Ability> GetAbilities()
	{
		return _abilities;
	}
	
	public void AttackAnimation(float direction)
	{
		Vector3 originalPosition = transform.position;
		transform.DOMoveX(transform.position.x + 0.85f * direction, 0.25f).OnComplete(() =>
		{
			transform.DOMoveX(originalPosition.x, 0.25f);
		});
	}
}