using System.Collections.Generic;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class EnemyUnit : Unit
{
	private Ability nextAbility;

	[SerializeField]
	private Image intentImage;

	public override void Start()
	{
		base.Start();
		DecideNextIntent();
	}

	public void DecideNextIntent()
	{
		intentImage.enabled = true;
		nextAbility = unitAbilityManager._abilities[Random.Range(0, unitAbilityManager._abilities.Count)];
		intentImage.sprite = nextAbility.icon;
	}

	public override void PerformAttackAnimation()
	{
		AttackAnimation(-1);
	}

	public override void AfterAbilityUse(Ability ability, bool hit)
	{
		base.AfterAbilityUse(ability, hit);
		intentImage.enabled = false;
	}

	public void PerformIntent()
	{
		List<Unit> targets = GameManager.Instance.unitManager.GetValidTargetsForEnemy(nextAbility);
		unitAbilityManager.UseAbility(nextAbility, targets);
	}
}