using UnityEngine;

public class PlayerUnit : Unit
{
	public void PerformAbility1(Unit target)
	{
		int damage = Random.Range(minDamage, maxDamage);
		target.TakeDamage(damage);
		TurnManager.Instance.OnPlayerActionChosen("Attacked");
		Debug.Log($"{unitName} attacks {target.unitName} for {damage} damage!");
	}
}