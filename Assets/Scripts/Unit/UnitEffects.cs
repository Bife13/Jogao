using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class UnitEffects : MonoBehaviour
{
	private Unit unit;
	public List<ActiveEffect> activeEffects = new List<ActiveEffect>();
	public WeaponCoating activeCoating = null;
	public int coatingDuration = 0;

	public void Initialize(Unit owner)
	{
		unit = owner;
	}

	public void ApplyEffect(Effect effect)
	{
		if (effect != null)
		{
			Debug.Log($"{unit.unitName} gains {effect.effectName}");

			ActiveEffect existing = activeEffects.Find(e => e.effect.statusEffect == effect.statusEffect);
			if (existing != null)
			{
				if (effect.isStackable)
				{
					existing.remainingDuration += effect.turnDuration; // Increase duration
					unit.unitUI.RefreshStatusIcons();
					return;
				}

				if (existing.remainingDuration < effect.turnDuration)
					existing.remainingDuration = effect.turnDuration; // Refresh duration
				unit.unitUI.RefreshStatusIcons();
				return;
			}

			activeEffects.Add(new ActiveEffect(effect, GameManager.Instance.turnManager.turnCounter));

			if (effect.isCoatingBuff)
			{
				Debug.Log("TEST1");
				coatingDuration--;
				if (activeCoating == null)
				{
					ApplyWeaponCoating(effect.weaponCoating);
					Debug.Log("Applied Coating");
				}
			}

			unit.unitUI.RefreshStatusIcons();
		}
	}

	public void CheckAndApplyEffects(Ability ability, Unit target)
	{
		if (ability.effects.Count > 0)
			if (ability.applyAllEffects)
				for (int i = 0; i < ability.effects.Count; i++)
				{
					float effectRoll = Random.Range(0f, 100f);
					if (ability.effects[i] != null && effectRoll <= ability.effectChances[i])
						target.unitEffects.ApplyEffect(ability.effects[i]);
				}
			else
			{
				int randomIndex = Random.Range(0, ability.effects.Count);
				float effectRoll = Random.Range(0f, 100f);
				if (ability.effects[randomIndex] != null && effectRoll <= ability.effectChances[randomIndex])
					target.unitEffects.ApplyEffect(ability.effects[randomIndex]);
			}
	}

	public bool CheckForActiveEffects(Unit target, List<Effect> effects)
	{
		if (target.unitEffects.activeEffects.Count > 0)
		{
			foreach (var effect in effects)
			{
				ActiveEffect existing =
					target.unitEffects.activeEffects.Find(e => e.effect.statusEffect == effect.statusEffect);
				if (existing == null)
					return false;
			}

			return true;
		}

		return false;
	}

	public void ProcessEffectsPerTurn(EffectTiming effectTiming, int currentTurn)
	{
		if (activeEffects.Count > 0)
		{
			for (int i = activeEffects.Count - 1; i >= 0; i--)
			{
				ActiveEffect appliedEffect = activeEffects[i];

				if (appliedEffect.effect.effectTiming == effectTiming)
				{
					switch (appliedEffect.effect.statusEffect)
					{
						case StatusType.Poison:

							unit.unitHealth.TakeDoTDamage(appliedEffect.remainingDuration, DamageType.DoT);
							Debug.Log(
								$"{unit.unitName} takes {appliedEffect.remainingDuration} damage from {appliedEffect.effect.effectName}");
							break;
						case StatusType.Bleed:
							unit.unitHealth.TakeDoTDamage(appliedEffect.remainingDuration, DamageType.DoT);
							Debug.Log(
								$"{unit.unitName} takes {appliedEffect.remainingDuration} damage from {appliedEffect.effect.effectName}");
							break;
						case StatusType.Burn:
							unit.unitHealth.TakeDoTDamage(
								Mathf.CeilToInt(unit.maxHP * (appliedEffect.effect.statusDamagePerTurn / 100f)),
								DamageType.DoT);
							Debug.Log(
								$"{unit.unitName} takes {Mathf.CeilToInt(unit.maxHP * (appliedEffect.effect.statusDamagePerTurn / 100f))} damage from {appliedEffect.effect.effectName}");
							break;
					}


					appliedEffect.TickEffect(currentTurn);
					unit.unitUI.RefreshStatusIcons();

					if (appliedEffect.IsExpired())
					{
						Debug.Log($"{appliedEffect.effect.effectName} expired on {unit.unitName}");
						activeEffects.Remove(appliedEffect);
						unit.unitUI.RefreshStatusIcons();
					}
				}
			}
		}
	}

	public virtual bool PerformEffectApplication(Ability ability, Unit target)
	{
		if (ability.abilityEffectType == AbilityEffectType.Debuff && target != this)
		{
			if (!unit.unitCombatCalculator.PerformAccuracyDodgeCheck(ability.accuracy, target))
				return false;
		}

		CheckAndApplyEffects(ability, target);
		return true;
	}

	public virtual void CleanseTarget(Unit target, Ability ability)
	{
		int amount = ability.cleanseAmount <= 0 ? int.MaxValue : ability.cleanseAmount;
		target.unitEffects.Cleanse(amount, ability.effectTypeToCleanse, ability.statusTypeToCleanse);
		Debug.Log($"{target.unitName} is cleansed of {amount} debuffs!");
	}

	public void Cleanse(int amount, EffectType abilityEffectTypeToCleanse, StatusType abilityStatusTypeToCleanse)
	{
		int removed = 0;
		if (activeEffects.Count > 0)
			for (int i = activeEffects.Count - 1; i >= 0; i--)
			{
				if (activeEffects[i].effect.effectType == abilityEffectTypeToCleanse)
				{
					switch (abilityEffectTypeToCleanse)
					{
						case EffectType.Buff:
						case EffectType.Debuff:
							Debug.Log($"{unit.unitName} had {activeEffects[i].effect.name} cleansed!");
							activeEffects.RemoveAt(i);
							unit.unitUI.RefreshStatusIcons();
							removed++;
							break;
						case EffectType.Status:
							if (activeEffects[i].effect.statusEffect == abilityStatusTypeToCleanse)
							{
								Debug.Log($"{unit.unitName} had {activeEffects[i].effect.name} cleansed!");
								activeEffects.RemoveAt(i);
								unit.unitUI.RefreshStatusIcons();
								removed++;
							}

							break;
					}

					if (removed >= amount)
					{
						unit.unitUI.RefreshStatusIcons();
						break;
					}
				}
			}

		unit.unitUI.RefreshStatusIcons();
	}


	public bool HasCounterBuff()
	{
		foreach (var activeEffect in activeEffects)
		{
			if (activeEffect.grantsCounter)
				return true;
		}

		return false;
	}

	public bool HasCoatingBuff()
	{
		foreach (var activeEffect in activeEffects)
		{
			if (activeEffect.isCoatingBuff)
				return true;
		}

		return false;
	}

	public bool HasShockedDebuff()
	{
		foreach (var activeEffect in activeEffects)
		{
			if (activeEffect.effect.statusEffect == StatusType.Shock)
				return true;
		}

		return false;
	}

	public bool HasStunDebuff()
	{
		foreach (var activeEffect in activeEffects)
		{
			if (activeEffect.effect.statusEffect == StatusType.Stun)
				return true;
		}

		return false;
	}

	public int CoatingBuffMultiplier()
	{
		foreach (var activeEffect in activeEffects)
		{
			if (activeEffect.isCoatingBuff)
				return activeEffect.effect.amount;
		}

		return 1;
	}

	public ActiveEffect GetCounterBuff()
	{
		return activeEffects.FirstOrDefault(e => e.grantsCounter);
	}

	public virtual void HandleWeaponCoating(Ability ability)
	{
		if (activeCoating != null && ability.isWeaponAttack)
		{
			coatingDuration--;
			unit.unitUI.RefreshCoatingUI(true);
			Debug.Log($"{unit.unitName}'s {activeCoating.coatingName}  has: " + coatingDuration);

			if (coatingDuration <= 0)
			{
				Debug.Log($"{unit.unitName}'s {activeCoating.coatingName} coating has worn off.");
				activeCoating = null;
				unit.unitUI.RefreshCoatingUI(false);
			}
		}

		if (ability.canCoat)
		{
			ApplyWeaponCoating(ability.coating);
			unit.unitUI.RefreshCoatingUI(true);
		}
	}

	public void ApplyWeaponCoating(WeaponCoating coating)
	{
		activeCoating = coating;
		coatingDuration = activeCoating.coatDuration;
		Debug.Log($"{unit.unitName} applied {coating.coatingName} coating to their weapon!");
		unit.unitUI.RefreshCoatingUI(true);
	}
}