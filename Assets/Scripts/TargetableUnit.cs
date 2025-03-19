using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class TargetableUnit : MonoBehaviour
{
	public Unit unit;
	private Renderer rend;
	private Color baseColor;
	public Color highlightColor = Color.yellow;
	public Color initialHighlightColor = Color.yellow;

	private void Awake()
	{
		rend = GetComponentInChildren<Renderer>();
		if (rend != null)
			baseColor = rend.material.color;
	}


	public void EnableSelectionHighlight(bool enable)
	{
		if (rend != null)
			rend.material.color = enable ? highlightColor : baseColor;
	}

	public void EnableInitialSelectionHighlight(bool enable)
	{
		if (rend != null)
			rend.material.color = enable ? initialHighlightColor : baseColor;
	}

	private void OnMouseDown()
	{
		if (!enabled) return;
		if (!TargetSelectionUI.Instance.isSelecting) return;

		TargetSelectionUI.Instance.OnUnitClicked(unit);
	}

	private void OnMouseEnter()
	{
		if (!enabled) return;
		if (!TargetSelectionUI.Instance.isSelecting) return;
		EnableSelectionHighlight(true);
	}

	private void OnMouseExit()
	{
		if (!enabled) return;
		if (!TargetSelectionUI.Instance.isSelecting) return;
		EnableInitialSelectionHighlight(true);
	}
}