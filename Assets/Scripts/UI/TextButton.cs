using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TextButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
	[Header("Hovered font scale")]
	[Range(-10, 10)]
	[SerializeField] private float hoveredScale;

	private Text text;
	private float percent;
	private float inc;

	private void Start()
	{
		percent = 0;
		text = GetComponentInChildren<Text>();
	}

	private void OnEnable()
	{
		StartCoroutine(Fade());
		percent = 0;
		inc = 0;
	}

	private void Update()
	{
		text.transform.localScale = new Vector2(1 + percent * (hoveredScale - 1), 1 + percent * (hoveredScale - 1));
	}

	private IEnumerator Fade()
	{
		float fadeTime = 0.2f;
		while (true)
		{
			percent = Mathf.Clamp(percent + Time.deltaTime * inc / fadeTime, 0, 1);
			yield return null;
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		inc = 1;
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		inc = -1;
	}
}
