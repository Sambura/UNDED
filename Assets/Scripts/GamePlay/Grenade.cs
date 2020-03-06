using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grenade : MonoBehaviour
{
	public float lifeTime;
	public GameObject blast;
	public GameObject symbol;

	private void Start()
	{
		StartCoroutine(CountDown());
		var f = GetComponent<Blast>();
	}

	private IEnumerator CountDown()
	{
		yield return new WaitForSeconds(lifeTime);
		Destroy(gameObject);
		Instantiate(blast, transform.position, Quaternion.identity).tag = gameObject.tag;
	}
}
