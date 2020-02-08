using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class Enemy : Entity
{
	/// <summary>
	/// Movement speed of enemy
	/// </summary>
	public float movementSpeed;
	/// <summary>
	/// Offset from ground
	/// </summary>
	public float deltaY;
	public GameObject dmgSystem;
	public float dodgePossibility;
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
	public AudioClip deathSound;
	public float ressurectionProbability;
	public float ressurectionDecrease;
	public int maxRessurectionCount;
	public float scaleIncrease;
	public float speedIncrease;
	public float healthIncrease;
	public float damageIncrease;
	public float distanceIncrease;
	public float scoreValue;
	[SerializeField] private DamageSpec[] damageSpecifications;
	public Dictionary<DamageType, float> dmgSpec;

	private Player player;
	private Animator animator;
	private SpriteRenderer spriteRenderer;
	private Controller controller;
	private LinkedListNode<Enemy> thisNode;
	private AudioSource audioSource;

	private float hp;
	private bool attack;
	private float lastHp;
	private bool left;
	private float lastAttack;
	private float lastDodge;
	private bool isDodging;
	private float lastSubAttack;
	private int subAttacks;
	private bool damageSystemPlay;
	private float currentRessurectionProbability;
	private int ressurectionCount;

	public void InitThis(Controller c, Player p)
	{
		controller = c;
		player = p;
	}

	private void Start()
	{
		animator = GetComponentInChildren<Animator>();
		spriteRenderer = GetComponentInChildren<SpriteRenderer>();
		audioSource = GetComponentInChildren<AudioSource>();
		transform.Translate(new Vector3(0, deltaY));
		hp = healthPoints;
		lastHp = healthPoints;
		thisNode = controller.Enemies.AddLast(this);
		currentRessurectionProbability = ressurectionProbability;
		dmgSpec = new Dictionary<DamageType, float>();
		foreach (var i in damageSpecifications)
		{
			dmgSpec.Add(i.damageType, i.multiplier);
		}
		foreach (var i in System.Enum.GetValues(typeof(DamageType)))
		{
			if (dmgSpec.ContainsKey((DamageType)i)) continue;
			dmgSpec.Add((DamageType)i, 1);
		}
	}

	private void Update()
	{
		if (lastHp != hp)
		{
			controller.InstantiateDamageLabel(transform.position, Mathf.RoundToInt(lastHp - hp));
			controller.IncreaseScore((lastHp - hp) / 25);
			if (Settings.particles && damageSystemPlay)
				Instantiate(dmgSystem, transform.position, Quaternion.Euler(0, 0, left ? 0 : 180));
		}
		lastHp = hp;
		if (IsDead) return;
		if (player.IsDead)
		{
			animator.SetBool("isWalking", false);
			return;
		}
		if (Time.time - lastDodge < dodgeTime) return;
		var dir = (int)Mathf.Sign(player.transform.position.x - transform.position.x);
		if (dir != transform.right.x) transform.Rotate(Vector3.up, 180);
		if (Mathf.Abs(transform.position.x) + collideWidth < controller.LevelWidth)
		if (Mathf.Abs(player.transform.position.x - transform.position.x) <= approachDistance && !attack)
		{
			attack = true;
			animator.SetBool("isWalking", false);
			if (!instantAttack) lastAttack = Mathf.Max(Time.time + attackDelay - 60 / shotRate, lastAttack);
		}
		if (attack && Mathf.Abs(player.transform.position.x - transform.position.x) > attackDistance)
		{
			attack = false;
		}
		if (!attack)
		{
			animator.SetBool("isWalking", true);
			transform.Translate(new Vector2(movementSpeed * Time.deltaTime, 0));
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
					animator.SetTrigger("Shot");
					audioSource.PlayOneShot(attackSound);
					player.GetHit(damage, transform.position.x, DamageType.Untagged);
					subAttacks--;
				}
			}
		}
	}

	public override bool GetHit(float damage, float x, DamageType damageType)
	{
		if (hp <= 0) return false;
		if ((Random.value < dodgePossibility) && (Time.time - lastDodge >= dodgeTime)
			&& (damageType == DamageType.SolidBullet || damageType == DamageType.PlasmBullet))
		{
			animator.SetTrigger("Dodge");
			lastDodge = Time.time;
			subAttacks = 0;
			return false;
		}
		damageSystemPlay = damageType != DamageType.Poison && damageType != DamageType.Electricity;
		hp -= damage * dmgSpec[damageType];
		left = transform.position.x < x;
		if (hp <= 0)
		{
			controller.KillsPlusPlus();
			controller.IncreaseScore(scoreValue);
			IsDead = true;
			attack = false;
			animator.SetBool("isDead", true);
			audioSource.pitch = Random.Range(0.97f, 1.1f);
			audioSource.PlayOneShot(deathSound);
			StartCoroutine(Dying());
		}
		return true;
	}

	private IEnumerator Dying()
	{
		spriteRenderer.sortingOrder--;
		yield return new WaitForSeconds(0.07f);
		GetComponent<BoxCollider2D>().enabled = false;
		if (thisNode != null)
		controller.Enemies.Remove(thisNode);
		thisNode = null;
		StartCoroutine(Destroing());
	}

	private IEnumerator Destroing()
	{
		yield return new WaitForSeconds(2);
		if (Random.value < currentRessurectionProbability && ressurectionCount < maxRessurectionCount)
		{
			StartCoroutine(Ressurect());
			yield break;
		}
		yield return new WaitForSeconds(3);
		if (thisNode != null) { 
			controller.Enemies.Remove(thisNode);
			thisNode = null;
		}

		for (float a = 1; a > 0; a -= 0.01f)
		{
			spriteRenderer.color = new Color(1, 1, 1, a);
			yield return new WaitForSeconds(0.01f);
		}
		Destroy(gameObject);
	}

	private IEnumerator Ressurect()
	{
		currentRessurectionProbability *= ressurectionDecrease;
		ressurectionCount++;
		animator.SetTrigger("Ressurection");
		spriteRenderer.sortingOrder++;
		healthPoints *= healthIncrease;
		damage *= damageIncrease;
		movementSpeed *= speedIncrease;
		attackDistance *= distanceIncrease;
		hp = healthPoints;
		lastHp = hp;
		GetComponent<BoxCollider2D>().enabled = true;
		if (thisNode != null)
		{
			controller.Enemies.Remove(thisNode);
			thisNode = null;
		}
		thisNode = controller.Enemies.AddLast(this);
		var startTime = Time.time;
		var currentScale = transform.localScale.x;
		for (float s = currentScale; !Mathf.Approximately(s, currentScale * scaleIncrease); s = Mathf.Lerp(currentScale, currentScale * scaleIncrease, Time.time - startTime))
		{
			transform.localScale = new Vector2(s, s);
			transform.localPosition = new Vector2(transform.position.x, -20 + deltaY * s);
			yield return null;
			if (Time.time - startTime >= 0.3f && IsDead)
			{
				IsDead = hp <= 0;
				GetComponent<BoxCollider2D>().enabled = hp > 0;
			}
		}
		if (scaleIncrease == 1)
		{
			yield return new WaitForSeconds(0.3f);
			IsDead = hp <= 0;
			GetComponent<BoxCollider2D>().enabled = hp > 0;
		}
	}
}