using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class StoryModeController : Controller
{
	#region Inspector fields
	[Header("Story Mode Controller Settings")]
	[SerializeField] private GameObject[] enemies;
	[Header("UI elements")]
	[Tooltip("Text UI element that have to display kills count")]
	[SerializeField] private TMPro.TextMeshProUGUI killsMonitor;
	//[Tooltip("Text UI element that have to display enemies count")]
	//[SerializeField] private TMPro.TextMeshProUGUI enemiesMonitor;
	[Tooltip("Waves of enemies on current level")]
	[SerializeField] private WaveData[] waves;
	#endregion

	private bool exit;

	private LevelController levelController;

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
		if (controller != null)
		{
			playerData = controller.playerData;
			SpawnPlayer(playerData);
			Destroy(controller.gameObject);
		} else
		{
			SpawnPlayer(playerData);
		}
		killsMonitor.text = "0 Kills";
		//enemiesMonitor.text = "0 Enemies";

		levelController = new LevelController(waves);

		StartCoroutine(MusicPlayer());
#if UNITY_ANDROID
		touchControls.SetActive(true);
		if (Settings.arrows) joystick.SetActive(false); else arrows.SetActive(false);
#endif
	}

	private void LateUpdate()
	{
		//enemiesMonitor.text = $"{Enemies} Enem" + (Enemies == 1 ? "y" : "ies");
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
				if (music.Length > 0)
				speaker.PlayOneShot(music[Random.Range(0, music.Length)]);
			}
			yield return new WaitForSecondsRealtime(3);
		}
	}

	private void FixedUpdate()
	{
		if (Player.IsDead) return;

		// Enemies spawn
		// Gameplay things managing
		// Other fun stuff

		var toSpawn = levelController.FixedUpdate();
		foreach (var i in toSpawn)
		{
			Instantiate(enemies[i], (Random.value > 0.5f) ? spawnNegative : spawnPositive, Quaternion.identity);
		}
	}

	public override void EnemyKilled(float scoreInc)
	{
		kills++;
		{
			killsMonitor.text = $"{kills} Kill" + (kills == 1 ? "" : "s");
		}
	}

	public override IEnumerator Death()
	{
		speaker.Stop();
		yield return new WaitForSecondsRealtime(deathSpalshDelay);
		deathScreen.SetActive(true);
		exit = false;
	}
}

public class LevelController
{
	public WaveData[] levelData;

	private SortedDictionary<float, int> spawnTimes;
	private int currentWave = -1;
	private float loopWave;
	private float timeOffset;

	private float Round(float n, int decimals)
	{
		float exp = Mathf.Pow(n, decimals);
		return Mathf.Round(n * exp) / exp;
	}

	private void GetSpawnTimes()
	{
		int minCount = 0, maxCount = 0;
		foreach (var i in levelData[currentWave].enemies)
		{
			minCount += i.minSpawnCount;
			maxCount += i.maxSpawnCount;
		}

		if (minCount > levelData[currentWave].maxEnemiesCount)
		{
			Debug.Log("Spawn times generating error: Enemies minimum is greater then wave maximum");
			return;
		}

		if (maxCount < levelData[currentWave].minEnemiesCount)
		{
			Debug.Log("Spawn times generating error: Enemies maximum is less then wave minimum");
			return;
		}

		var spawnCounts = new int[levelData[currentWave].enemies.Length];
		int count = 0;
		for (var i = 0; i < levelData[currentWave].enemies.Length; i++)
		{
			var enemyData = levelData[currentWave].enemies[i];
			spawnCounts[i] = Random.Range(enemyData.minSpawnCount, enemyData.maxSpawnCount + 1);
			count += spawnCounts[i];
		}

		while (count != Mathf.Clamp(count, levelData[currentWave].minEnemiesCount, levelData[currentWave].maxEnemiesCount))
		{
			int inc = (count > levelData[currentWave].maxEnemiesCount) ? -1 : 1;
			int index = Random.Range(0, levelData[currentWave].enemies.Length);
			if (spawnCounts[index] + inc == Mathf.Clamp(spawnCounts[index] + inc, 
				levelData[currentWave].enemies[index].minSpawnCount, levelData[currentWave].enemies[index].maxSpawnCount))
			{
				spawnCounts[index] += inc;
				count += inc;
			}
		}
		Debug.Log("Wave " + currentWave + ": " + count + " enemies allocated");
		string debugLog = "Spawn times: (Now: " + Time.time + timeOffset + ")";
		int decimals = 0;
		float time = Round(Random.Range(Time.time + timeOffset, Time.time + timeOffset + levelData[currentWave].spawnTime), decimals);
		for (var i = 0; i < levelData[currentWave].enemies.Length; i++)
		{
			for (var j = 0; j < spawnCounts[i]; j++)
			{
				int tries = 0;
				while (spawnTimes.ContainsKey(time))
				{
					time = Round(Random.Range(Time.time + timeOffset, Time.time + timeOffset + levelData[currentWave].spawnTime), decimals);
					tries++;
					if (tries > 650)
					{
						tries = 0;
						decimals++;
					}
				}
				debugLog += "\nEnemy " + levelData[currentWave].enemies[i].enemyIndex + ": +" + time;
				spawnTimes.Add(time, levelData[currentWave].enemies[i].enemyIndex);
			}
		}
		Debug.Log(debugLog);
	}

	public List<int> FixedUpdate()
	{
		if (levelData.Length >= currentWave + 2)
		{
			if (levelData[currentWave + 1].startTime <= Time.time + timeOffset)
			{
				currentWave++;
				GetSpawnTimes();
			}
		}
		else Debug.Log("No more waves");

		var spawnList = new List<int>();

		var keys = new List<float>(spawnTimes.Keys);
		foreach (var i in keys)
		{
			if (i <= Time.time + timeOffset)
			{
				spawnList.Add(spawnTimes[i]);
				spawnTimes.Remove(i);
			}
		}

		return spawnList;
	}

	public LevelController(WaveData[] waves)
	{
		levelData = waves;
		spawnTimes = new SortedDictionary<float, int>();
		timeOffset = -Time.time;
	}
}

[System.Serializable]
public class LevelData
{
	public string levelName;

	public WaveData[] waves;

	public int upgradePoints;
}

[System.Serializable]
public class EnemyData
{
	public int enemyIndex;
	public int minSpawnCount;
	public int maxSpawnCount;
}

[System.Serializable]
public class WaveData
{
	public float startTime;
	public float spawnTime;
	public EnemyData[] enemies;
	public int minEnemiesCount;
	public int maxEnemiesCount;
}