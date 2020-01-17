using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Blast_PhysicsTest : MonoBehaviour
{
	public float lifeTime;

	private AudioSource audioSource;

	private void Start()
	{
		audioSource = GetComponent<AudioSource>();
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
