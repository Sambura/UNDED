using UnityEngine;

public abstract class Weapon : MonoBehaviour
{
	public bool CanAttack { get; set; }
	public bool CanReload { get; set; }
	public bool IsAttacking { get; set; }
	public bool IsReloading { get; set; }
	public float Load { get; set; }
	public float BulletsY { get; set; }
	public bool ManualReload { get; set; }

	public abstract void SetAnimator(Animator customAnimator);

	public abstract void InitBullets();

	public abstract void SetDirection(int direction);

	public abstract void PerformAttack(int index);

	public abstract void PerformReload();

	public abstract void CancelReload();
}