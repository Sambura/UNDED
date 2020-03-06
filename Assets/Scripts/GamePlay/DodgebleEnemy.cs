using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DodgebleEnemy : Enemy
{
	[Header("Dodgeble parameters")]
	[SerializeField] protected float dodgePossibility;
	[SerializeField] protected float dodgeTime;

	protected float nextDodge;

	public override bool GetHit(float damage, float x, DamageType damageType)
	{
		if (hp <= 0) return true;
		if ((Random.value < dodgePossibility) && (Time.time >= nextDodge)
			&& (damageType == DamageType.SolidBullet || damageType == DamageType.PlasmBullet))
		{
			animator.SetTrigger("Dodge");
			isLocked = true;
			nextDodge = Time.time + dodgeTime;
			return false;
		}
		return base.GetHit(damage, x, damageType);
	}
}