using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering;
using Random = UnityEngine.Random;

public class PlayerUnit : Unit
{
	public override void PerformAttackAnimation()
	{
		AttackAnimation(1);
	}
}