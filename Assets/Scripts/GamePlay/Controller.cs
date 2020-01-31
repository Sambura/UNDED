using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Controller : MonoBehaviour
{
	#region Inspector fields
	[Header("Enemies")] [Tooltip("Enemies that will be spawned during game process")]
	[SerializeField] private GameObject[] enemies;
	[SerializeField] private int spawnTypeReductor;
	[Header("UI elements")]
	[Tooltip("Text UI element that have to display kills count")]
	[SerializeField] private Text killsMonitor;
	[Tooltip("Text UI element that have to display spawn percent")]
	[SerializeField] private Text spawnMonitor;
	[Tooltip("Text UI element that have to display enemies count")]
	[SerializeField] private Text enemiesMonitor;
	[Tooltip("Text UI element that have to display current achievement")]
	[SerializeField] private Text achievementMonitor;
	[SerializeField] private GameObject pauseScreen;
	[SerializeField] private GameObject worldCanvas;
	[SerializeField] private GameObject damageLabel;
	[SerializeField] private GameObject deathScreen;
	[SerializeField] private RawImage Fader;
	[Header("Location properties")]
	[Tooltip("Determines absolute x-coordinate, where enemies should be spawned")]
	[SerializeField] private float spawnX;
	[Tooltip("Floor's coordinate, where enemies should be spawned")]
	[SerializeField] private float spawnY;
	[Tooltip("Half of the level's width")]
	[SerializeField] private float levelWidth;
	[Tooltip("Half of the level's height")]
	[SerializeField] private float levelHeight;
	[Header("Level data")]
	[SerializeField] [Range(0, 1)] private float spawnReductor;
	[SerializeField] [Range(0, 1)] private float spawnInc;
	[SerializeField] [Range(0, 1)] private float maxReductor;
	[Header("Other settings")]
	[Tooltip("Determines, how long one achievement should be displayed")]
	[SerializeField] private float achievementDisplayDuration;
	[Tooltip("Determines delay before the death splash display")]
	[SerializeField] private float deathSpalshDelay;
	[SerializeField] private float fadeToMainMenuDuration;
	public AudioClip[] music;
	#endregion

	private bool _isPaused;
	/// <summary>
	/// Is the game currently paused?
	/// </summary>
	public bool IsPaused {
		get => _isPaused;
		set {
			_isPaused = value;
			if (value)
			{
				//Player.CancelThrowing();
				Time.timeScale = 0;
				pauseScreen.SetActive(true);
			} else
			{
				Time.timeScale = 1;
				pauseScreen.SetActive(false);
			}
		}
	}
	/// <summary>
	/// Alive enemies array
	/// </summary>
	public LinkedList<Enemy> Enemies { get; private set; }
	public float LevelWidth { get => levelWidth; }
	public float LevelHeight { get => levelHeight; }
	public Player Player { get; private set; }

	private int kills;
	/// <summary>
	/// Last time any achievement was gotten, used to calculate hide time
	/// </summary>
	private float achievementTime;
	private Vector3 spawnPositive, spawnNegative;
	private MenuController controller;

	private Weapon_gun w;
	private Weapon_knife k;
	private TeleportAcc tp;
	private AudioSource speaker;

	private bool exit;

	public void InstantiateDamageLabel(Vector2 position, float value)
	{
		if (!Settings.damageText) return;
		Instantiate(damageLabel, position, Quaternion.identity, worldCanvas.transform)
			.GetComponent<Label>().SetNumber(Mathf.RoundToInt(value));
	}

	private void Awake()
	{
		exit = true;
		Enemies = new LinkedList<Enemy>();
		spawnPositive = new Vector2(spawnX, spawnY);
		spawnNegative = new Vector2(-spawnX, spawnY);
		controller = FindObjectOfType<MenuController>();
		Player = FindObjectOfType<Player>();
		speaker = GetComponent<AudioSource>();
		tp = Player.equipment;
		w = Player.GetComponentInChildren<Weapon>() as Weapon_gun;
		k = Player.GetComponentInChildren<Weapon>() as Weapon_knife;
		if (controller != null)
		{
			//Player.grenade = controller.Grenade;
			var weapons = Player.GetComponentsInChildren<Weapon>(true);
			foreach (var weapon in weapons)
			{
				if (weapon.name != controller.Weapon)
				{
					Destroy(weapon.gameObject);
				} else
				{
					weapon.gameObject.SetActive(true);
				}
			}
			w = Player.GetComponentInChildren<Weapon>() as Weapon_gun;
			k = Player.GetComponentInChildren<Weapon>() as Weapon_knife;
			switch (controller.Stats)
			{
				case 3:
					Player.regeneration = 10000;
					Player.healthPoints = 100000;
					//Player.grenadeRate = 150;
					tp.tpAccum = 5;
					tp.tpChargeTime = 1;
					tp.TpSpeed = 0.05f;
					tp.tpDistance = 100;
					Player.movementSpeed = 75;
					if (w != null)
					{
						w.dmgMultiplier = 6.66f;
					}
					if (k != null)
					{
						k.damage *= 6.66f;
						k.attackDistance *= 2;
					}
					break;
				case 2:
					Player.regeneration = 500;
					Player.healthPoints = 2500;
					//Player.grenadeRate = 55;
					tp.tpAccum = 3;
					tp.tpChargeTime = 4;
					tp.TpSpeed = 0.1f;
					tp.tpDistance = 70;
					Player.movementSpeed = 50;
					break;
				case 1:
					Player.regeneration = 250;
					Player.healthPoints = 8000;
					//Player.grenadeRate = 8;
					tp.tpAccum = 1;
					tp.tpChargeTime = 5;
					tp.TpSpeed = 0.2f;
					tp.tpDistance = 50;
					Player.movementSpeed = 40;
					if (k != null)
					{
						k.damage *= 2f;
					}
					break;
				case 0:
					Player.regeneration = 750;
					Player.healthPoints = 1000;
					//Player.grenadeRate = 10;
					tp.tpAccum = 5;
					tp.tpChargeTime = 3;
					tp.TpSpeed = 0.08f;
					tp.tpDistance = 90;
					Player.movementSpeed = 60;
					if (w != null)
					{
						w.dmgMultiplier = 1.5f;
					}
					break;
			}
			switch (controller.Difficulty)
			{
				case 0:
					spawnReductor = 0.008f;
					spawnInc = 0.00003f;
					maxReductor = 0.1f;
					break;
				case 1:
					spawnReductor = 0.008f;
					spawnInc = 0.00005f;
					maxReductor = 0.2f;
					break;
				case 2:
					spawnReductor = 0.008f;
					spawnInc = 0.00007f;
					maxReductor = 0.3f;
					break;
				case 3:
					spawnReductor = 0.008f;
					spawnInc = 0.0001f;
					maxReductor = 0.4f;
					break;
				case 4:
					spawnReductor = 0.1f;
					spawnInc = 0.0002f;
					maxReductor = 1f;
					Player.regeneration *= 10;
					break;
			}
			Destroy(controller.gameObject);
		}
		Player.GetWeapon();
		if (spawnReductor == 0) spawnReductor = spawnInc;
		killsMonitor.text = "0 Kills";
		spawnMonitor.text = $"Spawn rate: {System.Math.Round(spawnReductor * 100, 1)}%";
		enemiesMonitor.text = "0 Enemies";
		achievementMonitor.text = "Good luck, soldier!";
		achievementTime = Time.time;
		StartCoroutine(MusicPlayer());
	}

	private void LateUpdate()
	{
		enemiesMonitor.text = $"{Enemies.Count} Enem" + (Enemies.Count == 1 ? "y" : "ies");
		if (Time.time - achievementTime >= achievementDisplayDuration)
		{
			achievementMonitor.text = "";
		}
		bool flag = false;
		if (Input.anyKey && Player.IsDead && !exit)
		{
			exit = true;
			GoToMainMenu();
		}
		if (IsPaused)
		{
			if (Input.GetKeyDown(KeyCode.Escape))
			{
				flag = true;
				IsPaused = false;
			}
		}
		if (!IsPaused && !flag)
		{
			if (Input.GetKeyDown(KeyCode.Escape))
			{
				if (!Player.IsDead)
				{
					IsPaused = true;
				}
			}
		}
	}

	private IEnumerator MusicPlayer()
	{
		while (!Player.IsDead)
		{
			if (!speaker.isPlaying)
			{
				speaker.PlayOneShot(music[Random.Range(0, music.Length)]);
			}
			yield return new WaitForSecondsRealtime(3);
		}
	}

	private void FixedUpdate()
	{
		if (Player.IsDead) return;
		float localReductor = spawnReductor;
		while (Random.value < localReductor)
		{
			var f = Instantiate(enemies[Random.Range(0, spawnTypeReductor)], 
				Random.Range(0, 2) == 0 ? spawnNegative : spawnPositive, Quaternion.identity);
			f.GetComponent<Enemy>().InitThis(this, Player);
			localReductor--;
		}
	}

	public void KillsPlusPlus()
	{
		kills++;
		{
			killsMonitor.text = $"{kills} Kill" + (kills == 1 ? "" : "s");
			spawnReductor = Mathf.Clamp(spawnReductor + spawnInc, 0, maxReductor);
			spawnMonitor.text = $"Spawn rate: {System.Math.Round(spawnReductor * 100, 1)}%";
		}
		bool achieveFlag = false;
		if (kills % 50 == 0)
		{
			if (Player.movementSpeed < 100)
			{
				Player.movementSpeed++;
				achievementMonitor.text = $"Congrats! Your movement speed now = {Player.movementSpeed}";
				achievementTime = Time.time;
				achieveFlag = true;
			}
		}
		if (kills % 75 == 0)
		{
			if (w != null)
			{
				for (int i = 0; i < w.shotRate.Length; i++)
					if (w.shotRate[i] < 2000)
					w.shotRate[i] *= 1.03f;
				w.UpdateDelays();
				achievementMonitor.text = $"Congrats! Your shot rate have been increased";
				achievementTime = Time.time;
				achieveFlag = true;
			}
			if (k != null)
			{
				k.damage++;
				achievementMonitor.text = $"Congrats! Your damage now = {k.damage}";
				achievementTime = Time.time;
				achieveFlag = true;
			}
		}
		if (kills % 100 == 0 && Player.healthPoints < 100000)
		{
			float delta = Player.healthPoints * 1.1f - Player.healthPoints;
			Player.healthPoints += delta;
			Player.GetHealth(delta);
			achievementMonitor.text = $"Congrats! Your maxHealth now = {Mathf.RoundToInt(Player.healthPoints)}";
			achievementTime = Time.time;
			achieveFlag = true;
		}
		if (kills % 125 == 0)
		{
			/*Player.grenadeRate *= 1.1f;
			achievementMonitor.text = $"Congrats! You now can throw {Mathf.RoundToInt(Player.grenadeRate)} grenades a minute!";
			achievementTime = Time.time;
			achieveFlag = true;*/
		}
		if (kills % 250 == 0 && Player.regeneration < 20000)
		{
			Player.regeneration *= 1.25f;
			achievementMonitor.text = $"Congrats! Your regeneration now = {Mathf.RoundToInt(Player.regeneration)}";
			achievementTime = Time.time;
			achieveFlag = true;
		}
		if (kills % 375 == 0)
		{
			tp.tpChargeTime *= 0.92f;
			if (tp.TpSpeed > 0.08f)
				tp.TpSpeed -= 0.01f;
			achievementMonitor.text = $"Congrats! Your teleport now can be charged in {System.Math.Round(tp.tpChargeTime, 1)} seconds!";
			achievementTime = Time.time;
			achieveFlag = true;
		}
		if (kills % 500 == 0)
		{
			if (w != null)
			{
				w.magazine *= 1.1f;
				Player.InitBullets();
				Player.InitOther();
				achievementMonitor.text = $"Congrats! Your magazine capacity now = {Mathf.RoundToInt(w.magazine)}";
				achievementTime = Time.time;
				achieveFlag = true;
			}
			if (k!= null)
			{
				k.attackDistance += 3;
				achievementMonitor.text = $"Congrats! Your attack distance now = {k.attackDistance}";
				achievementTime = Time.time;
				achieveFlag = true;
			}
		}
		if (kills % 625 == 0)
		{
			tp.tpDistance += 2;
			achievementMonitor.text = $"Congrats! Your teleport distance now = {tp.tpDistance}!";
			achievementTime = Time.time;
			achieveFlag = true;
		}
		if (kills % 750 == 0)
		{
			if (w != null)
			{
				if (w.reloadTime > 0.5f || w.partialReload)
				{
					w.reloadTime *= 0.9f;
					achievementMonitor.text = $"Congrats! Your reload time now = {System.Math.Round(w.reloadTime, 1)} seconds";
					achievementTime = Time.time;
					achieveFlag = true;
				}
			}
			if (k != null)
			{
				k.attackRate *= 1.1f;
				k.attackTime *= 0.95f;
				k.UpdateDelays();
				achievementMonitor.text = $"Congrats! Your attack rate now = {Mathf.RoundToInt(k.attackRate)}";
				achievementTime = Time.time;
				achieveFlag = true;
			}
		}
		if (kills % 1000 == 0)
		{
			if (Camera.main.orthographicSize < 100)
			{
				Camera.main.orthographicSize += 4;
				Player.InitBullets();
				Player.InitOther();
				achievementMonitor.text = "Congrats! You've increased your filed of view!";
				achievementTime = Time.time;
				achieveFlag = true;
			}
		}
		if (kills % 1500 == 0)
		{
			tp.tpAccum++;
			Player.InitOther();
			achievementMonitor.text = $"Congrats! Your teleport now have {tp.tpAccum} bars of energy!";
			achievementTime = Time.time;
			achieveFlag = true;
		}
		if (kills % 2000 == 0)
		{
			if (w != null)
			{
				w.dmgMultiplier *= 1.1f;
				achievementMonitor.text = $"Congrats! Your weapon became stronger!";
				achievementTime = Time.time;
				achieveFlag = true;
			}
		}
		if (achieveFlag) achievementMonitor.text += $" ({kills} kills)";
	}

	public void GoToMainMenu()
	{
		StartCoroutine(MainMenuOpen());
	}

	private IEnumerator MainMenuOpen()
	{
		var open = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("StartMenu",
						UnityEngine.SceneManagement.LoadSceneMode.Single);
		open.allowSceneActivation = false;
		float startTime = Time.unscaledTime;
		Color color = new Color(0, 0, 0, 0);
		for (float a = 0; a < 1; a = Mathf.Lerp(0, 1, (Time.unscaledTime - startTime) / fadeToMainMenuDuration))
		{
			color.a = a;
			Fader.color = color;
			yield return null;
		}
		open.allowSceneActivation = true;
	}

	public IEnumerator Death()
	{
		speaker.Stop();
		yield return new WaitForSecondsRealtime(deathSpalshDelay);
		var text1 = deathScreen.GetComponentInChildren<Text>();
		text1.text = $"{kills} Kill" + (kills == 1 ? "" : "s");
		deathScreen.SetActive(true);
		exit = false;
	}
}
