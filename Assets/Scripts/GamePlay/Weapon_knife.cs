﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon_knife : Weapon
{
	public float damage;
	public float attackRate;
	public float attackDistance;
	public float damageLose;
	public float attackTime;
	public AudioClip attackSound;

	private Controller controller;
	private Animator animator;
	private SpriteRenderer spriteRenderer;
	private AudioSource audioSource;

	private int direction;
	private float lastAttack;
	private float attackDelay;

	private void Start()
	{
		UpdateDelays();
		animator = GetComponent<Animator>();
		spriteRenderer = GetComponent<SpriteRenderer>();
		controller = FindObjectOfType<Controller>();
		audioSource = GetComponent<AudioSource>();
		audioSource.volume *= controller.sfxVolume;
		direction = 1;
		CanAttack = true;
	}

	public void UpdateDelays()
	{
		attackDelay = 60 / attackRate;
	}

	public override void SetAnimator(Animator customAnimator)
	{
		animator = customAnimator;
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
			if (Time.time - lastAttack >= attackTime)
			{
				IsAttacking = false;
				bool flag = false;
				foreach (var enemy in controller.Enemies)
				{
					float distance = enemy.transform.position.x - transform.position.x;
					if (Mathf.Sign(distance) == direction)
					{
						if (Mathf.Abs(distance) <= attackDistance)
						{
							flag = true;
							distance = Mathf.Pow(Mathf.Abs(distance), 0.5f);
							float dmg = Mathf.Max(0.1f, damage - distance * damageLose);
							enemy.GetHit(dmg, transform.position.x);
						}
					}
				}
				if (flag) audioSource.PlayOneShot(attackSound);
			}
		}
		if (Time.time - lastAttack >= attackDelay)
		{
			CanAttack = true;
		}
	}

	public override void PerformAttack(int index)
	{
		if (IsAttacking) return;
		if (!CanAttack) return;
		animator.Play("Shot");
		animator.speed = 1 / attackDelay;
		CanAttack = false;
		IsAttacking = true;
		lastAttack = Time.time;
	}

	public override void CancelReload()
	{
		return;
	}

	public override void InitBullets()
	{
		BulletsY = -15;
		return;
	}

	public override void PerformReload()
	{
		return;
	}
}
