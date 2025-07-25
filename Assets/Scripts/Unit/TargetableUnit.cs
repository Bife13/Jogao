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

	public Ability currentAbility { get; set; }

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
		if (unit.unitType == UnitType.PLAYER)
		{
			// GameManager.Instance.combatUIManager.ShowPlayerUnitStats(unit);
			// GameManager.Instance.combatUIManager.ShowPlayerUnitItems(unit);
		}

		if (unit.unitType == UnitType.ENEMY)
			GameManager.Instance.combatUIManager.ShowEnemyUnitStats(unit);

		if (!enabled) return;
		if (!TargetSelectionUI.Instance.isSelecting) return;
		EnableSelectionHighlight(true);

		if (currentAbility != null)
			GameManager.Instance.combatUIManager.UpdateAbilityTooltip(currentAbility, unit);
	}

	private void OnMouseExit()
	{
		Unit currentUnit = GameManager.Instance.unitManager.currentUnit;
		if (unit.unitType == UnitType.PLAYER && currentUnit != null)
		{
			// GameManager.Instance.combatUIManager.ShowPlayerUnitStats(currentUnit);
			// GameManager.Instance.combatUIManager.ShowPlayerUnitItems(currentUnit);
		}

		if (unit.unitType == UnitType.ENEMY)
			GameManager.Instance.combatUIManager.HideEnemyStats();

		if (!enabled) return;
		if (!TargetSelectionUI.Instance.isSelecting) return;
		EnableInitialSelectionHighlight(true);

		GameManager.Instance.combatUIManager.ResetAbilityTooltip();
	}
}