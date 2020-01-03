using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
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
	public Vector2 offset;
	public AudioClip reload;
	public bool animate;

	public bool CanFire { get; private set; }
	public bool CanReload { get; private set; }
	public bool IsShooting { get; private set; }
	public bool IsReloading { get; private set; }
	public bool IsWaiting { get; private set; }
	public float load { get; private set; }
	public float bulletsY { get; private set; }

	private AudioSource audioSource;
	private SpriteRenderer spriteRenderer;
	public Animator animator { get; set; }
	private bool player;
	private float[] fireDelay;

	private int direction;
	private int bulletIndex;
	private float lastShot;
	private float minIntake;
	private float shotStartTime;
	private int fireLeft;
	private float reloadStartTime;

	private SpriteRenderer[] bullets;

	private void Start()
	{
		load = magazine;
		CanFire = true;
		CanReload = false;
		minIntake = intake[0];
		foreach (var i in intake) minIntake = Mathf.Min(minIntake, i);
		audioSource = GetComponent<AudioSource>();
		spriteRenderer = GetComponent<SpriteRenderer>();
		if (animate)
			animator = GetComponent<Animator>();
		if (GetComponentInParent<Player>() != null) player = true;
		fireDelay = new float[shotRate.Length];
		for (int i = 0; i < fireDelay.Length; i++) 
			fireDelay[i] = 60 / shotRate[i];
	}

	public void InitBullets()
	{
		if (bullets != null)
		{
			for (int i = 0; i < bullets.Length; i++)
				Destroy(bullets[i]);
		}
		int total = (int)magazine;
		bullets = new SpriteRenderer[total];
		var corner = Camera.main.ScreenToWorldPoint(Vector3.zero);
		float width = Mathf.Abs((corner.x - Camera.main.transform.position.x) * 2);
		float sY = -23;
		int left = total;
		int index = 0;
		while (left > 0)
		{
			int now = left;
			while (now * 3 + 5 >= width) now--;
			float sX = -3 * now / 2f + 1 + Camera.main.transform.position.x
				;
			for (int i = 0; i < now; i++)
			{
				bullets[index] = Instantiate(fakeBullet, new Vector3(sX + i * 3, sY), 
					Quaternion.identity, GetComponentsInParent<Transform>()[1]).GetComponent<SpriteRenderer>();
				index++;
			}
			sY -= 4;
			left -= now;
		}
		bulletsY = sY;
		UpdateBullets();
	}

	private void UpdateBullets()
	{
		if (!player) return;
		for (int j = 0; j < bullets.Length; j++)
			bullets[j].color = new Color(1, 1, 1, j < (int)load ? 1 : 0.4f);
	}

	public void SetDirection(int direction)
	{
		this.direction = direction;
		spriteRenderer.flipX = direction == -1;
	}


	private void Update()
	{

		if (IsShooting)
		{
			if (intake[bulletIndex] > load || fireLeft == 0) // Breaking shot
			{
				IsShooting = false;
			}
			else
			if (Time.time - lastShot >= gunFireDelay[bulletIndex])
			{
				Shot();
			}
		}
		else if (IsReloading)
		{
			if (Time.time - reloadStartTime >= reloadTime)
			{
				if (partialReload)
				{
					audioSource.PlayOneShot(reload, 1);
					load = Mathf.Min(magazine, load + partialLoad);
				}
				else
				{
					load = magazine;
				}
				UpdateBullets();
				IsReloading = false;
			}
		}
		if (!IsShooting && (!IsReloading || partialReload))
		{
			if (Time.time - shotStartTime >= fireDelay[bulletIndex])
			{
				if (load >= minIntake) CanFire = true;
			}
			CanReload = (partialReload || load < minIntake) && !IsReloading;
		}
	}

	public void PerformShot(int index)
	{
		if (IsShooting) return;
		if (!CanFire) return; // If can't perform shot - exit
		if (index >= bullet.Length) return; // If there is no such bullets - exit
		if (intake[index] > load) return; // If bullet intake is higher than left in magazine - exit
		if (IsReloading && !partialReload) return;
		bulletIndex = index; // Updating index
		CanFire = false; // Weapon can't fire during shoting
		CanReload = false; // Weapon can't be reloaded during shoting
		IsShooting = true; // Weapon is shooting now
		IsReloading = false; // Weapon is not reloading now
		animator.speed = 1; // Reset animator speed
		shotStartTime = Time.time; // When this shot started
		fireLeft = gunFireLength[index]; // Bullets to spend
	}

	private void Shot()
	{
		animator.Play("Shot");
		audioSource.PlayOneShot(shot[bulletIndex], 1);
		var b = Instantiate(bullet[bulletIndex], new Vector3(transform.position.x + offset.x * direction,
			transform.position.y + offset.y), Quaternion.identity).GetComponent<Bullet>();
		b.SetDirection(direction);
		b.PlayerProperty = player;
		load -= intake[bulletIndex];
		fireLeft--;
		lastShot = Time.time;
		UpdateBullets();
	}

	public void PerformReload()
	{
		if (IsReloading) return;
		if (IsShooting) return;
		if (load == magazine) return;
		animator.speed = 1 / reloadTime;
		animator.Play("Reload");
		if (!partialReload)
			audioSource.PlayOneShot(reload, 1);
		reloadStartTime = Time.time;
		CanReload = false;
		IsReloading = true;
	}
}
