using UnityEngine;

public class Weapon : MonoBehaviour
{
	public GameObject tpOverlay;
	public bool CanAttack { get; set; }
	public bool CanReload { get; set; }
	public bool IsAttacking { get; set; }
	public bool IsReloading { get; set; }
	public float Load { get; set; }
	public bool ManualReload { get; set; }
	public Player player;
	public float damageMultiplier = 1;
	public bool UIVisible
	{
		get { return uiVisible; }
		set 
		{
			if (value == uiVisible) return;
			uiVisible = value;
			ToggleVisibility();
		}
	}
	private bool uiVisible = true;

	protected virtual void ToggleVisibility()
	{

	}

	public virtual Vector2 InitUIElements(Vector2 drawPosition, Transform parent)
	{
		return drawPosition;
	}

	public virtual void PerformAttack(int index)
	{

	}

	public virtual void PerformReload()
	{

	}

	public virtual void CancelReload()
	{

	}
}