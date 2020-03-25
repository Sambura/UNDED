using UnityEngine;

public class Thrower : MonoBehaviour
{
	public float grenadeRate = 30;
	public float throwingForce = 60;
	public float throwingAngle = Mathf.PI / 4;
	[SerializeField] private Transform throwPoint;
	public GameObject grenade;

	private float nextGrenade;
	private Animator animator;
	private Weapon weapon;
	private SpriteRenderer spriteRenderer;

	private Animator grenadeIcon;
	private float grenadeDelay;

	public bool IsThrowing { get; private set; }
	public bool CancelThrow { get; set; }

	private void Start()
	{
		animator = GetComponent<Animator>();
		weapon = GetComponentInParent<Player>().GetComponentInChildren<Weapon>();
		spriteRenderer = GetComponent<SpriteRenderer>();
		UpdateDelay();
	}

	public void UpdateDelay()
	{
		grenadeDelay = 60 / grenadeRate;
	}

	public Vector2 InitThrower(Vector2 drawPosition, Transform parent)
	{
		if (grenadeIcon != null)
				Destroy(grenadeIcon.gameObject);
		if (grenade == null) return drawPosition;
		var corner = Camera.main.ScreenToWorldPoint(Vector3.zero);
		float width = Mathf.Abs((corner.x - Camera.main.transform.position.x) * 2);
		if (drawPosition.x + 6 > width / 2)
		{
			drawPosition.Set(0, drawPosition.y - 5);
		}
		grenadeIcon = Instantiate(grenade.GetComponent<Grenade>().symbol, parent).GetComponent<Animator>();
		grenadeIcon.transform.Translate(drawPosition + new Vector2());
		return drawPosition;
	}

	public void PerformThrow()
	{
		if (Time.time < nextGrenade) return;
		if (IsThrowing) return;
		if (grenade == null) return;
		spriteRenderer.color = new Color(1, 1, 1, 1);
		weapon.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0);
		animator.SetTrigger("Throw");
		IsThrowing = true;
	}

	public void Throw()
	{
		if (CancelThrow)
		{
			CancelThrow = false;
			return;
		}
		var g = Instantiate(grenade, throwPoint.position, Quaternion.identity);
		var rb = g.GetComponent<Rigidbody2D>();
		g.tag = gameObject.tag;
		g.SetActive(true);
		rb.AddForce(new Vector2(
			throwingForce * Mathf.Cos(throwingAngle) * transform.right.x,
			throwingForce * Mathf.Sin(throwingAngle)),
			ForceMode2D.Impulse);
		rb.AddTorque(-5 * transform.right.x, ForceMode2D.Impulse);
		nextGrenade = Time.time + grenadeDelay;
		grenadeIcon.SetTrigger("Recharge");
		grenadeIcon.speed = 1 / grenadeDelay;
		spriteRenderer.color = new Color(1, 1, 1, 0);
		weapon.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 1);
		IsThrowing = false;
	}

}
