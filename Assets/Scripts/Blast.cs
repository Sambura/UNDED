using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Blast : MonoBehaviour
{
	public float damage;
	public float lifeTime;
	public float damageLose;
	public float radius;
	public GameObject fragment;
	public int fragsCount;
	public float explosionForce;

	private AudioSource audioSource;
	private Controller controller;

	private void Start()
	{
		audioSource = GetComponent<AudioSource>();
		controller = FindObjectOfType<Controller>();
		var player = FindObjectOfType<Player>();
		float distance = Mathf.Abs(transform.position.x - player.transform.position.x);
		if (distance <= radius)
		{
			distance = Mathf.Sqrt(distance);
			float dmg = Mathf.Max(0, damage - distance * damageLose);
			player.GetHit(dmg, transform.position.x);
		}
		foreach (var enemy in controller.Enemies)
		{
			distance = Mathf.Abs(transform.position.x - enemy.transform.position.x);
			if (distance <= radius)
			{
				float dmg = Mathf.Max(0, damage - Mathf.Sqrt(distance) * damageLose);
				enemy.GetHit(dmg, transform.position.x);
			}
		}
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

	private IEnumerator Life()
	{
		yield return new WaitForSeconds(lifeTime);
		Destroy(GetComponent<SpriteRenderer>());
		yield return new WaitWhile(() => audioSource.isPlaying);
		Destroy(gameObject);
	}
}
