using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : Weapon
{
	public float[] shotRate;
	public int[] gunFireLength;
	public float[] gunFireShotRate;
	public float[] intake;
	public string[] bulletName;
	public float magazineCapacity;
	public float reloadTime;
	public bool partialReload;
	public float partialLoad;

	[SerializeField] private Transform shotPoint;
	[SerializeField] private GameObject fakeBullet;
	[SerializeField] private AudioClip reload;

	private AudioSource audioSource;
	private Animator animator;
	private float[] fireDelay;
	[SerializeField] private GameObject[] bullet;

	private int bulletIndex;
	private float nextShot;
	private float minIntake;
	private float shotStartTime;
	private int fireLeft;
	private float reloadStartTime;
	private SpriteRenderer[] bullets;

	private void Start()
	{
		player = Player.Instance;
		Load = magazineCapacity;
		CanAttack = true;
		CanReload = false;
		ManualReload = !partialReload;
		minIntake = intake[0];
		foreach (var i in intake) minIntake = Mathf.Min(minIntake, i);
		audioSource = GetComponent<AudioSource>();
		animator = GetComponent<Animator>();
		UpdateDelays();
		/*
		bullet = new GameObject[bulletName.Length];
		for (var i = 0; i < bullet.Length; i++)
		{
			bullet[i] = PresetsManager.Instance.InstantiatePrefab("bullet", bulletName[i]);
		}*/
		if (bullets != null) UpdateBullets();
	}

	protected override void ToggleVisibility()
	{
		if (bullets == null) return;
		foreach (var i in bullets)
		{
			i.enabled = UIVisible;
		}
	}

	public void UpdateDelays()
	{
		fireDelay = new float[shotRate.Length];
		for (int i = 0; i < fireDelay.Length; i++)
			fireDelay[i] = 60 / shotRate[i];
	}

	public override Vector2 InitUIElements(Vector2 drawPosition, Transform parent)
	{
		if (bullets != null)
		{
			for (int i = 0; i < bullets.Length; i++)
				Destroy(bullets[i].gameObject);
		}
		int total = (int)magazineCapacity;
		bullets = new SpriteRenderer[total];
		var corner = Camera.main.ScreenToWorldPoint(Vector3.zero);
		float width = Mathf.Abs((corner.x - Camera.main.transform.position.x) * 2);
		float sY = drawPosition.y;
		int left = total;
		int index = 0;
		while (left > 0)
		{
			int now = left;
			while (now * 3 + 5 >= width) now--;
			float sX = -3 * now / 2f + 1;// + Camera.main.transform.position.x;
			for (int i = 0; i < now; i++)
			{
				bullets[index] = Instantiate(fakeBullet, parent).GetComponent<SpriteRenderer>();
				bullets[index].transform.Translate(new Vector3(sX + i * 3, sY));
				bullets[index].enabled = UIVisible;
				index++;
			}
			sY -= 4;
			left -= now;
		}
		UpdateBullets();
		return new Vector2(0, sY);
	}

	private void UpdateBullets()
	{
		for (int j = 0; j < bullets.Length; j++)
			bullets[j].color = new Color(1, 1, 1, j < (int)Load ? 1 : 0.2f);
	}

	private void FixedUpdate()
	{
		if (IsAttacking)
		{
			if (intake[bulletIndex] > Load || fireLeft == 0) // Breaking shot
			{
				IsAttacking = false;
			}
			else
			if (Time.time >= nextShot)
			{
				Attack();
			}
		}
		else if (IsReloading)
		{
			if (Time.time - reloadStartTime >= reloadTime)
			{
				if (partialReload)
				{
					audioSource.PlayOneShot(reload);
					Load = Mathf.Min(magazineCapacity, Load + partialLoad);
				}
				else
				{
					Load = magazineCapacity;
				}
				UpdateBullets();
				IsReloading = false;
			}
		}
		if (!IsAttacking && (!IsReloading || partialReload))
		{
			if (Time.time - shotStartTime >= fireDelay[bulletIndex])
			{
				if (Load >= minIntake) CanAttack = true;
			}
			CanReload = (partialReload || Load < minIntake) && !IsReloading;
		}
	}

	public override void PerformAttack(int index)
	{
		if (IsAttacking) return;
		if (!CanAttack) return; // If can't perform shot - exit
		if (index >= bullet.Length) index = 0; // If there is no such bullets = 0
		if (intake[index] > Load) return; // If bullet intake is higher than left in magazine - exit
		if (IsReloading && !partialReload) return;
		if (bullets == null) return;
		bulletIndex = index; // Updating index
		CanAttack = false; // Weapon can't fire during shoting
		CanReload = false; // Weapon can't be reloaded during shoting
		IsAttacking = true; // Weapon is shooting now
		IsReloading = false; // Weapon is not reloading now
		shotStartTime = Time.time; // When this shot started
		fireLeft = gunFireLength[index]; // Bullets to spend
	}

	private void Attack()
	{
		animator.speed = 1 / fireDelay[bulletIndex];
		if (gunFireShotRate[bulletIndex] != 0)
			animator.speed = gunFireShotRate[bulletIndex] / 60;
		animator.SetTrigger("Shot");
		var b = Instantiate(bullet[bulletIndex], shotPoint.position, Quaternion.identity).GetComponent<Bullet>();
		b.gameObject.SetActive(true);
		b.SetDirection((int)transform.right.x);
		b.MultiplyDamage(damageMultiplier * (player.CriticalHit ? player.criticalHitMultiplier : 1));
		audioSource.pitch = 1 + Random.Range(-b.pitchDelta, b.pitchDelta);
		audioSource.PlayOneShot(b.shotSound);
		Load -= intake[bulletIndex];
		fireLeft--;
		if (gunFireShotRate[bulletIndex] != 0)
			nextShot = Time.time + 60 / gunFireShotRate[bulletIndex];
		else nextShot = Time.time;
		UpdateBullets();
	}

	public override void PerformReload()
	{
		if (IsReloading) return;
		if (IsAttacking) return;
		if (Load == magazineCapacity) return;
		animator.speed = 1 / reloadTime;
		animator.SetTrigger("Reload");
		if (!partialReload)
		{
			audioSource.pitch = reload.length / reloadTime;
			audioSource.PlayOneShot(reload);
		}
		reloadStartTime = Time.time;
		CanReload = false;
		IsReloading = true;
	}

	public override void CancelReload()
	{
		IsReloading = false;
		animator.Play("Idle");
	}
}
