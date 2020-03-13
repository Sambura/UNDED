using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrowerEnemy : Enemy
{
	[Header("Thrower parameters")]
	[SerializeField] protected GameObject grenade;
	[SerializeField] protected float throwProbability;
	[SerializeField] protected float throwForce = 50;
	[SerializeField] protected float throwAngle = Mathf.PI / 4;
	[SerializeField] protected float throwDelay;
	[SerializeField] protected Transform throwPoint;

	private float throwDistance;
	private float nextGrenade;

	protected override void Start()
	{
		base.Start();
		float cos = throwForce * Mathf.Cos(throwAngle);
		throwDistance = cos * grenade.GetComponent<Grenade>().lifeTime * 0.8f;
	}

	public void Throw()
	{
		var g = Instantiate(grenade, throwPoint.position, Quaternion.identity);
		var rb = g.GetComponent<Rigidbody2D>();
		rb.AddForce(new Vector2(
			throwForce * Mathf.Cos(throwAngle) * transform.right.x,
			throwForce * Mathf.Sin(throwAngle)),
			ForceMode2D.Impulse);
		rb.AddTorque(-5 * transform.right.x, ForceMode2D.Impulse);
		g.tag = gameObject.tag;
	}

	protected override void Update()
	{
		base.Update();
		if (!(IsDead || player.IsDead || isLocked))
		{
			float distance = Mathf.Abs(throwPoint.position.x - player.transform.position.x);
			if (Mathf.Abs(distance - throwDistance) < 2
				&& Random.value < throwProbability && Time.time >= nextGrenade)
			{
				animator.SetTrigger("Throw");
				nextGrenade = Time.time + throwDelay;
				isLocked = true;
			}

		}
	}
}