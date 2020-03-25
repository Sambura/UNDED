using UnityEngine;

public class GrenadeInteractable : Interactable
{
    public GameObject grenade;

    private SpriteRenderer spriteRenderer;

    private void Start()
    {
        IsLocked = defaultLockState;
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void UnTake()
    {
        spriteRenderer.enabled = true;
    }

    public override void OnInteraction()
    {
        base.OnInteraction();
        spriteRenderer.enabled = false;
    }
}
