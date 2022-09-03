using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
	[Header("Components")]
	[SerializeField] private Resurrector resurrector = default;
	[SerializeField] private StatLibrary statl = default;
	[SerializeField] private SpawnController sc = default;
	[SerializeField] private AudioMixer audioMixer = default;
	[SerializeField] private PlayerInput playerInput = default;
	[SerializeField] private ParticleSystem SWIndicator = default;
	[SerializeField] private GameManagerScript gameManagerScript = default;

	[Header("UI TMPs")]
	[SerializeField] private TextMeshProUGUI TMPSOULS = default;
	[SerializeField] private TextMeshProUGUI TMPSOULSSPENT = default;
	[SerializeField] private TextMeshProUGUI TMPWeaponBank = default;
	[SerializeField] private TextMeshProUGUI TMPSpiritBank = default;
	[SerializeField] private TextMeshProUGUI TMPWeaponSpent = default;
	[SerializeField] private TextMeshProUGUI TMPSpiritSpent = default;
	[SerializeField] private TextMeshProUGUI TMPWeaponSelected = default;
	[SerializeField] private TextMeshProUGUI TMPOFFERING = default;
	[SerializeField] private TextMeshProUGUI TMPTUI = default;
	[SerializeField] private TextMeshProUGUI TMPHS = default;
	[SerializeField] private TextMeshProUGUI TMPHealthNumber = default;
	[SerializeField] private TextMeshProUGUI TMPHealNumber = default;
	[SerializeField] private TextMeshProUGUI TMPSummonNumber = default;
	[SerializeField] private TextMeshProUGUI TMPAmmoCount = default;
	[SerializeField] private TextMeshProUGUI TMPUIHealth_ = default;
	[SerializeField] private TextMeshProUGUI TMPUISpells_ = default;
	[SerializeField] private TextMeshProUGUI TMPTutorialPanelText = default;
	[SerializeField] private TextMeshProUGUI TMPDemoUnavailable = default;
	[Header("Tip TMPs")]
	[SerializeField] private TextMeshProUGUI TMPSummonTip = default;
	[SerializeField] private TextMeshProUGUI TMPHealTip = default;
	[SerializeField] private TextMeshProUGUI TMPAMMOTIP = default;
	[SerializeField] private TextMeshProUGUI TMPSWTIP = default;
	[SerializeField] private TextMeshProUGUI TMPWBankTip = default;
	[SerializeField] private TextMeshProUGUI TMPSBankTip = default;
	[SerializeField] private TextMeshProUGUI TMPDashTip = default;
	[SerializeField] private TextMeshProUGUI TMPTeleportTip = default;

	[Header("Bench TMPs")]
	[SerializeField] private TextMeshPro TMPWBank = default;
	[SerializeField] private TextMeshPro TMPSBank = default;
	[SerializeField] private TextMeshPro TMPTime = default;
	[SerializeField] private TextMeshPro TMPVoidCost = default;
	[SerializeField] private TextMeshPro TMPDevCost = default;

	[Header("Gameobjects")]
	[SerializeField] private GameObject TMPTutorialPanel = default;
	[SerializeField] private GameObject healthback = default;
	[SerializeField] private GameObject healback = default;
	[SerializeField] private GameObject summonback = default;
	[SerializeField] private GameObject dashback = default;
	[SerializeField] private GameObject teleportback = default;
	[SerializeField] private GameObject crosshair = default;

	[Header("Bench Icons")]
	[SerializeField] private GameObject PuIcon = default;
	[SerializeField] private GameObject PaIcon = default;
	[SerializeField] private GameObject WpIcon = default;
	[SerializeField] private GameObject[] OwnershipPipBacks = default;
	[SerializeField] private GameObject[] OwnershipPips = default;

	[Header("Bar and border images")]
	[SerializeField] private Image HealBar = default;
	[SerializeField] private Image SummonBar = default;
	[SerializeField] private Image healthbar = default;
	[SerializeField] private Image DashBar = default;
	[SerializeField] private Image TeleportBar = default;
	[SerializeField] private Image healthborder = default;
	[Header("Weapon UI images")]
	[SerializeField] private Image UIGauntlets = default;
	[SerializeField] private Image UIBonebag = default;
	[SerializeField] private Image UISnubnose = default;
	[SerializeField] private Image UIShotgun = default;
	[SerializeField] private Image UICrucifier = default;
	[SerializeField] private Image UIReaver = default;
	[SerializeField] private Image UIDevourer = default;
	[SerializeField] private Image UIVoidCannon = default;
	[SerializeField] private Image UIGodhand = default;

	[Header("Superweapon indicator Colors")]
	[SerializeField] private Color SWReady = Color.green;
	[SerializeField] private Color SWNotReady = Color.red;
	[SerializeField] private Color SWDef = Color.white;

	[Header("Paremeters")]
	[SerializeField] private float SWIParticleLifeMax = 0.25f;

	private Player player;
	private PlayerUnit playerunit;
	private PlayerMovement pm;
	private GameOptions gops;
	private GameObject currentweapon;
	private bool voidowned = false;
	private bool devowned = false;
	private bool UIon = true;
	private bool ammoUIEnabled = false;
	private bool cooldownUIEnabled = false;
	private bool toobrokeUIEnabled = false;
	private bool paiconUIEnabled = false;
	private bool puiconUIEnabled = false;
	private bool wpiconUIEnabled = false;
	private List<Image> weaponImageList = default;

	public void SetVoidOwned(bool value) { voidowned = value; }
	public void SetDevOwned(bool value) { devowned = value; }
	public bool GetUIOn() { return UIon; }
	public void SetUIOn(bool value) { UIon = value; crosshair.SetActive(value); }
	public bool GetAmmoUIEnabled() { return ammoUIEnabled; }
	public void SetAmmoUIEnabled(bool value) { ammoUIEnabled = value; }
	public bool GetCDUIEnabled() { return cooldownUIEnabled; }
	public void SetCDUIEnabled(bool value) { cooldownUIEnabled = value; }
	public bool GetBrokeUIEnabled() { return toobrokeUIEnabled; }
	public void SetBrokeUIEnabled(bool value) { toobrokeUIEnabled = value; }

	public void SetCrosshair(GameObject value) { crosshair = value; }
	public GameObject GetCrosshair() { return crosshair; }

	public bool GetPuIconEnabled() { return puiconUIEnabled; }
	public bool GetPaIconEnabled() { return paiconUIEnabled; }
	public bool GetWpIconEnabled() { return wpiconUIEnabled; }
	public void SetPuIconEnabled(bool value) { puiconUIEnabled = value; }
	public void SetPaIconEnabled(bool value) { paiconUIEnabled = value; }
	public void SetWpIconEnabled(bool value) { wpiconUIEnabled = value; }

	public GameObject GetTMPTutorialPanel() { return TMPTutorialPanel; }

	public void SetAmmoString(string value) { TMPAmmoCount.text = value; }

	private void Start()
	{
		gops = GameOptions.GetGOPS();
		player = gameManagerScript.GetPlayer().GetComponent<Player>();
		playerunit = gameManagerScript.GetPlayer().GetComponent<PlayerUnit>();
		pm = gameManagerScript.GetPlayer().GetComponent<PlayerMovement>();
		statl = gameManagerScript.GetStatl();
		sc = gameManagerScript.GetSpawnController();
		ClearUIStats();
		ClearOtherTexts();
		SetTutorialText();
		InitialiseWeaponIndicator();
	}

	private void InitialiseWeaponIndicator()
	{
		weaponImageList = new List<Image>();
		weaponImageList.AddRange(new Image[] { UIGauntlets, UIBonebag, UIShotgun, UISnubnose, UICrucifier, UIReaver, UIDevourer, UIVoidCannon, UIGodhand });
		foreach (Image image in weaponImageList)
		{
			Color tempcolor = Color.black;
			image.color = tempcolor;
		}
		if (gops.GetIsDemomode())
			TMPDemoUnavailable.text = "Not available in demo";
	}

	private void ClearOtherTexts()
	{
		TMPHS.text = "";
		TMPWBank.text = "";
		TMPSBank.text = "";
		TMPTime.text = "";
		TMPVoidCost.text = "";
		TMPDevCost.text = "";
		TMPOFFERING.text = "";
		TMPTutorialPanelText.text = "";
	}

	private void SetTutorialText()
	{
		if (gops.GetIsTutorialMode())
		{
			TMPTutorialPanelText.text = "Step onto the altar to begin.";
		}
	}

	private void Update()
	{
		if (!UIon && SWIndicator.isPlaying)
			SWIndicator.Stop();
		TMPWBank.text = statl.GetWeaponBank().ToString();
		TMPSBank.text = statl.GetSpiritBank().ToString();
		TMPAmmoCount.gameObject.SetActive(UIon);
		if (!player.gameObject.GetComponent<PlayerUnit>().IsDead() && sc.GetGameActive())
		{
			TMPTime.text = MakeRoman(Mathf.FloorToInt(Altar.GetTimeSinceStart()));
		}
		if (UIon && !player.gameObject.GetComponent<PlayerUnit>().IsDead() && sc.GetGameActive())
		{
			DisplayUIStats();
			SetCrosshairActive();
			UpdateSWIndicator();
			DisplayAmmoUI();
			DisplayBenchIcons();
		}
		else if (!player.gameObject.GetComponent<PlayerUnit>().IsDead() && !UIon)
		{
			ClearUIStats();
			HideBenchIcons();
		}

		HandleStatBars();
		DisplaySWCosts();
		UpdateFadingUIs();
		DisplayOwnershipBacks();
	}

	private void DisplayOwnershipBacks()
	{
		foreach (GameObject go in OwnershipPipBacks)
		{
			go.SetActive(UIon && sc.GetGameActive());
		}
	}

	public void HandleTutorialPips()
	{
		foreach (GameObject go in OwnershipPips)
		{
			if (go.name == "WeaponOwned1Pip" || go.name == "WeaponOwned8Pip")
				continue;
			else
				go.SetActive(true);
		}
	}

	public void HandleStatBars()
	{
		Color newcolor = healthborder.color;
		newcolor.a = (1.0f - ((float)playerunit.GetHealth() / (float)playerunit.GetMaxHealth()));
		healthborder.color = newcolor;
		healthbar.fillAmount = (float)playerunit.GetHealth() / (float)playerunit.GetMaxHealth();

		float healpercent = 0.0f;
		if (Resurrector.GetHealthCost() > 0)
			healpercent = (float)statl.GetSouls() / (float)Resurrector.GetHealthCost();
		float summonpercent = 0.0f;
		if (resurrector.GetResCost() > 0)
			summonpercent = (float)statl.GetSouls() / (float)resurrector.GetResCost();
		if (healpercent > 1)
			healpercent = 1;
		if (summonpercent > 1)
			summonpercent = 1;
		float dashpercent = 0.0f;
		if (pm.GetDashCDTimer() > 0)
			dashpercent = (1.0f - (float)pm.GetDashCDTimer() / (float)pm.GetDashCooldown());
		else
			dashpercent = 1;
		float telepercent = 0.0f;
		if (pm.GetTeleCDTimer() > 0)
			telepercent = (1.0f - (float)pm.GetTeleCDTimer() / (float)pm.GetTeleCooldown());
		else
			telepercent = 1;

		HealBar.fillAmount = healpercent;
		SummonBar.fillAmount = summonpercent;
		DashBar.fillAmount = dashpercent;
		TeleportBar.fillAmount = telepercent;

		healthback.SetActive(UIon && sc.GetGameActive());
		healback.SetActive(UIon && sc.GetGameActive());
		summonback.SetActive(UIon && sc.GetGameActive());
		dashback.SetActive(UIon && sc.GetGameActive());
		teleportback.SetActive(UIon && sc.GetGameActive() && pm.GetTeleportEnabled());
		TMPUIHealth_.gameObject.SetActive(UIon && sc.GetGameActive());
		TMPUISpells_.gameObject.SetActive(UIon && sc.GetGameActive());
	}

	private void HideBenchIcons()
	{
		if (PaIcon.activeInHierarchy == true || PuIcon.activeInHierarchy == true || WpIcon.activeInHierarchy == true)
		{
			PaIcon.SetActive(false);
			PuIcon.SetActive(false);
			WpIcon.SetActive(false);
			Debug.Log("done");
		}
	}

	private void DisplayBenchIcons()
	{
		if (paiconUIEnabled)
			PaIcon.SetActive(true);
		else
			PaIcon.SetActive(false);

		if (puiconUIEnabled)
			PuIcon.SetActive(true);
		else
			PuIcon.SetActive(false);

		if (wpiconUIEnabled)
			WpIcon.SetActive(true);
		else
			WpIcon.SetActive(false);
	}

	private void UpdateFadingUIs()
	{
		var tempscolor = TMPSOULSSPENT.color;
		var tmpwscolor = TMPWeaponSpent.color;
		var tmpsscolor = TMPSpiritSpent.color;
		var tempocolor = TMPOFFERING.color;
		var tempwselcolor = TMPWeaponSelected.color;
		var tempducolor = TMPDemoUnavailable.color;
		if (tempscolor.a >= 0)
		{
			tempscolor.a -= Time.deltaTime * 0.2f;
			TMPSOULSSPENT.color = tempscolor;
		}
		if (tmpwscolor.a >= 0)
		{
			tmpwscolor.a -= Time.deltaTime * 0.2f;
			TMPWeaponSpent.color = tmpwscolor;
		}
		if (tmpsscolor.a >= 0)
		{
			tmpsscolor.a -= Time.deltaTime * 0.2f;
			TMPSpiritSpent.color = tmpsscolor;
		}
		if (tempwselcolor.a >= 0)
		{
			tempwselcolor.a -= Time.deltaTime * 0.4f;
			TMPWeaponSelected.color = tempwselcolor;
		}
		if (tempocolor.a >= 0)
		{
			tempocolor.a -= Time.deltaTime * 0.3f;
			TMPOFFERING.color = tempocolor;
		}
		foreach (Image image in weaponImageList)
		{
			if (image.color.a >= 0)
			{
				Color tempcolor = image.color;
				tempcolor.a -= Time.deltaTime * 0.4f;
				image.color = tempcolor;
			}
		}
		if (tempducolor.a >= 0)
		{
			tempducolor.a -= Time.deltaTime * 0.4f;
			TMPDemoUnavailable.color = tempducolor;
		}


	}

	private void DisplayAmmoUI()
	{
		if (TMPAmmoCount.color.a != 1)
		{
			var tempcolor = TMPAmmoCount.color;
			tempcolor.a = 1;
			TMPAmmoCount.color = tempcolor;
		}

		if (ammoUIEnabled)
		{
			string key1 = GetKeyString("NextWeapon");
			string key2 = GetKeyString("CycleWeapons");
			key1 = key1.ToLower();
			key2 = key2.ToLower();
			TMPAMMOTIP.text = $"No ammo. Refill(swap) with [{key1}] or [{key2}].";
		}
		else if (cooldownUIEnabled)
		{
			TMPAMMOTIP.text = "Weapon cooldown";
		}
		else if (toobrokeUIEnabled)
		{
			HandleInsufficients();
		}
		else
		{
			TMPAMMOTIP.text = "";
		}
	}

	private void HandleInsufficients()
	{
		if (currentweapon)
		{
			switch (currentweapon.name)
			{
				case "Devourer":
					TMPAMMOTIP.text = "Insufficient spirit bank";
					break;
				case "Void Cannon":
					TMPAMMOTIP.text = "Insufficient weapon bank";
					break;
				default:
					TMPAMMOTIP.text = "Insufficient bank souls";
					break;
			}
		}
	}

	private void UpdateSWIndicator()
	{
		float UIpercent = SWIParticleLifeMax;
		float fillpercent;
		var main = SWIndicator.main;
		currentweapon = player.GetCurrentWeapon();
		if (currentweapon)
			switch (currentweapon.name)
			{
				case "Devourer":
					fillpercent = Mathf.Clamp((float)statl.GetSouls() / Devourer.GetDevCost(), 0, 1);
					UIpercent *= fillpercent;
					ActivateAndColorSWI(UIpercent, fillpercent, main);
					UpdateSoulsNeeded("Devourer");
					break;
				case "Void Cannon":
					fillpercent = Mathf.Clamp((float)statl.GetSouls() / VoidCannon.GetVoidCost(), 0, 1);
					UIpercent *= fillpercent;
					ActivateAndColorSWI(UIpercent, fillpercent, main);
					UpdateSoulsNeeded("Void Cannon");
					break;
				default:
					DeactivateSWI(main);
					UpdateSoulsNeeded("None");
					break;
			}
	}

	private void UpdateSoulsNeeded(string weapon)
	{
		switch (weapon)
		{
			case "Devourer":
				TMPSWTIP.text = $"Devourer souls cost: {Devourer.GetDevCost()}";
				break;
			case "Void Cannon":
				TMPSWTIP.text = $"Void Cannon souls cost: {VoidCannon.GetVoidCost()}";
				break;
			default:
				TMPSWTIP.text = "";
				break;
		}
	}

	private void DeactivateSWI(ParticleSystem.MainModule main)
	{
		main.startColor = SWDef;
		SetBrokeUIEnabled(false);
		if (SWIndicator.isPlaying)
			SWIndicator.Stop();
	}

	private void ActivateAndColorSWI(float UIpercent, float fillpercent, ParticleSystem.MainModule main)
	{
		if (!SWIndicator.isPlaying)
			SWIndicator.Play();
		main.startLifetime = UIpercent;
		if (fillpercent == 1)
		{
			main.startColor = SWReady;
			SetBrokeUIEnabled(false);
		}
		else
		{
			main.startColor = SWNotReady;
			SetBrokeUIEnabled(true);
		}
	}

	private void DebugSWIV(float UIpercent, float fillpercent)
	{
		Debug.Log("(float)statl.GetDetrizideBank() / VoidCannon.GetVoidCost() = " + (float)statl.GetSouls() / VoidCannon.GetVoidCost());
		Debug.Log("fillpercent = " + fillpercent);
		Debug.Log("UIpercent = " + UIpercent);
	}

	private void DebugSWID(float UIpercent, float fillpercent)
	{
		Debug.Log("(float)statl.GetDetrizideBank() / Devourer.GetDevCost() = " + (float)statl.GetSouls() / Devourer.GetDevCost());
		Debug.Log("fillpercent = " + fillpercent);
		Debug.Log("UIpercent = " + UIpercent);
	}

	private void SetCrosshairActive()
	{
		crosshair.SetActive(UIon);
	}

	private void DisplaySWCosts()
	{
		if (voidowned)
			TMPVoidCost.text = "Void Cannon cost : " + VoidCannon.GetVoidCost().ToString();
		if (devowned)
			TMPDevCost.text = "Devourer cost : " + Devourer.GetDevCost().ToString();
	}

	internal void DisplaySoulCost(int cost)
	{
		if (UIon)
		{
			TMPSOULSSPENT.text = "-" + cost.ToString();
			var tempcolor = Color.red;
			TMPSOULSSPENT.color = tempcolor;
		}
	}

	internal void DisplayBankCost(int cost, string type)
	{
		if (UIon)
		{
			if (type == "Weapon")
			{
				TMPWeaponSpent.text = "-" + cost.ToString();
				TMPWeaponSpent.color = Color.red;
			}
			else if (type == "Spirit")
			{
				TMPSpiritSpent.text = "-" + cost.ToString();
				TMPSpiritSpent.color = Color.red;
			}
		}
	}

	internal void DisplayBankAdd(int amount, string type)
	{
		if (UIon)
		{
			if (type == "Weapon")
			{
				TMPWeaponSpent.text = "+" + amount.ToString();
				TMPWeaponSpent.color = Color.green;
			}
			else if (type == "Spirit")
			{
				TMPSpiritSpent.text = "+" + amount.ToString();
				TMPSpiritSpent.color = Color.green;
			}
		}
	}

	internal void DisplayOffering()
	{
		if (UIon)
		{
			var tempcolor = TMPOFFERING.color;
			tempcolor.a = 1;
			TMPOFFERING.color = tempcolor;
			TMPOFFERING.text = "Offering Accepted!";
		}
	}

	internal void DisplayWeaponSelected(string nextWeapon)
	{
		if (UIon)
		{
			var tempcolor = TMPWeaponSelected.color;
			tempcolor.a = 1;
			TMPWeaponSelected.color = tempcolor;
			TMPWeaponSelected.text = $"Weapon Selected: {GetWeaponName(nextWeapon)} {GetWeaponKey(nextWeapon)} ";
			if (gops.GetIsDemomode())
			{
				var tempdcolor = TMPDemoUnavailable.color;
				tempdcolor.a = 1;
				TMPDemoUnavailable.color = tempdcolor;
			}
			ChooseHighlight(nextWeapon);
		}
	}

	private void ChooseHighlight(string nextWeapon)
	{
		switch (nextWeapon)
		{
			case "Gauntlets":
				HighlightSelectedWeaponImage(UIGauntlets, nextWeapon);
				break;
			case "BoneBag":
				HighlightSelectedWeaponImage(UIBonebag, nextWeapon);
				break;
			case "Shotgun":
				HighlightSelectedWeaponImage(UIShotgun, nextWeapon);
				break;
			case "Banshee":
				HighlightSelectedWeaponImage(UIShotgun, nextWeapon);
				break;
			case "Sledgehammer":
				HighlightSelectedWeaponImage(UIShotgun, nextWeapon);
				break;
			case "Snubnose":
				HighlightSelectedWeaponImage(UISnubnose, nextWeapon);
				break;
			case "Hand Cannon":
				HighlightSelectedWeaponImage(UISnubnose, nextWeapon);
				break;
			case "Penance":
				HighlightSelectedWeaponImage(UISnubnose, nextWeapon);
				break;
			case "Crucifier":
				HighlightSelectedWeaponImage(UICrucifier, nextWeapon);
				break;
			case "Triptikoss":
				HighlightSelectedWeaponImage(UICrucifier, nextWeapon);
				break;
			case "Longinus":
				HighlightSelectedWeaponImage(UICrucifier, nextWeapon);
				break;
			case "Reaver":
				HighlightSelectedWeaponImage(UIReaver, nextWeapon);
				break;
			case "Goregun":
				HighlightSelectedWeaponImage(UIReaver, nextWeapon);
				break;
			case "Judgement":
				HighlightSelectedWeaponImage(UIReaver, nextWeapon);
				break;
			case "Devourer":
				HighlightSelectedWeaponImage(UIDevourer, nextWeapon);
				break;
			case "Void Cannon":
				HighlightSelectedWeaponImage(UIVoidCannon, nextWeapon);
				break;
			case "Godhand":
				HighlightSelectedWeaponImage(UIGodhand, nextWeapon);
				break;
		}
	}

	private void HighlightSelectedWeaponImage(Image selectedweapon, string nextweapon)
	{
		foreach (Image image in weaponImageList)
		{
			var tempcolor = image.color;
			tempcolor.a = 1;

			if (image != selectedweapon)
			{
				string formattedname = image.name;
				formattedname = formattedname.Remove(0, 2);
				bool weaponowned = player.CheckWeaponOwned(formattedname);

				if (weaponowned)
				{
					tempcolor = Color.grey;
					image.color = tempcolor;
				}
				else
				{
					tempcolor = Color.black;
					image.color = tempcolor;
				}
			}

			else
			{
				tempcolor = Color.white;
				image.color = tempcolor;
			}


		}
	}

	private string GetWeaponName(string nextWeapon)
	{
		switch (nextWeapon)
		{
			case "BoneBag":
				return "Bone Bag";
			case "Snubnose":
				return "Leathian Snubnose";
			case "Hand Cannon":
				return "Hand Cannon";
			case "Penance":
				return "Penance";
			case "Shotgun":
				return "Bone Shotgun";
			case "Banshee":
				return "Banshee";
			case "Sledgehammer":
				return "Sledgehammer";
			case "Gauntlets":
				return "Wraith Gauntlets";
			case "Reaver":
				return "Reaver";
			case "Goregun":
				return "Goregun";
			case "Judgement":
				return "Judgement";
			case "Devourer":
				return "Devourer";
			case "Crucifier":
				return "Crucifier";
			case "Longinus":
				return "Longinus";
			case "Triptikoss":
				return "Triptikoss";
			case "Void Cannon":
				return "Void Cannon";
			case "Godhand":
				return "Godhand";
		}
		return "None";
	}

	private string GetWeaponKey(string nextWeapon)
	{
		GetKeyString("NextWeapon");
		switch (nextWeapon)
		{
			case "Gauntlets":
				return GetKeyString("Weapon1") == "" ? "" : $"[{GetKeyString("Weapon1")}]";
			case "BoneBag":
				return GetKeyString("Weapon2") == "" ? "" : $"[{GetKeyString("Weapon2")}]";
			case "Snubnose":
				return GetKeyString("Weapon2") == "" ? "" : $"[{GetKeyString("Weapon2")}]";
			case "Penance":
				return GetKeyString("Weapon2") == "" ? "" : $"[{GetKeyString("Weapon2")}]";
			case "Hand Cannon":
				return GetKeyString("Weapon2") == "" ? "" : $"[{GetKeyString("Weapon2")}]";
			case "Shotgun":
				return GetKeyString("Weapon3") == "" ? "" : $"[{GetKeyString("Weapon3")}]";
			case "Banshee":
				return GetKeyString("Weapon3") == "" ? "" : $"[{GetKeyString("Weapon3")}]";
			case "Sledgehammer":
				return GetKeyString("Weapon3") == "" ? "" : $"[{GetKeyString("Weapon3")}]";
			case "Crucifier":
				return GetKeyString("Weapon4") == "" ? "" : $"[{GetKeyString("Weapon4")}]";
			case "Triptikoss":
				return GetKeyString("Weapon4") == "" ? "" : $"[{GetKeyString("Weapon1")}]";
			case "Longinus":
				return GetKeyString("Weapon4") == "" ? "" : $"[{GetKeyString("Weapon4")}]";
			case "Reaver":
				return GetKeyString("Weapon5") == "" ? "" : $"[{GetKeyString("Weapon5")}]";
			case "Judgement":
				return GetKeyString("Weapon5") == "" ? "" : $"[{GetKeyString("Weapon5")}]";
			case "Goregun":
				return GetKeyString("Weapon5") == "" ? "" : $"[{GetKeyString("Weapon5")}]";
			case "Devourer":
				return GetKeyString("Weapon6") == "" ? "" : $"[{GetKeyString("Weapon6")}]";
			case "Void Cannon":
				return GetKeyString("Weapon7") == "" ? "" : $"[{GetKeyString("Weapon7")}]";
			case "Godhand":
				return GetKeyString("Weapon8") == "" ? "" : $"[{GetKeyString("Weapon8")}]";
		}
		return "None";
	}

	public void DisplayAndUploadHighScore()
	{
		if (gops)
		{
			int score = Mathf.FloorToInt(Mathf.Round(Altar.GetTimeSinceStart() * 1000f));
			string basictext = InitialiseHighScoreText(score);
			if (sc.GetGameActive() && score > gops.GetSave().highScore && gops.GetMaxHealth() <= 300 && !gops.GetIsTutorialMode())
			{
				SetPBAndSave(score);
				AppendPBtext(basictext);
				bool ShouldForce = CheckIfShouldForce(score);
				if(gameManagerScript.GetSteamo())
					gameManagerScript.GetSteamo().UpdateScore(gops.GetSave().highScore, ShouldForce);
			}
		}
	}

	private void SetPBAndSave(int pb)
	{
		gops.GetSave().highScore = pb;
		float mvol;
		float svol;
		audioMixer.GetFloat("MusicVolume", out mvol);
		audioMixer.GetFloat("SFXVolume", out svol);
		//save local
		SaveSystem.SaveGame(gops.GetSave().highScore, gops, gops.GetSave().inputSensitivity, mvol, svol, Screen.currentResolution.width, Screen.currentResolution.height, QualitySettings.GetQualityLevel(), gops.GetSave().sfov, gops.GetSave().inputOverrideJson, gops.GetSave().postProcessingEnabled);
	}

	private bool CheckIfShouldForce(int pb)
	{
		bool ShouldForce = false;
		if (gameManagerScript.GetSteamo())
		{
			if (gameManagerScript.GetSteamo().GetFoundOwnResult())
			{
				if (pb > gameManagerScript.GetSteamo().GetOwnScore())
					ShouldForce = true;
			}
			else
				ShouldForce = true;
		}
		Debug.Log("ShouldForce = " + ShouldForce);
		return ShouldForce;
	}

	private void AppendPBtext(string basictext)
	{
		string newhstxt = "\n!New personal best!";
		TMPHS.text = basictext + newhstxt;
	}

	private string InitialiseHighScoreText(int score)
	{
		string finalscore = score > gops.GetSave().highScore ? (score / 1000.0f).ToString() : (gops.GetSave().highScore / 1000.0f).ToString();
		finalscore = CheckSteamResultScore(score, finalscore);
		string basictext = $"Best time: {finalscore} seconds.\nPress {GetKeyString("Reset")} to try again.";
		TMPHS.text = basictext;
		return basictext;
	}

	private string CheckSteamResultScore(int score, string finalscore)
	{
		if (gameManagerScript.GetSteamo())
			if (gameManagerScript.GetSteamo().GetFoundOwnResult())
				finalscore = score > gops.GetSave().highScore ? (score / 1000.0f).ToString() : (gameManagerScript.GetSteamo().GetOwnScore() / 1000.0f).ToString();
		return finalscore;
	}

	public void DisplayUIStats()
	{
		TMPWeaponBank.text = $"Weapon Bank : {statl.GetWeaponBank()}";
		TMPSpiritBank.text = $"Spirit Bank : {statl.GetSpiritBank()}";
		TMPSOULS.text = $"Souls : {statl.GetSouls()}";
		TMPHealthNumber.text = $"{playerunit.GetHealth()} / {playerunit.GetMaxHealth()}";
		TMPHealNumber.text = $"{statl.GetSouls()} / {Resurrector.GetHealthCost()}";
		TMPSummonNumber.text = $"{statl.GetSouls()} / {resurrector.GetResCost()}";
		TMPDashTip.text = $"Dash : [{GetKeyString("Dash")}]";
		if(pm.GetTeleportEnabled())
			TMPTeleportTip.text = $"Teleport : (hold) [{GetKeyString("Teleport")}]";
		AddTips();
		if (!gops.GetIsTutorialMode())
			TMPTUI.text = $"{Altar.GetTimeSinceStart():F3}";
	}

	private void AddTips()
	{
		AddSoulTips();
	}

	private void AddSoulTips()
	{
		string heal = GetKeyString("Heal");
		string summon = GetKeyString("Summon");
		string wbank = GetKeyString("WeaponBank");
        string sbank = GetKeyString("SpiritBank");
        heal = heal.ToLower();
        summon = summon.ToLower();
        wbank = wbank.ToLower();
        sbank = sbank.ToLower();
        if (resurrector.GetHealAvailable())
            TMPHealTip.text = $"Heal : [{heal}]";
        else
            TMPHealTip.text = "";
        if (resurrector.GetSummonAvailable())
            TMPSummonTip.text = $"Summon : [{summon}]";
        else if (Resurrector.CountFriends("Friendlies") >= resurrector.GetResMax() && statl.GetSouls() > 0)
            TMPSummonTip.text = "Summon max reached";
        else
            TMPSummonTip.text = "";
        if (resurrector.GetWBankAvailable())
            TMPWBankTip.text = $"Offer Souls : [{wbank}]";
        else
            TMPWBankTip.text = "";
        if (resurrector.GetSBankAvailable())
            TMPSBankTip.text = $"Offer Souls : [{sbank}]";
        else
            TMPSBankTip.text = "";
    }

    public void ClearUIStats()
    {
        TMPHealthNumber.text = "";
        TMPHealNumber.text = "";
		TMPSummonNumber.text = "";
		TMPWeaponBank.text = "";
		TMPSpiritBank.text = "";
		TMPSOULS.text = "";
		TMPHealTip.text = "";
		TMPSummonTip.text = "";
		TMPWBankTip.text = "";
		TMPSBankTip.text = "";
		TMPSOULSSPENT.text = "";
		TMPWeaponSpent.text = "";
		TMPSpiritSpent.text = "";
		TMPAMMOTIP.text = "";
		TMPWeaponSelected.text = "";
		TMPTUI.text = "";
		TMPSWTIP.text = "";
		TMPDashTip.text = "";
		TMPTeleportTip.text = "";
		TMPAmmoCount.text = "";
		TMPDemoUnavailable.text = "";
	}

	public static string MakeRoman(int value)
	{
		var originalvalue = value;
		var arabic = new int[] { 1000, 900, 500, 400, 100, 90, 50, 40, 10, 9, 5, 4, 1 };
		var roman = new string[] { "M", "CM", "D", "CD", "C", "XC", "L", "XL", "X", "IX", "V", "IV", "I" };
		var result = "";
		for (int i = 0; i < 13; i++)
		{
			while (value >= arabic[i])
			{
				result = result + roman[i].ToString();
				value = value - arabic[i];
			}
		}
		if (originalvalue < 8000)
			return result;
		else
			return originalvalue.ToString();
	}

	public string GetKeyString(string value)
	{
		var actions = playerInput.currentActionMap.actions;
		InputBinding bindingmask = InputBinding.MaskByGroup(playerInput.currentControlScheme);
		foreach (InputAction action in actions)
		{
			if (value == action.name)
				return action.GetBindingDisplayString(bindingmask);
		}
		return "";
	}

	public static string GetKeyString(string value, PlayerInput playerInput)
	{
		var actions = playerInput.currentActionMap.actions;
		InputBinding bindingmask = InputBinding.MaskByGroup(playerInput.currentControlScheme);
		foreach (InputAction action in actions)
		{
			if (value == action.name)
				return action.GetBindingDisplayString(bindingmask);
		}
		return "";
	}
}