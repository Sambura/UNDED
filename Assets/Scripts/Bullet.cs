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

	public bool PlayerProperty { get; set; }
	public int Direction { get; private set; }
	public bool Active { get; private set;}

	private Vector3 startPoint;

	public void SetDirection(int d)
	{
		float deltaAngle = Random.Range(-angle, angle);
		if (d == -1) deltaAngle += 180;
		transform.Rotate(new Vector3(0, 0, deltaAngle));
		Direction = d;
	}

    void Start()
    {
		startPoint = transform.position;
		Active = true;
    }

	float Sqr(float n)
	{
		return n * n;
	}

    void Update()
    {
		transform.Translate(new Vector3(movementSpeed * Time.deltaTime, 0));
    }

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (!Active) return;
		var enemy = collision.gameObject.GetComponent<Enemy>();
		if (enemy != null && PlayerProperty)
		{
			float distance = Mathf.Pow(Sqr(transform.position.x - startPoint.x) + Sqr(transform.position.y - startPoint.y), 0.25f);
			float dmg = Mathf.Max(0.1f, damage - distance * damageLose);
			enemy.GetHit(dmg, transform.position.x);
			Destroy(gameObject);
			Active = false;
			return;
		}
		var player = collision.gameObject.GetComponent<Player>();
		if (player != null && !PlayerProperty)
		{
			float distance = Mathf.Pow(Sqr(transform.position.x - startPoint.x) + Sqr(transform.position.y - startPoint.y), 0.25f);
			float dmg = Mathf.Max(0.1f, damage - distance * damageLose);
			player.GetHit(dmg, transform.position.x);
			Destroy(gameObject);
			Active = false;
			return;
		}
		if (collision.gameObject.CompareTag("World"))
		{
			Destroy(gameObject);
			Active = false;
			return;
		}
	}
}
