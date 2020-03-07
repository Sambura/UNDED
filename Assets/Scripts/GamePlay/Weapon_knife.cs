using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon_knife : Weapon
{
	public Transform shotPoint;
	public float damage;
	public float attackRate;
	public float attackDistance;
	public float maxAttackDistance;
	public float damageLose;
	public AudioClip attackSound;
	public DamageType damageType;

	private Controller controller;
	private Animator animator;
	private AudioSource audioSource;
	private Player parent;

	private float nextAttack;
	private float attackDelay;

	private void Start()
	{
		UpdateDelays();
		animator = GetComponent<Animator>();
		controller = FindObjectOfType<Controller>();
		audioSource = GetComponent<AudioSource>();
		parent = GetComponentInParent<Player>();
		CanAttack = true;
	}

	public void UpdateDelays()
	{
		attackDelay = 60 / attackRate;
	}

	public void Attack()
	{
		IsAttacking = false;
		animator.SetBool("isAttacking", false);
		bool flag = false;
		var victims = Physics2D.CircleCastAll(shotPoint.position, attackDistance, Vector2.zero, 100, 512);
		foreach (var entity in victims)
		{
			var entityComp = entity.collider.GetComponent<Entity>();
			if (entityComp == parent) continue;
			flag = true;
			float dmg = Mathf.Max(0.1f, damage - Vector2.Distance(shotPoint.position, entity.transform.position) * damageLose);
			entityComp.GetHit(dmg, transform.position.x, damageType);
		}
		if (flag) audioSource.PlayOneShot(attackSound);
	}
	
	public void SetAttack()
	{
		animator.SetBool("isAttacking", true);
	}

	public override void PerformAttack(int index)
	{
		if (IsAttacking) return;
		if (nextAttack > Time.time) return;
		animator.SetFloat("attackSpeed", 1 / attackDelay);
		animator.SetTrigger("Shot");
		IsAttacking = true;
		nextAttack = Time.time + attackDelay;
	}

	private void OnDrawGizmosSelected()
	{
		if (shotPoint == null) return;
		Gizmos.DrawWireSphere(shotPoint.position, attackDistance);
	}
}