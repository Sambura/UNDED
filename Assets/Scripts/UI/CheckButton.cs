using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CheckButton : MonoBehaviour
{
	[Header("Unchecked state settings")]
	[SerializeField] private Color NormalColorUnchecked;
	[SerializeField] private Color HighlightedColorUnchecked;
	[SerializeField] private Color PressedColorUnchecked;
	[SerializeField] private Color SelectedColorUnchecked;

	[Header("Checked state settings")]
	[SerializeField] private Color NormalColorChecked;
	[SerializeField] private Color HighlightedColorChecked;
	[SerializeField] private Color PressedColorChecked;
	[SerializeField] private Color SelectedColorChecked;

	[Header("Fade duration")]
	[Range(0, 100)]
	[SerializeField] private float fadeDuration;

	private Toggle target;
	private ColorBlock checkedBlock;
	private ColorBlock uncheckedBlock;

	private void Start()
	{
		checkedBlock = new ColorBlock
		{
			highlightedColor = HighlightedColorChecked,
			normalColor = NormalColorChecked,
			pressedColor = PressedColorChecked,
			selectedColor = SelectedColorChecked,
			fadeDuration = fadeDuration,
			colorMultiplier = 1
		};
		uncheckedBlock = new ColorBlock
		{
			highlightedColor = HighlightedColorUnchecked,
			normalColor = NormalColorUnchecked,
			pressedColor = PressedColorUnchecked,
			selectedColor = SelectedColorUnchecked,
			fadeDuration = fadeDuration,
			colorMultiplier = 1
		};
		target = GetComponent<Toggle>();
		ChangeState(target.isOn);
	}

	public void ChangeState(bool state)
	{
		if (state)
		{
			target.colors = checkedBlock;
		} else
		{
			target.colors = uncheckedBlock;
		}
	}
}
