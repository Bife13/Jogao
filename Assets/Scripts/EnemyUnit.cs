using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class EnemyUnit : Unit
{
	private Ability nextAbility;

	[SerializeField]
	private Image intentImage;

	void Start()
	{
		DecideNextIntent();
	}

	public void DecideNextIntent()
	{
		intentImage.enabled = true;
		nextAbility = _abilities[Random.Range(0, _abilities.Count)];
		intentImage.sprite = nextAbility.icon;
	}

	protected override void PerformAttackAnimation()
	{
		AttackAnimation(-1);
	}

	protected override void AfterAbilityUse(Ability ability, bool hit)
	{
		base.AfterAbilityUse(ability, hit);
		intentImage.enabled = false;
	}

	public void PerformIntent()
	{
		List<Unit> targets = GameManager.Instance.GetValidTargetsForEnemy(nextAbility);
		UseAbility(nextAbility, targets);
	}
}