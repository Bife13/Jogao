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

public enum DamageType
{
	Direct,
	DoT
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
	private int speed;
	protected int minDamage;
	protected int maxDamage;
	protected int critChance;
	public UnitType unitType { get; private set; }

	[Header("UI Stuff")]
	public Image healthBar;

	[Header("Ability")]
	protected List<Ability> _abilities = new List<Ability>();

	public List<ActiveEffect> activeEffects = new List<ActiveEffect>();

	public GameObject statusIconPanel;
	public GameObject statusIconPrefab;

	private Dictionary<Effect, EffectIconUI> activeIcons = new Dictionary<Effect, EffectIconUI>();

	public int ModifiedSpeed => (int)(GetTotalModifiedStat(StatType.Speed, speed));

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

	public void TakeDamage(int damage, DamageType damageType)
	{
		int finalDamage = 0;
		switch (damageType)
		{
			case DamageType.Direct:
				float reducedDamage = damage * (GetTotalModifiedStat(StatType.Defense, baseDefense) / 100f);
				finalDamage = Mathf.Max(0, damage - Mathf.CeilToInt(reducedDamage));
				currentHP -= finalDamage;
				if (currentHP < 0)
				{
					currentHP = 0;
				}

				Debug.Log($"{unitName} took {finalDamage} damage! Remaining HP: {currentHP}");
				break;
			case DamageType.DoT:
				finalDamage = damage;
				currentHP -= finalDamage;
				if (currentHP < 0)
				{
					currentHP = 0;
				}

				Debug.Log($"{unitName} took {finalDamage} DoT damage! Remaining HP: {currentHP}");
				break;
		}

		DOTween.To(() => healthBar.fillAmount, x => healthBar.fillAmount = x, (float)currentHP / maxHP, 0.5f);
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
		float dodgeThreshold = target.GetTotalModifiedStat(StatType.Dodge, target.dodge);
		float currentAccuracy = GetTotalModifiedStat(StatType.Accuracy, accuracy);
		if (accuracyRoll <= abilityAccuracy + currentAccuracy - dodgeThreshold)
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

	public void ApplyEffect(Effect effect)
	{
		Debug.Log($"{unitName} gains {effect.effectName}");

		ActiveEffect existing = activeEffects.Find(e => e.effect == effect);
		if (existing != null)
		{
			if (effect.isStackable)
			{
				existing.remainingDuration += effect.turnDuration; // Increase duration
				RefreshStatusIcons();
				return;
			}

			existing.remainingDuration = effect.turnDuration; // Refresh duration
			RefreshStatusIcons();
			return;
		}

		activeEffects.Add(new ActiveEffect(effect));
		RefreshStatusIcons();
	}

	public void CheckAndApplyEffects(Ability ability, Unit target)
	{
		if (ability.effects.Count > 0)
			for (int i = 0; i < ability.effects.Count; i++)
			{
				float effectRoll = Random.Range(0f, 100f);
				if (ability.effects[i] != null && effectRoll <= ability.statusEffectChances[0])
					target.ApplyEffect(ability.effects[i]);
			}
	}

	public void ProcessEffectsPerTurn(EffectTiming effectTiming)
	{
		if (activeEffects.Count > 0)
		{
			for (int i = activeEffects.Count - 1; i >= 0; i--)
			{
				ActiveEffect appliedEffect = activeEffects[i];

				if (appliedEffect.effect.effectTiming == effectTiming)
				{
					if (appliedEffect.effect && appliedEffect.effect.statusDamagePerTurn > 0)
					{
						switch (appliedEffect.effect.statusEffect)
						{
							case StatusType.Poison:
								TakeDamage(appliedEffect.remainingDuration, DamageType.DoT);
								Debug.Log(
									$"{unitName} takes {appliedEffect.remainingDuration} damage from {appliedEffect.effect.effectName}");
								break;
							case StatusType.Bleed:
								TakeDamage(appliedEffect.remainingDuration, DamageType.DoT);
								Debug.Log(
									$"{unitName} takes {appliedEffect.remainingDuration} damage from {appliedEffect.effect.effectName}");
								break;
							case StatusType.Burn:
								TakeDamage(Mathf.CeilToInt(maxHP * (appliedEffect.effect.statusDamagePerTurn / 100f)),
									DamageType.DoT);
								Debug.Log(
									$"{unitName} takes {Mathf.CeilToInt(maxHP * (appliedEffect.effect.statusDamagePerTurn / 100f))} damage from {appliedEffect.effect.effectName}");
								break;
						}
					}

					appliedEffect.TickEffect();
					RefreshStatusIcons();

					if (appliedEffect.IsExpired())
					{
						Debug.Log($"{appliedEffect.effect.effectName} expired on {unitName}");
						activeEffects.Remove(appliedEffect);
						RefreshStatusIcons();
					}
				}
			}
		}
	}

	public float GetTotalModifiedStat(StatType statType, float initialValue)
	{
		float resultValue = initialValue;
		float modifier = 0;
		foreach (var activeEffect in activeEffects)
		{
			if (activeEffect.effect.statAffected == statType)
			{
				modifier += activeEffect.effect.amount;
			}
		}

		switch (statType)
		{
			case StatType.Speed:
			case StatType.Accuracy:
			case StatType.Crit:
			case StatType.Dodge:
			case StatType.Defense:
				resultValue += modifier;
				break;
			case StatType.Attack:
				modifier /= 100;
				resultValue *= modifier;
				break;
		}


		if (resultValue < 0)
			resultValue = 0;

		return resultValue;
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

	public void RefreshStatusIcons()
	{
		// Clear old icons that no longer exist
		List<Effect> effectsToRemove = new List<Effect>();
		foreach (var kvp in activeIcons)
		{
			bool stillActive = activeEffects.Exists(ae => ae.effect == kvp.Key);
			if (!stillActive)
			{
				Destroy(kvp.Value.gameObject);
				effectsToRemove.Add(kvp.Key);
			}
		}

		foreach (var effect in effectsToRemove)
			activeIcons.Remove(effect);

		// Update or Add icons
		foreach (var activeEffect in activeEffects)
		{
			if (activeIcons.ContainsKey(activeEffect.effect))
			{
				activeIcons[activeEffect.effect].UpdateDuration(activeEffect.remainingDuration);
			}
			else
			{
				var iconGO = Instantiate(statusIconPrefab, statusIconPanel.transform);
				var iconUI = iconGO.GetComponent<EffectIconUI>();
				iconUI.Setup(activeEffect.effect.effectIcon, activeEffect.remainingDuration);
				activeIcons.Add(activeEffect.effect, iconUI);
			}
		}
	}
}