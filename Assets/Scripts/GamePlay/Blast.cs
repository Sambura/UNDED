using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Blast : MonoBehaviour
{
	public float damage;
	public float damageLose;
	public float radius;
	public GameObject fragment;
	public int fragsCount;
	public float explosionForce;
	public DamageType damageType;

	private AudioSource audioSource;
	protected Controller controller;

	private void Start()
	{
		audioSource = GetComponent<AudioSource>();
		controller = FindObjectOfType<Controller>();
		Camera.main.gameObject.GetComponent<CameraHandle>().Shake(explosionForce);
		InflictDamage();
		for (int i = 0; i < fragsCount; i++)
		{
			var frag = Instantiate(fragment, transform.position, Quaternion.identity).GetComponent<Rigidbody2D>();
			float angle = Random.Range(0, Mathf.PI);
			frag.AddForce(new Vector2(explosionForce * Mathf.Cos(angle), explosionForce * Mathf.Sin(angle) / 2), ForceMode2D.Impulse);
			frag.AddTorque(Random.Range(-10f, 10f), ForceMode2D.Impulse);
			frag.GetComponentInParent<Grenade>().lifeTime += Random.Range(-0.15f, 0.5f);
		}
		StartCoroutine(Life());
	}

	protected virtual void DamageEntity(Entity entity)
	{
		float distance = Mathf.Abs(transform.position.x - entity.transform.position.x);
		if (distance <= radius + entity.collideWidth)
		{
			float dmg = Mathf.Max(0, damage - Mathf.Sqrt(distance) * damageLose);
			entity.GetHit(dmg, transform.position.x, damageType);
		}
	}

	protected virtual void InflictDamage()
	{
		DamageEntity(controller.Player);
		foreach (var enemy in controller.Enemies)
		{
			DamageEntity(enemy);
		}
	}

	private IEnumerator Life()
	{ 
		yield return new WaitWhile(() => audioSource.isPlaying);
		Destroy(gameObject);
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.DrawWireSphere(transform.position, radius);
	}
}
