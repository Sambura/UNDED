using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
	[SerializeField] private Image primaryHealthBar;
	[SerializeField] private Image redHealthBar;
	[SerializeField] private float shrinkDelay;
	[SerializeField] private float shrinkTime;
	[SerializeField] private Gradient[] gradients;

	private float lastHp;
	private float lastUpdHp;
	private float currentHp;
	private float shrinkMark;
	private float state;
	private float lastColor;

	private Color EvaluateGradient(float t)
	{
		lastColor = t;
		return Color.Lerp(gradients[(int)state].Evaluate(t), gradients[Mathf.CeilToInt(state)].Evaluate(t), state % 1);
	}

	public void Init(float currentHP)
	{
		state = 0;
		currentHp = currentHP;
		lastHp = currentHP;
		lastUpdHp = currentHP;
		primaryHealthBar.fillAmount = 1;
		primaryHealthBar.color = EvaluateGradient(currentHP);
		redHealthBar.fillAmount = 1;
	}

	public void UpdateHealth(float currentHP)
	{
		if (currentHP == lastHp) return;

		primaryHealthBar.fillAmount = currentHP;
		if (currentHP > redHealthBar.fillAmount)
		{
			primaryHealthBar.color = EvaluateGradient(currentHP);
			redHealthBar.fillAmount = currentHP - 0.002f;
			lastHp = currentHP;
		}

		currentHp = currentHP;		
		if (lastUpdHp > currentHP)
		{
			if (shrinkMark < Time.time)
			{
				shrinkMark = Time.time + shrinkDelay;
				StartCoroutine(Shrinker());
			} else 
				shrinkMark = Time.time + shrinkDelay;
		}
		lastUpdHp = currentHP;
	}

	private IEnumerator Shrinker()
	{
		yield return new WaitWhile(() => shrinkMark > Time.time);
		float targetHp = currentHp;
		float from = lastHp;
		float startTime = Time.time;
		if (from == targetHp) yield break;
		for (var t = from; Time.time - startTime < shrinkTime; 
			t = Mathf.SmoothStep(from, targetHp, (Time.time - startTime) / shrinkTime))
		{
			redHealthBar.fillAmount = t;
			primaryHealthBar.color = EvaluateGradient(t);
			yield return null;
		}
		redHealthBar.fillAmount = targetHp - 0.002f;
		lastHp = targetHp;
	}

	public void ChangeState(int toState, float time)
	{
		StartCoroutine(Changing((int)state, toState, time));
	}

	private IEnumerator Changing(int fromState, int toState, float time)
	{
		float startTime = Time.time;
		for (state = fromState; Time.time - startTime < time; state = Mathf.Lerp(fromState, toState, (Time.time - startTime) / time))
		{
			primaryHealthBar.color = EvaluateGradient(lastColor);
			yield return null;
		}
		state = toState;
	}
}
