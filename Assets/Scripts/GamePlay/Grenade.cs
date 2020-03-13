using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grenade : MonoBehaviour
{
	public float lifeTime;
	public string blastName;
	public GameObject blast;
	public GameObject symbol;

	private void Start()
	{
		StartCoroutine(CountDown());
	}

	private IEnumerator CountDown()
	{
		yield return new WaitForSeconds(lifeTime);
		Destroy(gameObject);
		var blastI = Instantiate(blast, transform.position, Quaternion.identity);
		blastI.tag = gameObject.tag;
		blastI.SetActive(true);
	}
}
