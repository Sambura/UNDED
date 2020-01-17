using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PhyscisController : MonoBehaviour
{
	public GameObject[] grenade;
	public GameObject trace;
	public Text gravityT;
	public Text forceT;
	public Text angleT;
	public Text massT;
	public Text gravityScaleT;

	private float gravity = -9.81f;
	private Vector3 grip;
	private Vector3 pos;
	private new Camera camera;
	private float angle;
	private float force;
	private int index;
	private LineRenderer trail;
	private float mass;
	private float gravityScale;

	private float Sqr(float n)
	{
		return n * n;
	}

	private void Start()
	{
		camera = Camera.main;
		gravityT.text = $"Current gravity g = {gravity}";
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Alpha1)) index = 0;
		if (Input.GetKeyDown(KeyCode.Alpha2)) index = 1;
		if (Input.GetKeyDown(KeyCode.Alpha3)) index = 2;
		if (Input.GetKeyDown(KeyCode.Alpha4)) index = 3;
		massT.text = $"Mass: {grenade[index].GetComponent<Rigidbody2D>().mass}";
		gravityScaleT.text = $"Gravity scale: {grenade[index].GetComponent<Rigidbody2D>().gravityScale}";
		mass = grenade[index].GetComponent<Rigidbody2D>().mass;
		gravityScale = grenade[index].GetComponent<Rigidbody2D>().gravityScale;
	}

	private void OnMouseDown()
	{
		grip = camera.ScreenToWorldPoint(Input.mousePosition);
		grip.z = 0;
		if (trail != null) Destroy(trail.gameObject);
		trail = Instantiate(trace).GetComponent<LineRenderer>();
		trail.positionCount = 100;
	}

	private void OnMouseDrag()
	{
		pos = camera.ScreenToWorldPoint(Input.mousePosition);
		pos.z = 0;
		force = Mathf.Sqrt(Sqr(grip.x - pos.x) + Sqr(grip.y - pos.y));
		forceT.text = $"Current force = {System.Math.Round(force, 2)}";
		angle = Mathf.Atan2(grip.y - pos.y, grip.x - pos.x);
		angleT.text = $"Current angle = {System.Math.Round(angle * Mathf.Rad2Deg, 1)}";

		float dx = pos.x;
		float dy = pos.y;
		float delta = 0.1f;
		float xV = force * Mathf.Cos(angle) / mass;
		float yV = force * Mathf.Sin(angle) / mass;
		for (int i = 0; i < 100; i++)
		{
			trail.SetPosition(i, new Vector3(dx, dy));
			dx += xV * delta;
			dy += yV * delta;
			yV += gravity * delta * gravityScale;
		}

	}

	private void OnMouseUp()
	{
		var last = Instantiate(grenade[index], pos, Quaternion.identity);
		last.GetComponent<Rigidbody2D>().AddForce(
			new Vector2(force * Mathf.Cos(angle), force * Mathf.Sin(angle)), ForceMode2D.Impulse);
		last.GetComponent<Rigidbody2D>().AddTorque(-force * Mathf.Cos(angle), ForceMode2D.Impulse);
	}
}
