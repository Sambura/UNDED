using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : Entity
{
	[SerializeField] protected float movementSpeed;
	[SerializeField] protected float deltaY;
	[SerializeField] protected GameObject dmgSystem;
	[SerializeField] protected float shotRate;
	[SerializeField] protected float attackDelay;
	[SerializeField] protected float damage;
	[SerializeField] protected float attackDistance;
	[SerializeField] protected float approachDistance;
	[SerializeField] protected float deltaDistance;
	[SerializeField] protected bool playSoundOnMiss;
	[SerializeField] protected float scoreValue;
	[SerializeField] protected AudioClip attackSound;
	[SerializeField] protected AudioClip deathSound;
	[SerializeField] protected DamageType damageType;
	[SerializeField] protected DamageSpec[] damageSpecifications;

	protected Player player;
	protected Animator animator;
	protected SpriteRenderer spriteRenderer;
	protected Controller controller;
	protected AudioSource audioSource;
	protected BoxCollider2D boxCollider;
	protected Dictionary<DamageType, float> dmgSpec;
	protected Vector2 horizontalVelocity;

	protected float hp;
	protected bool isAttacking;
	protected bool isLocked;
	protected bool isWalking;
	protected float damageTaken;
	protected int damageSide;
	protected float nextAttackTime;
	protected bool damageSystemPlay;

	protected virtual void Start()
	{
		animator = GetComponent<Animator>();
		spriteRenderer = GetComponent<SpriteRenderer>();
		audioSource = GetComponent<AudioSource>();
		boxCollider = GetComponent<BoxCollider2D>();
		player = Player.Instance;
		controller = Controller.Instance;
		horizontalVelocity = new Vector2(movementSpeed, 0);
		transform.Translate(new Vector3(0, deltaY));
		hp = maxHealth;
		controller.Enemies++;
		dmgSpec = new Dictionary<DamageType, float>();
		isWalking = true;
		animator.SetBool("isWalking", true);
		foreach (var i in System.Enum.GetValues(typeof(DamageType)))
		{
			dmgSpec.Add((DamageType)i, 1);
		}
		foreach (var i in damageSpecifications)
		{
			dmgSpec[i.damageType] = i.multiplier;
		}
		if (hp <= 0) Die();
	}

	protected virtual void Update()
	{
		if (damageTaken > 0.51f)
		{
			controller.InstantiateDamageLabel(transform.position, Mathf.RoundToInt(damageTaken));
			controller.IncreaseScore(Mathf.Min(damageTaken, maxHealth) / 25);
			if (Settings.particles && damageSystemPlay)
				Instantiate(dmgSystem, transform.position, Quaternion.Euler(0, 0, damageSide == -1 ? 0 : 180));
			damageTaken = 0;
			damageSystemPlay = false;
		}

		if (IsDead) return;

		if (isLocked) return;

		if (player.IsDead)
		{
			animator.SetBool("isWalking", false);
			return;
		}

		float relativeDistance = player.transform.position.x - transform.position.x;
		var dir = Mathf.Sign(relativeDistance);
		if (dir != transform.right.x) transform.Rotate(Vector3.up, 180);

		if (Mathf.Abs(transform.position.x) + collideWidth < controller.LevelWidth)
			if (Mathf.Abs(relativeDistance) <= approachDistance + Random.Range(-deltaDistance, deltaDistance) && isWalking)
			{
				isWalking = false;
				animator.SetBool("isWalking", false);
				nextAttackTime = Mathf.Max(Time.time + attackDelay, nextAttackTime);
			}

		if (!isWalking && Mathf.Abs(relativeDistance) > attackDistance)
		{
			isWalking = true;
			animator.SetBool("isWalking", true);
		}

		if (isWalking)
		{
			transform.Translate(horizontalVelocity * Time.deltaTime);
		}

		if (!isWalking && nextAttackTime <= Time.time)
		{
			animator.SetTrigger("Shot");
			nextAttackTime = Time.time + 60 / shotRate;
			isAttacking = true;
			isLocked = true;
		}
	}

	public virtual void Attack()
	{
		if (playSoundOnMiss) audioSource.PlayOneShot(attackSound);
		var distance = player.transform.position.x - transform.position.x;
		if (Mathf.Sign(distance) == transform.right.x && Mathf.Abs(distance) <= attackDistance)
		{
			if (!playSoundOnMiss) audioSource.PlayOneShot(attackSound);
			player.GetHit(damage, transform.position.x, damageType);
		}
	}

	public virtual void FinishAttack()
	{
		isAttacking = false;
		isLocked = false;
	}

	public virtual void Unlock()
	{
		isLocked = false;
	}

	public override bool GetHit(float damage, float x, DamageType damageType)
	{
		if (hp <= 0) return true;
		damageSystemPlay |= damageType != DamageType.Poison && damageType != DamageType.Electricity;
		float damageTakenLocal = damage * dmgSpec[damageType];
		damageTaken += damageTakenLocal;
		hp -= damageTakenLocal;
		damageSide = (int)Mathf.Sign(x - transform.position.x);
		if (hp <= 0) Die();
		return true;
	}

	public virtual void Die()
	{
		StartCoroutine(Dying());
	}

	protected virtual IEnumerator Dying()
	{
		controller.EnemyKilled(scoreValue);
		IsDead = true;
		animator.SetBool("isDead", true);
		audioSource.pitch = Random.Range(0.97f, 1.1f);
		audioSource.PlayOneShot(deathSound);
		spriteRenderer.sortingOrder--;
		yield return new WaitForSeconds(0.07f);
		boxCollider.enabled = false;
		StartCoroutine(Destroing());
	}

	private IEnumerator Destroing()
	{
		yield return new WaitForSeconds(5);
		float startTime = Time.time;
		float duration = 1;
		Color color = new Color(1, 1, 1);
		for (float a = 1; Time.time - startTime < duration; a = Mathf.Lerp(1, 0, (Time.time - startTime) / duration))
		{
			color.a = a;
			spriteRenderer.color = color;
			yield return null;
		}
		Destroy(gameObject);
	}
}