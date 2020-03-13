using UnityEngine;
using System.Collections;

public class ScaleEventRaiser : MonoBehaviour
{
	[SerializeField] private float scaleThreshold = 0.85f;

	private ScrollViewScript scrollView;
	private bool below;

	void Start()
	{
		scrollView = GetComponentInChildren<ScrollViewScript>();
		below = transform.localScale.x < scaleThreshold;
	}

	void Update()
	{
		if (transform.localScale.x >= scaleThreshold)
		{
			if (below)
			{
				scrollView.UpdateData();
				below = false;
			}
		}
		else below = true;
	}
}
