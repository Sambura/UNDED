using System.Collections.Generic;
using UnityEngine;

public class Player : Entity
{
	public float movementSpeed;
	public float regeneration;
	public float damageLockScale;
	public float damageLockTime;
	public float unlockSpeed;
	public float stillRegenerationBoost;
	public AudioClip damageTaken;
	public GameObject dmgSystem;
	[SerializeField] private Transform UISpawnPoint;
	public Transform weaponHolder;

	private Animator animator;
	private AudioSource audioSource;
	private Controller controller;
	[System.NonSerialized] public Weapon weapon;
	[System.NonSerialized] public Teleport teleport;
	[System.NonSerialized] public Thrower thrower;
	[System.NonSerialized] public Shield shield;

	[SerializeField] private DamageSpec[] damageSpecifications;
	public Dictionary<DamageType, float> dmgSpec;

	private HealthBar healthBar;
	public float hp { get; private set; }
	private float lastHp;
	private bool left;
	private float iconY;
	private bool damageSystemPlay;
	private float regenerationScale;
	private float regenUnlockTime;

	public bool Space { get; set; }
	public bool F { get; set; }
	public bool R { get; set; }
	public bool Fire1 { get; set; }
	public bool Fire2 { get; set; }
	public float movingControls { get; set; }

	#region Singleton
	public static Player Instance;

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
		}
		else Destroy(gameObject);
	}
	#endregion

	public void InitSpecs()
	{
		dmgSpec = new Dictionary<DamageType, float>();
		foreach (var i in damageSpecifications)
		{
			dmgSpec.Add(i.damageType, i.multiplier);
		}
		foreach (var i in System.Enum.GetValues(typeof(DamageType)))
		{
			if (dmgSpec.ContainsKey((DamageType)i)) continue;
			dmgSpec.Add((DamageType)i, 1);
		}
	}

	void Start()
	{
		animator = GetComponent<Animator>();
		audioSource = GetComponent<AudioSource>();
		controller = FindObjectOfType<Controller>();
		hp = maxHealth;
		healthBar = GameObject.FindGameObjectWithTag("HealthBar").GetComponent<HealthBar>();
		UISpawnPoint = GameObject.FindGameObjectWithTag("UIgrip").transform;
		healthBar.Init(hp / maxHealth);
		/*
		var f = new System.IO.FileInfo("player.json");
		var ff = f.CreateText();
		ff.Write(JsonUtility.ToJson(this, true));
		ff.Close();*/

		InitBullets();
		InitOther();
		InitSpecs();
	}

	public void GetHealth(float health)
	{
		hp += health;
	}

	public void InitBullets()
	{
		iconY = -2.5f;
		iconY = weapon.InitUIElements(new Vector2(0, iconY), UISpawnPoint).y;
	}

	public void InitOther()
	{
		Vector2 drawPosition = new Vector2(0, iconY);
		if (teleport != null) drawPosition = teleport.InitAccessory(drawPosition, UISpawnPoint);
		if (thrower != null) thrower.InitThrower(drawPosition, UISpawnPoint);
	}

	public override bool GetHit(float damage, float x, DamageType damageType)
	{
		if (IsDead) return false;
		if (shield != null)
			if (shield.toBlockSet.Contains(damageType))
			{
				damage = shield.GetHit(damage, x);
			}
		float dmg = damage * dmgSpec[damageType];
		if (dmg == 0) return true;
		damageSystemPlay = damageType != DamageType.Poison && damageType != DamageType.Electricity;
		hp -= dmg;
		if (!audioSource.isPlaying)
			audioSource.PlayOneShot(damageTaken);
		left = transform.position.x < x;
		regenerationScale = damageLockScale;
		regenUnlockTime = Time.time + damageLockTime;
		return true;
	}

	private void UpdateHealth()
	{
		hp = Mathf.Clamp(hp, 0, maxHealth);
		healthBar.UpdateHealth(hp / maxHealth);
		if (hp == 0)
		{
			animator.SetBool("isDead", true);
			IsDead = true;
			StartCoroutine(controller.Death());
			weapon.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 1);
			//CancelThrowing();
		}
	}

	public void GetAccessoryAction(int idx)
	{
		teleport.GetAccessoryAction(idx);
	}

	void Update()
	{
		if (IsDead || controller.IsPaused) return;

#if UNITY_STANDALONE || UNITY_EDITOR
		Fire1 = Input.GetKey(KeyCode.M) || Input.GetKey(KeyCode.Return);
		Fire2 = Input.GetKey(KeyCode.Quote) || Input.GetKey(KeyCode.N);
		F = Input.GetKey(KeyCode.F);
		R = Input.GetKey(KeyCode.R);
		Space = Input.GetKey(KeyCode.Space);
		movingControls = Input.GetAxisRaw("Horizontal");
#endif

		if (lastHp > hp && Settings.particles && damageSystemPlay)
		{
			Instantiate(dmgSystem, transform.position, Quaternion.Euler(0, 0, left ? 0 : 180));
			controller.InstantiateDamageLabel(transform.position + new Vector3(0, 10), Mathf.RoundToInt(lastHp - hp));
		}

		hp += regeneration / 60 * Time.deltaTime * regenerationScale;
		UpdateHealth();
		if (IsDead) return;
		lastHp = hp;

		if (Time.time > regenUnlockTime)
		{
			regenerationScale = Mathf.Clamp01(regenerationScale + unlockSpeed * Time.deltaTime);
		}

		bool isThrowing = thrower != null;
		if (isThrowing) isThrowing = thrower.IsThrowing;

		if (teleport != null) if (teleport.IsTeleporting) return;
		int delta = 0;
		if (movingControls > 0)
		{
			delta = 1;
			if (transform.right.x < 0)
				if (!isThrowing)
					transform.Rotate(Vector3.up, 180);
				else delta = -1;
			animator.SetFloat("walkSpeed", movementSpeed / 75);
			animator.SetBool("isWalking", true);
		}
		else
		if (movingControls < 0)
		{
			delta = 1;
			if (transform.right.x > 0)
				if (!isThrowing)
					transform.Rotate(Vector3.up, 180);
				else delta = -1;
			animator.SetFloat("walkSpeed", movementSpeed / 75);
			animator.SetBool("isWalking", true);
		}
		else
		{
			animator.SetBool("isWalking", false);
			hp += regeneration / 60 * Time.deltaTime * regenerationScale * stillRegenerationBoost;
		}

		if (F && (!weapon.IsReloading || !weapon.ManualReload) && thrower != null)
		{
			thrower.PerformThrow();
		}

		float d = movementSpeed * Time.deltaTime * delta;
		transform.Translate(new Vector3(d, 0));
		if (Mathf.Abs(transform.position.x) > 175)
		{
			transform.Translate(new Vector3(175 - Mathf.Abs(transform.position.x), 0));
		}
	}

	private void FixedUpdate()
	{
		if (IsDead || controller.IsPaused) return;

		if (teleport != null) if (teleport.IsTeleporting) return;

		bool isThrowing = thrower != null;
		if (isThrowing) isThrowing = thrower.IsThrowing;
		if (!isThrowing)
		{
			if (Fire1)
			{
				weapon.PerformAttack(0);
			}
			else
			if (Fire2)
			{
				weapon.PerformAttack(1);
			}
			if (weapon.CanReload || (R && weapon.ManualReload)) weapon.PerformReload();
		}

		if (Space && teleport != null)
		{
			teleport.InvokeTP();
		}
	}
}