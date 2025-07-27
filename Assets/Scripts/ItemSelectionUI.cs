using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemSelectionUI : MonoBehaviour
{
	[SerializeField]
	private List<Button> leftButtons;

	[SerializeField]
	private List<Button> rightButtons;

	[SerializeField]
	private List<GameObject> itemIcons;

	public void InitializeVisuals()
	{
		foreach (Button button in leftButtons)
		{
			Image buttonImage = button.GetComponent<Image>();
			buttonImage.sprite = GameManager.Instance.unitManager.allUnits[0].unitSprite;
		}

		foreach (Button button in rightButtons)
		{
			Image buttonImage = button.GetComponent<Image>();
			buttonImage.sprite = GameManager.Instance.unitManager.allUnits[1].unitSprite;
		}
	}

	public void UpdateItemIcon(int index, EquippableItem equippableItem)
	{
		itemIcons[index].GetComponent<Image>().sprite = equippableItem.icon;
		itemIcons[index].GetComponent<TooltipHandler>().SetToolTipString(equippableItem.description);
	}
}