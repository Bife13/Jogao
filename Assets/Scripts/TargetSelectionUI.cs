using System.Collections.Generic;
using UnityEngine;

public class TargetSelectionUI : MonoBehaviour
{
	public static TargetSelectionUI Instance;

	public bool isSelecting = false;
	public Ability currentAbility;
	private List<Unit> validTargets = new List<Unit>();

	private void Awake()
	{
		if (Instance == null)
			Instance = this;
		else
			Destroy(gameObject);
	}

	public void StartTargetSelection(Ability ability)
	{
		isSelecting = true;
		currentAbility = ability;

		// Define your targeting logic based on ability type
		validTargets = TurnManager.Instance.GetValidTargets(ability);

		Debug.Log($"Select target(s) for: {ability.abilityName}");

		foreach (Unit u in validTargets)
		{
			u.GetComponent<TargetableUnit>().enabled = true;
			u.GetComponent<TargetableUnit>().EnableInitialSelectionHighlight(true);
		}
	}

	public void OnUnitClicked(Unit unit)
	{
		if (!isSelecting || !validTargets.Contains(unit))
		{
			Debug.Log("Invalid target selection.");
			return;
		}

		Debug.Log($"Target {unit.unitName} selected!");

		// Apply the ability now that the target is chosen
		PlayerUnit actingPlayer = TurnManager.Instance.GetCurrentPlayerUnit();
		actingPlayer.UseAbility(currentAbility, unit);

		EndTargetSelection();
	}

	public void EndTargetSelection()
	{
		isSelecting = false;

		foreach (Unit u in validTargets)
		{
			u.GetComponent<TargetableUnit>().EnableSelectionHighlight(false);
			u.GetComponent<TargetableUnit>().enabled = false;
		}

		validTargets.Clear();
		currentAbility = null;

		// Let the turn manager know the action is done
		// TurnManager.Instance.OnPlayerActionComplete();
	}
}