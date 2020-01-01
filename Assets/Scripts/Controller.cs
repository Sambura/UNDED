using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour
{
	public GameObject[] enemy;
	public float spawnX;
	public float spawnY;
	public float spawnReductor;

	private int kills;
	private Player player;

	private void Start()
	{
		player = FindObjectOfType<Player>();
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
		}
		if (kills % 75 == 0)
		{
			for (int i = 0; i < w.shotRate.Length; i++)
				w.shotRate[i] *= 1.03f;
			Debug.Log("Congrats! Your shot rate have been increased");
		}
		if (kills % 100 == 0)
		{
			player.healthPoints *= 1.1f;
			Debug.Log($"Congrats! Your maxHealth now = {player.healthPoints}");
		}
		if (kills % 250 == 0)
		{
			player.regeneration *= 1.15f;
			Debug.Log($"Congrats! Your regeneration now = {player.regeneration}");
		}
		if (kills % 500 == 0)
		{
			w.magazine *= 1.1f;
			player.InitBullets();
			Debug.Log($"Congrats! Your magazine capacity now = {w.magazine}");
		}
		if (kills % 750 == 0)
		{
			if (w.reloadTime > 0.8f || w.partialReload)
			{
				w.reloadTime *= 0.9f;
				Debug.Log($"Congrats! Your reload time now = {w.reloadTime}");
			}
		}
		if (kills % 1000 == 0)
		{
			if (Camera.main.orthographicSize < 100)
			{
				Camera.main.orthographicSize += 5;
				Debug.Log("Congrats! You've increased your filed of view!");
			}
		}
	}
}
