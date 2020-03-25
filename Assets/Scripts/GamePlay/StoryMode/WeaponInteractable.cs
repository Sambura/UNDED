using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponInteractable : Interactable
{
    [SerializeField] private GameObject weapon;
    public GameObject weaponInstance;

    private SpriteRenderer spriteRenderer;

    private void Start()
    {
        IsLocked = defaultLockState;
        spriteRenderer = GetComponent<SpriteRenderer>();
        weaponInstance = Instantiate(weapon, Player.Instance.weaponHolder);
        weaponInstance.SetActive(false);
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
