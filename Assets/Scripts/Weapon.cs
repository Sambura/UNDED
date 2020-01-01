using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
	public float[] shotRate;
	public float[] gunFireLength;
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
	public float bullets { get; private set; }

	private AudioSource audioSource;
	private SpriteRenderer spriteRenderer;
	public Animator animator { get; set; }
	private bool player;

	private int direction;
	private int bulletIndex;
	private float lastShot;
	private float minIntake;

	private void Start()
	{
		bullets = magazine;
		CanFire = true;
		CanReload = false;
		minIntake = intake[0];
		foreach (var i in intake) minIntake = Mathf.Min(minIntake, i);
		audioSource = GetComponent<AudioSource>();
		spriteRenderer = GetComponent<SpriteRenderer>();
		if (animate)
		animator = GetComponent<Animator>();
		if (GetComponentInParent<Player>() != null) player = true;
	}

	public void SetDirection(int direction)
	{
		this.direction = direction;
		spriteRenderer.flipX = direction == -1;
	}

	public void PerformShot(int index)
	{
		if (index >= bullet.Length) return;
		if (intake[index] > bullets) return;
		bulletIndex = index;
		CanFire = false;
		CanReload = false;
		IsShooting = true;
		IsReloading = false;
		animator.speed = 1;
		StartCoroutine(Shot());
	}

	public void PerformReload()
	{
		animator.Play("Reload");
		animator.speed = 1 / reloadTime;
		StartCoroutine(Reload());
	}

	private IEnumerator Reload()
	{
		CanFire = partialReload;
		CanReload = false;
		IsReloading = true;
		if (!partialReload)
		audioSource.PlayOneShot(reload, 1);
		yield return new WaitForSeconds(reloadTime);
		if (IsReloading)
		{
			if (partialReload)
			{
				audioSource.PlayOneShot(reload, 1);
				bullets = Mathf.Min(magazine, bullets + partialLoad);
			}
			else
			{
				bullets = magazine;
			}
			CanFire = true;
			CanReload = bullets < magazine && partialReload;
			IsReloading = false;
		}
	}

	private IEnumerator Shot()
	{
		for (int i = 0; i < gunFireLength[bulletIndex]; i++)
		{
			animator?.Play("Shot");
			audioSource.PlayOneShot(shot[bulletIndex], 0.7f);
			var b = Instantiate(bullet[bulletIndex], new Vector3(transform.position.x + offset.x * direction, 
				transform.position.y + offset.y), Quaternion.identity).GetComponent<Bullet>();
			b.SetDirection(direction);
			b.PlayerProperty = player;
			bullets -= intake[bulletIndex];
			if (bullets < intake[bulletIndex])
			{
				CanFire = false;
				IsShooting = false;
				lastShot = Time.time;
				yield return new WaitForSeconds(60 / shotRate[bulletIndex]);
				CanReload = bullets < minIntake || partialReload;
				yield break;
			}
			if (gunFireDelay[bulletIndex] != 0)
				yield return new WaitForSeconds(gunFireDelay[bulletIndex]);
		}
		IsShooting = false;
		yield return new WaitForSeconds(60 / shotRate[bulletIndex]);
		CanFire = !IsReloading || partialReload;
		lastShot = Time.time;
		//yield return new WaitForSeconds(0.5f);
		//if (Time.time - lastShot >= 0.5f)
		CanReload = partialReload && !IsShooting && !IsReloading;
	}
}
