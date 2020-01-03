using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grenade : MonoBehaviour
{
	public float lifeTime;
	public GameObject blast;

	private void Start()
	{
		StartCoroutine(CountDown());
	}

	private IEnumerator CountDown()
	{
		yield return new WaitForSeconds(lifeTime);
		Destroy(gameObject);
		Instantiate(blast, transform.position, Quaternion.identity);
	}
}
