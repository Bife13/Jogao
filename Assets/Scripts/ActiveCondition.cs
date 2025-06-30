using System.Linq;
using UnityEngine;

public class ActiveCondition
{
	public Condition condition;
	public int remainingDuration;

	public bool grantsCounter => condition.effects.OfType<CounterattackEffect>().FirstOrDefault() != null;
	public bool isCoatingBuff => condition.effects.OfType<CoatingEffect>().FirstOrDefault() != null; // New property!

	public int turnCounter = 0;

	public ActiveCondition(Condition data, int applicationTurn)
	{
		condition = data;
		remainingDuration = data.turnDuration;
		turnCounter = applicationTurn;
	}

	public void TickCondition(int currentTurn)
	{
		if (currentTurn > turnCounter)
			remainingDuration--;
	}

	public bool IsExpired()
	{
		return remainingDuration <= 0;
	}
}