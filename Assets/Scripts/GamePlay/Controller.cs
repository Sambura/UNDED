using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Controller : MonoBehaviour
{
	#region Inspector fields
#pragma warning disable IDE0044
	[Header("Enemies")] [Tooltip("Enemies that will be spawned during game process")]
	[SerializeField] private GameObject[] enemies;
	[SerializeField] private int spawnTypeReductor;
	[Header("UI elements")]
	[Tooltip("Text UI element that have to display kills count")]
	[SerializeField] private Text killsMonitor;
	[SerializeField] private Text scoreMonitor;
	[SerializeField] private Text highScoreMonitor;
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
	[SerializeField] private GameObject highScoreObject;
	[SerializeField] private RawImage Fader;
	[SerializeField] private GameObject touchControls;
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
	public UnityEngine.Experimental.Rendering.Universal.Light2D globalLight;
	public AudioClip[] music;
	public GameObject arrows;
	public GameObject joystick;
#pragma warning restore IDE0044
	#endregion

	#region Singleton
	public static Controller Instance;
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
				Settings.sfxMixer.SetFloat("Pitch", 0);
				pauseScreen.SetActive(true);
			} else
			{
				Time.timeScale = 1;
				Settings.sfxMixer.SetFloat("Pitch", 1);
				pauseScreen.SetActive(false);
			}
		}
	}
	/// <summary>
	/// Alive enemies array
	/// </summary>
	[HideInInspector]
	public int Enemies;
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
	private Teleport tp;
	private Thrower th;
	private AudioSource speaker;

	private bool exit;
	private float score;
	private int difficulty;
	private int[] highScore;
	private bool enableHighScore;

	private LevelController levelController;

	public void InstantiateDamageLabel(Vector2 position, float value)
	{
		if (!Settings.damageText) return;
		Instantiate(damageLabel, position, Quaternion.identity, worldCanvas.transform)
			.GetComponent<Label>().SetNumber(Mathf.RoundToInt(value));
	}

	private void Awake()
	{
		if (Instance == null) Instance = this;
		exit = true;
		spawnPositive = new Vector2(spawnX, spawnY);
		spawnNegative = new Vector2(-spawnX, spawnY);
		controller = FindObjectOfType<MenuController>();
		Player = FindObjectOfType<Player>();
		speaker = GetComponent<AudioSource>();
		tp = Player.equipment;
		w = Player.GetComponentInChildren<Weapon>() as Weapon_gun;
		k = Player.GetComponentInChildren<Weapon>() as Weapon_knife;
		th = Player.GetComponentInChildren<Thrower>();
		Player.InitSpecs();
		difficulty = -1;
		if (controller != null)
		{
			th.grenade = controller.Grenade;
			difficulty = controller.Difficulty;
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
					enableHighScore = false;
					Player.regeneration = 500;
					Player.healthPoints = 2500;
					th.grenadeRate = 1500;
					tp.tpAccum = 3;
					tp.tpChargeTime = 0.1f;
					tp.TpSpeed = 0.05f;
					tp.tpDistance = 120;
					Player.movementSpeed = 75;
					Camera.main.orthographicSize = 40;
					if (w != null)
					{
						w.dmgMultiplier = 6.66f;
					}
					if (k != null)
					{
						k.damage *= 6.66f;
						//k.attackDistance *= 2;
					}
					foreach (var i in System.Enum.GetValues(typeof(DamageType)))
					{
						Player.dmgSpec[(DamageType)i] = 0;
					}
					break;
				case 2:
					enableHighScore = true;
					Player.regeneration = 500;
					Player.healthPoints = 2500;
					Player.dmgLockScale = 0;
					Player.dmgLockTime = 5;
					Player.unlockSpeed = 2;
					Player.stillRegenScaleInc = 0.03f;
					th.grenadeRate = 55;
					tp.tpAccum = 3;
					tp.tpChargeTime = 4;
					tp.TpSpeed = 0.1f;
					tp.tpDistance = 70;
					Player.movementSpeed = 50;
					Player.dmgSpec[DamageType.Explosion] = 0;
					break;
				case 1:
					enableHighScore = true;
					Player.regeneration = 300;
					Player.healthPoints = 8000;
					Player.dmgLockScale = 0.85f;
					Player.dmgLockTime = 2;
					Player.unlockSpeed = 1;
					Player.stillRegenScaleInc = 0.03f;
					th.grenadeRate = 8;
					tp.tpAccum = 1;
					tp.tpChargeTime = 5;
					tp.TpSpeed = 0.17f;
					tp.tpDistance = 50;
					Player.movementSpeed = 45;
					if (k != null)
					{
						k.damage *= 2f;
					}
					foreach (var i in System.Enum.GetValues(typeof(DamageType)))
					{
						Player.dmgSpec[(DamageType)i] = 0.85f;
					}
					break;
				case 0:
					enableHighScore = true;
					Player.regeneration = 750;
					Player.healthPoints = 1000;
					Player.dmgLockScale = 0;
					Player.dmgLockTime = 3;
					Player.unlockSpeed = 1.5f;
					Player.stillRegenScaleInc = 0.1f;
					th.grenadeRate = 10;
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
		scoreMonitor.text = $"Score: 0";
		spawnMonitor.text = $"Spawn rate: {System.Math.Round(spawnReductor * 100, 1)}%";
		enemiesMonitor.text = "0 Enemies";
		achievementMonitor.text = "Good luck, soldier!";
		achievementTime = Time.time;
		globalLight.intensity = Settings.brightness;
		StartCoroutine(MusicPlayer());
		highScoreMonitor.text = "Highscore: — ";
		var diffCount = 5;

		/*
		LevelData newLevel = new LevelData();
		newLevel.levelName = "Second level";
		newLevel.upgradePoints = 2;
		newLevel.waves = new WaveData[]
		{
			new WaveData()
			{
				previousWaveCounter = 1,
				previousWaveTimer = 5f,
				spawnTime = 45f,
				enemies = new EnemyData[]
				{
					new EnemyData()
					{
						enemyIndex = 0,
						minSpawnedCount = 4,
						maxSpawnedCount = 6
					},
					new EnemyData()
					{
						enemyIndex = 1,
						minSpawnedCount = 3,
						maxSpawnedCount = 4
					},
					new EnemyData()
					{
						enemyIndex = 2,
						minSpawnedCount = 1,
						maxSpawnedCount = 1
					}
				}
			}
		};
		var json = JsonUtility.ToJson(newLevel);
		var f = new System.IO.FileInfo(@"Assets\Levels\level02.txt");
		var tw = f.CreateText();
		tw.Write(json.Replace(",", ",\n"));
		tw.Close();
		*/


		highScore = new int[diffCount];
		//if (levelData.levelName == "null")
		{
			var f = new System.IO.FileInfo(@"Assets\Levels\default.json");
			var tw = f.OpenText();
			var json = tw.ReadToEnd();
			levelController = new LevelController();

			levelController.levelData = JsonUtility.FromJson<LevelData>(json);
		}
		if (difficulty != -1)
		{
			var path = Application.persistentDataPath + "/data.usf";
			var stream = new System.IO.FileStream(path, System.IO.FileMode.Open);
			var reader = new System.IO.BinaryReader(stream);
			for (int d = 0; d < diffCount; d++)
			{
				highScore[d] = reader.ReadInt32();
			}
			stream.Close();
			if (highScore[difficulty] > 0) highScoreMonitor.text = "Highscore: " + highScore[difficulty];
		}
		else highScoreObject.SetActive(false);
		if (!enableHighScore) highScoreObject.SetActive(false);
#if UNITY_ANDROID
		touchControls.SetActive(true);
		if (Settings.arrows) joystick.SetActive(false); else arrows.SetActive(false);
#endif
	}

	public void UpdateControls()
	{
		joystick.SetActive(true);
		arrows.SetActive(true);
		if (Settings.arrows) joystick.SetActive(false); else arrows.SetActive(false);
	}

	public void IncreaseScore(float inc)
	{
		if (Player.IsDead) return;
		int lastScore = (int)score;
		score += inc * Mathf.Max(Player.hp / Player.healthPoints, 0.1f);
		scoreMonitor.text = $"Score: {Mathf.RoundToInt(score)}";
		int sc = (int)score;
		int scoreNeed = 500;
		if (sc / scoreNeed > lastScore / scoreNeed)
		{
			if (Player.movementSpeed < 100)
			{
				Player.movementSpeed++;
				achievementMonitor.text = "Your movement speed have been increased";
				achievementTime = Time.time;
			}
		}
		scoreNeed = 750;
		if (sc / scoreNeed > lastScore / scoreNeed)
		{
			if (w != null)
			{
				for (int i = 0; i < w.shotRate.Length; i++)
					if (w.shotRate[i] < 2000)
						w.shotRate[i] *= 1.03f;
				w.UpdateDelays();
				achievementMonitor.text = $"Your shot rate have been increased";
				achievementTime = Time.time;
			}
			if (k != null)
			{
				k.damage++;
				achievementMonitor.text = $"Your damage have been increased";
				achievementTime = Time.time;
			}
		}
		scoreNeed = 1000;
		if (sc / scoreNeed > lastScore / scoreNeed && Player.healthPoints < 100000)
		{
			float delta = Mathf.Clamp(Player.healthPoints * 1.1f, 1, 100000) - Player.healthPoints;
			Player.healthPoints += delta;
			Player.GetHealth(delta);
			achievementMonitor.text = "Your maxHealth have been increased";
			achievementTime = Time.time;
		}
		scoreNeed = 1250;
		if (sc / scoreNeed > lastScore / scoreNeed)
		{
			th.grenadeRate *= 1.1f;
			th.UpdateDelay();
			achievementMonitor.text = $"Your grenade rate have been increased";
			achievementTime = Time.time;
		}
		scoreNeed = 2500;
		if (sc / scoreNeed > lastScore / scoreNeed && Player.regeneration < 20000)
		{
			Player.regeneration = Mathf.Clamp(Player.regeneration * 1.25f, 1, 20000);
			Player.stillRegenScaleInc += 0.001f;
			Player.unlockSpeed -= 0.001f;
			Player.dmgLockTime -= 0.002f;
			Player.dmgLockScale += 0.0001f;
			achievementMonitor.text = $"Your regeneration have been increased";
			achievementTime = Time.time;
		}
		scoreNeed = 3750;
		if (sc / scoreNeed > lastScore / scoreNeed)
		{
			tp.tpChargeTime *= 0.92f;
			if (tp.TpSpeed > 0.08f)
				tp.TpSpeed -= 0.01f;
			achievementMonitor.text = $"Your teleport charge time have been improved";
			achievementTime = Time.time;
		}
		scoreNeed = 5000;
		if (sc / scoreNeed > lastScore / scoreNeed)
		{
			if (w != null && w.magazine < 500)
			{
				w.magazine *= 1.1f;
				Player.InitBullets();
				Player.InitOther();
				achievementMonitor.text = $"Congrats! Your magazine capacity have been increased";
				achievementTime = Time.time;
			}
			if (k != null && k.attackDistance < k.maxAttackDistance)
			{
				k.attackDistance *= 1.1f;
				k.attackDistance = Mathf.Clamp(k.attackDistance, 0.1f, k.maxAttackDistance);
				achievementMonitor.text = $"Congrats! Your attack distance have been increased";
				achievementTime = Time.time;
			}
		}
		scoreNeed = 6250;
		if (sc / scoreNeed > lastScore / scoreNeed)
		{
			tp.tpDistance += 2;
			achievementMonitor.text = $"Congrats! Your teleport distance have been increased";
			achievementTime = Time.time;
		}
		scoreNeed = 7500;
		if (sc / scoreNeed > lastScore / scoreNeed)
		{
			if (w != null)
			{
				if (w.reloadTime > 0.5f || w.partialReload)
				{
					w.reloadTime *= 0.9f;
					achievementMonitor.text = $"Congrats! Your reload time have been improved";
					achievementTime = Time.time;
				}
			}
			if (k != null)
			{
				k.attackRate *= 1.1f;
				k.UpdateDelays();
				achievementMonitor.text = $"Congrats! Your attack rate have been increased";
				achievementTime = Time.time;
			}
		}
		scoreNeed = 10000;
		if (sc / scoreNeed > lastScore / scoreNeed)
		{
			if (Camera.main.orthographicSize < 100)
			{
				Camera.main.orthographicSize += 4;
				Player.InitBullets();
				Player.InitOther();
				achievementMonitor.text = "Your filed of view have been increased";
				achievementTime = Time.time;
			}
		}
		scoreNeed = 15000;
		if (sc / scoreNeed > lastScore / scoreNeed)
		{
			tp.tpAccum++;
			Player.InitOther();
			achievementMonitor.text = $"Your teleport capacity have been increased";
			achievementTime = Time.time;
		}
		scoreNeed = 20000;
		if (sc / scoreNeed > lastScore / scoreNeed)
		{
			if (w != null)
			{
				w.dmgMultiplier *= 1.1f;
				achievementMonitor.text = $"Your weapon became stronger";
				achievementTime = Time.time;
			}
		}
	}

	private void LateUpdate()
	{
		enemiesMonitor.text = $"{Enemies} Enem" + (Enemies == 1 ? "y" : "ies");
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
		/*
		float localReductor = spawnReductor;
		while (Random.value < localReductor)
		{
			Instantiate(enemies[Random.Range(0, spawnTypeReductor)], 
				(Random.value > 0.5f) ? spawnNegative : spawnPositive, Quaternion.identity);
			localReductor--;
		}
		*/

		levelController.NextWave();
		var toSpawn = levelController.GetSpawnList();

		foreach (var i in toSpawn)
		{
			Instantiate(enemies[i],
				(Random.value > 0.5f) ? spawnNegative : spawnPositive, Quaternion.identity);
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
		Fader.gameObject.SetActive(true);
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
		//text1.text = $"{kills} Kill" + (kills == 1 ? "" : "s");
		text1.text = $"Score: {Mathf.RoundToInt(score)}";
		if (difficulty != -1)
		{
			if (highScore[difficulty] >= (int)score) highScoreObject.SetActive(false);
			else if (enableHighScore)
			{
				var path = Application.persistentDataPath + "/data.usf";
				var stream = new System.IO.FileStream(path, System.IO.FileMode.Create);
				var writer = new System.IO.BinaryWriter(stream);
				highScore[difficulty] = (int)score;
				for (int d = 0; d < 5; d++)
				{
					writer.Write(highScore[d]);
				}
				stream.Close();
			}
		}
		deathScreen.SetActive(true);
		exit = false;
	}
}

public class LevelController
{
	public LevelData levelData;
	public float waveStartTime;
	public int enemyCount;

	private int currentWave = -1;
	private List<Enemy> waveEnemies;
	private SortedDictionary<float, int> spawnTimes;
	private float nextCheck;


	public void NextWave()
	{
		bool hasEnded;
		if (currentWave == -1) hasEnded = true; else
		{
			hasEnded = Time.time - waveStartTime > levelData.waves[currentWave].spawnTime;
		}
		if (levelData.waves.Length > currentWave + 1)
		{
			if ((levelData.waves[currentWave + 1].previousWaveCounter >= enemyCount ||
		levelData.waves[currentWave + 1].previousWaveTimer + waveStartTime - Time.time < 0) && hasEnded)
			{
				currentWave++;
				Debug.Log((currentWave + 1) + " Wave");
				waveStartTime = Time.time;
				waveEnemies = new List<Enemy>();
				GetSpawnTimes();
				Debug.Log(spawnTimes.Count + " Enemies allocated");
			}
		}
		else Debug.Log("End of wave list");
	}

	private void GetSpawnTimes()
	{
		spawnTimes = new SortedDictionary<float, int>();
		float time = Time.time;
		foreach (var i in levelData.waves[currentWave].enemies)
		{
			int count = Random.Range(i.minSpawnedCount, i.maxSpawnedCount + 1);
			for (var j = 0; j < count; j++)
			{
				while (spawnTimes.ContainsKey(time))
				{
					time = Random.Range(waveStartTime, waveStartTime + levelData.waves[currentWave].spawnTime);
				}
				spawnTimes.Add(time, i.enemyIndex);
			}
		}
	}

	public List<int> GetSpawnList()
	{
		if (nextCheck <= Time.time)
		{
			CheckEnemies();
			nextCheck += 1;
		}
		var spawnList = new List<int>();

		var keys = new List<float>(spawnTimes.Keys);
		foreach (var i in keys)
		{
			if (i <= Time.time)
			{
				spawnList.Add(spawnTimes[i]);
				spawnTimes.Remove(i);
			}
		}

		enemyCount += spawnList.Count;
		return spawnList;
	}

	private void CheckEnemies()
	{
		for (int i = 0; i < waveEnemies.Count; i++)
		{
			if (waveEnemies[i] == null)
			{
				waveEnemies.RemoveAt(i);
				i--;
				continue;
			} else if (waveEnemies[i].IsDead)
			{
				waveEnemies.RemoveAt(i);
				i--;
				continue;
			}
		}
		enemyCount = waveEnemies.Count;
	}
}

[System.Serializable]
public class LevelData {
	public string levelName;

	public WaveData[] waves;

	public int upgradePoints;
}

[System.Serializable]
public class EnemyData {
	public int enemyIndex;
	public int minSpawnedCount;
	public int maxSpawnedCount;

}

[System.Serializable]
public class WaveData
{
	public float spawnTime;
	public EnemyData[] enemies;
	public int previousWaveCounter;
	public float previousWaveTimer;
}