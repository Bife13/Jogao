using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Tooltip : MonoBehaviour
{
	private static Tooltip instance;

	[SerializeField]
	private Camera uiCamera;

	[SerializeField]
	private TMP_Text tooltipText;

	[SerializeField]
	private RectTransform backgroundRectTransform;

	private void Awake()
	{
		instance = this;
		HideTooltip();
	}

	private void Update()
	{
		Vector2 localPoint;
		RectTransformUtility.ScreenPointToLocalPointInRectangle(transform.parent.GetComponent<RectTransform>(),
			Input.mousePosition, uiCamera, out localPoint);

		transform.localPosition = localPoint;
	}

	public void ShowTooltip(string tooltipString)
	{
		gameObject.SetActive(true);

		tooltipText.text = tooltipString;
		float textPaddingSize = 8f;
		Vector2 backgroundSize = new Vector2(tooltipText.rectTransform.sizeDelta.x + textPaddingSize * 2f,
			tooltipText.preferredHeight + textPaddingSize * 2f);
		backgroundRectTransform.sizeDelta = backgroundSize;
	}

	public void HideTooltip()
	{
		gameObject.SetActive(false);
	}

	public static void ShowToolTip_Static(String tooltipString)
	{
		instance.ShowTooltip(tooltipString);
	}

	public static void HideTooltip_Static()
	{
		instance.HideTooltip();
	}
}