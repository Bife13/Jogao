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
		validTargets = GameManager.Instance.unitManager.GetValidTargets(ability);

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
		PlayerUnit actingPlayer = GameManager.Instance.unitManager.GetCurrentPlayerUnit();
		switch (currentAbility.targetType)
		{
			case AbilityTargetType.Ally:
			case AbilityTargetType.Enemy:
			case AbilityTargetType.Self:
				List<Unit> targets = new List<Unit>();
				targets.Add(unit);
				if (!actingPlayer.unitAbilityManager.CanUseAbility(currentAbility))
				{
					Debug.LogWarning($"{currentAbility.abilityName} is in cooldown");
					break;
				}

				if (!actingPlayer.unitAbilityManager.isTargetValid(currentAbility, targets[0]))
				{
					Debug.LogWarning($"Invalid target for ability {currentAbility.abilityName}");
					break;
				}

				actingPlayer.unitAbilityManager.UseAbility(currentAbility, targets);
				EndTargetSelection();

				break;
			case AbilityTargetType.AllAllies:
			case AbilityTargetType.AllEnemies:
				if (!actingPlayer.unitAbilityManager.CanUseAbility(currentAbility))
				{
					Debug.LogWarning($"{currentAbility.abilityName} is in cooldown");
					break;
				}

				bool correctTargets = false;
				foreach (Unit target in validTargets)
				{
					if (actingPlayer.unitAbilityManager.isTargetValid(currentAbility, target))
						correctTargets = true;
				}

				if (correctTargets)
				{
					actingPlayer.unitAbilityManager.UseAbility(currentAbility, validTargets);
					EndTargetSelection();
				}
				else
					Debug.LogWarning($"Invalid targets for ability {currentAbility.abilityName}");

				break;
		}
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