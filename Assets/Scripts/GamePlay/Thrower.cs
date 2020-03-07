using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Thrower : MonoBehaviour
{
	public GameObject grenade;
	public float grenadeRate;
	//public GameObject line;
	public Transform throwPoint;


	private float nextGrenade;
	public float throwingForce = 60;
	private bool isThrowing;
	public float throwingAngle = Mathf.PI / 4;
	private GameObject Line;
	private LineRenderer lineRenderer;
	private bool uprise;
	private Animator animator;
	private Weapon weapon;
	private SpriteRenderer spriteRenderer;

	private Animator grenadeIcon;
	private float grenadeMass;
	private float grenadeGravityScale;
	private float grenadeDelay;

	public bool IsThrowing { get; private set; }

	private void Start()
	{
		animator = GetComponent<Animator>();
		grenadeDelay = 60 / grenadeRate;
		weapon = GetComponentInParent<Player>().GetComponentInChildren<Weapon>();
		spriteRenderer = GetComponent<SpriteRenderer>();
	}

	public void UpdateDelay()
	{
		grenadeDelay = 60 / grenadeRate;
	}

	public Vector2 InitThrower(Vector2 drawPosition, Transform parent)
	{
		if (grenadeIcon != null)
				Destroy(grenadeIcon.gameObject);
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
		spriteRenderer.color = new Color(1, 1, 1, 1);
		weapon.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0);
		animator.SetTrigger("Throw");
		IsThrowing = true;
	}

	public void Throw()
	{
		var g = Instantiate(grenade, throwPoint.position, Quaternion.identity);
		var rb = g.GetComponent<Rigidbody2D>();
		g.tag = gameObject.tag;
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
