using System;
using UnityEngine;


public class UnitHealth : MonoBehaviour
{
	private Unit unit;
	private UnitStatCalculator unitStatCalculator;
	private UnitEffects unitEffects;
	private UnitUI unitUI;
	private UnitCombatCalculator unitCombatCalculator;
	public int currentHP;


	private void Awake()
	{
		unit = GetComponent<Unit>();
		unitStatCalculator = GetComponent<UnitStatCalculator>();
		unitEffects = GetComponent<UnitEffects>();
		unitUI = GetComponent<UnitUI>();
		unitCombatCalculator = GetComponent<UnitCombatCalculator>();
	}

	public void TakeDoTDamage(int damage, DamageType damageType)
	{
		int finalDamage = 0;
		switch (damageType)
		{
			case DamageType.DoT:
				finalDamage = damage;
				currentHP = Math.Max(currentHP - finalDamage, 0);
				Debug.Log($"{unit.unitName} took {finalDamage} DoT damage! Remaining HP: {currentHP}");
				//Move this to UI
				unitUI.ShowFloatingText(finalDamage.ToString(), Color.black);
				break;
		}

		//Move this to UI
		unitUI.UpdateHealthBar((float)currentHP / unit.maxHP);
		CheckDeath();
	}

	public void TakeDirectDamage(int damage, DamageType damageType, Unit originTarget)
	{
		int finalDamage = 0;
		switch (damageType)
		{
			case DamageType.Direct:
				float reducedDamage = damage *
				                      (unitStatCalculator.GetTotalModifiedStat(StatType.Defense) /
				                       100f);
				finalDamage = Mathf.Max(0, damage - Mathf.CeilToInt(reducedDamage));
				currentHP = Math.Max(currentHP - finalDamage, 0);
				Debug.Log($"{unit.unitName} took {finalDamage} damage! Remaining HP: {currentHP}");
				unitUI.ShowFloatingText(finalDamage.ToString(), Color.black);

				if (unitEffects.HasCounterBuff() && isAlive())
				{
					StartCoroutine(unitCombatCalculator.ExecuteCounterAttack(originTarget));
				}

				break;
		}

		unitUI.UpdateHealthBar((float)currentHP / unit.maxHP);
		CheckDeath();
	}

	public void Heal(int amount)
	{
		currentHP = Math.Min(currentHP + amount, unit.maxHP);

		unitUI.ShowFloatingText(amount.ToString(), Color.black);
		unitUI.UpdateHealthBar((float)currentHP / unit.maxHP);
	}

	public bool isAlive()
	{
		return currentHP > 0;
	}

	protected virtual void CheckDeath()
	{
		if (!isAlive())
		{
			//UI STUFF
			unitUI.displayObject.SetActive(false);
			unitUI.unitSprite.SetActive(false);
			Destroy(gameObject, 2f);
			GameManager.Instance.RemoveUnit(unit);
		}
	}
}