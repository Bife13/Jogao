using UnityEngine;

public class ActiveEffect
{
	public Effect effect;
	public int remainingDuration;
	
	public bool grantsCounter => effect.grantsCounter;
	public bool isCoatingBuff => effect.isCoatingBuff; // New property!

	public ActiveEffect(Effect data)
	{
		effect = data;
		remainingDuration = data.turnDuration;
	}

	public void TickEffect()
	{
		remainingDuration--;
	}
	
	public bool IsExpired()
	{
		return remainingDuration <= 0;
	}
}