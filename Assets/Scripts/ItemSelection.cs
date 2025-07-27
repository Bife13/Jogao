using System.Collections.Generic;
using UnityEngine;

public class ItemSelection : MonoBehaviour
{
	[SerializeField]
	private ItemSelectionUI itemSelectionUI;

	private List<EquippableItem> selectionItems;

	[SerializeField]
	private List<EquippableItem> availableItems;

	private List<EquippableItem> currentlyAvailableItems;

	private bool isInitialized;

	public void EquipFirstItemLeft() => EquipItem(0, "LEFT");
	public void EquipFirstItemRight() => EquipItem(0, "RIGHT");
	public void EquipSecondItemLeft() => EquipItem(1, "LEFT");
	public void EquipSecondItemRight() => EquipItem(1, "RIGHT");
	public void EquipThirdItemLeft() => EquipItem(2, "LEFT");
	public void EquipThirdItemRight() => EquipItem(2, "RIGHT");

	public void Initialize()
	{
		if (!isInitialized)
		{
			itemSelectionUI.InitializeVisuals();
			selectionItems = new List<EquippableItem>();
		}
	}

	public void ActivateItemSelection()
	{
		itemSelectionUI.gameObject.SetActive(true);
		selectionItems.Clear();
		currentlyAvailableItems = new List<EquippableItem>(availableItems);

		// DO REMAINING PICK LOGIC etc
		for (int i = 0; i < 3; i++)
		{
			int itemIndex = Random.Range(0, currentlyAvailableItems.Count);
			selectionItems.Add(currentlyAvailableItems[itemIndex]);
			currentlyAvailableItems.RemoveAt(itemIndex);
			itemSelectionUI.UpdateItemIcon(i, selectionItems[i]);
		}
	}

	public void EquipItem(int index, string side)
	{
		if (index < 0 || index > selectionItems.Count)
		{
			Debug.Log("Invalid item index");
			return;
		}

		int unitIndex = side == "LEFT" ? 0 : side == "RIGHT" ? 1 : -1;
		if (unitIndex == -1)
		{
			Debug.Log("Invalid side value");
			return;
		}

		GameManager.Instance.unitManager.allUnits[unitIndex].unitInventory.equippedItems.Add(selectionItems[index]);
		availableItems.Remove(selectionItems[unitIndex]);
		DeactivateItemSelection();
	}

	public void DeactivateItemSelection()
	{
		itemSelectionUI.gameObject.SetActive(false);
		GameManager.Instance.combatManager.ContinueCombat();
	}
}