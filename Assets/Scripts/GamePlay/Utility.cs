using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour
{
	public float healthPoints;
	public float collideWidth;
	public bool IsDead { get; protected set; }

    public virtual bool GetHit(float damage, float sourceX, DamageType damageType)
	{
		return false;
	}
}

public enum DamageType { Untagged, SolidBullet, PlasmBullet, Fire, Electricity, Poison, Melee, Explosion}

[System.Serializable]
public struct DamageSpec
{
	public DamageType damageType;
	public float multiplier;
}