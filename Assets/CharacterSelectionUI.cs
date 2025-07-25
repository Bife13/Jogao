using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class CharacterSelectionUI : MonoBehaviour
{
	[SerializeField]
	private Button[] characterButtons;

	private List<UnityAction> unityActions;

	public void Awake()
	{
		Initialize();
	}

	public void Initialize()
	{
		characterButtons = gameObject.GetComponentsInChildren<Button>();
		for (int i = 0; i < characterButtons.Length; i++)
		{
			int index = i;
			characterButtons[i].onClick.AddListener(() => GameManager.Instance.unitManager.SpawnPlayerCharacter(index));
			characterButtons[i].onClick.AddListener(() => SwapButtonSkill(index));
		}
	}

	public void SwapButtonSkill(int index)
	{
		characterButtons[index].onClick.RemoveAllListeners();
	}
}