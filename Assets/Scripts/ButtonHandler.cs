using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ButtonHandler : MonoBehaviour
{
	public Button button;
	public TMP_Text text;
	public Ability assignedAbility;
	public Image image;

	private void Awake()
	{
		button = GetComponent<Button>();
		text = GetComponentInChildren<TMP_Text>();
		image = GetComponent<Image>();
		button.onClick.AddListener(OnAbilityButtonClicked);
	}

	public void SetAbility(Ability newAbility)
	{
		assignedAbility = newAbility;
		text.text = assignedAbility.abilityName;
		image.sprite = assignedAbility.icon;
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