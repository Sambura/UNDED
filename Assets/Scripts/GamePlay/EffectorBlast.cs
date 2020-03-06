using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectorBlast : MonoBehaviour
{
	public float damagePerSecond;
	public float damageTime;
	public float lifeTime;
	public float damageDelta;
	public float damageDelay;
	public float explosionForce;
	public DamageType damageType;
	public float scoreValue;

	private AudioSource audioSource;
	private Controller controller;
	private ParticleSystem particle;
	private float radius;
	private ParticleSystem.Particle[] particles;

	private void Start()
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

	private void InflictDamage()
	{

		int count = particle.GetParticles(particles);
		for (int i = 0; i < count; i++)
		{
			radius = Mathf.Max(radius, Mathf.Abs(particles[i].position.x));
		}
		var victims = Physics2D.CircleCastAll(transform.position, radius, Vector2.zero, 200, 512);
		foreach (var entity in victims)
		{
			float dmg = Mathf.Max(0, (damagePerSecond + Random.Range(-damageDelta, damageDelta)) * damageDelay);
			entity.collider.GetComponent<Entity>().GetHit(dmg, transform.position.x, damageType);
		}
		controller.IncreaseScore(Mathf.Max(0, victims.Length - 1) * scoreValue);
	}

	private IEnumerator Life()
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
