using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ScrollViewScript : MonoBehaviour
{
	[Header("Local settings")]
	[Range(0, 10000)] [SerializeField] private float gripSpeed;
	[Range(0, 10)] [SerializeField] private float gripDuration;
	[Range(0, 1)] [SerializeField] private float unselectedScaling;
	[Range(0, 1)] [SerializeField] private float scalingSpeed;

	private Button[] items;
	private ScrollRect scrollRect;
	private bool isScrolling;
	private float gripTime;
	private float gripPosition;
	private bool isGripped;
	private RectTransform[] positions;
	private RectTransform position;
	private bool gripCompeted;
	private float leftBound, rightBound;

	public int SelectedIndex { get; private set; }
	public int ItemsCount { get; private set; }

	public void OnBeginDrag()
	{
		isScrolling = true;
		isGripped = false;
		gripCompeted = false;
		scrollRect.inertia = true;
	}
	
	public void OnEndDrag()
	{
		isScrolling = false;
	}

	public void GoToPanel(int panel)
	{
		SelectedIndex = panel;
		gripTime = Time.time;
		isGripped = true;
		gripPosition = position.anchoredPosition.x;
		scrollRect.inertia = false;
	}

	void Awake()
    {
		items = GetComponentsInChildren<Button>();
		ItemsCount = items.Length;
		scrollRect = GetComponentInParent<ScrollRect>();
		positions = new RectTransform[ItemsCount];
		for (int i = 0; i < ItemsCount; i++)
		{
			positions[i] = items[i].GetComponent<RectTransform>();
		}
		position = GetComponent<RectTransform>();
		leftBound = -positions[ItemsCount - 1].anchoredPosition.x - positions[ItemsCount - 1].sizeDelta.x / 2;
		rightBound = positions[0].sizeDelta.x / 2;
	}

	// Update is called once per frame
	void Update()
	{
		int nearestIndex = 0;
		float minDistance = float.MaxValue;
		for (int i = 0; i < ItemsCount; i++)
		{
			float distance = Mathf.Abs(position.anchoredPosition.x + positions[i].anchoredPosition.x);
			float size = Mathf.Clamp01(1 - distance * unselectedScaling);
			size = Mathf.SmoothStep(items[i].transform.localScale.x, size, scalingSpeed);
			items[i].transform.localScale = new Vector2(size, size);
			if (distance < minDistance)
			{
				nearestIndex = i;
				minDistance = distance;
			}
		}

		if (!isScrolling && position.anchoredPosition.x != Mathf.Clamp(position.anchoredPosition.x, leftBound, rightBound))
		{
			scrollRect.inertia = false;
		}

		if (Mathf.Abs(scrollRect.velocity.x) <= gripSpeed && !isGripped && !isScrolling && !gripCompeted)
		{
			gripTime = Time.time;
			isGripped = true;
			gripPosition = position.anchoredPosition.x;
			scrollRect.inertia = false;
			SelectedIndex = nearestIndex;
		}

		if (isGripped)
		{
			float time = (Time.time - gripTime) / gripDuration;
			float xPos = Mathf.SmoothStep(gripPosition, -positions[SelectedIndex].anchoredPosition.x, time);
			position.anchoredPosition = new Vector2(xPos, position.anchoredPosition.y);
			if (time >= 1)
			{
				isGripped = false;
				gripCompeted = true;
			}
		}
		
	}
}
