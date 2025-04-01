using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ButtonHandler : MonoBehaviour
{
	public Button button;
	public TMP_Text text;
	public TMP_Text cooldownTime;
	public Ability assignedAbility;
	public Image image;

	private void Awake()
	{
		button = GetComponent<Button>();
		image = GetComponent<Image>();
		button.onClick.AddListener(OnAbilityButtonClicked);
	}

	public void SetAbility(Ability newAbility, int cooldown)
	{
		assignedAbility = newAbility;
		text.text = assignedAbility.abilityName;
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
			Debug.Log($"Clicked ability: {assignedAbility.abilityName}");
			// Pass the selected ability to the TargetSelection system
			TargetSelectionUI.Instance.StartTargetSelection(assignedAbility);
		}
	}
}