using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

[RequireComponent(typeof(UnitHealth))]
[RequireComponent(typeof(UnitAbilityManager))]
[RequireComponent(typeof(UnitCombatCalculator))]
[RequireComponent(typeof(UnitStatCalculator))]
[RequireComponent(typeof(UnitStance))]
[RequireComponent(typeof(UnitUI))]
[RequireComponent(typeof(UnitEffects))]
public class Unit : MonoBehaviour
{
	[HideInInspector]
	public UnitHealth unitHealth;

	[HideInInspector]
	public UnitAbilityManager unitAbilityManager;

	[HideInInspector]
	public UnitCombatCalculator unitCombatCalculator;

	[HideInInspector]
	public UnitStatCalculator unitStatCalculator;

	[HideInInspector]
	public UnitStance unitStance;

	[HideInInspector]
	public UnitUI unitUI;

	[HideInInspector]
	public UnitEffects unitEffects;

	[Header("Runtime Stats")]
	[SerializeField]
	private UnitData unitData;

	public string unitName { get; private set; }
	public int maxHP { get; private set; }
	public int baseDefense { get; private set; }
	public int dodge { get; private set; }
	public int accuracy { get; private set; }
	public int speed { get; private set; }
	public int minDamage { get; private set; }
	public int maxDamage { get; private set; }
	public int critChance { get; private set; }
	public UnitType unitType { get; private set; }

	public void Awake()
	{
		unitHealth = GetComponent<UnitHealth>();
		unitAbilityManager = GetComponent<UnitAbilityManager>();
		unitCombatCalculator = GetComponent<UnitCombatCalculator>();
		unitStatCalculator = GetComponent<UnitStatCalculator>();
		unitStance = GetComponent<UnitStance>();
		unitUI = GetComponent<UnitUI>();
		unitEffects = GetComponent<UnitEffects>();

		if (unitData != null)
			LoadFromData();
		else
			Debug.LogWarning($"{gameObject.name} has no UnitData assigned");

		DOTween.Init();
		transform.DOMove(transform.position, 0.1f).SetAutoKill(true);
	}

	public virtual void Start()
	{
	}

	private void LoadFromData()
	{
		unitName = unitData.name;
		maxHP = unitData.maxHP;
		unitHealth.currentHP = maxHP;
		baseDefense = unitData.baseDefense;
		dodge = unitData.dodge;
		accuracy = unitData.accuracy;
		speed = unitData.speed;
		minDamage = unitData.minDamage;
		maxDamage = unitData.maxDamage;
		critChance = unitData.critChance;
		unitType = unitData.unitType;
		unitAbilityManager._abilities = unitData.abilities;
	}

	public virtual void BeforeAbility()
	{
		unitStance.CommitStance();
	}

	public virtual void PerformAttackAnimation()
	{
	}

	public virtual void AfterAbilityUse(Ability ability, bool hit)
	{
		if (ability.debuffSelf && hit)
		{
			unitEffects.CheckAndApplyEffects(ability, this);
		}
	}

	public void AttackAnimation(float direction)
	{
		Vector3 originalPosition = unitUI.unitSprite.transform.position;
		DOVirtual.DelayedCall(0.1f,
			() =>
			{
				unitUI.unitSprite.transform.DOMoveX(unitUI.unitSprite.transform.position.x + 0.85f * direction, 0.25f)
					.OnComplete(() => { unitUI.unitSprite.transform.DOMoveX(originalPosition.x, 0.25f); });
			});
	}
}