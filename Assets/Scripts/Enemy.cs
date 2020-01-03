using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class Enemy : MonoBehaviour
{
	/// <summary>
	/// Movement speed of enemy
	/// </summary>
	public float movementSpeed;
	/// <summary>
	/// Max / initial health points
	/// </summary>
	public float healthPoints;
	/// <summary>
	/// Offset from ground
	/// </summary>
	public float deltaY;
	public float shotRate;
	public float attackDelay;
	public float damage;
	public float attackDistance;
	public float approachDistance;
	public bool instantAttack;
	public GameObject label;
	public GameObject dmgSystem;
	public Weapon weapon;
	public float dodgeRate;
	public float dodgeSpeed;
	public float dodgeTime;

	private Player player;
	private Animator animator;
	private SpriteRenderer spriteRenderer;
	private Controller controller;

	private float hp;
	public bool IsDead { get; private set; }
	private int direction;
	private bool attack;
	private float lastHp;
	private bool left;
	private float lastAttack;
	private float lastDodge;
	private bool isDodging;

	public void SetController(Controller c)
	{
		controller = c;
	}

	private void Start()
	{
		player = FindObjectOfType<Player>();
		animator = GetComponent<Animator>();
		spriteRenderer = GetComponent<SpriteRenderer>();
		transform.Translate(new Vector3(0, deltaY));
		hp = healthPoints;
		if (weapon != null)
		weapon.animator = animator;
	}

	private void Update()
	{
		if (lastHp > hp)
		{
			int delta = Mathf.RoundToInt(lastHp - hp);
			var lb = Instantiate(label, transform.position, Quaternion.identity).GetComponentInChildren<Text>();
			lb.text = Mathf.RoundToInt(delta).ToString();
			Instantiate(dmgSystem, transform.position, Quaternion.Euler(0, 0, left ? 0 : 180));
		}
		lastHp = hp;
		if (IsDead)
		{
			animator.Play("Death");
			return;
		}
		if (player.IsDead)
		{
			animator.Play("Idle");
			return;
		}
		direction = (int)Mathf.Sign(player.transform.position.x - transform.position.x);
		spriteRenderer.flipX = direction == -1;
		weapon?.SetDirection(direction);

		if (isDodging)
		{
			float d = -dodgeSpeed * direction * Time.deltaTime;
			if (Mathf.Abs(transform.position.x + d) >= 175)
			{
				d = Mathf.Sign(transform.position.x) * 175 - transform.position.x;
			}
			transform.Translate(new Vector3(d, 0));
			if (Time.time - lastDodge >= dodgeTime)
			{
				isDodging = false;
			}
			else return;
		}

		if (dodgeRate != 0)
			if (Time.time - lastDodge >= 60 / dodgeRate)
			{
				var bullets = FindObjectsOfType<Bullet>();
				bool flag = false;
				foreach (var i in bullets)
				{
					if (!i.Active) continue;
					if (!i.PlayerProperty) continue;
					bool left = transform.position.x > i.transform.position.x;
					if (i.Direction == -1 ^ left)
					{
						flag = true;
						break;
					}
				}
				if (flag)
				{
					lastDodge = Time.time;
					isDodging = true;
					animator.Play("Dodge");
				}
			}

		if (Mathf.Abs(player.transform.position.x - transform.position.x) <= approachDistance && !attack)
		{
			attack = true;
			animator.Play("Idle");
			if (!instantAttack) lastAttack = Time.time + attackDelay;
		}
		if (attack && Mathf.Abs(player.transform.position.x - transform.position.x) > attackDistance)
		{
			attack = false;
		}
		if (!attack)
		{
			animator.Play("Walk");
			GetComponent<Rigidbody2D>().MovePosition(
				new Vector2(transform.position.x + movementSpeed * direction * Time.deltaTime, transform.position.y));
		} else
		{
			if (Time.time - lastAttack >= 60 / shotRate)
			{
				lastAttack = Time.time;
				if (weapon == null)
				{
					animator.Play("Shot");
					player.GetHit(damage, transform.position.x);
				} else
				{
					if (weapon.CanFire)
					{
						weapon.PerformShot(0);
					}
					if (weapon.CanReload)
					{
						weapon.PerformReload();
					}
				}
			}
		}
	}
	public void GetHit(float damage, float x)
	{
		hp -= damage;
		left = transform.position.x < x;
		if (hp <= 0 && !IsDead)
		{
			controller.KillsPlusPlus();
			IsDead = true;
			attack = false;
			animator.Play("Death");
			StartCoroutine(Dying());
		}
	}

	private IEnumerator Dying()
	{
		yield return new WaitForSeconds(0.07f);
		Destroy(GetComponent<Collider2D>());
		StartCoroutine(Destroing());
	}

	private IEnumerator Destroing()
	{
		yield return new WaitForSeconds(10);
		for (float a = 1; a > 0; a -= 0.005f)
		{
			transform.Translate(new Vector3(0, -0.04f));
			spriteRenderer.color = new Color(1, 1, 1, a);
			yield return new WaitForSeconds(0.01f);
		}
		Destroy(gameObject);
	}
}
