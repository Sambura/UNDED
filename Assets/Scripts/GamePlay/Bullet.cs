using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Bullet : MonoBehaviour
{
	public float movementSpeed;
	public float damage;
	public float angle;
	public float damageLose;
	public float minDamage;
	public DamageType damageType;
	public AudioClip shotSound;
	public float pitchDelta = 0.07f;
	[SerializeField] private TrailRenderer trail;
	[SerializeField] private Rigidbody2D rb;
	[SerializeField] private BoxCollider2D boxCollider;
	[SerializeField] private SpriteRenderer spriteRenderer;
	[SerializeField] private float playerTresholdTime = 0.3f;

	private Collider2D lastCollider;
	private float startTime;
	private bool isActive;
	private Vector3 startPoint;

	public void SetDirection(int d)
	{
		float deltaAngle = Random.Range(-angle, angle);
		if (d == -1) deltaAngle += 180;
		transform.Rotate(new Vector3(0, 0, deltaAngle));
		startPoint = transform.position;
		isActive = true;
		rb.velocity = transform.right * movementSpeed;
		startTime = Time.time;
	}

	public void MultiplyDamage(float multiplier)
	{
		damage *= multiplier;
		damageLose *= multiplier;
		minDamage *= multiplier;
	}

	private float GetDamage()
	{
		float distance = Mathf.Sqrt(Mathf.Abs(transform.position.x - startPoint.x));
		return Mathf.Max(minDamage, damage - distance * damageLose);
	}

	private void OnCollision(Collider2D collision)
	{
		if (!isActive) return;
		if (collision == lastCollider) return;
		var entity = collision.GetComponent<Entity>();
		if (entity != null)
		{
			if (entity is Player && Time.time - startTime < playerTresholdTime) return;
			lastCollider = collision; // Do not remove / replace
			if (!entity.GetHit(GetDamage(), transform.position.x, damageType)) return;
		} else lastCollider = collision;
		trail.emitting = false;
		isActive = false;
		boxCollider.enabled = false;
		spriteRenderer.enabled = false;
		if (trail != null)
			StartCoroutine(Destroying(trail.time));
		else
			Destroy(gameObject);
	}
	
	private void OnTriggerStay2D(Collider2D collision)
	{
		OnCollision(collision);
	}
	
	private void OnTriggerEnter2D(Collider2D collision)
	{
		OnCollision(collision);
	}
	
	private void OnTriggerExit2D(Collider2D collision)
	{
		OnCollision(collision);
	}

	private IEnumerator Destroying(float duration)
	{
		yield return new WaitForSeconds(duration);
		Destroy(gameObject);
	}
}
