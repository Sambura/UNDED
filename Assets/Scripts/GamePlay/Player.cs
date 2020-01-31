using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : Entity
{
	public float movementSpeed;
	public float regeneration;
	public AudioClip damageTaken;
	public GameObject dmgSystem;
	public Transform UISpawnPoint;

	private SpriteRenderer spriteRenderer;
	private Animator animator;
	private AudioSource audioSource;
	public RawImage healthBar;
	public RawImage healthBarBG;
	public RectTransform healthBarSize;
	private Weapon weapon;
	private Controller controller;
	public TeleportAcc equipment;
	public Thrower thrower;

	private int direction;
	private float hp;
	private float lastHp;
	private bool left;
	private float iconY;
	private bool damageSystemPlay;

	void Start()
	{
		spriteRenderer = GetComponent<SpriteRenderer>();
		animator = GetComponent<Animator>();
		audioSource = GetComponent<AudioSource>();
		//healthBar = GetComponentsInChildren<RawImage>()[1];
		//healthBarBG = GetComponentsInChildren<RawImage>()[0];
		//healthBarSize = GetComponentsInChildren<RectTransform>()[2];
		controller = FindObjectOfType<Controller>();
		direction = 1;
		hp = healthPoints;
	}

	public void GetWeapon()
	{
		weapon = GetComponentInChildren<Weapon>();
		InitBullets();
		InitOther();
	}

	public void GetHealth(float health)
	{
		hp += health;
	}

	public void InitBullets()
	{
		iconY = -2.5f;
		weapon.gameObject.SetActive(true);
		iconY = weapon.InitBullets(new Vector2(0, iconY), UISpawnPoint).y;
	}

	public void InitOther()
	{
		var temp = equipment.InitAccessory(new Vector2(0, iconY), UISpawnPoint);
		thrower.InitThrower(temp, UISpawnPoint);
	}

	public override bool GetHit(float damage, float x, DamageType damageType)
	{
		if (IsDead) return false;
		damageSystemPlay = !damageType.HasFlag(DamageType.Poison) && !damageType.HasFlag(DamageType.Electricity);
		hp -= damage;
		if (!audioSource.isPlaying)
			audioSource.PlayOneShot(damageTaken);
		left = transform.position.x < x;
		return true;
	}

	private void UpdateHealth()
	{
		hp = Mathf.Max(0, Mathf.Min(healthPoints, hp));
		if (hp == healthPoints)
		{
			healthBarBG.color = new Color(0.8f, 0.8f, 0.8f, 0);
			healthBar.color = new Color(0, 0, 0, 0);
		}
		else if (hp == 0)
		{
			healthBar.color = new Color(0, 0, 0, 0);
			animator.speed = 1;
			animator.Play("Death2");
			IsDead = true;
			StartCoroutine(controller.Death());
			//CancelThrowing();
		}
		else
		{
			float percent = hp / healthPoints;
			healthBarBG.color = new Color(0.8f, 0.8f, 0.8f);
			healthBar.color = new Color(1 - percent, percent, 0, 1);
			healthBarSize.sizeDelta = new Vector2(40 * percent, 2);
			//healthBarSize.anchoredPosition = new Vector2(0, 34);
		}
	}
	/*
	public void CancelThrowing()
	{
		isThrowing = false;
		if (Line != null) Destroy(Line);
		Time.timeScale = 1;
	}
	*/
	public void GetAccessoryAction(int idx)
	{
		equipment.GetAccessoryAction(idx);
	}

	void Update()
	{
		if (IsDead || controller.IsPaused) return;

		if (lastHp > hp && Settings.particles && damageSystemPlay)
		{
			Instantiate(dmgSystem, transform.position, Quaternion.Euler(0, 0, left ? 0 : 180));
		}

		hp += regeneration / 60 * Time.deltaTime;
		UpdateHealth();
		if (IsDead) return;
		lastHp = hp;

		int delta = 0;
		/*
		if (Time.time - lastGrenade >= 60 / grenadeRate)
		{
			grenadeIcon.color = new Color(1, 1, 1, 1);
		} else
		{
			grenadeIcon.color = new Color(1, 1, 1, 0.4f);
		}
		*/
		if (equipment.isTping) return;

		//if (!weapon.IsAttacking)
		{
			if (Input.GetKey(KeyCode.D))
			{
				if (direction == -1) transform.Rotate(Vector3.up, 180);
				delta = 1;
				direction = 1;
				//spriteRenderer.flipX = false;
				animator.Play("Walk");
				animator.speed = 1;
				//CancelThrowing();
			}
			else
				if (Input.GetKey(KeyCode.A))
			{
				delta = 1;
				if (direction == 1) transform.Rotate(Vector3.up, 180);
				direction = -1;
				//spriteRenderer.flipX = true;
				animator.Play("Walk");
				animator.speed = 1;
				//CancelThrowing();
			}
			///

			equipment.SetDirection(direction);
			weapon.SetDirection(direction);


			if (Input.GetKeyUp(KeyCode.F) && (!weapon.IsReloading || !weapon.ManualReload))
			{
				thrower.PerformThrow();
			}

			///
			/*
			if (Input.GetKeyDown(KeyCode.F) && (!weapon.IsReloading || !weapon.ManualReload))
			{
				if (Time.time - lastGrenade >= 60 / grenadeRate)
				{
					weapon.CancelReload();
					if (Line != null) Destroy(Line);
					isThrowing = true;
					Line = Instantiate(line, new Vector3(), Quaternion.identity);
					lineRenderer = Line.GetComponent<LineRenderer>();
					throwingForce = 20 * grenadeMass;
					uprise = true;
					Time.timeScale = 0.3f;
				}
			}
			if (isThrowing)
			{
				{
					var pos = new Vector3(transform.position.x + direction * 5, transform.position.y);
					float deltaL = 0.05f;
					float acceleration = -9.81f * grenadeGravityScale;
					float dY = throwingForce * Mathf.Sin(throwingAngle) / grenadeMass;
					float dX = throwingForce * Mathf.Cos(throwingAngle) * direction / grenadeMass;
					lineRenderer.positionCount = 50;
					for (int v = 0; v < lineRenderer.positionCount; v++)
					{
						lineRenderer.SetPosition(v, pos);
						pos.x += dX * deltaL;
						pos.y += dY * deltaL;
						dY += acceleration * deltaL;
						if (Mathf.Abs(pos.y) > 20 || Mathf.Abs(pos.x) > 186)
						{
							lineRenderer.positionCount = v + 2;
							lineRenderer.SetPosition(v + 1, pos);
							break;
						}
					}
				}
				throwingForce += ((uprise ? 1 : -1) * 70 * grenadeMass) * Time.unscaledDeltaTime;
				throwingForce = Mathf.Clamp(throwingForce, 18 * grenadeMass, 102 * grenadeMass);
				if (throwingForce >= 100 * grenadeMass) uprise = false;
				if (throwingForce <= 20 * grenadeMass) uprise = true;
			}
			if (Input.GetKeyUp(KeyCode.F))
			{
				if (isThrowing)
				{
					var g = Instantiate(grenade, new Vector2(transform.position.x + direction * 5, transform.position.y), Quaternion.identity);
					var rb = g.GetComponent<Rigidbody2D>();
					rb.AddForce(new Vector2(
						direction * throwingForce * Mathf.Cos(throwingAngle),
						throwingForce * Mathf.Sin(throwingAngle)), 
						ForceMode2D.Impulse);
					rb.AddTorque(-5 * direction, ForceMode2D.Impulse);
					lastGrenade = Time.time;
					CancelThrowing();
				}
			}
			*/
			float d = movementSpeed * Time.deltaTime* delta;
			if (Mathf.Abs(transform.position.x + d * direction) >= 175)
			{
				d = Mathf.Sign(transform.position.x) * 175 - transform.position.x;
			}
			transform.Translate(new Vector3(d, 0));
			///
			if (delta == 0) animator.Play("Idle");
			{
				
				if (Input.GetKey(KeyCode.M) || Input.GetKey(KeyCode.Return))
				{
					weapon.PerformAttack(0);
					//CancelThrowing();
				}
				else if (Input.GetKey(KeyCode.Quote) || Input.GetKey(KeyCode.N))
				{
					weapon.PerformAttack(1);
					//CancelThrowing();
				}
			}
			///
			if ((Input.GetKey(KeyCode.R) && weapon.ManualReload) || (weapon.CanReload && !thrower.IsThrowing))
			{
				weapon.PerformReload();
				//CancelThrowing();
			}
			if (!weapon.IsAttacking && Input.GetKey(KeyCode.Space))
			{
				equipment.InvokeTP();
			}
		}
	}
}
