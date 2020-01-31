using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class Settings : MonoBehaviour
{
	public static bool screenShake = true;
	public static bool particles = true;
	public static bool damageText = true;
	public static float sfxVolume;
	public static float musicVolume;

	public Toggle ScreenShake;
	public Toggle Particles;
	public Toggle DamageText;
	public Slider SfxVolume;
	public Slider MusicVolume;
	public bool keyScan;

	[SerializeField] private AudioMixer SfxMixer;
	[SerializeField] private AudioMixer MusicMixer;

	public static AudioMixer sfxMixer;
	public static AudioMixer musicMixer;
	private static bool loaded;

	private void Awake()
	{
		if (sfxMixer == null)
			sfxMixer = SfxMixer;
		if (musicMixer == null)
			musicMixer = MusicMixer;
	}

	private void OnEnable()
	{
		ScreenShake.isOn = screenShake;
		Particles.isOn = particles;
		DamageText.isOn = damageText;
		SfxVolume.value = sfxVolume;
		MusicVolume.value = musicVolume;
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Escape) && keyScan) gameObject.SetActive(false);
	}

	public static void LoadSettings()
	{
		if (loaded) return;
		loaded = true;
		screenShake = true;
		particles = true;
		damageText = true;
		sfxVolume = -5;
		musicVolume = -80;
		ApplySettings();
	}

	public static void ApplySettings()
	{
		sfxMixer.SetFloat("Volume", sfxVolume);
		musicMixer.SetFloat("Volume", musicVolume);
	}

	public void SetScreenShake(bool status)
	{
		screenShake = status;
	}

	public void SetParticles(bool status)
	{
		particles = status;
	}

	public void SetDamageText(bool status)
	{
		damageText = status;
	}

	public void SetSFXVolume(Slider value)
	{
		sfxVolume = value.value;
		if (sfxVolume == value.minValue) sfxVolume = -80;
		ApplySettings();
	}

	public void SetMusicVolume(Slider value)
	{
		musicVolume = value.value;
		if (musicVolume == value.minValue) musicVolume = -80;
		ApplySettings();
	}
}
