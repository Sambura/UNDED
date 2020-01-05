using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Label : MonoBehaviour
{
	public float fadeTime;

	private Text text;
	private Vector3 translate;
	private float r, g, b;

	public void SetNumber(float num)
	{
		text.text = num.ToString();
		var cl = Mathf.CorrelatedColorTemperatureToRGB(6700 - num * 10);
		r = cl.r;
		g = cl.g;
		b = cl.b;
	}

	private void Awake()
	{
		text = GetComponentInChildren<Text>();
		r = 1;
		g = 1;
		b = 1;
	}

	void Start()
    {
		translate = new Vector3(Random.Range(-0.1f, 0.1f), Random.Range(-0.1f, 0.1f));
		StartCoroutine(Fade());
    }

    private IEnumerator Fade()
	{
		for (float a = 1; a > 0; a -= 0.01f)
		{
			text.color = new Color(r, g, b, a);
			transform.Translate(translate);
			yield return new WaitForSeconds(fadeTime / 100);
		}
		Destroy(gameObject);
	}
}
