using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
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

public enum StanceType
{
	Neutral = 0,
	Offensive = 1,
	Defensive = 2
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

	public Dictionary<Ability, int> abilityCooldowns = new Dictionary<Ability, int>();

	public GameObject highlightEffect;

	[SerializeField]
	private GameObject floatingTextPrefab;

	[SerializeField]
	private Transform unitCanvas;

	public int ModifiedSpeed => (int)(GetTotalModifiedStat(StatType.Speed, speed));

	[Header("Stance Variables")]
	public StanceType currentStance = StanceType.Neutral;

	public StanceType previewStance = StanceType.Neutral;


	public int stanceBonus = 20;
	public int shockAmount = 25;

	public int stanceReduction = 10;
	public bool hasComittedStance = false;

	public WeaponCoating activeCoating = null;
	public int coatingDuration = 0;
	public EffectIconUI coatingIcon;


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

	public virtual void UseAbility(Ability ability, List<Unit> targets)
	{
		BeforeAbility();

		bool hit = false;

		foreach (Unit target in targets)
		{
			Debug.Log($"{gameObject.name} uses {ability.abilityName} on {target.unitName}!");
			if (!isTargetValid(ability, target))
			{
				Debug.LogWarning($"Invalid target for ability {ability.abilityName}");
				return;
			}

			HandleWeaponCoating(ability);

			switch (ability.abilityEffectType)
			{
				case AbilityEffectType.Damage:
					PerformAttackAnimation();

					float damage = CalculateDamage(ability, target);
					hit = ApplyDamageOrMiss(ability, target, damage);
					break;

				case AbilityEffectType.Heal:
					ApplyHealing(ability, target);
					break;
				case AbilityEffectType.Buff:
				case AbilityEffectType.Debuff:
				case AbilityEffectType.StatusEffect:
					hit = PerformEffectApplication(ability, target);
					break;
			}

			if (ability.canCleanse)
			{
				CleanseTarget(target, ability);
			}
		}

		ApplyAbilityCooldown(ability);
		AfterAbilityUse(ability, hit);
	}

	protected virtual void BeforeAbility()
	{
		CommitStance();
	}

	protected virtual void PerformAttackAnimation()
	{
	}

	protected virtual void AfterAbilityUse(Ability ability, bool hit)
	{
		if (ability.debuffSelf && hit)
		{
			CheckAndApplyEffects(ability, this);
		}
	}

	protected virtual void HandleWeaponCoating(Ability ability)
	{
		if (activeCoating != null && ability.isWeaponAttack)
		{
			coatingDuration--;
			RefreshCoatingUI(true);
			Debug.Log($"{unitName}'s {activeCoating.coatingName}  has: " + coatingDuration);

			if (coatingDuration <= 0)
			{
				Debug.Log($"{unitName}'s {activeCoating.coatingName} coating has worn off.");
				activeCoating = null;
				RefreshCoatingUI(false);
			}
		}

		if (ability.canCoat)
		{
			ApplyWeaponCoating(ability.coating);
			RefreshCoatingUI(true);
		}
	}

	protected virtual float CalculateDamage(Ability ability, Unit target)
	{
		float damage = Random.Range(minDamage, maxDamage + 1);

		if (PerformCriticalHitCheck((int)GetTotalModifiedStat(StatType.Crit,
			    critChance + ability.bonusCritical)))
		{
			damage = (1.5f * maxDamage);
			Debug.Log($"{target.unitName} hit a Critical Strike");
		}

		float baseDamage = damage;

		damage += CalculateStanceBonusDamage(baseDamage);
		damage += (GetTotalModifiedStat(StatType.Attack, baseDamage) - baseDamage);
		damage += baseDamage * (ability.basePower / 100f);

		if (target.CheckForActiveEffects(target, ability.boostingEffects))
			damage += baseDamage * (ability.statusBoost / 100f);

		if (target.HasShockedDebuff())
		{
			damage += baseDamage * (shockAmount / 100f);
			target.ProcessEffectsPerTurn(EffectTiming.OnHit, 500);
		}

		return damage;
	}

	protected virtual bool ApplyDamageOrMiss(Ability ability, Unit target, float damage)
	{
		if (PerformAccuracyDodgeCheck(ability.accuracy, target) || target == this)
		{
			if (activeCoating != null && ability.isWeaponAttack)
			{
				int coatDamage = activeCoating.bonusDamage;
				if (HasCoatingBuff())
				{
					coatDamage *= CoatingBuffMultiplier();
				}

				damage += coatDamage;
				target.ApplyEffect(activeCoating.effect);
			}

			target.TakeDirectDamage(Mathf.CeilToInt(damage), DamageType.Direct, this);
			CheckAndApplyEffects(ability, target);
			return true;
		}

		GameManager.Instance.ActionChosen("Miss");
		return false;
	}

	protected virtual void ApplyHealing(Ability ability, Unit target)
	{
		float healAmount = Random.Range(ability.basePower, ability.maxPower);
		if (PerformCriticalHitCheck((int)GetTotalModifiedStat(StatType.Crit, 15 + ability.bonusCritical)))
			healAmount *= 2;

		target.Heal(Mathf.CeilToInt(healAmount));
		CheckAndApplyEffects(ability, target);
	}

	protected virtual bool PerformEffectApplication(Ability ability, Unit target)
	{
		if (ability.abilityEffectType == AbilityEffectType.Debuff && target != this)
		{
			if (!PerformAccuracyDodgeCheck(ability.accuracy, target))
				return false;
		}

		CheckAndApplyEffects(ability, target);
		return true;
	}

	protected virtual void CleanseTarget(Unit target, Ability ability)
	{
		int amount = ability.cleanseAmount <= 0 ? int.MaxValue : ability.cleanseAmount;
		target.Cleanse(amount, ability.effectTypeToCleanse, ability.statusTypeToCleanse);
		Debug.Log($"{target.unitName} is cleansed of {amount} debuffs!");
	}

	protected virtual void ApplyAbilityCooldown(Ability ability)
	{
		abilityCooldowns[ability] = ability.cooldown;
	}

	public bool isTargetValid(Ability ability, Unit target)
	{
		if (target == null)
		{
			Debug.LogWarning("No target selected!");
			return false;
		}

		if (!target.isAlive())
		{
			Debug.LogWarning($"{target.unitName} is already dead!");
			return false;
		}

		if (this is PlayerUnit)
			switch (ability.targetType)
			{
				case AbilityTargetType.Enemy:
					if (target.unitType != UnitType.ENEMY)
						return false;
					break;
				case AbilityTargetType.Ally:
					if (target.unitType != UnitType.PLAYER)
						return false;
					break;
				case AbilityTargetType.Self:
					if (target != this.GetComponent<Unit>())
						return false;
					break;
				case AbilityTargetType.AllEnemies:
					if (target.unitType != UnitType.ENEMY)
						if (!ability.hitsSelf)
							return false;
					break;
				case AbilityTargetType.AllAllies:
					if (target.unitType != UnitType.PLAYER)
						return false;
					break;
				default:
					return false;
			}
		else if (this is EnemyUnit)
		{
			switch (ability.targetType)
			{
				case AbilityTargetType.Enemy:
					if (target.unitType != UnitType.PLAYER)
						return false;
					break;
				case AbilityTargetType.Ally:
					if (target.unitType != UnitType.ENEMY)
						return false;
					break;
				case AbilityTargetType.Self:
					if (target != this.GetComponent<Unit>())
						return false;
					break;
				case AbilityTargetType.AllEnemies:
					if (target.unitType != UnitType.PLAYER)
						if (!ability.hitsSelf)
							return false;
					break;
				case AbilityTargetType.AllAllies:
					if (target.unitType != UnitType.ENEMY)
						return false;
					break;
				default:
					return false;
			}
		}

		return true;
	}

	public void TakeDoTDamage(int damage, DamageType damageType)
	{
		int finalDamage = 0;
		switch (damageType)
		{
			case DamageType.DoT:
				finalDamage = damage;
				currentHP -= finalDamage;
				if (!isAlive())
				{
					currentHP = 0;
				}

				Debug.Log($"{unitName} took {finalDamage} DoT damage! Remaining HP: {currentHP}");
				ShowFloatingText(finalDamage.ToString(), Color.yellow);
				break;
		}

		DOTween.To(() => healthBar.fillAmount, x => healthBar.fillAmount = x, (float)currentHP / maxHP, 0.5f);
		CheckDeath();
	}


	public void TakeDirectDamage(int damage, DamageType damageType, Unit originTarget)
	{
		int finalDamage = 0;
		switch (damageType)
		{
			case DamageType.Direct:
				float reducedDamage = damage *
				                      (GetTotalModifiedStat(StatType.Defense, CalculateStanceDefense(baseDefense)) /
				                       100f);
				finalDamage = Mathf.Max(0, damage - Mathf.CeilToInt(reducedDamage));
				currentHP -= finalDamage;
				Debug.Log($"{unitName} took {finalDamage} damage! Remaining HP: {currentHP}");
				ShowFloatingText(finalDamage.ToString(), Color.black);
				if (!isAlive())
				{
					currentHP = 0;
				}

				if (HasCounterBuff() && isAlive())
				{
					StartCoroutine(ExecuteCounterAttack(originTarget));
				}

				break;
		}

		DOTween.To(() => healthBar.fillAmount, x => healthBar.fillAmount = x, (float)currentHP / maxHP, 0.5f);
		CheckDeath();
	}

	protected virtual void CheckDeath()
	{
		if (!isAlive())
		{
			Destroy(gameObject, 1f);
			GameManager.Instance.allUnits.Remove(this);
		}
	}

	IEnumerator ExecuteCounterAttack(Unit originTarget)
	{
		yield return new WaitForSeconds(0.15f);

		Ability counterAbility = GetCounterBuff().effect.counterAbility;
		List<Unit> originTargets = new List<Unit>();
		originTargets.Add(originTarget);
		UseAbility(counterAbility, originTargets);
	}

	public void Heal(int amount)
	{
		currentHP += amount;
		if (currentHP > unitData.maxHP)
			currentHP = unitData.maxHP;

		ShowFloatingText(amount.ToString(), Color.blue);
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
		if (effect != null)
		{
			Debug.Log($"{unitName} gains {effect.effectName}");

			ActiveEffect existing = activeEffects.Find(e => e.effect.statusEffect == effect.statusEffect);
			if (existing != null)
			{
				if (effect.isStackable)
				{
					existing.remainingDuration += effect.turnDuration; // Increase duration
					RefreshStatusIcons();
					return;
				}

				if (existing.remainingDuration < effect.turnDuration)
					existing.remainingDuration = effect.turnDuration; // Refresh duration
				RefreshStatusIcons();
				return;
			}

			activeEffects.Add(new ActiveEffect(effect, GameManager.Instance.turnCounter));

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

			RefreshStatusIcons();
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
						target.ApplyEffect(ability.effects[i]);
				}
			else
			{
				int randomIndex = Random.Range(0, ability.effects.Count);
				float effectRoll = Random.Range(0f, 100f);
				if (ability.effects[randomIndex] != null && effectRoll <= ability.effectChances[randomIndex])
					target.ApplyEffect(ability.effects[randomIndex]);
			}
	}

	public bool CheckForActiveEffects(Unit target, List<Effect> effects)
	{
		if (target.activeEffects.Count > 0)
		{
			foreach (var effect in effects)
			{
				ActiveEffect existing = target.activeEffects.Find(e => e.effect.statusEffect == effect.statusEffect);
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
							TakeDoTDamage(appliedEffect.remainingDuration, DamageType.DoT);
							Debug.Log(
								$"{unitName} takes {appliedEffect.remainingDuration} damage from {appliedEffect.effect.effectName}");
							break;
						case StatusType.Bleed:
							TakeDoTDamage(appliedEffect.remainingDuration, DamageType.DoT);
							Debug.Log(
								$"{unitName} takes {appliedEffect.remainingDuration} damage from {appliedEffect.effect.effectName}");
							break;
						case StatusType.Burn:
							TakeDoTDamage(
								Mathf.CeilToInt(maxHP * (appliedEffect.effect.statusDamagePerTurn / 100f)),
								DamageType.DoT);
							Debug.Log(
								$"{unitName} takes {Mathf.CeilToInt(maxHP * (appliedEffect.effect.statusDamagePerTurn / 100f))} damage from {appliedEffect.effect.effectName}");
							break;
					}


					appliedEffect.TickEffect(currentTurn);
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

	//TODO CHECK THIS LOGIC HERE FOR NEGATIVE STUFF 
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
			case StatType.Defense:
				resultValue += modifier;
				break;
			case StatType.Crit:
			case StatType.Dodge:
				resultValue += modifier;
				if (resultValue < 0)
					resultValue = 0;
				break;
			case StatType.Attack:
				modifier += 100;
				modifier /= 100;
				resultValue *= modifier;
				break;
		}

		return resultValue;
	}

	public List<Ability> GetAbilities()
	{
		return _abilities;
	}

	public void SwapAbilities(int firstIndex, int secondIndex)
	{
		(_abilities[firstIndex], _abilities[secondIndex]) = (_abilities[secondIndex], _abilities[firstIndex]);
	}

	public void StartTurn()
	{
		List<Ability> keys = new List<Ability>(abilityCooldowns.Keys);
		foreach (var ability in keys)
		{
			abilityCooldowns[ability]--;
			if (abilityCooldowns[ability] <= 0)
				abilityCooldowns.Remove(ability);
		}
	}

	public void CommitStance()
	{
		if (hasComittedStance)
		{
			Debug.Log("You've already committed to a stance!");
			return;
		}

		if (previewStance != StanceType.Neutral)
		{
			currentStance = previewStance;
			hasComittedStance = true;
			Debug.Log($"Stance committed: {currentStance}");
		}
	}

	public bool CanUseAbility(Ability ability)
	{
		return !abilityCooldowns.ContainsKey(ability);
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

	public void RefreshCoatingUI(bool isActive)
	{
		coatingIcon.gameObject.SetActive(isActive);
		coatingIcon.Setup(activeCoating.coatingSprite, coatingDuration);
	}

	public void SetHighlight(bool isOn)
	{
		highlightEffect.SetActive(isOn);
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
							Debug.Log($"{unitName} had {activeEffects[i].effect.name} cleansed!");
							activeEffects.RemoveAt(i);
							RefreshStatusIcons();
							removed++;
							break;
						case EffectType.Status:
							if (activeEffects[i].effect.statusEffect == abilityStatusTypeToCleanse)
							{
								Debug.Log($"{unitName} had {activeEffects[i].effect.name} cleansed!");
								activeEffects.RemoveAt(i);
								RefreshStatusIcons();
								removed++;
							}

							break;
					}

					if (removed >= amount)
					{
						RefreshStatusIcons();
						break;
					}
				}
			}

		RefreshStatusIcons();
	}

	public void ApplyWeaponCoating(WeaponCoating coating)
	{
		activeCoating = coating;
		coatingDuration = activeCoating.coatDuration;
		Debug.Log($"{unitName} applied {coating.coatingName} coating to their weapon!");
		RefreshCoatingUI(true);
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

	public float CalculateStanceBonusDamage(float damage)
	{
		float modifier = 0f;
		switch (currentStance)
		{
			case StanceType.Offensive:
				modifier += (stanceBonus / 100f); // + Bonus % damage
				break;
			case StanceType.Defensive:
				modifier -= (stanceReduction / 100f); // - Bonus % damage
				break;
		}

		return damage * modifier;
	}

	public float CalculateStanceDefense(int defense)
	{
		float finalDefense = defense;
		switch (currentStance)
		{
			case StanceType.Offensive:
				finalDefense -= stanceReduction; // - Bonus defense
				break;
			case StanceType.Defensive:
				finalDefense += stanceBonus; // + Bonus  defense
				break;
		}

		return finalDefense;
	}

	public void ShowFloatingText(string message, Color color)
	{
		GameObject textObj = Instantiate(floatingTextPrefab, unitCanvas);

		// Set the position near the unit in world space
		Vector3 worldPosition = transform.position + Vector3.up * 2f;

		// Convert the world position to local position on the canvas (if needed)
		textObj.transform.position = worldPosition;

		textObj.GetComponent<FloatingText>().Show(message, color);
	}
}