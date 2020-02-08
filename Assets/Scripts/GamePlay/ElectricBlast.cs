using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElectricBlast : Blast
{
	protected override void InflictDamage()
	{
		var victims = Physics2D.CircleCastAll(transform.position, radius, Vector2.zero, 100, 512);

		damage *= 1 + 0.1f * victims.Length;
		damageLose *= 1 + 0.1f * victims.Length;

		foreach (var entity in victims)
		{
			float dmg = Mathf.Max(0, damage - Mathf.Sqrt(entity.distance) * damageLose);
			entity.collider.gameObject.GetComponent<Entity>().GetHit(dmg, transform.position.x, damageType);
		}
		controller.IncreaseScore(Mathf.Max(0, victims.Length - 1) * scoreValue);
	}
}
