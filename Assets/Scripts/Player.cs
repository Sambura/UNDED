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

	public bool IsDead { get; private set; }

	private Camera mainCamera;
	private SpriteRenderer spriteRenderer;
	private Animator animator;
	private AudioSource audioSource;
	private new ParticleSystem particleSystem;
	private RawImage healthBar;
	private RawImage healthBarBG;
	private RectTransform healthBarSize;
	private Weapon weapon;

	private bool idle;
	private int direction;
	private float hp;
	private bool tp;
	private float tpTime;

	void Start()
	{
		mainCamera = Camera.main;
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
	}

	public void InitBullets()
	{
		weapon.InitBullets();
	}

	public void GetHit(float damage, float x)
	{
		hp -= damage;
		if (!audioSource.isPlaying)
			audioSource.Play();
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

	void Update()
	{
		if (IsDead) return;
		hp += regeneration / 60 * Time.deltaTime;
		UpdateHealth();
		int delta = 0;
		if (tp)
		{
			if (Time.time - tpTime >= 0.42f)
			{
				tp = false;
				weapon.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 1);
				animator.Play("Idle");
				float d = direction * movementSpeed;
				if (Mathf.Abs(transform.position.x + d) >= 175)
				{
					d = Mathf.Sign(transform.position.x) * 175 - transform.position.x;
				}
				var t = new Vector3(d, 0);
				transform.Translate(t);
				mainCamera.transform.Translate(t);
			} else
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
				idle = false;
			}
			else
				if (Input.GetKey(KeyCode.A))
			{
				delta = -1;
				direction = -1;
				spriteRenderer.flipX = true;
				animator.Play("Walk");
				idle = false;
			}
			///
			weapon.SetDirection(direction);
			///
			float d = delta * movementSpeed * Time.deltaTime;
			if (Mathf.Abs(transform.position.x + d) >= 175)
			{
				d = Mathf.Sign(transform.position.x) * 175 - transform.position.x;
			}
			var t = new Vector3(d, 0);
			transform.Translate(t);
			mainCamera.transform.Translate(t);
			///
			if (delta == 0)
			{
				animator.Play("Idle");
				idle = true;
				if (Input.GetKey(KeyCode.M) || Input.GetKey(KeyCode.Return))
				{
					weapon.PerformShot(0);
				}
				else if (Input.GetKey(KeyCode.Quote) || Input.GetKey(KeyCode.N))
				{
					weapon.PerformShot(1);
				}
			}
			///
			if ((Input.GetKey(KeyCode.R) && !weapon.partialReload) || weapon.CanReload)
			{
				weapon.PerformReload();
			}
			if (!weapon.IsShooting && Input.GetKey(KeyCode.Space))
			{
				animator.Play("TP");
				tp = true;
				tpTime = Time.time;
				weapon.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0);
			}
		}
	}
}
