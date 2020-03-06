using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraHandle : MonoBehaviour
{
	public float mass;
	public float kinematicDecreaseEffect;
	public float maxShake;

	private Player player;
	private Controller controller;
	private new Camera camera;

	private float kinematic;
	private float lastDelta;
	private bool isShaking;
	private float magnitude;

	private void Start()
	{
		player = FindObjectOfType<Player>();
		controller = FindObjectOfType<Controller>();
		camera = Camera.main;
	}

	public void Shake(float force)
	{
		if (!Settings.screenShake) return;
		isShaking = true;
		magnitude += force;
		if (magnitude > maxShake) magnitude = maxShake;
	}

	void FixedUpdate()
    {
		kinematic += (player.transform.position.x - transform.position.x) / mass;
		float currentDelta = (kinematic + lastDelta) / 2;
		transform.Translate(new Vector3(currentDelta, 0));
		lastDelta = currentDelta;
		kinematic /= kinematicDecreaseEffect;
		if (isShaking)
		{
			camera.transform.localPosition = Random.insideUnitCircle * magnitude;
			magnitude /= kinematicDecreaseEffect;
			if (magnitude < 0.01f)
			{
				isShaking = false;
				magnitude = 0;
				camera.transform.localPosition = Vector3.zero;
			}
		}
    }
}
