using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSelector : MonoBehaviour
{
    public WeaponInteractable[] weapons;

    public bool UpdateIcon { get; set; } = false;

    int selected = -1;

    private void Update()
    {
        for (var i = 0; i < weapons.Length; i++)
        {
            if (weapons[i].Interacted && i != selected)
            {
                for (var j = 0; j < weapons.Length; j++)
                {
                    if (i != j)
                    {
                        weapons[j].Interacted = false;
                        weapons[j].IsLocked = false;
                        weapons[j].UnTake();
                    }
                }
                selected = i;
                Player.Instance.weapon.UIVisible = false;
                Player.Instance.weapon.gameObject.SetActive(false);
                weapons[i].weaponInstance.SetActive(true);
                Player.Instance.weapon = weapons[i].weaponInstance.GetComponent<Weapon>();
                Player.Instance.weapon.UIVisible = true;
                if (UpdateIcon)
                {
                    (Player.Instance as StoryPlayer).InitUI();
                }
            }
        }
    }
}
