using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
	public float movementSpeed;
	public float healthPoints;
	public ParticleSystem deathFX;
	public float regeneration;
	public GameObject grenade;
	public float grenadeRate;
	public float tpDistance;
	public float tpRate;
	public GameObject line;
	public GameObject tpIcon;
	public AudioClip damageTaken;
	public AudioClip teleportSound;

	public bool IsDead { get; private set; }

	private SpriteRenderer spriteRenderer;
	private Animator animator;
	private AudioSource audioSource;
	private new ParticleSystem particleSystem;
	private RawImage healthBar;
	private RawImage healthBarBG;
	private RectTransform healthBarSize;
	private Weapon weapon;

	private int direction;
	private float hp;
	private bool tp;
	private bool tpin;
	private float tpTime;
	private float lastGrenade;
	private float tpSpeed = 0.2f;
	private float throwingForce = 75;
	private bool isThrowing;
	private float throwingAngle;
	private GameObject Line;
	private LineRenderer lineRenderer;
	private bool uprise;
	private SpriteRenderer TPIcon;

	void Start()
	{
		spriteRenderer = GetComponent<SpriteRenderer>();
		animator = GetComponent<Animator>();
		audioSource = GetComponent<AudioSource>();
		particleSystem = GetComponent<ParticleSystem>();
		healthBar = GetComponentsInChildren<RawImage>()[1];
		healthBarBG = GetComponentsInChildren<RawImage>()[0];
		healthBarSize = GetComponentsInChildren<RectTransform>()[2];
		weapon = GetComponentInChildren<Weapon>();
		direction = 1;
		hp = healthPoints;
		InitBullets();
		lastGrenade = Time.time - 60 / grenadeRate;
		var f = Instantiate(tpIcon, transform);
		f.transform.Translate(new Vector3(0, weapon.bulletsY + 5));
		TPIcon = f.GetComponent<SpriteRenderer>();
		tpTime = Time.time - 60 / tpRate;
	}

	public void InitBullets()
	{
		weapon.InitBullets();
	}

	public void GetHit(float damage, float x)
	{
		hp -= damage;
		if (!audioSource.isPlaying)
			audioSource.PlayOneShot(damageTaken);
		particleSystem.Play();
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
			healthBarBG.color = new Color(0, 0, 0, 0);
			healthBar.color = new Color(0, 0, 0, 0);
			Instantiate(deathFX, transform.position, Quaternion.identity);
			animator.Play("Idle");
			IsDead = true;
		}
		else
		{
			float percent = hp / healthPoints;
			healthBarBG.color = new Color(0.8f, 0.8f, 0.8f);
			healthBar.color = new Color(1 - percent, percent, 0, 1);
			healthBarSize.sizeDelta = new Vector2(20 * percent, 2);
			healthBarSize.anchoredPosition = new Vector2(0, 34);
		}
	}

	private void CancelThrowing()
	{
		isThrowing = false;
		if (Line != null) Destroy(Line);
	}

	void Update()
	{
		if (IsDead) return;
		hp += regeneration / 60 * Time.deltaTime;
		UpdateHealth();
		int delta = 0;
		if (Time.time - tpTime >= 60 / tpRate)
		{
			TPIcon.color = new Color(1, 1, 1, 1);
		} else
		{
			TPIcon.color = new Color(1, 1, 1, 0.3f);
		}
		if (tp)
		{
			if (Time.time - tpTime >= tpSpeed && !tpin)
			{
				tpin = true;
				weapon.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 1);
				animator.Play("TPout");
				float d = direction * tpDistance;
				if (Mathf.Abs(transform.position.x + d) >= 175)
				{
					d = Mathf.Sign(transform.position.x) * 175 - transform.position.x;
				}
				transform.Translate(new Vector3(d, 0));
			}
			if (Time.time - tpTime >= tpSpeed * 2)
			{
				tpin = false;
				tp = false;
			}
			return;
		}
		if (!weapon.IsShooting)
		{
			if (Input.GetKey(KeyCode.D))
			{
				delta = 1;
				direction = 1;
				spriteRenderer.flipX = false;
				animator.Play("Walk");
				animator.speed = 1;
				CancelThrowing();
			}
			else
				if (Input.GetKey(KeyCode.A))
			{
				delta = -1;
				direction = -1;
				spriteRenderer.flipX = true;
				animator.Play("Walk");
				animator.speed = 1;
				CancelThrowing();
			}
			///
			weapon.SetDirection(direction);
			///
			if (Input.GetKeyDown(KeyCode.F))
			{
				if (Time.time - lastGrenade >= 60 / grenadeRate)
				{
					if (Line != null) Destroy(Line);
					isThrowing = true;
					Line = Instantiate(line, new Vector3(), Quaternion.identity);
					lineRenderer = Line.GetComponent<LineRenderer>();
					lineRenderer.positionCount = 10;
					throwingAngle = Mathf.PI / 2.5f;
					uprise = false;
				}
			}
			if (isThrowing)
			{
				{
					var pos = new Vector3(transform.position.x + direction * 5, transform.position.y);
					float acceleration = 1.2f;
					float dY = throwingForce / 10 * Mathf.Sin(throwingAngle);
					float dX = throwingForce / 10 * Mathf.Cos(throwingAngle) * direction;
					lineRenderer.positionCount = 30;
					for (int v = 0; v < lineRenderer.positionCount; v++)
					{
						lineRenderer.SetPosition(v, pos);
						pos.x += dX;
						pos.y += dY;
						dY -= acceleration;
						if (pos.y <= -20)
						{
							var dZ = pos.y + 20;
							pos.y = -20;
							pos.x -= dZ / dY * dX;
							lineRenderer.positionCount = v + 2;
							lineRenderer.SetPosition(v + 1, pos);
							break;
						}
						if (pos.y >= 20)
						{
							var dZ = pos.y - 20;
							pos.y = 20;
							pos.x -= dZ / dY * dX;
							lineRenderer.positionCount = v + 2;
							lineRenderer.SetPosition(v + 1, pos);
							break;
						}
					}
				}
				throwingAngle += (uprise ? 1 : -1) * 0.02f;
				if (throwingAngle >= Mathf.PI / 2.5) uprise = false;
				if (throwingAngle <= 0) uprise = true;
			}
			if (Input.GetKeyUp(KeyCode.F))
			{
				if (isThrowing)
				{
					var g = Instantiate(grenade, new Vector3(transform.position.x + direction * 5, transform.position.y), Quaternion.identity);
					g.GetComponent<Rigidbody2D>().AddForce(new Vector2(
						direction * throwingForce * Mathf.Cos(throwingAngle),
						throwingForce * Mathf.Sin(throwingAngle)), 
						ForceMode2D.Impulse);
					lastGrenade = Time.time;
					CancelThrowing();
				}
			}
			float d = delta * movementSpeed * Time.deltaTime;
			if (Mathf.Abs(transform.position.x + d) >= 175)
			{
				d = Mathf.Sign(transform.position.x) * 175 - transform.position.x;
			}
			transform.Translate(new Vector3(d, 0));
			///
			if (delta == 0)
			{
				animator.Play("Idle");
				if (Input.GetKey(KeyCode.M) || Input.GetKey(KeyCode.Return))
				{
					weapon.PerformShot(0);
					CancelThrowing();
				}
				else if (Input.GetKey(KeyCode.Quote) || Input.GetKey(KeyCode.N))
				{
					weapon.PerformShot(1);
					CancelThrowing();
				}
			}
			///
			if ((Input.GetKey(KeyCode.R) && !weapon.partialReload) || weapon.CanReload)
			{
				weapon.PerformReload();
				CancelThrowing();
			}
			if (!weapon.IsShooting && Input.GetKey(KeyCode.Space) && Time.time - tpTime >= 60 / tpRate)
			{
				CancelThrowing();
				audioSource.PlayOneShot(teleportSound);
				animator.speed = 1 / tpSpeed;
				animator.Play("TPin");
				tp = true;
				tpin = false;
				tpTime = Time.time;
				weapon.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0);
			}
		}
	}
}
