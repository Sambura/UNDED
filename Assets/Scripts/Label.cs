using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Label : MonoBehaviour
{
	private Text text;
	private Vector3 translate;

    void Start()
    {
		text = GetComponentInChildren<Text>();
		translate = new Vector3(Random.Range(-0.1f, 0.1f), Random.Range(-0.1f, 0.1f));
		StartCoroutine(Fade());
    }

    private IEnumerator Fade()
	{
		for (float a = 1; a > 0; a -= 0.01f)
		{
			text.color = new Color(1, 1, 1, a);
			transform.Translate(translate);
			yield return new WaitForSeconds(0.01f);
		}
		Destroy(gameObject);
	}
}
