using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
	public ScrollViewScript weaponSelector;
	public ScrollViewScript grenadeSelector;
	public ScrollViewScript statSelector;
	public ScrollViewScript difficultySelector;
	public GameObject[] screens;
	public GameObject[] Grenades;
	public RawImage fader;
	public string[] weapons;

	[Tooltip("Crossfade's duration")]
	[Range(0f, 10f)]
	[SerializeField] private float tranzitionDuration;

	private int weaponIndex;
	private int grenadeIndex;
	private int screen;
	private int lastScreen;
	private bool fadeOut;

	public string Weapon { get; set; }
	public GameObject Grenade { get; set; }
	public int Difficulty { get; set; }
	public int Stats { get; set; }

	private void Start()
	{
		Time.timeScale = 1;
		DontDestroyOnLoad(this);
		Settings.LoadSettings();
		for (int i = 0; i < screens.Length; i++) screens[i].SetActive(false);
		StartCoroutine(FadeOut());
		SwitchScreen(0);
	}

	public void Exit()
	{
		StartCoroutine(Fader(new System.Action(() => 
		{
			#if UNITY_EDITOR
				UnityEditor.EditorApplication.isPlaying = false;
			#else
				Application.Quit();
			#endif
		})));
	}

	public void StartNewGame()
	{
		StartCoroutine(Fader(new System.Action(() =>
		{
			SwitchScreen(1);
			weaponSelector.GoToPanel(Random.Range(0, weaponSelector.ItemsCount));
			grenadeSelector.GoToPanel(Random.Range(0, grenadeSelector.ItemsCount));
			statSelector.GoToPanel(Random.Range(0, statSelector.ItemsCount));
			difficultySelector.GoToPanel(Random.Range(0, difficultySelector.ItemsCount));
		})));
	}

	public void GoToSettings()
	{
		StartCoroutine(Fader(new System.Action(() =>
		{
			SwitchScreen(2);
		})));
	}

	public void GoToMainMenu()
	{
		StartCoroutine(Fader(new System.Action(() =>
		{
			SwitchScreen(0);
		})));
	}

	public void PauseGame()
	{
		SwitchScreen(3);
	}

	public void StartTheGame()
	{
		var process = SceneManager.LoadSceneAsync(1, LoadSceneMode.Single);
		process.allowSceneActivation = false;
		StartCoroutine(Fader(new System.Action(() =>
		{
			Weapon = weapons[weaponSelector.SelectedIndex];
			Grenade = Grenades[grenadeSelector.SelectedIndex];
			Stats = statSelector.SelectedIndex;
			Difficulty = difficultySelector.SelectedIndex;
			process.allowSceneActivation = true;
			fadeOut = false;
			SwitchScreen(-1);
		})));
	}

	private void SwitchScreen(int toScreen)
	{
		lastScreen = screen;
		screens[screen].SetActive(false);
		if (toScreen == -1) return;
		screen = toScreen;
		screens[screen].SetActive(true);
	}
	
	private IEnumerator Fader(System.Action action)
	{
		float startTime = Time.time;
		fadeOut = true;
		for (float a = 0; a < 1; a = Mathf.Lerp(0, 1, (Time.time - startTime) / tranzitionDuration))
		{
			fader.color = new Color(0, 0, 0, a);
			yield return null;
		}
		action();
		if (fadeOut)
		StartCoroutine(FadeOut());
	}

	private IEnumerator FadeOut()
	{
		float startTime = Time.time;
		for (float a = 1; a > 0; a = Mathf.Lerp(1, 0, (Time.time - startTime) / tranzitionDuration))
		{
			fader.color = new Color(0, 0, 0, a);
			yield return null;
		}
	}
}
