using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
	public float movementSpeed;
	public float healthPoints;
	public float regeneration;
	public GameObject grenade;
	public float grenadeRate;
	public float tpDistance;
	public float tpChargeTime;
	public int tpAccum;
	public GameObject line;
	public GameObject teleportSymbol;
	public AudioClip damageTaken;
	public AudioClip teleportSound;
	public GameObject dmgSystem;

	public bool IsDead { get; private set; }

	private SpriteRenderer spriteRenderer;
	private Animator animator;
	private AudioSource audioSource;
	private new ParticleSystem particleSystem;
	private RawImage healthBar;
	private RawImage healthBarBG;
	private RectTransform healthBarSize;
	private Weapon weapon;
	private Controller controller;

	private int direction;
	private float hp;
	private bool tp;
	private bool tpin;
	private float lastTP;
	private float lastGrenade;
	private float tpSpeed = 0.2f;
	private float throwingForce = 75;
	private bool isThrowing;
	private float throwingAngle = Mathf.PI / 4;
	private GameObject Line;
	private LineRenderer lineRenderer;
	private bool uprise;
	private Animator[] TPIcon;
	private int tpCharged;
	private float lastChargeTime;
	private float lastHp;
	private bool left;

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
		controller = FindObjectOfType<Controller>();
		direction = 1;
		hp = healthPoints;
		lastGrenade = Time.time - 60 / grenadeRate;
		lastTP = Time.time - 60 / tpChargeTime;
		tpCharged = tpAccum;
		InitBullets();
		InitTeleport();
	}

	public void InitBullets()
	{
		weapon.InitBullets();
	}

	public void InitTeleport()
	{
		
		if (TPIcon != null)
		{
			for (int i = 0; i < TPIcon.Length; i++)
				Destroy(TPIcon[i].gameObject);
		}
		TPIcon = new Animator[tpAccum];
		var corner = Camera.main.ScreenToWorldPoint(Vector3.zero);
		float width = Mathf.Abs((corner.x - Camera.main.transform.position.x) * 2);
		float sY = weapon.BulletsY - 5;
		int left = tpAccum;
		int index = 0;
		while (left > 0)
		{
			int now = left;
			while (now * 3 + 5 >= width) now--;
			float sX = -8 * now / 2f + 1;// + Camera.main.transform.position.x;
			for (int i = 0; i < now; i++)
			{
				TPIcon[index] = Instantiate(teleportSymbol, transform).GetComponent<Animator>();
				TPIcon[index].transform.Translate(new Vector3(sX + i * 8, sY));
				index++;
			}
			sY -= 4;
			left -= now;
		}
	}

	public void GetHit(float damage, float x)
	{
		hp -= damage;
		if (!audioSource.isPlaying)
			audioSource.PlayOneShot(damageTaken);
		left = transform.position.x < x;
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
			particleSystem.Play();
			animator.Play("Idle");
			IsDead = true;
		}
		else
		{
			float percent = hp / healthPoints;
			healthBarBG.color = new Color(0.8f, 0.8f, 0.8f);
			healthBar.color = new Color(1 - percent, percent, 0, 1);
			healthBarSize.sizeDelta = new Vector2(40 * percent, 2);
			healthBarSize.anchoredPosition = new Vector2(0, 34);
		}
	}

	private void CancelThrowing()
	{
		isThrowing = false;
		if (Line != null) Destroy(Line);
		Time.timeScale = 1;
	}

	void Update()
	{
		if (IsDead) return;

		if (lastHp > hp)
		{
			Instantiate(dmgSystem, transform.position, Quaternion.Euler(0, 0, left ? 0 : 180));
		}

		hp += regeneration / 60 * Time.deltaTime;
		UpdateHealth();
		lastHp = hp;

		int delta = 0;

		if (Time.time - lastChargeTime >= tpChargeTime && tpCharged < tpAccum)
		{
			tpCharged++;
			lastChargeTime = Time.time;
			if (tpCharged < tpAccum)
			{
				TPIcon[tpCharged].speed = 1 / tpChargeTime;
				TPIcon[tpCharged].Play("Tranzit");
			}
		}

		if (tp)
		{
			if (Time.time - lastTP >= tpSpeed && !tpin)
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
			if (Time.time - lastTP >= tpSpeed * 2)
			{
				tpin = false;
				tp = false;
			}
			return;
		}
		if (!weapon.IsAttacking)
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
			if (Input.GetKeyDown(KeyCode.F) && (!weapon.IsReloading || !weapon.ManualReload))
			{
				if (Time.time - lastGrenade >= 60 / grenadeRate)
				{
					weapon.CancelReload();
					if (Line != null) Destroy(Line);
					isThrowing = true;
					Line = Instantiate(line, new Vector3(), Quaternion.identity);
					lineRenderer = Line.GetComponent<LineRenderer>();
					lineRenderer.positionCount = 10;
					//throwingAngle = Mathf.PI / 2.5f;
					throwingForce = 20;
					uprise = true;
					Time.timeScale = 0.4f;
				}
			}
			if (isThrowing)
			{
				{
					var pos = new Vector3(transform.position.x + direction * 5, transform.position.y);
					float mass = grenade.GetComponent<Rigidbody2D>().mass;
					float acceleration = 1.2f * mass;
					float dY = throwingForce / 10 * Mathf.Sin(throwingAngle) / mass;
					float dX = throwingForce / 10 * Mathf.Cos(throwingAngle) * direction / mass;
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
				//throwingAngle += (uprise ? 1 : -1) * 0.02f;
				//if (throwingAngle >= Mathf.PI / 2.5) uprise = false;
				//if (throwingAngle <= 0) uprise = true;
				throwingForce += (uprise ? 1 : -1) * 2;
				if (throwingForce >= 100) uprise = false;
				if (throwingForce <= 20) uprise = true;
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
					g.GetComponent<Rigidbody2D>().AddTorque(-5 * direction, ForceMode2D.Impulse);
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
					weapon.PerformAttack(0);
					CancelThrowing();
				}
				else if (Input.GetKey(KeyCode.Quote) || Input.GetKey(KeyCode.N))
				{
					weapon.PerformAttack(1);
					CancelThrowing();
				}
			}
			///
			if ((Input.GetKey(KeyCode.R) && weapon.ManualReload) || (weapon.CanReload && !isThrowing))
			{
				weapon.PerformReload();
				CancelThrowing();
			}
			if (!weapon.IsAttacking && Input.GetKey(KeyCode.Space))
			{
				if (tpCharged > 0)
				{
					CancelThrowing();
					if (tpCharged != tpAccum)
					{
						TPIcon[tpCharged].transform.Translate(new Vector3(-8, 0));
						TPIcon[tpCharged - 1].transform.Translate(new Vector3(8, 0));
						var temp = TPIcon[tpCharged];
						TPIcon[tpCharged] = TPIcon[tpCharged - 1];
						TPIcon[tpCharged - 1] = temp;
						TPIcon[tpCharged].Play("Empty");
					}
					else
					{
						lastChargeTime = Time.time;
						TPIcon[tpCharged - 1].speed = 1 / tpChargeTime;
						TPIcon[tpCharged - 1].Play("Tranzit");
					}
					tpCharged--;
					audioSource.PlayOneShot(teleportSound);
					animator.speed = 1 / tpSpeed;
					animator.Play("TPin");
					tp = true;
					tpin = false;
					lastTP = Time.time;
					weapon.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0);
				}
			}
		}
	}
}
