using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{

	#region exposed parameters

	[Header("Components")]
	[SerializeField] private Resurrector resurrector = default;
	[SerializeField] private SoundLibrary sl = default;
	[SerializeField] private StatLibrary statl = default;
	[SerializeField] private SpawnController sc = default;
	[SerializeField] private AudioMixer audioMixer = default;
	[SerializeField] private PlayerInput playerInput = default;
	[SerializeField] private ParticleSystem SWIndicator = default;
	[SerializeField] private GameManagerScript gameManagerScript = default;

	[Header("UI TMPs")]
	[SerializeField] private TextMeshProUGUI TMPSOULS = default;
	[SerializeField] private TextMeshProUGUI TMPSOULSSPENT = default;
	[SerializeField] private TextMeshProUGUI TMPWeaponSelected = default;
	[SerializeField] private TextMeshProUGUI TMPOFFERING = default;
	[SerializeField] private TextMeshProUGUI TMPTUI = default;
	[SerializeField] internal TextMeshProUGUI TMPHS = default;
	[SerializeField] private TextMeshProUGUI TMPAmmoCount = default;
	[SerializeField] private TextMeshProUGUI TMPTutorialPanelText = default;
	[SerializeField] private TextMeshProUGUI TMPDemoUnavailable = default;


	[Header("Tip TMPs")]
	[SerializeField] private TextMeshProUGUI TMPSummonTip = default;
	[SerializeField] private TextMeshProUGUI TMPRingTip = default;
	[SerializeField] private TextMeshProUGUI TMPAMMOTIP = default;
	[SerializeField] private TextMeshProUGUI TMPSWTIP = default;
	[SerializeField] private TextMeshProUGUI TMPDashTip = default;
	[SerializeField] private TextMeshProUGUI TMPTeleportTip = default;

	[Header("UI Towers")]
	[SerializeField] internal TextMeshPro TMPCBankNumTower = default;
	[SerializeField] internal TextMeshPro TMPHBankNumTower = default;
	[SerializeField] internal TextMeshPro TMPBBankNumTower = default;
	[SerializeField] internal TextMeshPro TMPCBankNameTower = default;
	[SerializeField] internal TextMeshPro TMPHBankNameTower = default;
	[SerializeField] internal TextMeshPro TMPBBankNameTower = default;
	[SerializeField] private TextMeshPro TMPTimeTower = default;
	[SerializeField] private TextMeshPro TMPSoulTower = default;
	[SerializeField] private TextMeshPro TMPVoidCost = default;
	[SerializeField] private TextMeshPro TMPDevCost = default;
	[SerializeField] private MeshRenderer[] upgradeGems = default;
	[SerializeField] internal Transform TMPCBankBar = default;
	[SerializeField] internal Transform TMPHBankBar = default;
	[SerializeField] internal Transform TMPBBankBar = default;

	[Header("Gameobjects")]
	[SerializeField] private GameObject TMPTutorialPanel = default;
	[SerializeField] private GameObject dashback = default;
	[SerializeField] private GameObject teleportback = default;
	[SerializeField] private GameObject crosshair = default;
	[SerializeField] private GameObject TMPRes = default;


	[Header("Bench Icons and pips")]
	//[SerializeField] private GameObject PuIcon = default;
	//[SerializeField] private GameObject PaIcon = default;
	//[SerializeField] private GameObject WpIcon = default;
	[SerializeField] private GameObject ResPipBack = default;
	[SerializeField] private GameObject EDPipBack1 = default;
	[SerializeField] private GameObject EDPipBack2 = default;

	[Header("Bar and border images")]
	[SerializeField] private Image DashBar = default;
	[SerializeField] private Image TeleportBar = default;
	[SerializeField] private Animator PBBorder = default;

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

	[Header("Dropshadow images")]
	[SerializeField] private Image shadowTop = default;
	[SerializeField] private Image shadowHS = default;
	[SerializeField] private Image shadowRes = default;
	[SerializeField] private Image shadowDash = default;
	[SerializeField] private Image shadowHook = default;
	[SerializeField] private Image shadowAmmo = default;
	[SerializeField] private Image shadowSummon = default;
	[SerializeField] private Image shadowUpgrade = default;

	[Header("Colors")]
	[SerializeField] private Color SWReady = Color.green;
	[SerializeField] private Color SWNotReady = Color.red;
	[SerializeField] private Color SWDef = Color.white;
	[SerializeField] private Material onMaterial = default;

	[Header("Parameters")]
	[SerializeField] private float SWIParticleLifeMax = 0.25f;
	#endregion

	#region locals and get set

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
    internal bool readyToPlayDashPing = false;
	internal bool readyToPlayTPPing = false;
	private List<Image> weaponImageList = default;


    public void SetVoidOwned(bool value) { voidowned = value; }
	public void SetDevOwned(bool value) { devowned = value; }

	public bool GetVoidOwned() { return voidowned; }
	public bool GetDevOwned() { return devowned; }
	public bool GetUIOn() { return UIon; }
	public void SetUIOn(bool value) { UIon = value; /*crosshair.SetActive(value);*/ }
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

	#endregion

	#region startup

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
		SetTowerNameTips();
		shadowHS.gameObject.SetActive(false);
	}

    private void SetTowerNameTips()
	{
		string key1 = GetKeyString("NextWeapon");
		string key2 = GetKeyString("CycleWeapons");
		key1 = key1.ToLower();
		key2 = key2.ToLower();


		TMPCBankNameTower.text = "Corporeal upgrades spawn here.";
		TMPHBankNameTower.text = "Holy upgrades spawn here.";
		TMPBBankNameTower.text = "Blood upgrades spawn here.";
        TMPCBankNumTower.text = $"Switch weapons to reload.";
        TMPHBankNumTower.text = "Get resurrections\nfor an extra\nlife.";
        TMPBBankNumTower.text = "Grab the gem to begin.";
    }

    private void ClearOtherTexts()
	{
		TMPHS.text = "";
		TMPCBankNameTower.text = "";
		TMPHBankNameTower.text = "";
		TMPBBankNameTower.text = "";
		TMPTimeTower.text = "";
		TMPSoulTower.text = "";
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


	#endregion

	#region updates

	private void Update()
	{
		if (!UIon && SWIndicator.isPlaying)
			SWIndicator.Stop();
		//TMPCBankNumTower.text = statl.GetWeaponBank().ToString();
		//TMPHBankNumTower.text = statl.GetSpiritBank().ToString();
		TMPAmmoCount.gameObject.SetActive(UIon && sc.GetGameActive());
		shadowAmmo.gameObject.SetActive(UIon && sc.GetGameActive());

		if (!player.gameObject.GetComponent<PlayerUnit>().GetIsDead() && sc.GetGameActive())
		{
			TMPTimeTower.text = Mathf.FloorToInt(Altar.GetTimeSinceStart()).ToString();
			TMPSoulTower.text = "Souls\n" + statl.GetSouls().ToString();
		}
		if (UIon && !player.gameObject.GetComponent<PlayerUnit>().GetIsDead() && sc.GetGameActive())
		{
			DisplayUIStats();
			SetCrosshairActive();
			UpdateSWIndicator();
			DisplayAmmoUI();
			DisplayBenchIcons();
		}
		else if (!player.gameObject.GetComponent<PlayerUnit>().GetIsDead() && !UIon)
		{
			ClearUIStats();
			HideBenchIcons();
		}
		if (!sc.GetGameActive())
			AddSoulTips();
		if (sc.GetGameActive())
			DisplayTowerCosts();
		HandleStatBars();
		DisplaySWCosts();
		UpdateFadingUIs();
		DisplayPipBacks();
	}

    private void DisplayTowerCosts()
	{
		if (sc.currentCorpPickup != null)
        {
			string str = statl.GetSouls() < sc.corpCostTree[sc.upgradeLevel] ?
				$"[{statl.GetSouls()} / {sc.corpCostTree[sc.upgradeLevel]}] " :
				"Purchase available here.";
			TMPCBankNumTower.text = str;
			UpdateTowerBar(TMPCBankBar, sc.corpCostTree[sc.upgradeLevel]);
		}
		else
        {
			TMPCBankNumTower.text = "";
			UpdateTowerBar(TMPCBankBar, 1, true);
		}
		if (sc.currentHolyPickup != null)
		{
			string str = statl.GetSouls() < sc.holyCostTree[sc.upgradeLevel] ?
				$"[{statl.GetSouls()} / {sc.holyCostTree[sc.upgradeLevel]}] " :
				"Purchase available here.";
			TMPHBankNumTower.text = str;
			UpdateTowerBar(TMPHBankBar, sc.holyCostTree[sc.upgradeLevel]);
		}
		else
		{
			TMPHBankNumTower.text = "";
			UpdateTowerBar(TMPHBankBar, 1, true);
		}
		if (sc.currentBloodPickup != null)
		{
			string str = statl.GetSouls() < sc.bloodCostTree[sc.upgradeLevel] ?
				$"[{statl.GetSouls()} / {sc.bloodCostTree[sc.upgradeLevel]}] " :
				"Purchase available here.";
			TMPBBankNumTower.text = str;
			UpdateTowerBar(TMPBBankBar, sc.bloodCostTree[sc.upgradeLevel]);
		}
		else
		{
			TMPBBankNumTower.text = "";
			UpdateTowerBar(TMPBBankBar, 1, true);
		}

		if(gops.GetIsDemomode() && sc.upgradeLevel + 1 == sc.corporealTreeDemo.Length)
        {
			if(TMPCBankNumTower.text != "Unavailable in demo version")
			{
				TMPCBankNumTower.text = "Unavailable in demo version";
                TMPHBankNumTower.text = "Unavailable in demo version";
                TMPBBankNumTower.text = "Unavailable in demo version";
                UpdateTowerBar(TMPCBankBar, 1, true);
                UpdateTowerBar(TMPHBankBar, 1, true);
                UpdateTowerBar(TMPBBankBar, 1, true);
                sc.currentCorpPickup.HighlightRed();
                sc.currentHolyPickup.HighlightRed();
                sc.currentBloodPickup.HighlightRed();

            }
        }
    }

    private void UpdateTowerBar(Transform bar, int cost, bool clear = false)
	{
        Vector3 newscale = Vector3.one;
		newscale.y *= Mathf.Clamp01((float)statl.GetSouls() / cost);
		newscale.y *= clear ? 0 : 1;
		bar.localScale = newscale;
	}

	public void DisplayUIStats()
    {
        TMPSOULS.text = $"Souls : {statl.GetSouls()}";
        DisplayMovementTips();
        AddSoulTips();
        if (!gops.GetIsTutorialMode())
            TMPTUI.text = $"{Altar.GetTimeSinceStart():F3}";
    }

    private void DisplayMovementTips()
    {
        TMPDashTip.text = $"Dash : [{GetKeyString("Dash")}]";
        if (pm.GetTeleportEnabled())
            TMPTeleportTip.text = $"Teleport : (hold) [{GetKeyString("Teleport")}]";
        else if (pm.assimilationEnabled)
            TMPTeleportTip.text = $"Assimilation : [{GetKeyString("Teleport")}]";
        else if (pm.viperEnabled)
            TMPTeleportTip.text = $"Hook : [{GetKeyString("Teleport")}]";
    }

    private void AddSoulTips()
    {
        string summon = GetKeyString("Summon");
        summon = summon.ToLower();

        HandleGetTips(summon);

        if (resurrector.GetSummonAvailable())
        {
			TMPSummonTip.text = $"Summon : [{summon}]";
			shadowSummon.gameObject.SetActive(true);
			return;
		}
		else if (MouseLook.LookTargetTransform != null)
        {
            if (MouseLook.LookTargetTransform.CompareTag("ResurrectableSoul") && Resurrector.CountFriends("Friendlies") >= resurrector.GetResMax())
                TMPSummonTip.text = $"Summon max reached.";
			shadowSummon.gameObject.SetActive(true);
			return;
        }
        else
        {
            TMPSummonTip.text = "";
			shadowSummon.gameObject.SetActive(false);
		}

		/*if (resurrector.CheckCanAffordTarget() && !sc.awaitingAcquire)
            TMPSummonTip.text = $"Give souls : [{summon}]";
		else
			TMPSummonTip.text = "";*/
	}

	private void HandleGetTips(string summon)
    {
		if (sc.powerupAvailable)
            TMPRingTip.text = "powerup available";
        else if (sc.canAffordAny)
            TMPRingTip.text = "upgrade available";
		else
			TMPRingTip.text = "";

		shadowUpgrade.gameObject.SetActive(TMPRingTip.text == "" ? false : true);

		/*		if (sc.awaitingAcquire)
				{
					TMPRingTip.text = "Upgrade available";
					return;
				}

				if (!RitualRing.isInRing && sc.canAffordAny)
					TMPRingTip.text = "Upgrade ready.\n\nStep into the ring\nto begin upgrading.";
				else if (RitualRing.isInRing && sc.canAffordAny && MouseLook.LookTargetShrine == "none")
					TMPRingTip.text = "Aim at your desired\nupgrade shrine.";
				else if (RitualRing.isInRing && sc.canAffordAny && MouseLook.LookTargetShrine != "none")
					if (resurrector.CheckCanAffordTarget())
						TMPRingTip.text = $"Press the [{summon}] button\nto spawn the upgrade.";
					else
						TMPRingTip.text = "You can't afford\nthis upgrade yet.";
		*/
	}

	private void SetCrosshairActive()
	{
		crosshair.SetActive(UIon);
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

	private void DisplayBenchIcons()
	{
		/*if (paiconUIEnabled)
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
			WpIcon.SetActive(false);*/
	}

	public void ClearUIStats()
	{
		TMPSOULS.text = "";
		TMPRingTip.text = "";
		TMPSummonTip.text = "";
		TMPSOULSSPENT.text = "";
		TMPAMMOTIP.text = "";
		TMPWeaponSelected.text = "";
		TMPTUI.text = "";
		TMPSWTIP.text = "";
		TMPDashTip.text = "";
		TMPTeleportTip.text = "";
		TMPAmmoCount.text = "";
		TMPDemoUnavailable.text = "";
	}

	private void HideBenchIcons()
	{
		/*if (PaIcon.activeInHierarchy == true || PuIcon.activeInHierarchy == true || WpIcon.activeInHierarchy == true)
		{
			PaIcon.SetActive(false);
			PuIcon.SetActive(false);
			WpIcon.SetActive(false);
		}*/
	}

	public void HandleStatBars()
	{
        //float healpercent = 0.0f;
        //if (Resurrector.GetHealthCost() > 0)
        //	healpercent = (float)statl.GetSouls() / (float)Resurrector.GetHealthCost();
        //if (healpercent > 1)
        //	healpercent = 1;
        float dashpercent = 0.0f;
        if (pm.GetDashCDTimer() > 0)
            dashpercent = (1.0f - (float)pm.GetDashCDTimer() / (float)pm.GetDashCooldown());
        else
        {
            dashpercent = 1;
            if (readyToPlayDashPing)
            {
				SoundLibrary.PlayFromTimedASO(sl.dashReadyPing, player.transform.position);
				readyToPlayDashPing = false;
			}
        }
        float telepercent = CalculateTelePercent();

        float weaponpercent = (float)statl.GetWeaponBank() / (float)sc.GetNextWeaponCost();
		float passivepercent = (float)statl.GetSpiritBank() / (float)sc.GetNextPassiveCost();

		DashBar.fillAmount = dashpercent;
		TeleportBar.fillAmount = telepercent;

		//handle dash and teleport bar normally
		dashback.SetActive(UIon && sc.GetGameActive());
		teleportback.SetActive(UIon && sc.GetGameActive() && (pm.GetTeleportEnabled() || pm.assimilationEnabled || pm.viperEnabled));
		TMPDashTip.gameObject.SetActive(UIon);
		TMPTeleportTip.gameObject.SetActive(UIon);

		shadowDash.gameObject.SetActive(UIon && sc.GetGameActive());
		shadowHook.gameObject.SetActive(UIon && sc.GetGameActive());
		shadowTop.gameObject.SetActive(UIon && sc.GetGameActive());

		//handle pregame dash and teleport bars
		if (!sc.GetGameActive() && pm.viperEnabled)
        {
			dashback.SetActive(UIon);
			teleportback.SetActive(UIon);
			TMPDashTip.gameObject.SetActive(UIon);
			TMPTeleportTip.gameObject.SetActive(UIon);
			shadowDash.gameObject.SetActive(UIon);
			shadowHook.gameObject.SetActive(UIon);
			DisplayMovementTips();
		}

		TMPRes.gameObject.SetActive(UIon && sc.GetGameActive() && gameManagerScript.GetPowerupBench().resesAcquired > 0);
	}

	private float CalculateTelePercent()
	{
		float telepercent = 0;
		if (pm.GetTeleportEnabled())
        {
            if (pm.GetTeleCDTimer() > 0)
                telepercent = (1.0f - (float)pm.GetTeleCDTimer() / (float)pm.GetTeleCooldown());
            else
            {
                telepercent = 1;
                if (readyToPlayTPPing)
                {
                    SoundLibrary.PlayFromTimedASO(sl.cdReadyPing, player.transform.position);
                    readyToPlayTPPing = false;
                }
            }

            return telepercent;
        }
        if (pm.assimilationEnabled || pm.viperEnabled)
        {
            float totalduration = pm.shootRateTimeStamp - pm.lastShotTimeStamp;
            float timeElapsed = Time.time - pm.lastShotTimeStamp;

            if (timeElapsed < totalduration)
                telepercent = (timeElapsed / totalduration);
            else
            {
                telepercent = 1;
                if (readyToPlayTPPing)
                {
                    SoundLibrary.PlayFromTimedASO(sl.cdReadyPing, player.transform.position);
                    readyToPlayTPPing = false;
                }
            }
            return telepercent;
        }
        return 0;

    }

    private void DisplaySWCosts()
	{
		if (voidowned)
			TMPVoidCost.text = "Void Cannon cost : " + VoidCannon.GetVoidCost().ToString();
		else
			TMPVoidCost.text = "No Void Cannon yet...";
		if (devowned)
			TMPDevCost.text = "Devourer cost : " + Devourer.GetDevCost().ToString();
		else
			TMPDevCost.text = "No Devourer yet...";
	}

	private void UpdateFadingUIs()
	{
		var tempscolor = TMPSOULSSPENT.color;
		var tempocolor = TMPOFFERING.color;
		var tempwselcolor = TMPWeaponSelected.color;
		var tempducolor = TMPDemoUnavailable.color;
		if (tempscolor.a >= 0)
		{
			tempscolor.a -= Time.deltaTime * 0.2f;
			TMPSOULSSPENT.color = tempscolor;
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

    private void DisplayPipBacks()
    {
		ResPipBack.SetActive(UIon && sc.GetGameActive() && gameManagerScript.GetPowerupBench().resesAcquired > 0);

		shadowRes.gameObject.SetActive(UIon && sc.GetGameActive() && gameManagerScript.GetPowerupBench().resesAcquired > 0);

		EDPipBack1.SetActive(UIon && sc.GetGameActive() && pm.assimilationEnabled);
		EDPipBack2.SetActive(UIon && sc.GetGameActive() && pm.assimilationEnabled);
	}

	#endregion

	#region weapon selecting

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

	#endregion

	#region keys and weapon name get

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

    #endregion

	#region pb

	public void DisplayAndUploadHighScore(bool PB, int score)
	{
		if (gops)
        {
            string basictext = InitialiseHighScoreText(score);
            HandlePB(PB, score, basictext);
		}
	}

    private void HandlePB(bool PB, int score, string basictext)
    {
		if (PB)
		{
			SetPBAndSave(score);
			AppendPBtext(basictext);
			bool ShouldForce = CheckIfShouldForce(score);
			if (gameManagerScript.GetSteamo())
				gameManagerScript.GetSteamo().UpdateScore(gops.GetSave().highScore, ShouldForce);
			shadowHS.gameObject.SetActive(true);
			PBBorder.SetTrigger("dead");
		}
	}

    private void SetPBAndSave(int pb)
	{
		gops.GetSave().highScore = pb;
		//save local
		SaveSystem.SaveGame(gops, audioMixer);
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

	#endregion

	#region utility

	public void ActivateGem(int index)
    {
		upgradeGems[index].material = onMaterial;
	}

	internal void ClearBars()
	{
		TMPTeleportTip.gameObject.SetActive(false);
		teleportback.gameObject.SetActive(false);
		TMPDashTip.gameObject.SetActive(false);
		dashback.gameObject.SetActive(false);
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

	internal void DisplaySoulCost(int cost)
	{
		if (UIon)
		{
			TMPSOULSSPENT.text = "-" + cost.ToString();
			var tempcolor = Color.red;
			TMPSOULSSPENT.color = tempcolor;
		}
	}

/*	public void HandleTutorialPips()
	{
		foreach (GameObject go in OwnershipPips)
		{
			if (go.name == "WeaponOwned1Pip" || go.name == "WeaponOwned8Pip")
				continue;
			else
				go.SetActive(true);
		}
	}*/

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
		if (playerInput.currentActionMap == null)
			return "";
		var actions = playerInput.currentActionMap.actions;
		InputBinding bindingmask = InputBinding.MaskByGroup(playerInput.currentControlScheme);
		foreach (InputAction action in actions)
		{
			if (value == action.name)
			{
				if (action.GetBindingDisplayString(bindingmask) == "RMB")
					return "Mouse Right";
				if (action.GetBindingDisplayString(bindingmask) == "LMB")
					return "Mouse Left";
				return action.GetBindingDisplayString(bindingmask);
			}
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

	#endregion
}