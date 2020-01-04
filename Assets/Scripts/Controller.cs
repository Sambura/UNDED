using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour
{
	public GameObject[] enemy;
	public float spawnX;
	public float spawnY;
	public float spawnReductor;

	public LinkedList<Enemy> Enemies { get; private set; }

	private int kills;
	private Player player;

	private void Start()
	{
		player = FindObjectOfType<Player>();
		Enemies = new LinkedList<Enemy>();
	}

	private void Update()
	{
		if (player.IsDead) return;
		if (Random.value < spawnReductor)
		{
			int side = Random.Range(1, 3);
			if (side == 2) side = -1;
			var f = Instantiate(enemy[Random.Range(0, enemy.Length)], new Vector3(spawnX * side, spawnY), Quaternion.identity);
			f.GetComponent<Enemy>().SetController(this);
		}
	}

	public void KillsPlusPlus()
	{
		kills++;
		Debug.Log($"Kills: {kills}");
		spawnReductor += 0.00005f;
		var w = player.GetComponentInChildren<Weapon>();
		if (kills % 50 == 0)
		{
			if (player.movementSpeed < 100)
			{
				player.movementSpeed++;
				Debug.Log($"Congrats! Your movement speed now = {player.movementSpeed}");
			}
		}/*
		if (kills % 75 == 0)
		{
			for (int i = 0; i < w.shotRate.Length; i++)
				w.shotRate[i] *= 1.03f;
			Debug.Log("Congrats! Your shot rate have been increased");
		}*/
		if (kills % 100 == 0)
		{
			player.healthPoints *= 1.1f;
			Debug.Log($"Congrats! Your maxHealth now = {player.healthPoints}");
		}
		if (kills % 125 == 0)
		{
			player.grenadeRate += 1;
			Debug.Log($"Congrats! You now can throw {player.grenadeRate} grenades per minute!");
		}
		if (kills % 250 == 0)
		{
			player.regeneration *= 1.25f;
			Debug.Log($"Congrats! Your regeneration now = {player.regeneration}");
		}
		if (kills % 375 == 0)
		{
			player.tpChargeTime *= 0.9f;
			Debug.Log($"Congrats! Your teleport {player.tpChargeTime} now became faster!");
		}/*
		if (kills % 500 == 0)
		{
			w.magazine *= 1.1f;
			player.InitBullets();
			player.InitTeleport();
			Debug.Log($"Congrats! Your magazine capacity now = {w.magazine}");
		}*/
		if (kills % 625 == 0)
		{
			player.tpDistance += 2;
			Debug.Log($"Congrats! Your teleport distance now =  {player.tpDistance}!");
		}/*
		if (kills % 750 == 0)
		{
			if (w.reloadTime > 0.8f || w.partialReload)
			{
				w.reloadTime *= 0.9f;
				Debug.Log($"Congrats! Your reload time now = {w.reloadTime}");
			}
		}*/
		if (kills % 1000 == 0)
		{
			if (Camera.main.orthographicSize < 100)
			{
				Camera.main.orthographicSize += 5;
				player.InitBullets();
				player.InitTeleport();
				Debug.Log("Congrats! You've increased your filed of view!");
			}
		}
		if (kills % 1500 == 0)
		{
			player.tpAccum++;
			player.InitTeleport();
			Debug.Log($"Congrats! Your teleport now have {player.tpAccum} bars of energy!");
		}
	}
}
