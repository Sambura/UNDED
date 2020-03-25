using UnityEngine;

public class GrenadeSelector : MonoBehaviour
{
    public GrenadeInteractable[] grenades;

    int selected = -1;
    public GameObject selectedGO;
    public bool UpdateIcon { get; set; } = false;

    private void Update()
    {
        for (var i = 0; i < grenades.Length; i++)
        {
            if (grenades[i].Interacted && i != selected)
            {
                for (var j = 0; j < grenades.Length; j++)
                {
                    if (i != j)
                    {
                        grenades[j].Interacted = false;
                        grenades[j].IsLocked = false;
                        grenades[j].UnTake();
                    }
                }
                selected = i;
                selectedGO = grenades[i].grenade;
                Player.Instance.thrower.grenade = selectedGO;
                if (UpdateIcon)
                    (Player.Instance as StoryPlayer).InitUI();
            }
        }
    }
}
