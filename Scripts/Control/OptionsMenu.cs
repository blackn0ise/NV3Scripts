using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using System.Text;
using UnityEngine.EventSystems;
using UnityEngine.Rendering.Universal;

public class OptionsMenu : MonoBehaviour
{
	Resolution[] resolutions;
	//TMPro.TMP_Dropdown.OptionData[] healths;
	[SerializeField] private AudioMixer audioMixer = default;
	[SerializeField] private GameObject gamePersistent = default;
	[SerializeField] private TMP_Dropdown resolutionDropDown = default;
	[SerializeField] private TMP_Dropdown graphicsDropDown = default;
	[SerializeField] private Slider msslider = default;
	[SerializeField] private Slider fovslider = default;
	[SerializeField] private Slider mpslider = default;
	[SerializeField] private Slider sfxslider = default;
	[SerializeField] private Toggle ppToggle = default;
	[SerializeField] private TMP_InputField msfield = default;
	[SerializeField] private TextMeshProUGUI fovnumber = default;
	[SerializeField] private TextMeshProUGUI musicnumber = default;
	[SerializeField] private TextMeshProUGUI sfxnumber = default;
	[SerializeField] private AudioSource clickeraso = default;
	[SerializeField] private EventSystem eventSystem = default;
	[SerializeField] private GameObject esFirstSelected = default;
	[SerializeField] private Camera _camera = default;

	private AudioSource mpaso;
	private MusicPlayer mp;
	private GameOptions gops;
	private GameObject gopsgo;

	private void OnEnable()
	{
		eventSystem.SetSelectedGameObject(esFirstSelected);
	}

	private void Start()
	{
		if (!FindObjectOfType<GameOptions>())
		{
			gopsgo = Instantiate(gamePersistent);
			gopsgo.name = gamePersistent.name;
		}
		gopsgo = FindObjectOfType<GameOptions>().gameObject;
		mp = gopsgo.GetComponentInChildren<MusicPlayer>();
		mpaso = mp.gameObject.GetComponent<AudioSource>();
		gops = gopsgo.GetComponent<GameOptions>();
		StartCoroutine("BuildOptions");
	}

	IEnumerator BuildOptions()
	{
		yield return new WaitForFixedUpdate();
		BuildResolution();
		BuildGraphicsQual();
		//BuildHealth();
		BuildMouseSensitivity();
		BuildFOV();
		BuildVolumes();
		BuildPPToggle();
	}

	private void BuildPPToggle()
	{
		ppToggle.isOn = gops.GetSave().postProcessingEnabled;
		_camera.GetUniversalAdditionalCameraData().renderPostProcessing = gops.GetSave().postProcessingEnabled;
	}

	private void BuildVolumes()
	{
		float mvol;
		float svol;
		audioMixer.GetFloat("MusicVolume", out mvol);
		audioMixer.GetFloat("SFXVolume", out svol);
		float mvolaspercent = Mathf.Exp(mvol / 20) * 100;
		float svolaspercent = Mathf.Exp(svol / 20) * 100;
		//music volume
		mpslider.value = Mathf.Exp(mvol / 20);
		musicnumber.text = TruncateString(mvolaspercent.ToString(), 4);
		//sound volume
		sfxslider.value = Mathf.Exp(svol / 20);
		sfxnumber.text = TruncateString(svolaspercent.ToString(), 4);
	}

	private void BuildMouseSensitivity()
	{
		msslider.value = gops.GetSave().inputSensitivity;
		msfield.text = Format2DP(gops.GetSave().inputSensitivity);
	}

	private void BuildFOV()
	{
		fovslider.value = gops.GetSave().sfov;
		fovnumber.text = Format2DP(fovslider.value);
	}

	private string Format2DP(object args)
	{
		var finalstring = new StringBuilder();
		finalstring.Append(String.Format("{0,4:0.00}", args));
		return finalstring.ToString();
	}

	//private void BuildHealth()
	//{
	//	//max health
	//	healths = healthDropdown.options.ToArray();
	//	//set health dropdown value index/ current health value
	//	int currentHealthIndex = 0;
	//	for (int i = 0; i < healths.Length; i++)
	//	{
	//		if (healths[i].text == gops.GetMaxHealth().ToString())
	//		{
	//			currentHealthIndex = i;
	//		}
	//	}
	//	healthDropdown.value = currentHealthIndex;
	//	healthDropdown.RefreshShownValue();
	//}

	private void BuildGraphicsQual()
	{
		graphicsDropDown.value = QualitySettings.GetQualityLevel();
		graphicsDropDown.captionText.SetText(ChooseSelectedGqual(QualitySettings.GetQualityLevel()));
	}

	private void BuildResolution()
	{
		resolutions = Screen.resolutions.Select(resolution => new Resolution { width = resolution.width, height = resolution.height }).Distinct().ToArray();
		resolutionDropDown.ClearOptions();
		List<string> options = new List<string>();
		// set dropdown value index and so; value for resolution
		int currentResoIndex = 0;
		for (int i = 0; i < resolutions.Length; i++)
		{
			string option = resolutions[i].width + "x" + resolutions[i].height;
			options.Add(option);

			if (resolutions[i].width == Screen.currentResolution.width
				&& resolutions[i].height == Screen.currentResolution.height)
			{
				currentResoIndex = i;
			}
		}
		resolutionDropDown.AddOptions(options);
		resolutionDropDown.value = currentResoIndex;
		resolutionDropDown.RefreshShownValue();
	}

	private string ChooseSelectedGqual(int quality)
	{
		return graphicsDropDown.options.ElementAt(quality).text;
	}

	public void DeleteSettings()
	{
		SaveSystem.ClearSave();
		gops.LoadSaveData();
	}

	public void SaveSettings()
	{
		float mvol;
		float svol;
		audioMixer.GetFloat("MusicVolume", out mvol);
		audioMixer.GetFloat("SFXVolume", out svol);
		SaveSystem.SaveGame(gops.GetSave().highScore, gops, gops.GetSave().inputSensitivity, mvol, svol, Screen.currentResolution.width, Screen.currentResolution.height, QualitySettings.GetQualityLevel(), gops.GetSave().sfov, gops.GetSave().inputOverrideJson, gops.GetSave().postProcessingEnabled);
		GameLog.Log("Settings saved.");
		gops.LoadSaveData();

	}

	public static string TruncateString(string input, int limit)
	{
		string substring = input;

		if (substring.Length >= limit)
		{
			substring = "";

			for (int i = 0; i < limit; i++)
			{
				substring += input[i];
			}
		}
		return substring;

	}
	public void SetMusicVolume(float value)
	{
		audioMixer.SetFloat("MusicVolume", Mathf.Log(value) * 20);
		float vol;
		audioMixer.GetFloat("MusicVolume", out vol);
		float volaspercent = mpslider.value * 100;
		musicnumber.text = TruncateString(volaspercent.ToString(), 4);
		//DebugVolumeValues(value, vol);
	}

	private static void DebugVolumeValues(float value, float vol)
	{
		Debug.Log("value = " + value);
		Debug.Log("Mathf.Log(value) = " + Mathf.Log(value));
		Debug.Log("Mathf.Log(value) * 20 = " + Mathf.Log(value) * 20);
		Debug.Log("vol = " + vol);
		Debug.Log("Mathf.Exp(1) = " + Mathf.Exp(1));
		Debug.Log("Mathf.Log(vol) = " + Mathf.Log(vol));
		Debug.Log("Mathf.Exp(vol) = " + Mathf.Exp(vol));
		Debug.Log("Mathf.Exp(vol*20) = " + Mathf.Exp(vol * 20));
		Debug.Log("Mathf.Exp(vol/20) = " + Mathf.Exp(vol / 20));
	}

	public void SetSFXVolume(float value)
	{
		audioMixer.SetFloat("SFXVolume", Mathf.Log(value) * 20);
		float vol;
		audioMixer.GetFloat("SFXVolume", out vol);
		float volaspercent = sfxslider.value * 100;
		sfxnumber.text = TruncateString(volaspercent.ToString(), 4);
	}
	public void SetResolution(int resIndex)
	{
		Resolution resolution = resolutions[resIndex];
		Screen.SetResolution(resolution.width, resolution.height, true);
	}
	public void SetGraphicsQuality(int qualityindex)
	{
		QualitySettings.SetQualityLevel(qualityindex);
	}

	//public void SetMaxHealth(int healthindex)
	//{
	//	gops.SetMaxHealth(ExtractIntFromStr(healths[healthindex].text));
	//	if (gops.GetMaxHealth() > 250)
	//		GameObject.Find("HealthTip").GetComponent<TextMeshProUGUI>().text = "(Note: High Score will only track for 250 or less Max Health.)";
	//	else
	//		GameObject.Find("HealthTip").GetComponent<TextMeshProUGUI>().text = "";
	//}

	public void SetMouseSensitivity(float sindex)
	{
		gops.GetSave().inputSensitivity = sindex;
		msfield.text = Format2DP(msslider.value);
	}

	public void SetMouseSensitivity(string sindex)
	{
		float value = float.Parse(sindex);
		gops.GetSave().inputSensitivity = value;
		msfield.text = Format2DP(float.Parse(msfield.text));
	}

	public void SetFOV(float sindex)
	{
		gops.GetSave().sfov = sindex;
		fovnumber.text = Format2DP(fovslider.value);
	}

	public void SetPostProcessingEnabled(bool value)
	{
		gops.GetSave().postProcessingEnabled = value;
		ppToggle.isOn = gops.GetSave().postProcessingEnabled;
		_camera.GetUniversalAdditionalCameraData().renderPostProcessing = gops.GetSave().postProcessingEnabled;
	}

	public static int ExtractIntFromStr(string str)
	{
		string a = str;
		string b = string.Empty;
		int val = 0;

		for (int i = 0; i < a.Length; i++)
		{
			if (char.IsDigit(a[i]))
				b += a[i];
		}

		if (b.Length > 0)
			val = int.Parse(b);
		return val;
	}

	public void PlayClickSound()
	{
		clickeraso.Play();
	}
}
