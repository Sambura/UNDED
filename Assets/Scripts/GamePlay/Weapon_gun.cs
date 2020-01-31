using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon_gun : Weapon
{
	public Transform shotPoint;
	public float[] shotRate;
	public int[] gunFireLength;
	public float[] gunFireDelay;
	public float[] intake;
	public GameObject[] bullet;
	public AudioClip[] shot;
	public float magazine;
	public float reloadTime;
	public bool partialReload;
	public float partialLoad;
	public GameObject fakeBullet;
	public AudioClip reload;
	public float dmgMultiplier = 1;

	private AudioSource audioSource;
	private SpriteRenderer spriteRenderer;
	private Animator animator;
	private Controller controller;
	private float[] fireDelay;

	private int direction;
	private int bulletIndex;
	private float nextShot;
	private float minIntake;
	private float shotStartTime;
	private int fireLeft;
	private float reloadStartTime;
	private SpriteRenderer[] bullets;

	private void Awake()
	{
		Load = magazine;
		CanAttack = true;
		CanReload = false;
		ManualReload = !partialReload;
		minIntake = intake[0];
		foreach (var i in intake) minIntake = Mathf.Min(minIntake, i);
		audioSource = GetComponent<AudioSource>();
		spriteRenderer = GetComponent<SpriteRenderer>();
		animator = GetComponent<Animator>();
		controller = FindObjectOfType<Controller>();
		UpdateDelays();
	}

	public void UpdateDelays()
	{
		fireDelay = new float[shotRate.Length];
		for (int i = 0; i < fireDelay.Length; i++)
			fireDelay[i] = 60 / shotRate[i];
	}

	public override Vector2 InitBullets(Vector2 drawPosition, Transform parent)
	{
		if (bullets != null)
		{
			for (int i = 0; i < bullets.Length; i++)
				Destroy(bullets[i].gameObject);
		}
		int total = (int)magazine;
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
			bullets[j].color = new Color(1, 1, 1, j < (int)Load ? 1 : 0.4f);
	}

	public override void SetDirection(int direction)
	{
		this.direction = direction;
		//spriteRenderer.flipX = direction == -1;
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
					Load = Mathf.Min(magazine, Load + partialLoad);
				}
				else
				{
					Load = magazine;
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
		if (index >= bullet.Length) return; // If there is no such bullets - exit
		if (intake[index] > Load) return; // If bullet intake is higher than left in magazine - exit
		if (IsReloading && !partialReload) return;
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
		if (gunFireDelay[bulletIndex] != 0)
			animator.speed = 1 / gunFireDelay[bulletIndex];
		animator.SetTrigger("Shot");
		audioSource.PlayOneShot(shot[bulletIndex]);
		var b = Instantiate(bullet[bulletIndex], shotPoint.position, Quaternion.identity).GetComponent<Bullet>();
		b.SetDirection((int)transform.right.x);
		b.MultiplyDamage(dmgMultiplier);
		Load -= intake[bulletIndex];
		fireLeft--;
		nextShot = Time.time + gunFireDelay[bulletIndex];
		UpdateBullets();
	}

	public override void PerformReload()
	{
		if (IsReloading) return;
		if (IsAttacking) return;
		if (Load == magazine) return;
		animator.speed = 1 / reloadTime;
		animator.SetTrigger("Reload");
		if (!partialReload)
			audioSource.PlayOneShot(reload);
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
