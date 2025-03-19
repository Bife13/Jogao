using System;
using UnityEngine;
using UnityEngine.UI;

public class ButtonHandler : MonoBehaviour
{
	public Button button;
	public Ability assignedAbility;

	private void Awake()
	{
		button = GetComponent<Button>();
		button.onClick.AddListener(OnAbilityButtonClicked);
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