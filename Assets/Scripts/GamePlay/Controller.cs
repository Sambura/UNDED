using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Controller : MonoBehaviour
{
	public GameObject[] enemy;
	public Text killsMonitor;
	public Text spawnMonitor;
	public Text enemiesMonitor;
	public Text achievementMonitor;
	public float spawnX;
	public float spawnY;
	public float spawnReductor;
	public float spawnInc;
	public bool enableScreenShake;
	public bool enableParticles;
	public bool enableDamageText;

	public LinkedList<Enemy> Enemies { get; private set; }

	private int kills;
	private Player player;

	private float achieveDuration = 7.5f;
	private float achieveTime;

	private void Start()
	{
		var data = FindObjectOfType<MenuController>();
		if (data != null)
		{
			player = Instantiate(data.Weapon, new Vector3(0, -8), Quaternion.identity).GetComponent<Player>();
			player.grenade = data.Grenade;
			switch (data.Difficulty)
			{
				case 0:
					spawnReductor = 0.005f;
					spawnInc = 0.00001f;
					player.regeneration = 1000;
					player.healthPoints = 2500;
					player.grenadeRate = 25;
					player.tpAccum = 3;
					player.tpChargeTime = 4;
					player.movementSpeed = 60;
					break;
				case 1:
					spawnReductor = 0.005f;
					spawnInc = 0.00005f;
					player.regeneration = 450;
					player.healthPoints = 2000;
					player.grenadeRate = 12;
					player.tpAccum = 2;
					player.tpChargeTime = 5;
					player.movementSpeed = 50;
					break;
				case 2:
					spawnReductor = 0.05f;
					spawnInc = 0.0001f;
					player.regeneration = 350;
					player.healthPoints = 1000;
					player.grenadeRate = 7;
					player.tpAccum = 1;
					player.tpChargeTime = 6;
					player.movementSpeed = 40;
					break;
			}
		}
		else player = FindObjectOfType<Player>();
		Enemies = new LinkedList<Enemy>();
		if (spawnReductor == 0) spawnReductor = spawnInc;
		killsMonitor.text = "0 Kills";
		spawnMonitor.text = $"Spawn rate: {System.Math.Round(spawnReductor * 100, 1)}%";
		enemiesMonitor.text = "0 Enemies";
		achievementMonitor.text = "Good luck, soldier!";
		achieveTime = Time.time;
	}

	private void Update()
	{
		if (player.IsDead) return;
		float localReductor = spawnReductor;
		while (Random.value < localReductor)
		{
			int side = Random.Range(1, 3);
			if (side == 2) side = -1;
			var f = Instantiate(enemy[Random.Range(0, enemy.Length)], new Vector3(spawnX * side, spawnY), Quaternion.identity);
			f.GetComponent<Enemy>().InitThis(this, player);
			localReductor--;
		}
		enemiesMonitor.text = $"{Enemies.Count} Enem" + (Enemies.Count == 1 ? "y" : "ies");
		if (Time.time - achieveTime >= achieveDuration)
		{
			achievementMonitor.text = "";
		}
	}

	public void KillsPlusPlus()
	{
		kills++;
		{
			killsMonitor.text = $"{kills} Kills";
			spawnReductor += spawnInc;
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
				achieveTime = Time.time;
				achieveFlag = true;
			}
		}
		if (kills % 75 == 0)
		{
			if (w != null)
			{
				for (int i = 0; i < w.shotRate.Length; i++)
					w.shotRate[i] *= 1.03f;
				w.UpdateDelays();
				achievementMonitor.text = $"Congrats! Your shot rate have been increased";
				achieveTime = Time.time;
				achieveFlag = true;
			}
			if (k != null)
			{
				k.damage++;
				achievementMonitor.text = $"Congrats! Your damage now = {k.damage}";
				achieveTime = Time.time;
				achieveFlag = true;
			}
		}
		if (kills % 100 == 0)
		{
			player.healthPoints *= 1.1f;
			achievementMonitor.text = $"Congrats! Your maxHealth now = {Mathf.RoundToInt(player.healthPoints)}";
			achieveTime = Time.time;
			achieveFlag = true;
		}
		if (kills % 125 == 0)
		{
			player.grenadeRate *= 1.1f;
			achievementMonitor.text = $"Congrats! You now can throw {Mathf.RoundToInt(player.grenadeRate)} grenades a minute!";
			achieveTime = Time.time;
			achieveFlag = true;
		}
		if (kills % 250 == 0)
		{
			player.regeneration *= 1.25f;
			achievementMonitor.text = $"Congrats! Your regeneration now = {Mathf.RoundToInt(player.regeneration)}";
			achieveTime = Time.time;
			achieveFlag = true;
		}
		if (kills % 375 == 0)
		{
			player.tpChargeTime *= 0.92f;
			if (player.TpSpeed > 0.08f)
				player.TpSpeed -= 0.01f;
			achievementMonitor.text = $"Congrats! Your teleport now can be charged in {System.Math.Round(player.tpChargeTime, 1)} seconds!";
			achieveTime = Time.time;
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
				achieveTime = Time.time;
				achieveFlag = true;
			}
			if (k!= null)
			{
				k.attackDistance += 3;
				achievementMonitor.text = $"Congrats! Your attack distance now = {k.attackDistance}";
				achieveTime = Time.time;
				achieveFlag = true;
			}
		}
		if (kills % 625 == 0)
		{
			player.tpDistance += 2;
			achievementMonitor.text = $"Congrats! Your teleport distance now = {player.tpDistance}!";
			achieveTime = Time.time;
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
					achieveTime = Time.time;
					achieveFlag = true;
				}
			}
			if (k != null)
			{
				k.attackRate *= 1.1f;
				k.attackTime *= 0.95f;
				k.UpdateDelays();
				achievementMonitor.text = $"Congrats! Your attack rate now = {Mathf.RoundToInt(k.attackRate)} seconds";
				achieveTime = Time.time;
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
				achieveTime = Time.time;
				achieveFlag = true;
			}
		}
		if (kills % 1500 == 0)
		{
			player.tpAccum++;
			player.InitTeleport();
			achievementMonitor.text = $"Congrats! Your teleport now have {player.tpAccum} bars of energy!";
			achieveTime = Time.time;
			achieveFlag = true;
		}
		if (achieveFlag) achievementMonitor.text += $" ({kills} kills)";
	}
}
