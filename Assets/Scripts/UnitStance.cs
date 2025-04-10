using UnityEngine;

public class UnitStance : MonoBehaviour
{
	public StanceType currentStance = StanceType.Neutral;
	public StanceType previewStance = StanceType.Neutral;
	public int stanceBonus = 20;
	public int stanceReduction = 10;
	public bool hasComittedStance = false;

	public void CommitStance()
	{
		if (hasComittedStance)
		{
			Debug.Log("You've already committed to a stance!");
			return;
		}

		if (previewStance != StanceType.Neutral)
		{
			currentStance = previewStance;
			hasComittedStance = true;
			Debug.Log($"Stance committed: {currentStance}");
		}
	}

	public float CalculateStanceBonusAttackAmount()
	{
		float modifier = 0f;
		switch (currentStance)
		{
			case StanceType.Offensive:
				modifier += stanceBonus; // + Bonus % damage
				break;
			case StanceType.Defensive:
				modifier -= stanceReduction; // - Bonus % damage
				break;
		}

		return modifier;
	}

	public float CalculateStanceBonusDefense()
	{
		float finalBonusDefense = 0;
		switch (currentStance)
		{
			case StanceType.Offensive:
				finalBonusDefense -= stanceReduction; // - Bonus defense
				break;
			case StanceType.Defensive:
				finalBonusDefense += stanceBonus; // + Bonus  defense
				break;
		}

		return finalBonusDefense;
	}
}