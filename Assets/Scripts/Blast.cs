using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Blast : MonoBehaviour
{
	public float damage;
	public float lifeTime;
	public float damageLose;
	public float radius;

	private AudioSource audioSource;
	private float sqrRadius;

	private void Start()
	{
		audioSource = GetComponent<AudioSource>();
		sqrRadius = Sqr(radius);
		var player = FindObjectOfType<Player>();
		if(Sqr(transform.position.x - player.transform.position.x) +
				Sqr(transform.position.y - player.transform.position.y) <= sqrRadius)
		{
			float distance = Mathf.Pow(Sqr(transform.position.x - player.transform.position.x) +
				Sqr(transform.position.y - player.transform.position.y), 0.25f);
			float dmg = Mathf.Max(0, damage - distance * damageLose);
			player.GetHit(dmg, transform.position.x);
		}
		var enemies = FindObjectsOfType<Enemy>();
		foreach (var enemy in enemies)
		{
			if (enemy.IsDead) continue;
			float distance = Sqr(transform.position.x - enemy.transform.position.x) +
				Sqr(transform.position.y - enemy.transform.position.y);
			if(distance <= sqrRadius)
			{
				float dmg = Mathf.Max(0, damage - Mathf.Pow(distance, 0.25f) * damageLose);
				enemy.GetHit(dmg, transform.position.x);
			}
		}
		StartCoroutine(Life());
	}

	private float Sqr(float n)
	{
		return n * n;
	}
	
	private IEnumerator Life()
	{
		yield return new WaitForSeconds(lifeTime);
		Destroy(GetComponent<SpriteRenderer>());
		yield return new WaitWhile(() => audioSource.isPlaying);
		Destroy(gameObject);
	}
}
