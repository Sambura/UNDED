using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteSlider : MonoBehaviour
{
	[SerializeField] private Sprite[] sprites;
	[SerializeField] private SpriteRenderer spriteRenderer;

	private int spriteMaxIndex;

	public float Value
	{
		set
		{
			int spriteIndex = (int)Mathf.Clamp(value * spriteMaxIndex + (KeepFirst ? 1 : 0), 0, spriteMaxIndex);
			spriteRenderer.sprite = sprites[spriteIndex];
		}
	}

	public bool KeepFirst { get; set; }

    void Awake()
    {
		spriteMaxIndex = sprites.Length - 1;
	}
}
