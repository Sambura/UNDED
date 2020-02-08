using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lamp : MonoBehaviour
{
	public float updateDelay = 0.1f;
	public float eventRate;
	public int eventCount;
	public AudioClip[] sounds;
	public float pitchDelta;

	private Animator animator;
	private AudioSource audioSource;

    void Start()
    {
		animator = GetComponent<Animator>();
		audioSource = GetComponent<AudioSource>();
		StartCoroutine(Eventer());
    }

	public void PlaySound(int index)
	{
		audioSource.pitch = 1 + Random.Range(-pitchDelta, pitchDelta);
		audioSource.PlayOneShot(sounds[index]);
	}

	public void SetStatus(int status)
	{
		animator.SetBool("isPlaying", status == 1);
	}

    private IEnumerator Eventer()
	{
		while (true)
		{
			yield return new WaitForSeconds(updateDelay);
			if (Random.value < eventRate)
			{
				animator.SetInteger("event", Random.Range(0, eventCount));
				animator.SetTrigger("Flick");
			}
		}
	}
}
