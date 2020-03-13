using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElectricBlast : Blast
{
	public float damageIncrease = 1.01f;

	protected override void InflictDamage()
	{
		var victims = Physics2D.CircleCastAll(transform.position, radius, Vector2.zero, 100, 512);

		damage *= 1 + damageIncrease * victims.Length;
		damageLose *= 1 + damageIncrease * victims.Length;

		foreach (var entity in victims)
		{
			float dmg = Mathf.Max(0, damage - Mathf.Sqrt(entity.distance) * damageLose);
			entity.collider.gameObject.GetComponent<Entity>().GetHit(dmg, transform.position.x, damageType);
		}
		controller.IncreaseScore(Mathf.Max(0, victims.Length - 1) * scoreValue);
	}
}
