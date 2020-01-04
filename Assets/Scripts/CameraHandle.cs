using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraHandle : MonoBehaviour
{
	public float kinematicAccelerationRatio;
	public float kineaticDecreaseEffect;

	private Player player;

	private float kinematic;

	private void Start()
	{
		player = FindObjectOfType<Player>();
	}

	void Update()
    {
		kinematic += (player.transform.position.x - transform.position.x) / kinematicAccelerationRatio;
		transform.Translate(new Vector3(kinematic, 0));
		kinematic /= kineaticDecreaseEffect;
    }
}
