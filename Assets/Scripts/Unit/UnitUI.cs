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

	private Dictionary<Condition, ConditionIconUI> activeIcons = new Dictionary<Condition, ConditionIconUI>();

	public GameObject highlightEffect;

	[SerializeField]
	public GameObject displayObject;

	[SerializeField]
	public GameObject unitSprite;

	[SerializeField]
	private GameObject floatingTextPrefab;

	[SerializeField]
	private Transform unitCanvas;

	public ConditionIconUI coatingIcon;
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
		List<Condition> conditionsToRemove = new List<Condition>();
		foreach (var kvp in activeIcons)
		{
			bool stillActive = unit.unitConditions.activeConditions.Exists(ae => ae.condition == kvp.Key);
			if (!stillActive)
			{
				Destroy(kvp.Value.gameObject);
				conditionsToRemove.Add(kvp.Key);
			}
		}

		foreach (Condition condition in conditionsToRemove)
			activeIcons.Remove(condition);

		// Update or Add icons
		foreach (ActiveCondition activeCondition in unit.unitConditions.activeConditions)
		{
			if (activeIcons.ContainsKey(activeCondition.condition))
			{
				activeIcons[activeCondition.condition].UpdateDuration(activeCondition.remainingDuration);
			}
			else
			{
				var iconGO = Instantiate(statusIconPrefab, statusIconPanel.transform);
				var iconUI = iconGO.GetComponent<ConditionIconUI>();
				iconUI.Setup(activeCondition.condition.conditionIcon, activeCondition.remainingDuration);
				activeIcons.Add(activeCondition.condition, iconUI);
			}
		}
	}

	public void RefreshCoatingUI(bool isActive)
	{
		coatingIcon.gameObject.SetActive(isActive);
		if (unit.unitConditions.activeCoating)
			coatingIcon.Setup(unit.unitConditions.activeCoating.coatingSprite, unit.unitConditions.coatingDuration);
	}

	public void SetHighlight(bool isOn)
	{
		highlightEffect.SetActive(isOn);
	}
}