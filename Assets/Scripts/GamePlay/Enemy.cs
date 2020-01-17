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
	public GameObject dmgSystem;
	public float dodgeRate;
	public float dodgeSpeed;
	public float dodgeTime;
	public float shotRate;
	public float attackDelay;
	public int attackLength;
	public float subDelay;
	public float damage;
	public float attackDistance;
	public float approachDistance;
	public bool instantAttack;
	public AudioClip attackSound;

	private Player player;
	private Animator animator;
	private SpriteRenderer spriteRenderer;
	private Controller controller;
	private LinkedListNode<Enemy> thisNode;
	private AudioSource audioSource;

	private float hp;
	public bool IsDead { get; private set; }
	private int direction;
	private bool attack;
	private float lastHp;
	private bool left;
	private float lastAttack;
	private float lastDodge;
	private bool isDodging;
	private float lastSubAttack;
	private int subAttacks;

	public void InitThis(Controller c, Player p)
	{
		controller = c;
		player = p;
	}

	private void Start()
	{
		animator = GetComponent<Animator>();
		spriteRenderer = GetComponent<SpriteRenderer>();
		audioSource = GetComponent<AudioSource>();
		transform.Translate(new Vector3(0, deltaY));
		hp = healthPoints;
		thisNode = controller.Enemies.AddLast(this);
		audioSource.volume *= controller.sfxVolume;
	}

	private void Update()
	{
		if (lastHp > hp)
		{
			controller.InstantiateDamageLabel(transform.position, Mathf.RoundToInt(lastHp - hp));
			if (controller.enableParticles)
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
			if (Time.time - lastDodge >= 60 / dodgeRate && Mathf.Abs(transform.position.x) < controller.LevelWidth)
			{
				var bullets = FindObjectsOfType<Bullet>();
				bool flag = false;
				foreach (var i in bullets)
				{
					if (!i.Active) continue;
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
			transform.Translate(new Vector2(movementSpeed * direction * Time.deltaTime, 0));
		}
		else
		{
			if (Time.time - lastAttack >= 60 / shotRate)
			{
				lastAttack = Time.time;
				subAttacks = attackLength;
			}
			if (subAttacks > 0)
			{
				if (Time.time - lastSubAttack >= subDelay)
				{
					lastSubAttack = Time.time;
					animator.Play("Shot");
					audioSource.PlayOneShot(attackSound);
					player.GetHit(damage, transform.position.x);
					subAttacks--;
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
			animator?.Play("Death");
			StartCoroutine(Dying());
		}
	}

	private IEnumerator Dying()
	{
		spriteRenderer.sortingOrder--;
		yield return new WaitForSeconds(0.07f);
		controller.Enemies.Remove(thisNode);
		Destroy(GetComponent<Collider2D>());
		StartCoroutine(Destroing());
	}

	private IEnumerator Destroing()
	{
		yield return new WaitForSeconds(5);
		for (float a = 1; a > 0; a -= 0.01f)
		{
			transform.Translate(new Vector3(0, -0.04f));
			spriteRenderer.color = new Color(1, 1, 1, a);
			yield return new WaitForSeconds(0.01f);
		}
		Destroy(gameObject);
	}
}
