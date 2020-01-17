using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectorBlast : MonoBehaviour
{
	public float damagePerSecond;
	public float damageTime;
	public float lifeTime;
	public float damageDelta;
	public float radius;
	public float damageDelay;
	public float explosionForce;

	private AudioSource audioSource;
	private Controller controller;

	private void Start()
	{
		audioSource = GetComponent<AudioSource>();
		controller = FindObjectOfType<Controller>();
		audioSource.volume *= controller.sfxVolume;
		Camera.main.gameObject.GetComponent<CameraHandle>().Shake(explosionForce);
		if (controller.LevelHeight - Mathf.Abs(transform.position.y) < 7)
			transform.Translate(new Vector2(0, -7 * Mathf.Sign(transform.position.y)));
		StartCoroutine(Life());
	}

	private void InflictDamage()
	{
		var player = FindObjectOfType<Player>();
		float distance = Mathf.Abs(transform.position.x - player.transform.position.x);
		if (distance <= radius)
		{
			float dmg = Mathf.Max(0, damagePerSecond * damageDelay + Random.Range(-damageDelta, damageDelta));
			player.GetHit(dmg, transform.position.x);
		}
		foreach (var enemy in controller.Enemies)
		{
			distance = Mathf.Abs(transform.position.x - enemy.transform.position.x);
			if (distance <= radius)
			{
				float dmg = Mathf.Max(0, damagePerSecond * damageDelay + Random.Range(-damageDelta, damageDelta));
				enemy.GetHit(dmg, transform.position.x);
			}
		}
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
