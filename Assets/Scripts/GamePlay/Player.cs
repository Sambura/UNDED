using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : Entity
{
	public float movementSpeed;
	public float regeneration;
	public float dmgLockScale;
	public float dmgLockTime;
	public float unlockSpeed;
	public float stillRegenScaleInc;
	public AudioClip damageTaken;
	public GameObject dmgSystem;
	public Transform UISpawnPoint;

	private Animator animator;
	private AudioSource audioSource;
	private Weapon weapon;
	private Controller controller;
	public TeleportAcc equipment;
	public Thrower thrower;
	public HealthBar healthBar;
	[SerializeField] private DamageSpec[] damageSpecifications;
	public Dictionary<DamageType, float> dmgSpec;
	public Shield shield;
	

	public float hp { get; private set; }
	private float lastHp;
	private bool left;
	private float iconY;
	private bool damageSystemPlay;
	private float regenerationScale;
	private float regenUnlockTime;
	private float regenLockScale;

#if UNITY_ANDROID
	public bool Space { get; set; }
	public bool F { get; set; }
	public bool R { get; set; }
	public bool Fire1 { get; set; }
	public bool Fire2 { get; set; }
	public float movingControls { get; set; }
#endif

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
		hp = healthPoints;
		healthBar.Init(hp / healthPoints);
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
		iconY = weapon.InitUIElements(new Vector2(0, iconY), UISpawnPoint).y;
	}

	public void InitOther()
	{
		var temp = equipment.InitAccessory(new Vector2(0, iconY), UISpawnPoint);
		thrower.InitThrower(temp, UISpawnPoint);
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
		regenerationScale = dmgLockScale;
		regenUnlockTime = Time.time + dmgLockTime;
		return true;
	}

	private void UpdateHealth()
	{
		hp = Mathf.Clamp(hp, 0, healthPoints);
		healthBar.UpdateHealth(hp / healthPoints);
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
		equipment.GetAccessoryAction(idx);
	}

	void Update()
	{
		if (IsDead || controller.IsPaused) return;

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

		if (equipment.isTping) return;
		int delta = 0;
#if UNITY_STANDALONE
		if (Input.GetKey(KeyCode.D))
#elif UNITY_ANDROID
		if (movingControls > 0)
#endif
		{
			delta = 1;
			if (transform.right.x < 0)
				if (!thrower.IsThrowing)
					transform.Rotate(Vector3.up, 180);
				else delta = -1;
			animator.SetFloat("walkSpeed", movementSpeed / 75);
			animator.SetBool("isWalking", true);
		}
		else
#if UNITY_STANDALONE
		if (Input.GetKey(KeyCode.A))
#elif UNITY_ANDROID
		if (movingControls < 0)
#endif
		{
			delta = 1;
			if (transform.right.x > 0)
				if (!thrower.IsThrowing)
					transform.Rotate(Vector3.up, 180);
				else delta = -1;
			animator.SetFloat("walkSpeed", movementSpeed / 75);
			animator.SetBool("isWalking", true);
		}
		else
		{
			animator.SetBool("isWalking", false);
			hp += regeneration / 60 * Time.deltaTime * regenerationScale * stillRegenScaleInc;
		}
		if (
#if UNITY_STANDALONE
		Input.GetKeyUp(KeyCode.F)
#elif UNITY_ANDROID
		F
#endif
			&& (!weapon.IsReloading || !weapon.ManualReload))
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

		if (equipment.isTping) return;
		
		if (!thrower.IsThrowing) {

#if UNITY_STANDALONE
		if (Input.GetKey(KeyCode.M) || Input.GetKey(KeyCode.Return))
#elif UNITY_ANDROID
		if (Fire1)
#endif
			{
				weapon.PerformAttack(0);
			}
			else
#if UNITY_STANDALONE
		if (Input.GetKey(KeyCode.Quote) || Input.GetKey(KeyCode.N))
#elif UNITY_ANDROID
		if (Fire2)
#endif
			{
				weapon.PerformAttack(1);
			}
		}
		///
		if ((
#if UNITY_STANDALONE
		Input.GetKey(KeyCode.R)
#elif UNITY_ANDROID
		R
#endif
			&& weapon.ManualReload) || (weapon.CanReload && !thrower.IsThrowing))
		{
			weapon.PerformReload();
		}
		if (!weapon.IsAttacking &&
#if UNITY_STANDALONE
		Input.GetKey(KeyCode.Space)
#elif UNITY_ANDROID
		Space
#endif
			)
		{
			equipment.InvokeTP();
		}
	}
}
