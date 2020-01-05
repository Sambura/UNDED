using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleSystemDestroyer : MonoBehaviour
{
	private new ParticleSystem particleSystem;

    void Start()
    {
		particleSystem = GetComponent<ParticleSystem>();
		StartCoroutine(Destroying());
    }

    IEnumerator Destroying()
	{
		yield return new WaitWhile(() => particleSystem.isPlaying);
		Destroy(gameObject);
	}
}
