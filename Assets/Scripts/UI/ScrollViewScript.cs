using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScrollViewScript : MonoBehaviour
{
	[Header("Local settings")]
	[Range(0, 10000)] [SerializeField] private float gripSpeed;
	[Range(0, 10)] [SerializeField] private float gripDuration;
	[Range(0, 1)] [SerializeField] private float unselectedScaling;
	[Range(0, 1)] [SerializeField] private float scalingSpeed;
	[SerializeField] private Text selectedName;
	[SerializeField] private Text selectedDescription;

	private Button[] items;
	private ScrollViewItem[] itemsData;
	private ScrollRect scrollRect;
	private bool isScrolling;
	private float gripTime;
	private Vector2 gripPosition;
	private bool isGripped;
	private RectTransform[] positions;
	private RectTransform position;
	private bool gripCompeted;
	private float leftBound, rightBound, topBound, bottomBound;
	private int lastUpdate;

	public ScrollViewItem SelectedItem { get; private set; }
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
		UpdateData(panel);
		gripTime = Time.time;
		isGripped = true;
		gripPosition = position.anchoredPosition;
		scrollRect.inertia = false;
	}

	public void Increment()
	{
		if (SelectedIndex >= ItemsCount - 1) return;
		GoToPanel(SelectedIndex + 1);
	}

	public void Decrement()
	{
		if (SelectedIndex == 0) return;
		GoToPanel(SelectedIndex - 1);
	}

	void Start()
	{
		UpdateContent();
	}

	public void UpdateContent()
	{
		var children = new List<ScrollViewItem>();
		foreach (var i in GetComponentsInChildren<ScrollViewItem>())
		{
			if (i.transform.parent == transform) children.Add(i);
		}
		itemsData = children.ToArray();
		ItemsCount = itemsData.Length;
		items = new Button[ItemsCount];
		scrollRect = GetComponentInParent<ScrollRect>();
		positions = new RectTransform[ItemsCount];
		for (int i = 0; i < ItemsCount; i++)
		{
			positions[i] = itemsData[i].GetComponent<RectTransform>();
			items[i] = itemsData[i].gameObject.GetComponent<Button>();
			/*if (items[i] != null)
			{
				UnityEngine.Events.UnityAction action = new UnityEngine.Events.UnityAction(() => { GoToPanel(i); });
				items[i].onClick.AddListener(action);
			}*/
		}
		position = GetComponent<RectTransform>();
		if (ItemsCount != 0)
		{
			leftBound = -positions[ItemsCount - 1].anchoredPosition.x - positions[ItemsCount - 1].sizeDelta.x / 2;
			rightBound = positions[0].sizeDelta.x / 2;
			topBound = -positions[ItemsCount - 1].anchoredPosition.y + positions[ItemsCount - 1].sizeDelta.y / 2;
			bottomBound = -positions[0].sizeDelta.y / 2 - positions[0].anchoredPosition.y; 
			lastUpdate = -1;
			UpdateData(0);
		}
	}

	public void UpdateData(int index = -1)
	{
		if (index == -1)
		{
			if (lastUpdate == -1) return;
			index = lastUpdate;
		} else
		{
			if (index == lastUpdate) return;
			//if (selectedName != null)
				//if (selectedName.text == itemsData[index].itemName) return;
		}
		if (selectedName != null)
			selectedName.text = itemsData[index].itemName;
		if (selectedDescription != null)
			selectedDescription.text = itemsData[index].itemDescription;
		SelectedItem = itemsData[index];
		lastUpdate = index;	
	}

	void Update()
	{
		int nearestIndex = 0;
		float minDistance = float.MaxValue;
		for (int i = 0; i < ItemsCount; i++)
		{
			float distance = (positions[i].anchoredPosition + position.anchoredPosition).magnitude;
			float size = Mathf.Clamp01(1 - distance * unselectedScaling);
			size = Mathf.SmoothStep(itemsData[i].transform.localScale.x, size, scalingSpeed);
			itemsData[i].transform.localScale = new Vector2(size, size);
			if (distance < minDistance)
			{
				nearestIndex = i;
				minDistance = distance;
			}
		}
		UpdateData(nearestIndex);

		
		if (!isScrolling && (position.anchoredPosition.x != Mathf.Clamp(position.anchoredPosition.x, leftBound, rightBound) ||
			position.anchoredPosition.y != Mathf.Clamp(position.anchoredPosition.y, bottomBound, topBound)))
		{
			scrollRect.inertia = false;
		}
		

		if (Mathf.Abs(scrollRect.velocity.magnitude) <= gripSpeed && !isGripped && !isScrolling && !gripCompeted)
		{
			gripTime = Time.time;
			isGripped = true;
			gripPosition = position.anchoredPosition;
			scrollRect.inertia = false;
			SelectedIndex = nearestIndex;
			UpdateData(SelectedIndex);
		}

		if (isGripped)
		{
			float time = (Time.time - gripTime) / gripDuration;
			float xPos = Mathf.SmoothStep(gripPosition.x, -positions[SelectedIndex].anchoredPosition.x, time);
			float yPos = Mathf.SmoothStep(gripPosition.y, -positions[SelectedIndex].anchoredPosition.y, time);
			position.anchoredPosition = new Vector2(xPos, yPos);
			if (time >= 1)
			{
				isGripped = false;
				gripCompeted = true;
			}
		}
		
	}
}
