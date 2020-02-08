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
	public static int qualityLevel;
	public static bool fullScreen;
	public static float brightness = 0.3f;
	public static bool arrows;
	private static List<Resolution> resolutions;
	private static string filePath;

	public Toggle ScreenShake;
	public Toggle Particles;
	public Toggle DamageText;
	public Slider SfxVolume;
	public Slider MusicVolume;
	public Slider Brightness;
	public Dropdown QualitySetter;
	public Dropdown ResolutionSetter;
	public Toggle FullScreen;
	public Toggle Arrows;
	public bool keyScan;

	[SerializeField] private AudioMixer SfxMixer;
	[SerializeField] private AudioMixer MusicMixer;

	public static AudioMixer sfxMixer;
	public static AudioMixer musicMixer;
	private static bool loaded;

	private void Awake()
	{
		filePath = Application.persistentDataPath + "/settings.ubf";
		if (sfxMixer == null)
			sfxMixer = SfxMixer;
		if (musicMixer == null)
			musicMixer = MusicMixer;
		//qualityLevel = QualitySettings.GetQualityLevel();
		ResolutionSetter.ClearOptions();
		var res = Screen.resolutions;
		var list = new List<string>();
		foreach (var i in res) {
			list.Add(i.width + " × " + i.height);
		}
		list.Add("Experimental: " + 640 + " × " + 480);
		list.Add("Experimental: " + 640 + " × " + 320);
		list.Add("Experimental: " + 1280 + " × " + 720);
		list.Add("Experimental: " + 1280 + " × " + 640);
		resolutions = new List<Resolution>(Screen.resolutions);
		resolutions.Add(new Resolution()
		{
			width = 640,
			height = 480
		});
		resolutions.Add(new Resolution()
		{
			width = 640,
			height = 320
		});
		resolutions.Add(new Resolution()
		{
			width = 1280,
			height = 720
		});
		resolutions.Add(new Resolution()
		{
			width = 1280,
			height = 640
		});
		ResolutionSetter.AddOptions(list);
		//fullScreen = Screen.fullScreen;
	}

	private void OnEnable()
	{
		if (!loaded) return;
		ScreenShake.isOn = screenShake;
		Particles.isOn = particles;
		DamageText.isOn = damageText;
		SfxVolume.value = sfxVolume;
		MusicVolume.value = musicVolume;
		QualitySetter.value = qualityLevel;
		Arrows.isOn = arrows;
		for (var i = 0; i < Screen.resolutions.Length; i++)
		{
			if (Screen.resolutions[i].width == Screen.currentResolution.width
				&& Screen.resolutions[i].height == Screen.currentResolution.height) ResolutionSetter.value = i;
		}
		FullScreen.isOn = Screen.fullScreen;
		Brightness.value = brightness;
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Escape) && keyScan) gameObject.SetActive(false);
	}

	public static void LoadSettings()
	{
		if (loaded) return;
		loaded = true;
		var file = new System.IO.FileInfo(filePath);
		if (!file.Exists) SaveDefault();

		var stream = new System.IO.FileStream(filePath, System.IO.FileMode.Open);
		var reader = new System.IO.BinaryReader(stream);

		screenShake = reader.ReadBoolean();
		particles = reader.ReadBoolean();
		damageText = reader.ReadBoolean();

		sfxVolume = reader.ReadSingle();
		musicVolume = reader.ReadSingle();

		qualityLevel = reader.ReadInt32();
		QualitySettings.SetQualityLevel(qualityLevel);
		int w = reader.ReadInt32();
		int h = reader.ReadInt32();
		fullScreen = reader.ReadBoolean();

		Screen.SetResolution(w, h, fullScreen);

		brightness = reader.ReadSingle();
		arrows = reader.ReadBoolean();
		stream.Close();

		ApplySettings();
	}

	public static void ApplySettings()
	{
		sfxMixer.SetFloat("Volume", sfxVolume);
		musicMixer.SetFloat("Volume", musicVolume);
		var controller = FindObjectOfType<Controller>();
		if (controller != null)
		{
			controller.globalLight.intensity = brightness;
			controller.UpdateControls();
		}
	}

	public void SetScreenShake(bool status)
	{
		screenShake = status;
		SaveSettings();
	}

	public void SetParticles(bool status)
	{
		particles = status;
		SaveSettings();
	}

	public void SetDamageText(bool status)
	{
		damageText = status;
		SaveSettings();
	}

	public void SetSFXVolume(Slider value)
	{
		sfxVolume = value.value;
		if (sfxVolume == value.minValue) sfxVolume = -80;
		ApplySettings();
		SaveSettings();
	}

	public void SetMusicVolume(Slider value)
	{
		musicVolume = value.value;
		if (musicVolume == value.minValue) musicVolume = -80;
		ApplySettings();
		SaveSettings();
	}

	public void SetQualityLevel(int qualityIndex)
	{
		qualityLevel = qualityIndex;
		QualitySettings.SetQualityLevel(qualityIndex);
		SaveSettings();
	}

	public void SetResolution(int index)
	{
		Screen.SetResolution(resolutions[index].width, resolutions[index].height, fullScreen);
		SaveSettings();
	}

	public void SetFullScreen(bool fs)
	{
		fullScreen = fs;
		Screen.fullScreen = fs;
		SaveSettings();
	}

	public void SetBrightness(float value)
	{
		brightness = value;
		ApplySettings();
		SaveSettings();
	}

	public void SetControls(bool fs)
	{
		arrows = fs;
		ApplySettings();
		SaveSettings();
	}

	private static void SaveSettings()
	{
		var stream = new System.IO.FileStream(filePath, System.IO.FileMode.Create);
		var writer = new System.IO.BinaryWriter(stream);

		writer.Write(screenShake); // screenshake
		writer.Write(particles); // particles
		writer.Write(damageText); // damage text

		writer.Write(sfxVolume); // sfx volume
		writer.Write(musicVolume); // music volume

		writer.Write(qualityLevel); // [Ultra] Quality level
		writer.Write(Screen.width); writer.Write(Screen.height); // Resolution

		writer.Write(fullScreen); // Fullscreen

		writer.Write(brightness); // Brightness
		writer.Write(arrows); // Arrows
		stream.Close();
	}

	public void ResetToDefaults()
	{
		SaveDefault();
		if (loaded)
		{
			loaded = false;
			LoadSettings();
			OnEnable();
		}
	}

	private static void SaveDefault()
	{
		var stream = new System.IO.FileStream(filePath, System.IO.FileMode.Create);
		var writer = new System.IO.BinaryWriter(stream);

		writer.Write(true); // screenshake
		writer.Write(true); // particles
		writer.Write(true); // damage text

		writer.Write(-5f); // sfx volume
		writer.Write(-80f); // music volume

		writer.Write(5); // [Ultra] Quality level
		writer.Write(Screen.width); writer.Write(Screen.height); // Resolution

		writer.Write(false); // Fullscreen

		writer.Write(0.3f); // Brightness

		writer.Write(false); // Arrows
		stream.Close();
	}
}
