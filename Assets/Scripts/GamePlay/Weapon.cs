using UnityEngine;

public abstract class Weapon : MonoBehaviour
{
	public GameObject tpOverlay;
	public bool CanAttack { get; set; }
	public bool CanReload { get; set; }
	public bool IsAttacking { get; set; }
	public bool IsReloading { get; set; }
	public float Load { get; set; }
	public bool ManualReload { get; set; }

	public abstract Vector2 InitBullets(Vector2 drawPosition, Transform parent);

	public abstract void PerformAttack(int index);

	public abstract void PerformReload();

	public abstract void CancelReload();
}