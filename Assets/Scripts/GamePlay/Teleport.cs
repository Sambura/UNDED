using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Teleport : MonoBehaviour
{
	public float tpDistance;
	public float tpChargeTime;
	public int tpAccum;
	public float TpSpeed;
	public GameObject teleportSymbol;
	public AudioClip teleportSound;

	public bool isTping { get; private set; }

	private SpriteRenderer spriteRenderer;
	private Animator animator;
	private AudioSource audioSource;
	private Controller controller;
	private Weapon weapon;
	private Player parent;

	private Animator[] TPIcon;
	private int tpCharged;

	void Start()
	{
		spriteRenderer = GetComponent<SpriteRenderer>();
		animator = GetComponentInParent<Animator>();
		audioSource = GetComponent<AudioSource>();
		controller = FindObjectOfType<Controller>();
		parent = GetComponentInParent<Player>();
		weapon = parent.GetComponentInChildren<Weapon>();
		tpCharged = tpAccum;
		StartCoroutine(Charger());
	}

	public Vector2 InitAccessory(Vector2 drawPosition, Transform parent)
	{
		if (TPIcon != null)
		{
			for (int i = 0; i < TPIcon.Length; i++)
				Destroy(TPIcon[i].gameObject);
		}
		TPIcon = new Animator[tpAccum];
		var corner = Camera.main.ScreenToWorldPoint(Vector3.zero);
		float width = Mathf.Abs((corner.x - Camera.main.transform.position.x) * 2);
		float sY = drawPosition.y - 1;
		float sX = 0;
		int left = tpAccum;
		int index = 0;
		while (left > 0)
		{
			int now = left;
			while (now * 8 + 10 >= width) now--;
			sX = -8 * now / 2f + 1;
			for (int i = 0; i < now; i++)
			{
				TPIcon[index] = Instantiate(teleportSymbol, parent).GetComponent<Animator>();
				TPIcon[index].transform.Translate(new Vector3(sX + i * 8, sY));
				index++;
			}
			sX = -8 * now / 2f + 1 + now * 8;
			sY -= 4;
			left -= now;
		}
		return new Vector2(sX, sY + 4);
	}

	public void InvokeTP() // Fisrt stage
	{
		if (tpCharged == 0) return;
		if (isTping) return;
		if (tpCharged != tpAccum)
		{
			var temp = TPIcon[tpCharged];
			var pos = temp.transform.position;
			TPIcon[tpCharged].transform.position = TPIcon[tpCharged - 1].transform.position;
			TPIcon[tpCharged - 1].transform.position = pos;
			TPIcon[tpCharged] = TPIcon[tpCharged - 1];
			TPIcon[tpCharged - 1] = temp;
			TPIcon[tpCharged].Play("Empty");
		}
		tpCharged--;
		audioSource.PlayOneShot(teleportSound);
		animator.SetFloat("TPspeed", 1 / TpSpeed);
		animator.SetTrigger("TP");
		isTping = true;
		var go = Instantiate(weapon.tpOverlay, weapon.transform);
		spriteRenderer = go.GetComponent<SpriteRenderer>();
		go.GetComponent<Animator>().speed = 1 / TpSpeed;
		spriteRenderer.color = new Color(0, 0, 0, 0);
	}

	public void GetAccessoryAction(int idx)
	{
		switch (idx)
		{
			case 0:
				InvokeWeaponTP();
				break;
			case 1:
				InvokeOutTP();
				break;
			case 2:
				FinishWeaponTP();
				break;
			case 3:
				FinishTP();
				break;
		}
	}

	public void InvokeWeaponTP() // Second stage, called by event in animation
	{
		weapon.GetComponent<SpriteRenderer>().color = new Color(0, 0, 0, 0);
		spriteRenderer.color = new Color(1, 1, 1);
	}

	public void InvokeOutTP() // Third stage, called by end event in animation
	{
		float d = tpDistance;
		if (Mathf.Abs(controller.Player.transform.position.x * controller.Player.transform.right.x + d) >= 175)
		{
			d = 175 - Mathf.Abs(controller.Player.transform.position.x);
		}
		parent.transform.Translate(new Vector2(d, 0));
		spriteRenderer.gameObject.GetComponent<Animator>().Play("TPout");
	}

	public void FinishWeaponTP()
	{
		Destroy(spriteRenderer.gameObject);
		weapon.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1);
	}

	public void FinishTP()
	{
		isTping = false;
	}

	private IEnumerator Charger()
	{
		while (!parent.IsDead)
		{
			yield return new WaitUntil(() => tpCharged < tpAccum);
			TPIcon[tpCharged].speed = 1 / tpChargeTime;
			TPIcon[tpCharged].Play("Tranzit");
			yield return new WaitForSeconds(tpChargeTime);
			tpCharged++;
		}
		TPIcon[Mathf.Clamp(tpCharged, 0, tpAccum - 1)].speed = 0;
	}
}
