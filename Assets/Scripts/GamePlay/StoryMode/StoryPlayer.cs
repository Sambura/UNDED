using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoryPlayer : Player
{
	public bool ControlsBlock { get; set; }
	public bool TeleportBlock { get; set; }
	public bool ShotBlock { get; set; }
	public bool GrenadeBlock { get; set; }
	public bool ReloadBlock { get; set; }

	public Vector2 targetScale;
	public bool scalePlayer;
	public bool nullParent;
	public float scaleDuration;

	private Interactable target;

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
		}
		else Destroy(gameObject);
	}

	void Start()
	{
		animator = GetComponent<Animator>();
		audioSource = GetComponent<AudioSource>();
		controller = FindObjectOfType<Controller>();
		Hp = maxHealth;
		healthBar = GameObject.FindGameObjectWithTag("HealthBar").GetComponent<HealthBar>();
		UISpawnPoint = GameObject.FindGameObjectWithTag("UIgrip").transform;
		healthBar.Init(Hp / maxHealth);
		StartCoroutine(StartRoutine());
		if (controller.Player == null)
		{
			controller.Player = this;
		}
		InitSpecs();
	}

	public void InitUI()
	{
		InitBullets();
		InitOther();
		InitSpecs();
	}

	private IEnumerator StartRoutine()
	{
		if (nullParent)
			transform.parent = null;
		GetComponent<Rigidbody2D>().isKinematic = false;
		//foreach (var i in GetComponents<BoxCollider2D>()) i.enabled = true;

		GetComponent<SpriteRenderer>().sortingOrder = 10;
		weapon.GetComponent<SpriteRenderer>().sortingOrder = 11;

		Vector2 scale = transform.localScale;
		float startTime = Time.time;
		var camera = GameObject.FindGameObjectWithTag("VirtualCamera");
		camera.GetComponent<Cinemachine.CinemachineVirtualCamera>().Follow = transform;
		while (Time.time - startTime < scaleDuration)
		{
			transform.localScale = Vector2.Lerp(scale, targetScale, (Time.time - startTime) / scaleDuration);
			yield return null;
		}
		transform.localScale = targetScale;
	}

	private void UpdateHealth()
	{
		Hp = Mathf.Clamp(Hp, 0, maxHealth);
		healthBar.UpdateHealth(Hp / maxHealth);
		if (Hp == 0)
		{
			animator.SetBool("isDead", true);
			IsDead = true;
			StartCoroutine(controller.Death());
			weapon.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 1);
			if (thrower != null) thrower.CancelThrow = true;
		}
	}

	private void OnTriggerStay2D(Collider2D collision)
	{
		if (target == null)
		{
			var inter = collision.GetComponent<Interactable>();
			if (inter != null)
			{
				if (!inter.IsLocked)
				{
					target = inter;
					inter.OnInteractionEnter();
				}
			}
		}
	}

	private void OnTriggerExit2D(Collider2D collision)
	{
		if (target != null)
		{
			var inter = collision.GetComponent<Interactable>();
			if (inter != null)
			{
				if (inter == target)
				{
					target = null;
					inter.OnInteractionExit();
				}
			}
		}
	}


	void Update()
	{
		if (IsDead || controller.IsPaused) return;

#if UNITY_STANDALONE || UNITY_EDITOR
		if (!ControlsBlock && !DialogueManager.Instance.dialogueInProcess)
		{
			if (!ShotBlock)
			{
				Fire1 = Input.GetKey(KeyCode.M) || Input.GetKey(KeyCode.Return);
				Fire2 = Input.GetKey(KeyCode.Quote) || Input.GetKey(KeyCode.N);
			} else
			{
				Fire1 = Fire2 = false;
			}
			if (!GrenadeBlock)
				F = Input.GetKey(KeyCode.F);
			else F = false;
			if (!ReloadBlock)
				R = Input.GetKey(KeyCode.R);
			else R = false;
			if (!TeleportBlock)
				Space = Input.GetKey(KeyCode.Space);
			else Space = false;
			movingControls = Input.GetAxisRaw("Horizontal");
		} else
		{
			movingControls = 0;
			Fire1 = Fire2 = F = R = Space = false;
		}
#endif

		if (lastHp > Hp && Settings.particles && damageSystemPlay)
		{
			Instantiate(dmgSystem, transform.position, Quaternion.Euler(0, 0, left ? 0 : 180));
			controller.InstantiateDamageLabel(transform.position + new Vector3(0, 10), Mathf.RoundToInt(lastHp - Hp));
		}

		Hp += regeneration / 60 * Time.deltaTime * regenerationScale;
		UpdateHealth();
		if (IsDead) return;
		lastHp = Hp;

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
			animator.SetFloat("walkSpeed", movementSpeed / 50);
			animator.SetBool("isWalking", true);
		}
		else
		{
			animator.SetBool("isWalking", false);
			Hp += regeneration / 60 * Time.deltaTime * regenerationScale * stillRegenerationBoost;
		}

		if (F && (!weapon.IsReloading || !weapon.ManualReload) && thrower != null)
		{
			thrower.PerformThrow();
		}

		float d = movementSpeed * Time.deltaTime * delta;
		transform.Translate(new Vector3(d, 0));
		/*
		if (Mathf.Abs(transform.position.x) > 175)
		{
			transform.Translate(new Vector3(175 - Mathf.Abs(transform.position.x), 0));
		}
		*/
	}

	private void FixedUpdate()
	{
		if (IsDead || controller.IsPaused) return;
		CriticalHit = Random.value <= criticalHitChance && (criticalHitChance > 0);
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