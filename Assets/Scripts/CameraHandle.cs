using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraHandle : MonoBehaviour
{
	private Player player;

	private void Start()
	{
		player = FindObjectOfType<Player>();
	}

	void Update()
    {
		float d = player.transform.position.x - transform.position.x;
		//if (Mathf.Abs(d) < 1) return;
		transform.Translate(new Vector3(d / 8, 0));       
    }
}
