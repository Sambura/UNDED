using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireShot : MonoBehaviour
{
	public float damagePerSecond;
	public float damageLose;
	public float damageDelta;
	public float initialDamage;
	public float warmUpTime;
	public DamageType damageType;
	private ParticleSystem system;
	private Animator animator;

	private float startTime;

	void Start()
	{
		system = GetComponent<ParticleSystem>();
		animator = GetComponent<Animator>();
		startTime = Time.time;
	}

	public void MultiplyDamage(float multiplier)
	{
		damagePerSecond *= multiplier;
		damageLose *= multiplier;
	}

	private void OnTriggerStay2D(Collider2D collision)
	{
		var entity = collision.GetComponent<Entity>();
		float damage = damagePerSecond + Random.Range(-damageDelta, damageDelta);
		float lose = Mathf.Abs(collision.transform.position.x - transform.position.x) * damageLose;
		if (entity != null)
		{
			entity.GetHit(Mathf.Max(Mathf.Lerp(initialDamage, damage, (Time.time - startTime) / warmUpTime) - lose, 0) * Time.fixedDeltaTime, transform.position.x, damageType);
		}
	}

	public void Finish()
	{
		system.Stop(false, ParticleSystemStopBehavior.StopEmitting);
		animator.SetTrigger("EndFire");
		StartCoroutine(Destroyer());
	}

	private IEnumerator Destroyer()
	{
		yield return new WaitUntil(() => system.particleCount == 0);
		Destroy(gameObject);
	}

}
