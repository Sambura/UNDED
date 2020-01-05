using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
	public RawImage[] weapons;
	public RawImage[] grenades;
	public RawImage[] difficulties;
	public GameObject[] Weapons;
	public GameObject[] Grenades;

	private int weaponIndex;
	private int grenadeIndex;
	private bool active;

	public GameObject Weapon { get; set; }
	public GameObject Grenade { get; set; }
	public int Difficulty { get; set; }

	private void Start()
	{
		DontDestroyOnLoad(this);
		weaponIndex = Random.Range(0, weapons.Length);
		grenadeIndex = Random.Range(0, grenades.Length);
		Difficulty = Random.Range(0, difficulties.Length);
		active = true;
	}

	private void Update()
	{
		if (!active) return;
		for (var i = 0; i < weapons.Length; i++)
		{
			if (weaponIndex == i) weapons[i].color = new Color(1, 1, 1, 0.2f); else weapons[i].color = new Color(1, 1, 1, 0.05f);
		}

		for (var i = 0; i < grenades.Length; i++)
		{
			if (grenadeIndex == i) grenades[i].color = new Color(1, 1, 1, 0.2f); else grenades[i].color = new Color(1, 1, 1, 0.05f);
		}

		for (var i = 0; i < difficulties.Length; i++)
		{
			if (Difficulty == i) difficulties[i].color = new Color(1, 1, 1, 0.2f); else difficulties[i].color = new Color(1, 1, 1, 0.05f);
		}

		if (Input.GetKey(KeyCode.Alpha1)) weaponIndex = 0;
		if (Input.GetKey(KeyCode.Alpha2)) weaponIndex = 1;
		if (Input.GetKey(KeyCode.Alpha3)) weaponIndex = 2;
		if (Input.GetKey(KeyCode.Alpha4)) weaponIndex = 3;
		if (Input.GetKey(KeyCode.Alpha5)) grenadeIndex = 0;
		if (Input.GetKey(KeyCode.Alpha6)) grenadeIndex = 1;
		if (Input.GetKey(KeyCode.Alpha7)) grenadeIndex = 2;
		if (Input.GetKey(KeyCode.Alpha8)) Difficulty = 0;
		if (Input.GetKey(KeyCode.Alpha9)) Difficulty = 1;
		if (Input.GetKey(KeyCode.Alpha0)) Difficulty = 2;

		if (Input.GetKeyDown(KeyCode.Return))
		{
			Weapon = Weapons[weaponIndex];
			Grenade = Grenades[grenadeIndex];
			active = false;
			SceneManager.LoadScene("GamePlay", new LoadSceneParameters(LoadSceneMode.Single));
		}
	}
}
