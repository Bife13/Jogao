using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
	public Button button;
	public TMP_Text text;
	public TMP_Text cooldownTime;
	public Ability assignedAbility;
	public Image image;

	private void Awake()
	{
		button = GetComponent<Button>();
		button.onClick.AddListener(OnAbilityButtonClicked);
	}

	public void SetAbility(Ability newAbility, int cooldown)
	{
		assignedAbility = newAbility;
		image.sprite = assignedAbility.icon;
		if (cooldown == 0)
		{
			cooldownTime.text = "";
			button.interactable = true;
		}
		else
		{
			cooldownTime.text = cooldown.ToString();
			button.interactable = false;
		}
	}

	public void ResetAbility()
	{
		assignedAbility = null;
		text.text = "";
		image.sprite = null;
	}

	private void OnAbilityButtonClicked()
	{
		if (assignedAbility != null)
		{
			text.text = assignedAbility.abilityName;
			Debug.Log($"Clicked ability: {assignedAbility.abilityName}");
			// GameManager.Instance.combatUIManager.AddLog($"Clicked ability: {assignedAbility.abilityName}");

			TargetSelectionUI.Instance.StartTargetSelection(assignedAbility);
			GameManager.Instance.combatUIManager.ResetAbilityTooltip();
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		Tooltip.ShowToolTip_Static(assignedAbility.description);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		Tooltip.HideTooltip_Static();
	}
}