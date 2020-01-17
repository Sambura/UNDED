using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Controller : MonoBehaviour
{
	#region Inspector fields
	[Header("Enemies")] [Tooltip("Enemies that will be spawned during game process")]
	[SerializeField] private GameObject[] enemies;
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
	[SerializeField] private RawImage deathScreen;
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
	[Header("Game settings")]
	public bool enableScreenShake;
	public bool enableParticles;
	public bool enableDamageText;
	[Range(0, 1)] public float sfxVolume;
	[Header("Other settings")]
	[Tooltip("Determines, how long one achievement should be displayed")]
	[SerializeField] private float achievementDisplayDuration;
	[Tooltip("Determines delay before the death splash display")]
	[SerializeField] private float deathSpalshDelay;
	[SerializeField] private float fadeToMainMenuDuration;
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
				player.CancelThrowing();
				Time.timeScale = 0;
				pauseScreen.SetActive(true);
			} else
			{
				Time.timeScale = 0;
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

	private Player player;
	private int kills;
	/// <summary>
	/// Last time any achievement was gotten, used to calculate hide time
	/// </summary>
	private float achievementTime;
	private Vector3 spawnPositive, spawnNegative;

	public void InstantiateDamageLabel(Vector2 position, float value)
	{
		if (!enableDamageText) return;
		Instantiate(damageLabel, position, Quaternion.identity, worldCanvas.transform)
			.GetComponent<Label>().SetNumber(Mathf.RoundToInt(value));
	}

	private void Start()
	{
		Enemies = new LinkedList<Enemy>();
		spawnPositive = new Vector2(spawnX, spawnY);
		spawnNegative = new Vector2(-spawnX, spawnY);
		var data = FindObjectOfType<MenuController>();
		if (data != null)
		{
			player = Instantiate(data.Weapon, new Vector3(0, -8), Quaternion.identity).GetComponent<Player>();
			player.grenade = data.Grenade;
			enableScreenShake = data.ScreenShake;
			enableParticles = data.Particles;
			enableDamageText = data.Labels;
			switch (data.Difficulty)
			{
				case 0:
					spawnReductor = 0.008f;
					spawnInc = 0.000003f;
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
			}
			switch (data.Stats)
			{
				case 3:
					player.regeneration = 10000;
					player.healthPoints = 100000;
					player.grenadeRate = 60;
					player.tpAccum = 5;
					player.tpChargeTime = 2;
					player.movementSpeed = 75;
					break;
				case 2:
					player.regeneration = 600;
					player.healthPoints = 3000;
					player.grenadeRate = 20;
					player.tpAccum = 3;
					player.tpChargeTime = 4;
					player.movementSpeed = 55;
					break;
				case 1:
					player.regeneration = 450;
					player.healthPoints = 2000;
					player.grenadeRate = 11;
					player.tpAccum = 2;
					player.tpChargeTime = 5;
					player.movementSpeed = 50;
					break;
				case 0:
					player.regeneration = 350;
					player.healthPoints = 1000;
					player.grenadeRate = 6;
					player.tpAccum = 1;
					player.tpChargeTime = 6;
					player.movementSpeed = 45;
					break;
			}
			Destroy(data.gameObject);
		}
		else player = FindObjectOfType<Player>();
		if (spawnReductor == 0) spawnReductor = spawnInc;
		killsMonitor.text = "0 Kills";
		spawnMonitor.text = $"Spawn rate: {System.Math.Round(spawnReductor * 100, 1)}%";
		enemiesMonitor.text = "0 Enemies";
		achievementMonitor.text = "Good luck, soldier!";
		achievementTime = Time.time;
	}

	private void LateUpdate()
	{
		enemiesMonitor.text = $"{Enemies.Count} Enem" + (Enemies.Count == 1 ? "y" : "ies");
		if (Time.time - achievementTime >= achievementDisplayDuration)
		{
			achievementMonitor.text = "";
		}
		bool flag = false;
		if (IsPaused)
		{
			if (Input.GetKeyDown(KeyCode.Escape))
			{
				flag = true;
				IsPaused = false;
			}
			if (Input.GetKeyDown(KeyCode.Backspace))
			{
				IsPaused = false;
				StartCoroutine(MainMenuOpen());
			}
		}
		if (!IsPaused && !flag)
		{
			if (Input.GetKeyDown(KeyCode.Escape))
			{
				if (!player.IsDead)
				{
					IsPaused = true;
				} else
				{
					StartCoroutine(MainMenuOpen());
				}
			}
		}
	}

	private void FixedUpdate()
	{
		if (player.IsDead) return;
		float localReductor = spawnReductor;
		while (Random.value < localReductor)
		{
			var f = Instantiate(enemies[Random.Range(0, enemies.Length)], 
				Random.Range(0, 2) == 0 ? spawnNegative : spawnPositive, Quaternion.identity);
			f.GetComponent<Enemy>().InitThis(this, player);
			localReductor--;
		}
	}

	public void KillsPlusPlus()
	{
		kills++;
		{
			killsMonitor.text = $"{kills} Kills";
			spawnReductor = Mathf.Clamp(spawnReductor + spawnInc, 0, maxReductor);
			spawnMonitor.text = $"Spawn rate: {System.Math.Round(spawnReductor * 100, 1)}%";
		}
		bool achieveFlag = false;
		var w = player.GetComponentInChildren<Weapon_gun>();
		var k = player.GetComponentInChildren<Weapon_knife>();
		if (kills % 50 == 0)
		{
			if (player.movementSpeed < 100)
			{
				player.movementSpeed++;
				achievementMonitor.text = $"Congrats! Your movement speed now = {player.movementSpeed}";
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
		if (kills % 100 == 0 && player.healthPoints < 100000)
		{
			float delta = player.healthPoints * 1.1f - player.healthPoints;
			player.healthPoints += delta;
			player.GetHealth(delta);
			achievementMonitor.text = $"Congrats! Your maxHealth now = {Mathf.RoundToInt(player.healthPoints)}";
			achievementTime = Time.time;
			achieveFlag = true;
		}
		if (kills % 125 == 0)
		{
			player.grenadeRate *= 1.1f;
			achievementMonitor.text = $"Congrats! You now can throw {Mathf.RoundToInt(player.grenadeRate)} grenades a minute!";
			achievementTime = Time.time;
			achieveFlag = true;
		}
		if (kills % 250 == 0)
		{
			player.regeneration *= 1.25f;
			achievementMonitor.text = $"Congrats! Your regeneration now = {Mathf.RoundToInt(player.regeneration)}";
			achievementTime = Time.time;
			achieveFlag = true;
		}
		if (kills % 375 == 0)
		{
			player.tpChargeTime *= 0.92f;
			if (player.TpSpeed > 0.08f)
				player.TpSpeed -= 0.01f;
			achievementMonitor.text = $"Congrats! Your teleport now can be charged in {System.Math.Round(player.tpChargeTime, 1)} seconds!";
			achievementTime = Time.time;
			achieveFlag = true;
		}
		if (kills % 500 == 0)
		{
			if (w != null)
			{
				w.magazine *= 1.1f;
				player.InitBullets();
				player.InitTeleport();
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
			player.tpDistance += 2;
			achievementMonitor.text = $"Congrats! Your teleport distance now = {player.tpDistance}!";
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
				achievementMonitor.text = $"Congrats! Your attack rate now = {Mathf.RoundToInt(k.attackRate)} seconds";
				achievementTime = Time.time;
				achieveFlag = true;
			}
		}
		if (kills % 1000 == 0)
		{
			if (Camera.main.orthographicSize < 100)
			{
				Camera.main.orthographicSize += 4;
				player.InitBullets();
				player.InitTeleport();
				achievementMonitor.text = "Congrats! You've increased your filed of view!";
				achievementTime = Time.time;
				achieveFlag = true;
			}
		}
		if (kills % 1500 == 0)
		{
			player.tpAccum++;
			player.InitTeleport();
			achievementMonitor.text = $"Congrats! Your teleport now have {player.tpAccum} bars of energy!";
			achievementTime = Time.time;
			achieveFlag = true;
		}
		if (achieveFlag) achievementMonitor.text += $" ({kills} kills)";
	}

	private IEnumerator MainMenuOpen()
	{
		var open = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("StartMenu",
						UnityEngine.SceneManagement.LoadSceneMode.Single);
		open.allowSceneActivation = false;
		float startTime = Time.time;
		Color color = new Color(0, 0, 0, 0);
		for (float a = 0; a < 1; a = Mathf.Lerp(0, 1, (Time.time - startTime) / fadeToMainMenuDuration))
		{
			color.a = a;
			Fader.color = color;
			yield return null;
		}
		open.allowSceneActivation = true;
	}

	public IEnumerator Death()
	{
		yield return new WaitForSeconds(deathSpalshDelay);
		var killsMonitorTransform = killsMonitor.GetComponent<RectTransform>();
		float dx = killsMonitorTransform.anchoredPosition.x;
		float dy = killsMonitorTransform.anchoredPosition.y;
		Vector2 anchorMin = killsMonitorTransform.anchorMin;
		Vector2 anchorMax = killsMonitorTransform.anchorMax;
		killsMonitor.alignment = TextAnchor.MiddleCenter;
		var text1 = deathScreen.GetComponentInChildren<Text>();
		for (float a = 0; a < 1; a += 0.01f)
		{
			deathScreen.color = new Color(deathScreen.color.r, deathScreen.color.g, deathScreen.color.b, a);
			text1.color = new Color(text1.color.r, text1.color.g, text1.color.b, a);
			killsMonitorTransform.anchoredPosition = new Vector2(Mathf.Lerp(dx, 0, a), Mathf.Lerp(dy, 0, a));
			killsMonitorTransform.anchorMin = new Vector2(Mathf.Lerp(anchorMin.x, 0.1f, a), Mathf.Lerp(anchorMin.y, 0.2f, a));
			killsMonitorTransform.anchorMax = new Vector2(Mathf.Lerp(anchorMax.x, 0.9f, a), Mathf.Lerp(anchorMax.y, 0.6f, a));
			yield return null;
		}
	}
}
