using System.Collections;
using UnityEngine;

public class Teleport : MonoBehaviour
{
	public float maxDistance = 75;
	public float chargeTime = 4;
	public int capacity = 3;
	public float teleportationDuration = 0.04f;
	public float leftOptional = float.NegativeInfinity;
	public float rightOptional = float.PositiveInfinity;

	[SerializeField] private GameObject teleportSymbol;
	[SerializeField] private AudioClip teleportSound;


	public bool IsTeleporting { get; private set; }

	private SpriteRenderer spriteRenderer;
	private Animator animator;
	private AudioSource audioSource;
	private Controller controller;
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
		tpCharged = capacity;
		StartCoroutine(Charger());
	}

	public Vector2 InitAccessory(Vector2 drawPosition, Transform parent)
	{
		if (TPIcon != null)
		{
			for (int i = 0; i < TPIcon.Length; i++)
				Destroy(TPIcon[i].gameObject);
		}
		TPIcon = new Animator[capacity];
		var corner = Camera.main.ScreenToWorldPoint(Vector3.zero);
		float width = Mathf.Abs((corner.x - Camera.main.transform.position.x) * 2);
		float sY = drawPosition.y - 1;
		float sX = 0;
		int left = capacity;
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
		if (IsTeleporting) return;
		if (tpCharged != capacity)
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
		animator.SetFloat("TPspeed", 0.5f / teleportationDuration);
		animator.SetTrigger("TP");
		IsTeleporting = true;
		var go = Instantiate(parent.weapon.tpOverlay, parent.weapon.transform);
		spriteRenderer = go.GetComponent<SpriteRenderer>();
		go.GetComponent<Animator>().speed = 0.5f / teleportationDuration;
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
		parent.weapon.GetComponent<SpriteRenderer>().color = new Color(0, 0, 0, 0);
		spriteRenderer.color = new Color(1, 1, 1);
	}

	public void InvokeOutTP() // Third stage, called by end event in animation
	{
		float d = maxDistance;
		if (leftOptional == float.NegativeInfinity)
		{
			if (Mathf.Abs(parent.transform.position.x * parent.transform.right.x + d) >= controller.LevelWidth)
			{
				d = controller.LevelWidth - Mathf.Abs(parent.transform.position.x);
			}
		} else
		{
			if (parent.transform.position.x + parent.transform.right.x * d < leftOptional)
			{
				d = Mathf.Abs(leftOptional - parent.transform.position.x);
			}
			if (parent.transform.position.x + parent.transform.right.x * d > rightOptional)
			{
				d = Mathf.Abs(rightOptional - parent.transform.position.x);
			}
		}
		parent.transform.Translate(new Vector2(d, 0));
		spriteRenderer.gameObject.GetComponent<Animator>().Play("TPout");
	}

	public void FinishWeaponTP()
	{
		Destroy(spriteRenderer.gameObject);
		parent.weapon.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1);
	}

	public void FinishTP()
	{
		IsTeleporting = false;
	}

	private IEnumerator Charger()
	{
		while (!parent.IsDead)
		{
			yield return new WaitUntil(() => tpCharged < capacity);
			TPIcon[tpCharged].speed = 1 / chargeTime;
			TPIcon[tpCharged].Play("Tranzit");
			yield return new WaitForSeconds(chargeTime);
			tpCharged++;
		}
		TPIcon[Mathf.Clamp(tpCharged, 0, capacity - 1)].speed = 0;
	}
}
