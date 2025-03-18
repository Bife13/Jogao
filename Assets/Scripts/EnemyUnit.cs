using UnityEngine;

public class EnemyUnit : Unit
{
	void Start()
	{
		DecideNextIntent();
	}
	
	public void DecideNextIntent()
	{
		// int damage = Random.Range(minDamage, maxDamage);
		// target.TakeDamage(damage);
		// Debug.Log($"{unitName} attacks {target.unitName} for {damage} damage!");
	}

	public void PerformIntent(Unit target)
	{
		int damage = Random.Range(minDamage, maxDamage);
		Debug.Log($"{unitName} attacks {target.unitName} for {damage} damage!");
		target.TakeDamage(damage);
	}

}
