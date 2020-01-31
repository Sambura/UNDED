using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Label : MonoBehaviour
{
	public float fadeTime;

	private Text text;
	private Vector2 translate;
	private Color color;

	public void SetNumber(float num)
	{
		if (num >= 0)
		{
			text.text = num.ToString();
			color = Mathf.CorrelatedColorTemperatureToRGB(6700 - num * 10);
		}
		else
		{
			text.text = $"+{-num}";
			color = Color.green;
		}
	}

	private void Awake()
	{
		text = GetComponentInChildren<Text>();
		color = new Color(1, 1, 1);
	}

	void Start()
    {
		translate = Random.insideUnitCircle * 3;
		StartCoroutine(Fade());
    }

    private IEnumerator Fade()
	{
		float startTime = Time.time;
		for (float a = 1; a > 0; a = Mathf.Lerp(1, 0, (Time.time - startTime) / fadeTime))
		{
			color.a = a;
			text.color = color;
			transform.Translate(translate * Time.deltaTime);
			yield return null;
		}
		Destroy(gameObject);
	}
}
