using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectorBlast : Blast
{
	public float damageTime;
	public float lifeTime;
	public float damageDelta;
	public float damageDelay;

	private AudioSource audioSource;
	private ParticleSystem particle;
	private ParticleSystem.Particle[] particles;

	protected override void Start()
	{
		audioSource = GetComponent<AudioSource>();
		controller = FindObjectOfType<Controller>();
		particle = GetComponent<ParticleSystem>();
		particles = new ParticleSystem.Particle[particle.main.maxParticles];
		Camera.main.GetComponentInParent<CameraHandle>().Shake(explosionForce);
		if (controller.LevelHeight - Mathf.Abs(transform.position.y) < 7)
			transform.Translate(new Vector2(0, -7 * Mathf.Sign(transform.position.y)));
		StartCoroutine(Life());
	}

	protected override void InflictDamage()
	{
		int count = particle.GetParticles(particles);
		for (int i = 0; i < count; i++)
		{
			radius = Mathf.Max(radius, Mathf.Abs(particles[i].position.x));
		}
		var victims = Physics2D.CircleCastAll(transform.position, radius, Vector2.zero, 200, 512);
		foreach (var entity in victims)
		{
			float dmg = Mathf.Max(0, (damage + Random.Range(-damageDelta, damageDelta)) * damageDelay);
			entity.collider.GetComponent<Entity>().GetHit(dmg, transform.position.x, damageType);
		}
		controller.IncreaseScore(Mathf.Max(0, victims.Length - 1) * scoreValue);
	}

	protected override IEnumerator Life()
	{
		float startTime = Time.time;
		while (Time.time - startTime < damageTime)
		{
			InflictDamage();
			yield return new WaitForSeconds(damageDelay);
		}
		yield return new WaitForSeconds(lifeTime - damageTime);
		yield return new WaitWhile(() => audioSource.isPlaying);
		Destroy(gameObject);
	}
}
