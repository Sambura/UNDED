using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireGun : Weapon
{
	public GameObject fire;
	public float overheatTime;
	public float coolingTime;
	public float overheatDelay;
	public GameObject heatSymbol;
	public Transform shotPoint;
	public AudioSource audioSource;
	public float loopTime;
	public AudioClip fireSound;
	public SpriteSlider attackHeatSlider;
	public SpriteSlider idleHeatSlider;

	private float coolingStartTime;
	private float heat;
	private SpriteSlider heatIcon;
	private bool isOverheated;
	private FireShot shot;
	private float nextSound;

	public float damageMultiplier;

	private void Start()
	{
		attackHeatSlider.KeepFirst = true;
	}

	private void FixedUpdate()
	{
		if (IsAttacking) heat += Time.fixedDeltaTime / overheatTime; else
			if (Time.time >= coolingStartTime) heat -= Time.fixedDeltaTime / coolingTime;
		heat = Mathf.Clamp01(heat);
		heatIcon.Value = heat;
		if (IsAttacking)
		{
			idleHeatSlider.Value = heat;
			attackHeatSlider.Value = heat;
		}
		else
		{
			attackHeatSlider.Value = heat;
			idleHeatSlider.Value = heat;
		}
		if (isOverheated && heat == 0)
		{
			isOverheated = false;
			heatIcon.KeepFirst = false;
			idleHeatSlider.KeepFirst = false;
		}
		else if (heat == 1 && !isOverheated)
		{
			isOverheated = true;
			coolingStartTime = Time.time + overheatDelay;
			IsAttacking = false;
			heatIcon.KeepFirst = true;
			idleHeatSlider.KeepFirst = true;
		}

		if (!IsAttacking && shot != null)
		{
			shot.gameObject.transform.parent = null;
			shot.Finish();
			shot = null;
		}

		IsAttacking = false;
	}

	public override Vector2 InitUIElements(Vector2 drawPosition, Transform parent)
	{
		if (heatIcon != null) return new Vector2(0, drawPosition.y - 5);

		heatIcon = Instantiate(heatSymbol, parent).GetComponent<SpriteSlider>();
		heatIcon.transform.Translate(drawPosition);

		return new Vector2(0, drawPosition.y - 4);
	}

	public override void PerformAttack(int index)
	{
		if (isOverheated) return;
		IsAttacking = true;
		if (Time.time >= nextSound || !audioSource.isPlaying)
		{
			audioSource.PlayOneShot(fireSound);
			nextSound = Time.time + loopTime;
		}
		if (shot == null)
		{
			shot = Instantiate(fire, shotPoint.position, shotPoint.rotation, transform).GetComponent<FireShot>();
			shot.MultiplyDamage(damageMultiplier);
		}
	}
}
