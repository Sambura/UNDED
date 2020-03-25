using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InfinityController : Controller
{
	#region Inspector fields
	[Header("Enemies")] [Tooltip("Enemies that will be spawned during game process")]
	[SerializeField] private GameObject[] enemies;
	[SerializeField] private int spawnTypeReductor;
	[Header("UI elements")]
	[Tooltip("Text UI element that have to display kills count")]
	[SerializeField] private TMPro.TextMeshProUGUI killsMonitor;
	[SerializeField] private TMPro.TextMeshProUGUI scoreMonitor;
	[SerializeField] private TMPro.TextMeshProUGUI highScoreMonitor;
	[Tooltip("Text UI element that have to display spawn percent")]
	[SerializeField] private TMPro.TextMeshProUGUI spawnMonitor;
	[Tooltip("Text UI element that have to display enemies count")]
	[SerializeField] private TMPro.TextMeshProUGUI enemiesMonitor;
	[Tooltip("Text UI element that have to display current achievement")]
	[SerializeField] private TMPro.TextMeshProUGUI achievementMonitor;
	[SerializeField] private GameObject highScoreObject;
	[Header("Level data")]
	[SerializeField] [Range(0, 1)] private float spawnReductor;
	[SerializeField] [Range(0, 1)] private float spawnInc;
	[SerializeField] [Range(0, 1)] private float maxReductor;
	[Header("Other settings")]
	[Tooltip("Determines, how long one achievement should be displayed")]
	[SerializeField] private float achievementDisplayDuration;
	#endregion

	private float achievementTime;

	private Gun w;
	private Knife k;
	private Teleport tp;
	private Thrower th;

	private bool exit;
	/// <summary>
	/// Defines difficulty in current game
	/// -2 = No difficulty, executting level file
	/// -1 = No difficulty, used through editor
	/// 0...N = Some difficulty from list
	/// </summary>
	private int difficulty;
	private int[] highScore;
	private bool enableHighScore;

	private void Awake()
	{
		if (Instance == null) Instance = this; else
		{
			Destroy(this);
			return;
		}
		FindObjectOfType<PresetsManager>().Awake();
		exit = true;
		spawnPositive = new Vector2(spawnX, spawnY);
		spawnNegative = new Vector2(-spawnX, spawnY);
		var controller = FindObjectOfType<MenuController>();
		speaker = GetComponent<AudioSource>();
		difficulty = -1;
		if (controller != null)
		{
			difficulty = controller.Difficulty;
			playerData = controller.playerData;
			SpawnPlayer(controller.playerData);
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
		} else
		{
			SpawnPlayer(playerData);
		}
		if (spawnReductor == 0) spawnReductor = spawnInc;
		killsMonitor.text = "0 Kills";
		scoreMonitor.text = $"Score: 0";
		spawnMonitor.text = $"Spawn rate: {System.Math.Round(spawnReductor * 100, 1)}%";
		enemiesMonitor.text = "0 Enemies";
		achievementMonitor.text = "Good luck, soldier!";
		achievementTime = Time.time;
		StartCoroutine(MusicPlayer());
		highScoreMonitor.text = "Highscore: — ";
		var diffCount = 5;

		highScore = new int[diffCount];
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

	public override void EnemyKilled(float scoreInc)
	{
		kills++;
		Enemies--;
		{
			killsMonitor.text = $"{kills} Kill" + (kills == 1 ? "" : "s");
			spawnReductor = Mathf.Clamp(spawnReductor + spawnInc, 0, maxReductor);
			spawnMonitor.text = $"Spawn rate: {System.Math.Round(spawnReductor * 100, 1)}%";
		}
		if (Player.IsDead) return;
		int lastScore = (int)score;
		score += scoreInc * Mathf.Max(Player.Hp / Player.maxHealth, 0.1f);
		scoreMonitor.text = $"Score: {Mathf.RoundToInt(score)}";
		/*
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
		*/
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
		float localReductor = spawnReductor / 2;
		while (Random.value < localReductor)
		{
			Instantiate(enemies[Random.Range(0, spawnTypeReductor)],
				(Random.value > 0.5f) ? spawnNegative : spawnPositive, Quaternion.identity);
			localReductor--;
		}
	}

	public override IEnumerator Death()
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

public class Controller : MonoBehaviour
{
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

	[SerializeField] protected GameObject worldCanvas;
	[SerializeField] protected GameObject damageLabel;
	[SerializeField] protected GameObject pauseScreen;
	[SerializeField] protected GameObject deathScreen;
	[SerializeField] protected RawImage Fader;
	[Header("Location properties")]
	[Tooltip("Determines absolute x-coordinate, where enemies should be spawned")]
	[SerializeField] protected float spawnX;
	[Tooltip("Floor's coordinate, where enemies should be spawned")]
	[SerializeField] protected float spawnY;
	[Tooltip("Half of the level's width")]
	[SerializeField] protected float levelWidth;
	[Tooltip("Half of the level's height")]
	[SerializeField] protected float levelHeight;
	[Header("Touch controls")]
	[SerializeField] protected GameObject touchControls;
	[SerializeField] protected Slider movementControl;
	[SerializeField] protected EventTrigger teleportControl;
	[SerializeField] protected EventTrigger throwControl;
	[SerializeField] protected EventTrigger fire1Control;
	[SerializeField] protected EventTrigger fire2Control;
	[SerializeField] protected EventTrigger reloadControl;
	[Header("Other settings")]
	[Tooltip("Determines delay before the death splash display")]
	[SerializeField] protected float deathSpalshDelay;
	[SerializeField] protected float fadeToMainMenuDuration;
	[SerializeField] protected AudioClip[] music;
	[Header("Editor settings")]
	[SerializeField] protected PlayerData playerData;

	[HideInInspector]
	public int Enemies;
	public Player Player { get; set; }
	public float LevelWidth { get => levelWidth; }
	public float LevelHeight { get => levelHeight; }
	public bool IsPaused
	{
		get => _isPaused;
		set
		{
			_isPaused = value;
			if (value)
			{
				//Player.CancelThrowing();
				Time.timeScale = 0;
				//Settings.sfxMixer.SetFloat("Pitch", 0);
				pauseScreen.SetActive(true);
			}
			else
			{
				Time.timeScale = 1;
				//Settings.sfxMixer.SetFloat("Pitch", 1);
				pauseScreen.SetActive(false);
			}
		}
	}

	protected bool _isPaused;
	protected int kills;
	protected float score;
	protected Vector3 spawnPositive, spawnNegative;
	protected AudioSource speaker;

	public static Controller Instance;

	public void InstantiateDamageLabel(Vector2 position, float value)
	{
		if (!Settings.damageText) return;
		Instantiate(damageLabel, position, Quaternion.identity, worldCanvas.transform)
			.GetComponent<Label>().SetNumber(Mathf.RoundToInt(value));
	}

	public virtual void EnemyKilled(float scoreInc)
	{
		kills++;
		Enemies--;
		IncreaseScore(scoreInc);
	}

	public virtual void IncreaseScore(float scoreInc)
	{
		if (Player.IsDead) return;
		score += scoreInc * Mathf.Max(Player.Hp / Player.maxHealth, 0.1f);
	}

	public virtual IEnumerator Death()
	{
		speaker.Stop();
		yield return new WaitForSecondsRealtime(deathSpalshDelay);
		deathScreen.SetActive(true);
	}

	protected void SpawnPlayer(PlayerData playerData)
	{
		var playerGO = PresetsManager.Instance.InstantiatePrefab("player", playerData.playerName);
		Player = playerGO.GetComponent<Player>();
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

	protected void PrepareGrenade(Grenade grenade)
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

	public void GoToMainMenu()
	{
		StartCoroutine(MainMenuOpen());
	}

	protected IEnumerator MainMenuOpen()
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
}