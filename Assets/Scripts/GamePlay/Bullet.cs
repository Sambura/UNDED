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

	private Controller controller;

	public int Direction { get; private set; }
	public bool Active { get; private set;}

	private Vector3 startPoint;

	public void SetController(Controller c)
	{
		controller = c;
	}

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

    void FixedUpdate()
    {
		transform.Translate(new Vector3(movementSpeed * Time.deltaTime, 0));
		if (Mathf.Abs(transform.position.x) > controller.levelWidth)
		{
			Destroy(gameObject);
			Active = false;
			return;
		} 
    }

	private float GetDamage()
	{
		float distance = Mathf.Sqrt(Mathf.Abs(transform.position.x - startPoint.x));
		return Mathf.Max(minDamage, damage - distance * damageLose);
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (!Active) return;
		var enemy = collision.gameObject.GetComponent<Enemy>();
		if (enemy != null)
		{
			enemy.GetHit(GetDamage(), transform.position.x);
			Destroy(gameObject);
			Active = false;
			return;
		}
	}
}
