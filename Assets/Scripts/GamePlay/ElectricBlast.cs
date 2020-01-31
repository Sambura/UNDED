using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElectricBlast : Blast
{
	private float Distance(Entity entity)
	{
		return Mathf.Abs(transform.position.x - entity.transform.position.x);
	}

	protected override void InflictDamage()
	{
		Dictionary<Entity, float> victims = new Dictionary<Entity, float>();
		float distance = Distance(controller.Player);
		if (distance <= radius) victims.Add(controller.Player, distance);
		foreach (var enemy in controller.Enemies)
		{
			distance = Distance(enemy);
			if (distance <= radius + enemy.collideWidth) victims.Add(enemy, distance);
		}
		damage *= 1 + 0.1f * victims.Count;
		damageLose *= 1 + 0.1f * victims.Count;

		foreach (var entity in victims)
		{
			float dmg = Mathf.Max(0, damage - Mathf.Sqrt(entity.Value) * damageLose);
			entity.Key.GetHit(dmg, transform.position.x, damageType);
		}
	}
}
