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

	private float kinematic;
	private float lastDelta;
	private bool isShaking;
	private float shakeForce;

	private void Start()
	{
		player = FindObjectOfType<Player>();
		controller = FindObjectOfType<Controller>();
	}

	public void Shake(float force)
	{
		if (!Settings.screenShake) return;
		isShaking = true;
		shakeForce += force;
		if (shakeForce > maxShake) shakeForce = maxShake;
	}

	void Update()
    {
		kinematic += (player.transform.position.x - transform.position.x) / mass;
		float currentDelta = (kinematic + lastDelta) / 2;
		transform.Translate(new Vector3(currentDelta, 0));
		lastDelta = currentDelta;
		kinematic /= kinematicDecreaseEffect;
		if (isShaking)
		{
			var delta = Random.insideUnitSphere;
			delta.Scale(new Vector3(shakeForce, shakeForce, 0));
			if (Mathf.Sign(transform.position.y) == Mathf.Sign(delta.y)) delta.y *= -1;
			transform.Translate(delta);
			shakeForce /= kinematicDecreaseEffect;
			if (shakeForce < 0.01f)
			{
				isShaking = false;
				shakeForce = 0;
				transform.Translate(new Vector3(0, -transform.position.y));
			}
		}
    }
}
