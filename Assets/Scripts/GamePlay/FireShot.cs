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
	[System.NonSerialized] public float multiplier;
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

	private void OnTriggerStay2D(Collider2D collision)
	{
		var entity = collision.GetComponent<Entity>();
		float damage = damagePerSecond * multiplier + Random.Range(-damageDelta, damageDelta);
		float lose = Mathf.Abs(collision.transform.position.x - transform.position.x) * damageLose * multiplier;
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
