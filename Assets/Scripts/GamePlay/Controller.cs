using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

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
	public Slider movementControl;
	public EventTrigger teleportControl;
	public EventTrigger throwControl;
	public EventTrigger fire1Control;
	public EventTrigger fire2Control;
	public EventTrigger reloadControl;
#pragma warning restore IDE0044
	#endregion

	public PlayerData playerData;

	[System.Serializable]
	public struct PlayerData
	{
		public string playerName;
		public string weaponName;
		public string throwerName;
		public string grenadeName;
		public string teleportName;
		public string shieldName;
	}

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

	private Gun w;
	private Knife k;
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
		if (Instance == null) Instance = this; else
		{
			Destroy(this);
			return;
		}
		exit = true;
		spawnPositive = new Vector2(spawnX, spawnY);
		spawnNegative = new Vector2(-spawnX, spawnY);
		controller = FindObjectOfType<MenuController>();
		speaker = GetComponent<AudioSource>();
		difficulty = -1;
		if (controller != null)
		{
			difficulty = controller.Difficulty;
			SpawnPlayer(controller);
			w = Player.GetComponentInChildren<Weapon>() as Gun;
			k = Player.GetComponentInChildren<Weapon>() as Knife;
			tp = Player.teleport;
			th = Player.thrower;

			switch (difficulty)
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

		highScore = new int[diffCount];
		//if (levelData.levelName == "null")
		if (difficulty == -2)
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

	private void SpawnPlayer(MenuController controller)
	{
		playerData = controller.playerData;
		var playerGO = PresetsManager.Instance.InstantiatePrefab("player", playerData.playerName);
		Player = playerGO.GetComponent<Player>();
		/*
		{
			var pData = JsonUtility.FromJson<PlayerData>(PresetsManager.players[playerData.playerName]);
			if (playerData.weaponName == "") playerData.weaponName = pData.weaponName;
			if (playerData.throwerName == "") playerData.throwerName = pData.throwerName;
			if (playerData.grenadeName == "") playerData.grenadeName = pData.grenadeName;
			if (playerData.teleportName == "") playerData.teleportName = pData.teleportName;
			if (playerData.shieldName == "") playerData.shieldName = pData.shieldName;
		}*/
		{
			var go = PresetsManager.Instance.InstantiatePrefab("weapon", playerData.weaponName);
			Player.weapon = go.GetComponent<Weapon>();
			go.transform.parent = Player.weaponHolder;
			go.transform.localPosition = Vector3.zero;
			go.SetActive(true);
		}
		if (playerData.throwerName != "")
		{
			var go = PresetsManager.Instance.InstantiatePrefab("thrower", playerData.throwerName);
			Player.thrower = go.GetComponent<Thrower>();
			go.transform.parent = Player.transform;
			go.transform.localPosition = Vector3.zero;
			Player.thrower.grenade = PresetsManager.Instance.InstantiatePrefab("grenade", playerData.grenadeName);
			PrepareGrenade(Player.thrower.grenade.GetComponent<Grenade>());
			go.SetActive(true);
		}
		if (playerData.teleportName != "")
		{
			var go = PresetsManager.Instance.InstantiatePrefab("teleport", playerData.teleportName);
			Player.teleport = go.GetComponent<Teleport>();
			go.transform.parent = Player.transform;
			go.transform.localPosition = Vector3.zero;
			go.SetActive(true);
		}
		if (playerData.shieldName != "")
		{
			var go = PresetsManager.Instance.InstantiatePrefab("shield", playerData.shieldName);
			Player.shield = go.GetComponent<Shield>();
			go.transform.parent = Player.transform;
			go.transform.localPosition = Vector3.zero;
			go.SetActive(true);
		}
#if UNITY_ANDROID
		movementControl.onValueChanged.AddListener((float x) => Player.movingControls = x);

		var OnSpaceDown = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
		var OnSpaceUp = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
		OnSpaceDown.callback.AddListener((BaseEventData e) => Player.Space = true);
		OnSpaceUp.callback.AddListener((BaseEventData e) => Player.Space = false);
		teleportControl.triggers.Add(OnSpaceDown); teleportControl.triggers.Add(OnSpaceUp);

		var OnFDown = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
		var OnFUp = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
		OnFDown.callback.AddListener((BaseEventData e) => Player.F = true);
		OnFUp.callback.AddListener((BaseEventData e) => Player.F = false);
		throwControl.triggers.Add(OnFDown); throwControl.triggers.Add(OnFUp);

		var OnRDown = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
		var OnRUp = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
		OnRDown.callback.AddListener((BaseEventData e) => Player.R = true);
		OnRUp.callback.AddListener((BaseEventData e) => Player.R = false);
		reloadControl.triggers.Add(OnRDown); reloadControl.triggers.Add(OnRUp);

		var OnFire1Down = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
		var OnFire1Up = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
		OnFire1Down.callback.AddListener((BaseEventData e) => Player.Fire1 = true);
		OnFire1Up.callback.AddListener((BaseEventData e) => Player.Fire1 = false);
		fire1Control.triggers.Add(OnFire1Down); fire1Control.triggers.Add(OnFire1Up);

		var OnFire2Down = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
		var OnFire2Up = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
		OnFire2Down.callback.AddListener((BaseEventData e) => Player.Fire2 = true);
		OnFire2Up.callback.AddListener((BaseEventData e) => Player.Fire2 = false);
		fire2Control.triggers.Add(OnFire2Down); fire2Control.triggers.Add(OnFire2Up);
#endif

		playerGO.SetActive(true);
	}

	private void PrepareGrenade(Grenade grenade)
	{
		grenade.blast = PresetsManager.Instance.InstantiatePrefab("blast", grenade.blastName);
		var blast = grenade.blast.GetComponent<Blast>();
		if (blast.fragmentName != "")
		{
			var fragment = PresetsManager.Instance.InstantiatePrefab("grenade", blast.fragmentName);
			blast.fragment = fragment;
			PrepareGrenade(fragment.GetComponent<Grenade>());
		}
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
		score += inc * Mathf.Max(Player.hp / Player.maxHealth, 0.1f);
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
		if (sc / scoreNeed > lastScore / scoreNeed && Player.maxHealth < 100000)
		{
			float delta = Mathf.Clamp(Player.maxHealth * 1.1f, 1, 100000) - Player.maxHealth;
			Player.maxHealth += delta;
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
			Player.stillRegenerationBoost += 0.001f;
			Player.unlockSpeed -= 0.001f;
			Player.damageLockTime -= 0.002f;
			Player.damageLockScale += 0.0001f;
			achievementMonitor.text = $"Your regeneration have been increased";
			achievementTime = Time.time;
		}
		scoreNeed = 3750;
		if (sc / scoreNeed > lastScore / scoreNeed)
		{
			tp.chargeTime *= 0.92f;
			if (tp.teleportationDuration > 0.08f)
				tp.teleportationDuration -= 0.01f;
			achievementMonitor.text = $"Your teleport charge time have been improved";
			achievementTime = Time.time;
		}
		scoreNeed = 5000;
		if (sc / scoreNeed > lastScore / scoreNeed)
		{
			if (w != null && w.magazineCapacity < 500)
			{
				w.magazineCapacity *= 1.1f;
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
			tp.maxDistance += 2;
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
			tp.capacity++;
			Player.InitOther();
			achievementMonitor.text = $"Your teleport capacity have been increased";
			achievementTime = Time.time;
		}
		scoreNeed = 20000;
		if (sc / scoreNeed > lastScore / scoreNeed)
		{
			if (w != null)
			{
				w.damageMultiplier *= 1.1f;
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

		if (difficulty != -2)
		{
			float localReductor = spawnReductor / 2;
			while (Random.value < localReductor) 
			{
				Instantiate(enemies[Random.Range(0, spawnTypeReductor)],
					(Random.value > 0.5f) ? spawnNegative : spawnPositive, Quaternion.identity);
				localReductor--;
			}
		}
		else
		{
			levelController.NextWave();
			var toSpawn = levelController.GetSpawnList();

			foreach (var i in toSpawn)
			{
				Instantiate(enemies[i],
					(Random.value > 0.5f) ? spawnNegative : spawnPositive, Quaternion.identity);
			}
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