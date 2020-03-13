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
	[Header("Debug scrolls")]
	public ScrollViewScript weapon;
	public ScrollViewScript grenade;
	public ScrollViewScript player;
	public ScrollViewScript thrower;
	public ScrollViewScript teleport;
	public ScrollViewScript shield;

	public GameObject[] screens;
	public GameObject[] Grenades;
	public RawImage fader;
	public string[] weapons;
	public GameObject loadingScreen;

	[Tooltip("Crossfade's duration")]
	[Range(0f, 10f)]
	[SerializeField] private float tranzitionDuration;

	private int weaponIndex;
	private int grenadeIndex;
	private int screen;
	private int lastScreen;
	private bool fadeOut;
	private bool fading;

	public Controller.PlayerData playerData;
	public string Weapon { get; set; }
	public GameObject Grenade { get; set; }
	public int Difficulty { get; set; }
	public int Stats { get; set; }
	public bool Loaded { get; set; }

	private void Start()
	{
		Screen.orientation = ScreenOrientation.Landscape;
		Screen.autorotateToPortraitUpsideDown = false;
		Screen.autorotateToPortrait = false;
		Screen.autorotateToLandscapeLeft = true;
		Screen.autorotateToLandscapeRight = true;
		Time.timeScale = 1;
		DontDestroyOnLoad(this);
		Settings.LoadSettings();
		for (int i = 0; i < screens.Length; i++) screens[i].SetActive(false);
		StartCoroutine(FadeOut());
		SwitchScreen(0);
		{
			var path = Application.persistentDataPath + "/data.usf";
			var file = new System.IO.FileInfo(path);
			if (!file.Exists)
			{
				ResetHighScore();
			}
		}
	}

	public void ResetHighScore()
	{
		var path = Application.persistentDataPath + "/data.usf";
		var stream = new System.IO.FileStream(path, System.IO.FileMode.Create);
		var writer = new System.IO.BinaryWriter(stream);
		for (int d = 0; d < 5; d++)
		{
			writer.Write(0);
		}
		stream.Close();
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

	public void GoToScreen(int screen)
	{
		StartCoroutine(Fader(new System.Action(() =>
		{
			SwitchScreen(screen);
		})));
	}

	public void StartTheGame(int source)
	{
		StartCoroutine(Fader(new System.Action(() =>
		{
			StartCoroutine(LevelLoader(source));
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
		if (fading) yield break;
		fading = true;
		float startTime = Time.time;
		fadeOut = true;
		for (float a = 0; a < 1; a = Mathf.Lerp(0, 1, (Time.time - startTime) / tranzitionDuration))
		{
			fader.color = new Color(0, 0, 0, a);
			yield return null;
		}
		action();
		fading = false;
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

	private IEnumerator LevelLoader(int source)
	{
		loadingScreen.SetActive(true);
		yield return null;
		if (source == 0)
		{
			playerData.weaponName = weaponSelector.SelectedItem.legacyName;
			playerData.throwerName = "Default thrower";
			playerData.teleportName = "Default teleport";
			playerData.grenadeName = grenadeSelector.SelectedItem.legacyName;
			playerData.playerName = statSelector.SelectedItem.legacyName;
			playerData.shieldName = "Default shield";

			Difficulty = difficultySelector.SelectedIndex;
		}
		else if (source == 1)
		{
			playerData.grenadeName = grenade.SelectedItem.legacyName;
			playerData.playerName = player.SelectedItem.legacyName;
			if (shield.SelectedIndex != 0)
				playerData.shieldName = shield.SelectedItem.legacyName;
			if (teleport.SelectedIndex != 0)
				playerData.teleportName = teleport.SelectedItem.legacyName;
			if (thrower.SelectedIndex != 0)
				playerData.throwerName = thrower.SelectedItem.legacyName;
			playerData.weaponName = weapon.SelectedItem.legacyName;
			Difficulty = 0;
		}
		yield return null;
		var process = SceneManager.LoadSceneAsync(1, LoadSceneMode.Single);
		process.allowSceneActivation = false;
		fadeOut = false;
		SwitchScreen(-1);
		yield return new WaitWhile(() => process.progress < 0.9f);
		loadingScreen.GetComponent<Animator>().SetTrigger("fadeOut");
		yield return new WaitForSeconds(0.5f);
		process.allowSceneActivation = true;
	}
}
