using UnityEngine;

public class ActiveEffect
{
	public Effect effect;
	public int remainingDuration;

	public bool grantsCounter => effect.grantsCounter;
	public bool isCoatingBuff => effect.isCoatingBuff; // New property!

	public int turnCounter = 0;

	public ActiveEffect(Effect data, int applicationTurn)
	{
		effect = data;
		remainingDuration = data.turnDuration;
		turnCounter = applicationTurn;
	}

	public void TickEffect(int currentTurn)
	{
		if (currentTurn > turnCounter)
			remainingDuration--;
	}

	public bool IsExpired()
	{
		return remainingDuration <= 0;
	}
}