using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Target : Enemy
{
    public UnityAction onHit;
	public AudioClip hitSound;
	public float ariseTime = float.PositiveInfinity;

	private float hitTime;

    public bool IsOnGround
	{
		get { return isOnGround; }
		set
		{
			if (isOnGround == value) return;
			isOnGround = value;
			if (!value)
			{
				animator.SetTrigger("Rise");
				boxCollider.enabled = true;
			}
			else {
				animator.SetTrigger("Fall");
				boxCollider.enabled = false;
			}
		}
	}

	private bool isOnGround;

	protected override void Start()
	{
		animator = GetComponentInChildren<Animator>();
		audioSource = GetComponentInChildren<AudioSource>();
		boxCollider = GetComponentInChildren<BoxCollider2D>();
		controller = Controller.Instance;
		if (controller == null) controller = FindObjectOfType<Controller>();
		hp = maxHealth;
		IsOnGround = true;
	}

	protected override void Update()
    {
		if (IsOnGround && Time.time - hitTime >= ariseTime && !IsDead)
		{
			IsOnGround = false;
		}

		if (damageTaken > 0.51f)
		{
			controller.InstantiateDamageLabel(transform.position, Mathf.RoundToInt(damageTaken));
			controller.IncreaseScore(Mathf.Min(damageTaken, maxHealth) / 25);
			if (Settings.particles && damageSystemPlay)
				Instantiate(dmgSystem, transform.position, Quaternion.Euler(0, 0, damageSide == -1 ? 0 : 180));
			damageTaken = 0;
			damageSystemPlay = false;
			audioSource.PlayOneShot(hitSound);
			onHit.Invoke();
			IsOnGround = true;
			hitTime = Time.time;
		}
	}

	public override bool GetHit(float damage, float x, DamageType damageType)
	{
		if (hp <= 0) return true;
		damageSide = (int)Mathf.Sign(x - transform.position.x);
		//if (damageSide == Mathf.Sign(transform.right.x)) return false;
		damageSystemPlay |= damageType != DamageType.Poison && damageType != DamageType.Electricity;
		damageTaken += damage;
		hp -= damage;
		if (hp <= 0)
		{
			controller.EnemyKilled(scoreValue);
			IsDead = true;
			animator.SetBool("isDead", true);
			audioSource.pitch = Random.Range(0.97f, 1.1f);
			audioSource.PlayOneShot(deathSound);
			boxCollider.enabled = false;
		}
		return true;
	}
}
