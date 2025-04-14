using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UnitUI : MonoBehaviour
{
	private Unit unit;

	public GameObject statusIconPanel;
	public GameObject statusIconPrefab;

	private Dictionary<Effect, EffectIconUI> activeIcons = new Dictionary<Effect, EffectIconUI>();

	public GameObject highlightEffect;

	[SerializeField]
	public GameObject displayObject;

	[SerializeField]
	public GameObject unitSprite;

	[SerializeField]
	private GameObject floatingTextPrefab;

	[SerializeField]
	private Transform unitCanvas;

	public EffectIconUI coatingIcon;
	private Vector3 worldPositionForFloatingText;
	public Image healthBar;

	[SerializeField]
	private TMP_Text healthAmounts;

	public void Initialize(Unit owner)
	{
		unit = owner;
	}

	private void Start()
	{
		worldPositionForFloatingText = unitSprite.transform.position + Vector3.up * 2f;
	}

	public void UpdateHealthBar(int currentHP, int maxHP)
	{
		DOTween.To(() => healthBar.fillAmount, x => healthBar.fillAmount = x, (float)currentHP / maxHP, 0.5f);
		healthAmounts.text = currentHP + " / " + maxHP;
	}

	public void ShowFloatingText(string message, Color color)
	{
		GameObject textObj = Instantiate(floatingTextPrefab, unitCanvas);

		// Set the position near the unit in world space
		Vector3 worldPosition = worldPositionForFloatingText;

		// Convert the world position to local position on the canvas (if needed)
		textObj.transform.position = worldPosition;

		textObj.GetComponent<FloatingText>().Show(message, color);
	}

	public void RefreshStatusIcons()
	{
		// Clear old icons that no longer exist
		List<Effect> effectsToRemove = new List<Effect>();
		foreach (var kvp in activeIcons)
		{
			bool stillActive = unit.unitEffects.activeEffects.Exists(ae => ae.effect == kvp.Key);
			if (!stillActive)
			{
				Destroy(kvp.Value.gameObject);
				effectsToRemove.Add(kvp.Key);
			}
		}

		foreach (var effect in effectsToRemove)
			activeIcons.Remove(effect);

		// Update or Add icons
		foreach (var activeEffect in unit.unitEffects.activeEffects)
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
		if (unit.unitEffects.activeCoating)
			coatingIcon.Setup(unit.unitEffects.activeCoating.coatingSprite, unit.unitEffects.coatingDuration);
	}

	public void SetHighlight(bool isOn)
	{
		highlightEffect.SetActive(isOn);
	}
}