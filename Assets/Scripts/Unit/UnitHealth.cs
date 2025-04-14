using System;
using UnityEngine;


public class UnitHealth : MonoBehaviour
{
	private Unit unit;
	public int currentHP;


	public void Initialize(Unit owner)
	{
		unit = owner;
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
				unit.unitUI.ShowFloatingText(finalDamage.ToString(), Color.black);
				break;
		}

		//Move this to UI
		unit.unitUI.UpdateHealthBar(currentHP, unit.maxHP);
		CheckDeath();
	}

	public void TakeDirectDamage(int damage, DamageType damageType, Unit originTarget)
	{
		int finalDamage = 0;
		switch (damageType)
		{
			case DamageType.Direct:
				float reducedDamage = damage *
				                      (unit.unitStatCalculator.GetTotalModifiedStat(StatType.Defense) /
				                       100f);
				finalDamage = Mathf.Max(0, damage - Mathf.CeilToInt(reducedDamage));
				currentHP = Math.Max(currentHP - finalDamage, 0);
				Debug.Log($"{unit.unitName} took {finalDamage} damage! Remaining HP: {currentHP}");
				unit.unitUI.ShowFloatingText(finalDamage.ToString(), Color.black);

				if (unit.unitEffects.HasCounterBuff() && isAlive())
				{
					StartCoroutine(unit.unitCombatCalculator.ExecuteCounterAttack(originTarget));
				}

				break;
		}

		unit.unitUI.UpdateHealthBar(currentHP, unit.maxHP);
		CheckDeath();
	}

	public void Heal(int amount)
	{
		currentHP = Math.Min(currentHP + amount, unit.maxHP);

		unit.unitUI.ShowFloatingText(amount.ToString(), Color.black);
		unit.unitUI.UpdateHealthBar(currentHP, unit.maxHP);
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
			unit.unitUI.displayObject.SetActive(false);
			unit.unitUI.unitSprite.SetActive(false);
			Destroy(gameObject, 2f);
			GameManager.Instance.unitManager.RemoveUnit(unit);
		}
	}
}