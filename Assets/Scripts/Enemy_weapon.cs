using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_weapon : Weapon
{
	public int[] gunFireLength;
	public float[] gunFireDelay;
	public GameObject[] bullet;
	public AudioClip[] shot;
	public Vector2 offset;

	private AudioSource audioSource;
	private SpriteRenderer spriteRenderer;
	private Animator animator;

	private int direction;
	private int bulletIndex;
	private float lastShot;
	private float shotStartTime;
	private int fireLeft;

	private void Start()
	{
		audioSource = GetComponent<AudioSource>();
		spriteRenderer = GetComponent<SpriteRenderer>();
		if (animator == null)
		animator = GetComponent<Animator>();
	}

	public override void SetAnimator(Animator customAnimator)
	{
		animator = customAnimator;
	}

	public override void InitBullets()
	{
		return;
	}

	public override void SetDirection(int direction)
	{
		this.direction = direction;
		spriteRenderer.flipX = direction == -1;
	}


	private void Update()
	{

		if (IsAttacking)
		{
			if (fireLeft == 0)
			{
				IsAttacking = false;
			}
			if (Time.time - lastShot >= gunFireDelay[bulletIndex])
			{
				Attack();
			}
		}
	}

	public override void PerformAttack(int index)
	{
		bulletIndex = index; // Updating index
		IsAttacking = true; // Weapon is shooting now
		animator.speed = 1; // Reset animator speed
		shotStartTime = Time.time; // When this shot started
		fireLeft = gunFireLength[index]; // Bullets to spend
	}

	private void Attack()
	{
		animator.Play("Shot");
		audioSource.PlayOneShot(shot[bulletIndex], 1);
		var b = Instantiate(bullet[bulletIndex], new Vector3(transform.position.x + offset.x * direction,
			transform.position.y + offset.y), Quaternion.identity).GetComponent<Bullet>();
		b.SetDirection(direction);
		b.PlayerProperty = false;
		fireLeft--;
		lastShot = Time.time;
	}

	public override void PerformReload()
	{
		return;
	}

	public override void CancelReload()
	{
		IsAttacking = false;
	}
}
