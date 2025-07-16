using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


[CreateAssetMenu(fileName = "NewAbility", menuName = "Game/Abilities/NewAbility")]
public class Ability : ScriptableObject
{
	public string abilityName;
	public Sprite icon;

	[TextArea]
	public string description;

	public TargetType targetType;
	public int cooldown;

	[SerializeReference]
	public List<AbilityModule> modules = new List<AbilityModule>();
}

public interface IOnlyOnHit
{
}

[Serializable]
public abstract class AbilityModule
{
	public virtual void BeforeExecute(Unit abilityUser)
	{
	}

	public virtual bool Execute(Unit abilityUser, Unit abilityTarget)
	{
		return true;
	}

	public virtual bool Execute(Unit abilityUser, Unit abilityTarget, bool hit)
	{
		return true;
	}

	public virtual void AfterExecute(Unit abilityUser, Ability ability)
	{
	}
}

[Serializable]
public class DamageModule : AbilityModule
{
	[Header("Damage Variables")]
	public int basePower;

	public int accuracy;
	public int bonusCriticalChance;
	public bool isWeaponAttack;
	public int statusBoost;
	public List<StatusType> boostingStatusTypes;

	public override bool Execute(Unit abilityUser, Unit abilityTarget)
	{
		if (isWeaponAttack)
			abilityUser.unitConditions.HandleWeaponCoating();

		bool hit;
		int damage = Mathf.CeilToInt(abilityUser.unitCombatCalculator.CalculateDamage(basePower, bonusCriticalChance,
			boostingStatusTypes, statusBoost, abilityTarget));
		hit = abilityUser.unitCombatCalculator.ApplyDamageOrMiss(accuracy, isWeaponAttack, abilityTarget, damage);
		return hit;
	}

	public List<int> CalculateDamage(Unit currentUnit, Unit abilityTarget)
	{
		List<int> damageWindow = new List<int>();
		int minDamage = Mathf.CeilToInt(currentUnit.unitCombatCalculator.CalculateDamage(basePower, bonusCriticalChance,
			boostingStatusTypes, statusBoost, abilityTarget, DamageWindow.Minimum));

		damageWindow.Add(abilityTarget.unitHealth.CalculateReducedDamage(minDamage));

		int maxDamage = Mathf.CeilToInt(currentUnit.unitCombatCalculator.CalculateDamage(basePower, bonusCriticalChance,
			boostingStatusTypes, statusBoost, abilityTarget, DamageWindow.Maximum));

		damageWindow.Add(abilityTarget.unitHealth.CalculateReducedDamage(maxDamage));

		return damageWindow;
	}

	public int CalculateAccuracy(Unit currentUnit, Unit abilityTarget)
	{
		return currentUnit.unitCombatCalculator.CalculateAccuracy(accuracy, abilityTarget);
	}
}

[Serializable]
public class ConditionModule : AbilityModule, IOnlyOnHit
{
	[Header("Conditions Variables")]
	public bool applyAllConditions;

	public List<Condition> conditions;
	public List<int> conditionChances;

	public override bool Execute(Unit abilityUser, Unit abilityTarget)
	{
		abilityUser.unitConditions.CheckAndApplyAbilityConditions(conditions, applyAllConditions, conditionChances,
			abilityTarget);
		return true;
	}

	public void ExecuteOnSelf(Unit abilityUser)
	{
		abilityUser.unitConditions.ApplyAbilityConditions(conditions, applyAllConditions, abilityUser);
	}
}

[Serializable]
public class HealModule : AbilityModule
{
	public int minHealAmount;
	public int maxHealAmount;
	public int bonusCriticalChance;

	public override bool Execute(Unit abilityUser, Unit abilityTarget)
	{
		abilityUser.unitCombatCalculator.ApplyHealing(minHealAmount, maxHealAmount, bonusCriticalChance, abilityTarget);
		return true;
	}
}


[Serializable]
public class CleanseModule : AbilityModule
{
	public int cleanseAmount;
	public ConditionType conditionTypeToCleanse;
	public StatusType statusTypeToCleanse;

	public override bool Execute(Unit abilityUser, Unit abilityTarget)
	{
		abilityUser.unitConditions.CleanseTarget(cleanseAmount, conditionTypeToCleanse, statusTypeToCleanse,
			abilityTarget);
		return true;
	}
}

[Serializable]
public class CoatModule : AbilityModule
{
	public WeaponCoating coating;

	public override void BeforeExecute(Unit abilityUser)
	{
		abilityUser.unitConditions.ApplyWeaponCoating(coating);
		abilityUser.unitUI.RefreshCoatingUI(true);
	}

	public override bool Execute(Unit abilityUser, Unit abilityTarget)
	{
		return true;
	}
}

[Serializable]
public class SwapModule : AbilityModule
{
	public List<int> abilityIndexes;

	public override bool Execute(Unit abilityUser, Unit abilityTarget)
	{
		foreach (int index in abilityIndexes)
		{
			abilityUser.unitAbilityManager.SwapAbilities(index, 4 + index);
		}

		return true;
	}
}

[Serializable]
public class SelfHitModule : AbilityModule
{
	public bool damageSelf;
	public bool applyConditionSelf;

	[Range(0f, 1f)]
	public float damagePercentage;

	public override void AfterExecute(Unit abilityUser, Ability ability)
	{
		if (damageSelf)
		{
			abilityUser.unitHealth.TakeDirectDamage(Mathf.CeilToInt(abilityUser.maxHP * damagePercentage),
				DamageType.Direct,
				abilityUser);
		}

		if (applyConditionSelf)
		{
			ConditionModule conditionModule = ability.modules.OfType<ConditionModule>().FirstOrDefault();
			conditionModule.ExecuteOnSelf(abilityUser);
		}
	}
}