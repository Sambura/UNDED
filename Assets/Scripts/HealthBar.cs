using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
	[SerializeField] private RectTransform primaryHealthBar;
	[SerializeField] private RectTransform redHealthBar;
	[SerializeField] private RectTransform healthBarBG;
	[SerializeField] private RawImage healthBarColor;
	[SerializeField] private float shrinkDelay;
	[SerializeField] private float shrinkTime;
	[SerializeField] private Color oneColor;
	[SerializeField] private Color halfColor;
	[SerializeField] private Color zeroColor;

	//private Controller controller;
	private float lastHp;
	private float lastUpdHp;
	private float currentHp;
	private float shrinkMark;

	public void Init(float currentHP)
	{
		currentHp = currentHP;
		lastHp = currentHP;
		lastUpdHp = currentHP;
		primaryHealthBar.sizeDelta = new Vector2(healthBarBG.sizeDelta.x * currentHP, healthBarBG.sizeDelta.y);
		redHealthBar.sizeDelta = new Vector2(0, redHealthBar.sizeDelta.y);
		//healthBarColor.color = Color.Lerp(zeroColor, oneColor, currentHP);
		healthBarColor.color = InterpGYR(zeroColor, halfColor, oneColor, currentHP);
	}

	public void UpdateHealth(float currentHP)
	{
		if (currentHP == lastHp) return;

		redHealthBar.sizeDelta = new Vector2(healthBarBG.sizeDelta.x * (1 - currentHP - 0.002f), redHealthBar.sizeDelta.y);
		primaryHealthBar.sizeDelta = new Vector2(primaryHealthBar.sizeDelta.x, primaryHealthBar.sizeDelta.y);
		currentHp = currentHP;		
		if (lastUpdHp > currentHP)
		{
			if (shrinkMark < Time.time)
			{
				shrinkMark = Time.time + shrinkDelay;
				StartCoroutine(Shrinker(true));
			} else
				shrinkMark = Time.time + shrinkDelay;
		} else
		{
			if (primaryHealthBar.sizeDelta.x / healthBarBG.sizeDelta.x < currentHP)
			{
				primaryHealthBar.sizeDelta = new Vector2(healthBarBG.sizeDelta.x * currentHP, healthBarBG.sizeDelta.y);
				//healthBarColor.color = Color.Lerp(zeroColor, oneColor, currentHP);
				healthBarColor.color = InterpGYR(zeroColor, halfColor, oneColor, currentHP);
				lastHp = currentHP;
			}
		}
		lastUpdHp = currentHP;
	}

	private IEnumerator Shrinker(bool wait)
	{
		if (wait)
		yield return new WaitWhile(() => shrinkMark > Time.time);
		float targetHp = currentHp;
		float from = lastHp;
		float startTime = Time.time;
		if (lastHp == currentHp) yield break;
		for (var t = lastHp; Time.time - startTime < shrinkTime; 
			t = Mathf.SmoothStep(lastHp, currentHp, (Time.time - startTime) / shrinkTime))
		{
			primaryHealthBar.sizeDelta = new Vector2(healthBarBG.sizeDelta.x * t, healthBarBG.sizeDelta.y);
			//healthBarColor.color = Color.Lerp(zeroColor, oneColor, t);
			healthBarColor.color = InterpGYR(zeroColor, halfColor, oneColor, t);
			yield return null;
		}
		primaryHealthBar.sizeDelta = new Vector2(healthBarBG.sizeDelta.x * currentHp, healthBarBG.sizeDelta.y);
		lastHp = currentHp;
	}

	public static Color InterpGYR(Color a, Color i, Color b, float hp)
	{
		if (hp > 0.5f)
			return Color.Lerp(i, b, 2 * (hp - 0.5f));
		return Color.Lerp(a, i, 2 * hp);
	}
}
