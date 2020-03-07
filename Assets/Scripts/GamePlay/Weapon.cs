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