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
	public RawImage[] stats;
	public GameObject[] Weapons;
	public GameObject[] Grenades;
	public RawImage fader;
	public GameObject GameSet;
	public GameObject Settings;
	public RawImage screenShake;
	public RawImage particles;
	public RawImage labels;	

	private int weaponIndex;
	private int grenadeIndex;
	private bool active;
	private int screen;

	public GameObject Weapon { get; set; }
	public GameObject Grenade { get; set; }
	public int Difficulty { get; set; }
	public int Stats { get; set; }
	public bool ScreenShake { get; set; }
	public bool Particles { get; set; }
	public bool Labels { get; set; }


	private void Start()
	{
		DontDestroyOnLoad(this);
		weaponIndex = Random.Range(0, weapons.Length);
		grenadeIndex = Random.Range(0, grenades.Length);
		Difficulty = Random.Range(0, difficulties.Length);
		Stats = Random.Range(0, stats.Length);
		ScreenShake = true;
		Particles = true;
		Labels = true;
		active = true;
		Settings.SetActive(false);
	}

	private void Update()
	{
		if (!active) return;
		if (screen == 0)
		{
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

			for (var i = 0; i < stats.Length; i++)
			{
				if (Stats == i) stats[i].color = new Color(1, 1, 1, 0.2f); else stats[i].color = new Color(1, 1, 1, 0.05f);
			}

			if (Input.GetKey(KeyCode.Alpha1)) weaponIndex = 0;
			if (Input.GetKey(KeyCode.Alpha2)) weaponIndex = 1;
			if (Input.GetKey(KeyCode.Alpha3)) weaponIndex = 2;
			if (Input.GetKey(KeyCode.Alpha4)) weaponIndex = 3;
			if (Input.GetKey(KeyCode.Alpha5)) grenadeIndex = 0;
			if (Input.GetKey(KeyCode.Alpha6)) grenadeIndex = 1;
			if (Input.GetKey(KeyCode.Alpha7)) grenadeIndex = 2;
			if (Input.GetKey(KeyCode.Alpha8)) Stats = 0;
			if (Input.GetKey(KeyCode.Alpha9)) Stats = 1;
			if (Input.GetKey(KeyCode.Alpha0)) Stats = 2;
			if (Input.GetKey(KeyCode.E)) Difficulty = 0;
			if (Input.GetKey(KeyCode.M)) Difficulty = 1;
			if (Input.GetKey(KeyCode.H)) Difficulty = 2;

			if (Input.GetKeyDown(KeyCode.Return))
			{
				Weapon = Weapons[weaponIndex];
				Grenade = Grenades[grenadeIndex];
				screen++;
				active = false;
				StartCoroutine(ToSettings());
			}
		} else
		{
			if (ScreenShake) screenShake.color = new Color(1, 1, 1, 0.2f); else screenShake.color = new Color(1, 1, 1, 0.05f);
			if (Particles) particles.color = new Color(1, 1, 1, 0.2f); else particles.color = new Color(1, 1, 1, 0.05f);
			if (Labels) labels.color = new Color(1, 1, 1, 0.2f); else labels.color = new Color(1, 1, 1, 0.05f);

			if (Input.GetKeyDown(KeyCode.Alpha1)) ScreenShake = !ScreenShake;
			if (Input.GetKeyDown(KeyCode.Alpha2)) Particles = !Particles;
			if (Input.GetKeyDown(KeyCode.Alpha3)) Labels = !Labels;

			if (Input.GetKeyDown(KeyCode.Return))
			{
				active = false;
				StartCoroutine(ToGame());
			}
		}

		if (Input.GetKeyDown(KeyCode.Escape))
		{
			active = false;
			StartCoroutine(Quit());
		}
	}

	private IEnumerator ToSettings()
	{
		for (float a = 0; a < 1; a+= 0.01f)
		{
			fader.color = new Color(0, 0, 0, a);
			yield return new WaitForSeconds(0.01f);
		}
		active = true;
		GameSet.SetActive(false);
		Settings.SetActive(true);
		for (float a = 1; a > 0; a -= 0.01f)
		{
			fader.color = new Color(0, 0, 0, a);
			yield return new WaitForSeconds(0.01f);
		}
	}

	private IEnumerator ToGame()
	{
		for (float a = 0; a < 1; a += 0.01f)
		{
			fader.color = new Color(0, 0, 0, a);
			yield return new WaitForSeconds(0.01f);
		}
		SceneManager.LoadScene("GamePlay", new LoadSceneParameters(LoadSceneMode.Single));
	}

	private IEnumerator Quit()
	{
		for (float a = 0; a < 1; a += 0.01f)
		{
			fader.color = new Color(0, 0, 0, a);
			yield return new WaitForSeconds(0.01f);
		}
		#if UNITY_EDITOR
			UnityEditor.EditorApplication.isPlaying = false;
		#else
            Application.Quit();
		#endif
	}
}
