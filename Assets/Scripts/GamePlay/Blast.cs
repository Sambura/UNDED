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
	public float scoreValue;
	[SerializeField] private bool randomRotation;

	private AudioSource audioSource;
	protected Controller controller;

	protected virtual void Start()
	{
		if (randomRotation)
			transform.Rotate(Vector3.back, Random.Range(0f, 360f));
		audioSource = GetComponent<AudioSource>();
		controller = FindObjectOfType<Controller>();
		Camera.main.gameObject.GetComponentInParent<CameraHandle>().Shake(explosionForce);
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

	protected virtual void InflictDamage()
	{
		var victims = Physics2D.CircleCastAll(transform.position, radius, Vector2.zero, 100, 512);
		foreach (var enemy in victims)
		{
			float dmg = Mathf.Max(0, damage - Mathf.Sqrt(Vector2.Distance(transform.position, enemy.transform.position)) * damageLose);
			if (enemy.collider.GetComponent<Entity>() is Enemy && gameObject.CompareTag("Enemy")) dmg *= 0.08f;
			enemy.collider.GetComponent<Entity>().GetHit(dmg, transform.position.x, damageType);
		}
		controller.IncreaseScore(Mathf.Max(0, victims.Length - 1) * scoreValue);
	}

	protected virtual IEnumerator Life()
	{
		/*
		float timeScale = 0.5f;
		Time.timeScale = timeScale;
		float startTime = Time.time;
		for (float a = timeScale; a < 1; a = Mathf.Lerp(timeScale, 1.05f, Time.time - startTime))
		{
			Time.timeScale = a;
			yield return null;
		}
		Time.timeScale = 1;*/
		yield return new WaitWhile(() => audioSource.isPlaying);
		Destroy(gameObject);
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.DrawWireSphere(transform.position, radius);
	}
}
