using UnityEngine;
using UnityEngine.EventSystems;

public class TooltipHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
	private string stringToDisplay;

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (stringToDisplay != null)
			Tooltip.ShowToolTip_Static(stringToDisplay);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		Tooltip.HideTooltip_Static();
	}

	public void SetToolTipString(string newString)
	{
		stringToDisplay = newString;
	}
}