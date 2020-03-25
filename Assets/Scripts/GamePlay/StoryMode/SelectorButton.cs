using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectorButton : MonoBehaviour
{
    public int sortingLayerIncrease = 250;

    public bool selected;
    public bool trueSelected;
    private SpriteRenderer[] sprites;

    private void Awake()
    {
        sprites = GetComponentsInChildren<SpriteRenderer>();
        enabled = false;
    }

    public void Select()
    {
        if (selected) return;
        if (!enabled) return;
        selected = true;
        foreach (var i in sprites)
        {
            i.sortingOrder += sortingLayerIncrease;
        }
    }

    public void Deselect()
    {
        if (!selected) return;
        if (!enabled) return;
        selected = false;
        foreach (var i in sprites)
        {
            i.sortingOrder -= sortingLayerIncrease;
        }
    }

    private void OnMouseEnter()
    {
        Select();
    }

    private void OnMouseExit()
    {
        if (!trueSelected)
        Deselect();
    }

    private void OnMouseDown()
    {
        if (!enabled) return;
        Select();
        trueSelected = true;
    }
}
