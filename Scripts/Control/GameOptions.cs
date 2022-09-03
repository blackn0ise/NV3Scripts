using UnityEngine.SceneManagement;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.Audio;
using UnityEngine.InputSystem;

public class GameOptions : MonoBehaviour
{

	[Header("Game settings")]
	[SerializeField] private AudioMixer audioMixer = default;
	[SerializeField] private int defaultMaxHealth = default;
	[SerializeField] private int skipToSpawnIndex = 0;
	[SerializeField] private int customStartingSouls = 0;
	[SerializeField] private int soulMultiplier = 4;
	[SerializeField] private float fastModeFactor = 4;
	[SerializeField] private float demomsgdelay = 735.0f;
	[SerializeField] private float customWalkSpeed = 200;
	[SerializeField] private float customTPCooldown = 5;
	[SerializeField] private bool isTutorialMode = false;
	[SerializeField] private bool godModeEnabled = false;
	[SerializeField] private bool saveHealthEnabled = false;
	[SerializeField] private bool isFastmode = false;
	[SerializeField] private bool isDemomode = false;
	[SerializeField] private bool startWithPowerup = false;
	[SerializeField] private bool startWithAllUpgrades = false;
	[SerializeField] private bool quickStart = false;
	[SerializeField] private bool bigGainsMode = false;
	[SerializeField] private bool debugspawns = false;
	[SerializeField] private bool cameraDebugMode = false;
	[SerializeField] private bool customWalkSpeedEnabled = false;
	[SerializeField] private bool customTPEnabled = false;
	[SerializeField] private bool customSoulsEnabled = false;

	private bool isNewSession = true;
	private bool HasGauntlets = false;
	private SaveData savedata;
	private Type savetype = typeof(SaveData);
	private static GameOptions instance;

	public static GameOptions GetGOPS() { return instance; }
	public SaveData GetSave() { return savedata; }
	public bool GetStartWithPowerup() { return startWithPowerup; }
	public bool GetStartWithAllUpgrades() { return startWithAllUpgrades; }
	public bool GetIsFastmode() { return isFastmode; }
	public bool GetIsDemomode() { return isDemomode; }
	public bool GetIsQuickStart() { return quickStart; }
	public bool GetBigGainsMode() { return bigGainsMode; }
	public bool GetCameraDebugMode() { return cameraDebugMode; }
	public int GetSoulMultiplier() { return soulMultiplier; }
	public int GetMaxHealth() { return defaultMaxHealth; }
	public bool GetSaveHealthEnabled() { return saveHealthEnabled; }
	public bool GetGodModeEnabled() { return godModeEnabled; }
	public bool GetIsNewSession() { return isNewSession; }
	public bool GetHasGauntlets() { return HasGauntlets; }
	public bool GetIsTutorialMode() { return isTutorialMode; }
	public bool GetDebugSpawns() { return debugspawns; }
	public int GetSkipToSpawnIndex() { return skipToSpawnIndex; }
	public int GetDefaultMaxHealth() { return defaultMaxHealth; }
	public bool GetIsFreshSave() { return savedata.highScore == 0; }
	public bool GetCustomWalkSpeedEnabled() { return customWalkSpeedEnabled; }
	public bool GetCustomTPEnabled() { return customTPEnabled; }
	public int GetStartingSouls() { return customStartingSouls; }
	public float GetDemoMsgDelay() { return demomsgdelay; }
	public float GetFastModeFactor() { return fastModeFactor; }
	public float GetCustomWalkSpeed() { return customWalkSpeed; }
	public float GetCustomTPCooldown() { return customTPCooldown; }
	public bool GetCustomSouls() { return customSoulsEnabled; }
	public void SetStartWithPowerup(bool value) { startWithPowerup = value; }
	public void SetStartWithAllUpgrades(bool value) { startWithAllUpgrades = value; }
	public void SetMaxHealth(int value) { defaultMaxHealth = value; }
	public void SetIsNewSession(bool value) { isNewSession = value; }
	public void SetHasGauntlets(bool value) { HasGauntlets = value; }
	public void SetTutorialMode(bool value) { isTutorialMode = value; }

    private void Awake()
    {
		instance = this;
	}
    private void Start()
	{
		//SetFramerate(); //needs an option in the menu
		LoadSaveData();
		DontDestroyOnLoad(gameObject);
	}

	private static void SetFramerate()
	{
		QualitySettings.vSyncCount = 0;  // VSync must be disabled
		Application.targetFrameRate = 60;
	}

	public void LoadSaveData()
	{
		CheckCreateDefault();
		if(saveHealthEnabled)
			defaultMaxHealth = savedata.defaultMaxHealth;
		LoadInputOverrides();
	}

	private void LoadInputOverrides()
	{
		PlayerInput playerInput = FindObjectOfType<PlayerInput>();
		if (playerInput)
		{
			string rebinds = GetSave().inputOverrideJson;
			if (!string.IsNullOrEmpty(rebinds))
				playerInput.actions.LoadBindingOverridesFromJson(rebinds);
		}
		else
			GameLog.Log("Unable to find player input component. Input binding overrides not loaded.");
	}

	private void CheckCreateDefault()
	{
		if (SaveSystem.LoadGame() != null)
		{
			savedata = SaveSystem.LoadGame();
			CheckForDeadFields(savedata);
			LoadVolResAndGQual();
			if (savedata.inputSensitivity == 0)
				savedata.inputSensitivity = 2;
		}

		else
		{
			SaveSystem.SaveGame(0, this, 2, 0, 0, Screen.currentResolution.width, Screen.currentResolution.height, QualitySettings.GetQualityLevel(), 60, "", true);
			GameLog.Log("Created default save.");
			savedata = SaveSystem.LoadGame();
			LoadVolResAndGQual();
		}
	}

	private void CheckForDeadFields(SaveData savedata)
	{
		foreach (System.Reflection.FieldInfo savefield in savetype.GetFields())
		{
			if (savefield.GetValue(savedata) != null)
			{
				//Debug.Log("Field " + savefield.Name + " holds value " + savefield.GetValue(savedata));
			}
			else
			{
				Debug.Log("null field " + savefield.ToString() + " detected");
			}
			ResetNewFields(savedata);
		}
	}

	private static void ResetNewFields(SaveData savedata)
	{
		if (savedata.sfov < 40)
			savedata.sfov = 60; // reset to default if no found value

		if (savedata.inputOverrideJson == null)
			savedata.inputOverrideJson = "";
	}
	
	public void LoadVolResAndGQual()
	{
		if(saveHealthEnabled)
			SetMaxHealth(savedata.defaultMaxHealth);
		audioMixer.SetFloat("MusicVolume", savedata.musicVolume);
		audioMixer.SetFloat("SFXVolume", savedata.soundVolume);
		Screen.SetResolution(savedata.reswidth, savedata.resheight, true);
		QualitySettings.SetQualityLevel(savedata.gqualindex);
	}
}
