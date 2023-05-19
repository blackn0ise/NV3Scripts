using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;

public class SpawnController : MonoBehaviour
{
	#region exposed parameters
	[Header("Parameters")]
	[SerializeField] private Material greyHologram = default;
	[SerializeField] private Material greenHologram = default;
	[SerializeField] private Material redHologram = default;
	[SerializeField] private int maxregularspawns = 20;
	[SerializeField] private int maxnecrospawns = 20;
	[SerializeField] private int randomSpawnChancePercent = 20;
	[SerializeField] private float timeTillAdvent = 30.0f;
	[SerializeField] private float EGLShrinkrate = default;
	[SerializeField] private float EGLMinimum = default;
	[SerializeField] private float godhandtimer = 10;
	[SerializeField] internal float hologramScale = 1;

	[Header("Benches")]
	[SerializeField] private TextMeshPro TMPweaponnum = default;
	[SerializeField] private TextMeshPro TMPWeaponName = default;
	[SerializeField] private TextMeshPro TMPPaName = default;
	[SerializeField] private TextMeshPro TMPPuName = default;
	[SerializeField] private GameObject weaponSpawnAnim = default;
	[SerializeField] private GameObject enemyCollection = default;
	[SerializeField] private GameObject wbench = default;
	[SerializeField] private GameObject wbprops = default;
	[SerializeField] private GameObject pabench = default;
	[SerializeField] private GameObject pubench = default;
	[SerializeField] private GameObject corpPickupPos = default;
	[SerializeField] private GameObject holyPickupPos = default;
	[SerializeField] private GameObject powerupPickupPos = default;
	[SerializeField] private GameObject bloodPickupPos = default;
	[SerializeField] private GameObject resurrectableSoul = default;
	[SerializeField] private GameObject waypointer = default;
	[SerializeField] private CorporealShrine corpShrine = default;
	[SerializeField] private HolyShrine holyShrine = default;
	[SerializeField] private PowerupBench pubenchscript = default;
	[SerializeField] private BloodShrine bloodShrine = default;
	[SerializeField] private ParticleSystem ringParticles = default;
	[SerializeField] private GameObject ringGO = default;

	[Header("Enemies")]
	[SerializeField] private GameObject revenant = default;
	[SerializeField] private GameObject tutorialrevenant = default;
	[SerializeField] private GameObject eliterevenant = default;
	[SerializeField] private GameObject juggernaut = default;
	[SerializeField] private GameObject dreadnought = default;
	[SerializeField] private GameObject terrorNecromancer = default;
	[SerializeField] private GameObject eliteNecromancer = default;
	[SerializeField] private GameObject lich = default;
	[SerializeField] private GameObject lacerator = default;
	[SerializeField] private GameObject vizier = default;
	[SerializeField] private GameObject soulcollector = default;

	[Header("Guns and upgrades")]
	[SerializeField] private GameObject bonebag = default;
	[SerializeField] private GameObject shotgun = default;
	[SerializeField] private GameObject banshee = default;
	[SerializeField] private GameObject sledgehammer = default;
	[SerializeField] private GameObject gauntlets = default;
	[SerializeField] private GameObject snubnose = default;
	[SerializeField] private GameObject penance = default;
	[SerializeField] private GameObject handcannon = default;
	[SerializeField] private GameObject reaver = default;
	[SerializeField] private GameObject goregun = default;
	[SerializeField] private GameObject judgement = default;
	[SerializeField] private GameObject devourer = default;
	[SerializeField] private GameObject voidcannon = default;
	[SerializeField] private GameObject crucifier = default;
	[SerializeField] private GameObject triptikoss = default;
	[SerializeField] private GameObject longinus = default;
	[SerializeField] private GameObject godhand = default;
	[SerializeField] private GameObject friendlyjuggernaut = default;
	[SerializeField] private GameObject friendlyrevenant = default;
	[SerializeField] private GameObject friendlyeliterevenant = default;

	[Header("Gun pickups")]
	[SerializeField] private GameObject shotgunpickup = default;
	[SerializeField] private GameObject bansheepickup = default;
	[SerializeField] private GameObject sledgehammerpickup = default;
	[SerializeField] private GameObject gauntletspickup = default;
	[SerializeField] private GameObject snubnosepickup = default;
	[SerializeField] private GameObject handcannonpickup = default;
	[SerializeField] private GameObject penancepickup = default;
	[SerializeField] private GameObject crucifierpickup = default;
	[SerializeField] private GameObject longinuspickup = default;
	[SerializeField] private GameObject triptikosspickup = default;
	[SerializeField] private GameObject reaverpickup = default;
	[SerializeField] private GameObject limbopickup = default;
	[SerializeField] private GameObject judgementpickup = default;
	[SerializeField] private GameObject devourerpickup = default;
	[SerializeField] private GameObject voidcannonpickup = default;
	[SerializeField] private GameObject medusapickup = default;

	[Header("Other pickups")]
	[SerializeField] private GameObject annihilationpickup = default;
	[SerializeField] private GameObject resurrectionpickup = default;
	[SerializeField] private GameObject masssummonpickup = default;
	[SerializeField] private GameObject godhandpickup = default;
	[SerializeField] private GameObject juggernautpickup = default;
	[SerializeField] private GameObject elitesummonspickup = default;
	[SerializeField] private GameObject teleportPickup = default;
	[SerializeField] private GameObject assimilationpickup = default;
	[SerializeField] private GameObject viperpickup = default;

	#endregion

	#region locals and internals

	private bool isEGLActive = false;
	private bool gameActive = false;
	private bool godhandgained = false;
	private bool tutpowerupspawned = false;
	private bool tutweaponspawned = false;
	private bool tutpassivespawned = false;
	private bool adventspawned = false;
	internal bool awaitingNextPurchase = true;
	internal bool firstUpgradesSpawned = false;
	internal bool purchaseAvailable = false;
	internal bool awaitingAcquire = false;
	internal bool canAffordCorp = false;
	internal bool canAffordHoly = false;
	internal bool canAffordBlood = false;
	internal bool canAffordAny = false;
	internal bool onCooldown = false;
	internal bool powerupAvailable = false;
	public const string epath = "spawns";
	public const string eglpath = "EGLspawns";
	public const string wpath = "weaponspawns";
	public const string papath = "passivespawns";
	public const string poupath = "powerupspawns";
	private float timeSinceLastKill = 0.0f;
	private float spawnTimestamp = 0;
	private float puTimestamp = 0;
	private float EGLMultiplier = 1;
	private float debugtimer = 0.0f;
	private int maxSpawnPoints = 20;
	private int nextWeaponCost;
	private int nextPassiveCost;
    private int spawnCounter = 0;
	private static int randomSeed;
	private static Material staticGreyHologram = default;
	private static Material staticGreenHologram = default;
	private static Material staticRedHologram = default;
	private List<MeshRenderer> ringRenderers;
	private SpawnPoint[] spawnPoints;
	private PlayerUnit playerUnit;
	private UIController uic;
	private GameOptions gops;
	private SpawnsetEnemies es;
	private SpawnsetEnemies egl;
	private SoundLibrary sl;
	private PowerupSpawnset pus;
	private StatLibrary statl;
	private List<Spawn> spawnList;
	private List<Spawn> EGLSpawnList;
	private List<Spawn> usedSpawnList;
	private List<Spawn> usedEGLSpawnList;
	private List<Weapon> weaponList;
	private List<Weapon> spawnedweaponlist = new List<Weapon>();
	private List<PassiveBonus> paList;
	private List<PassiveBonus> spawnedpalist = new List<PassiveBonus>();
	private List<Powerup> pulist;
	private List<Powerup> spawnedpulist = new List<Powerup>();
	public Pickup currentCorpPickup { get; private set; }
	public Pickup currentHolyPickup { get; private set; }
	public Pickup currentBloodPickup { get; private set; }

	#endregion

	#region provide methods

	public GameObject provideGameObject(string name)
	{
		switch (name)
		{
			//enemies
			case "Revenant":
				return revenant;
			case "TutorialRevenant":
				return tutorialrevenant;
			case "EliteRevenant":
				return eliterevenant;
			case "Juggernaut":
				return juggernaut;
			case "Dreadnought":
				return dreadnought;
			case "TerrorNecromancer":
				return terrorNecromancer;
			case "EliteNecromancer":
				return eliteNecromancer;
			case "Lich":
				return lich;
			case "Lacerator":
				return lacerator;
			case "Vizier":
				return vizier;
			case "SoulCollector":
				return soulcollector;

			//guns and summons
			case "BoneBag":
				return bonebag;
			case "Gauntlets":
				return gauntlets;
			case "Snubnose":
				return snubnose;
			case "Penance":
				return penance;
			case "Hand Cannon":
				return handcannon;
			case "Shotgun":
				return shotgun;
			case "Sledgehammer":
				return sledgehammer;
			case "Banshee":
				return banshee;
			case "Reaver":
				return reaver;
			case "Goregun":
				return goregun;
			case "Judgement":
				return judgement;
			case "Devourer":
				return devourer;
			case "Void Cannon":
				return voidcannon;
			case "Crucifier":
				return crucifier;
			case "Longinus":
				return longinus;
			case "Triptikoss":
				return triptikoss;
			case "Godhand":
				return godhand;
			case "Friendly Revenant":
				return friendlyrevenant;
			case "Friendly Juggernaut":
				return friendlyjuggernaut;
			case "Friendly Elite Revenant":
				return friendlyeliterevenant;

			//other
			case "ResurrectableSoul":
				return resurrectableSoul;
			default:
				return null;
		}
	}
	public GameObject providePickup(string name)
	{
		switch (name)
		{
			case "Shotgun":
				return shotgunpickup;
			case "Banshee":
				return bansheepickup;
			case "Sledgehammer":
				return sledgehammerpickup;
			case "Snubnose":
				return snubnosepickup;
			case "Hand Cannon":
				return handcannonpickup;
			case "Penance":
				return penancepickup;
			case "Gauntlets":
				return gauntletspickup;
			case "Reaver":
				return reaverpickup;
			case "Goregun":
				return limbopickup;
			case "Judgement":
				return judgementpickup;
			case "Devourer":
				return devourerpickup;
			case "Medusa":
				return medusapickup;
			case "Void Cannon":
				return voidcannonpickup;
			case "Crucifier":
				return crucifierpickup;
			case "Longinus":
				return longinuspickup;
			case "Triptikoss":
				return triptikosspickup;
			case "Annihilation":
				return annihilationpickup;
			case "Resurrection":
				return resurrectionpickup;
			case "Mass Summons":
				return masssummonpickup;
			case "Godhand":
				return godhandpickup;
			case "Juggernaut":
				return juggernautpickup;
			case "Elite Summons":
				return elitesummonspickup;
			case "Teleport":
				return teleportPickup;
			case "Assimilation":
				return assimilationpickup;
			case "Viper":
				return viperpickup;
			default:
				return null;
		}
	}

    #endregion

    #region getters and setters

    public static Material GetGreyHologram() { return staticGreyHologram; }
	public static Material GetGreenHologram() { return staticGreenHologram; }
	public static Material GetRedHologram() { return staticRedHologram; }

	public int getMaxNecroSpawns() { return maxnecrospawns; }
	public bool GetTutWeaponSpawned() { return tutweaponspawned; }
	public bool GetTutPassiveSpawned() { return tutpassivespawned; }
	public bool GetTutPowerupSpawned() { return tutpowerupspawned; }
	public void SetBoneBag(GameObject value) { bonebag = value; }


	public Powerup GetFirstPowerup() { return pulist.ElementAt(1); }
	public GameObject GetWaypointer() { return waypointer; }

	public List<Weapon> GetWeaponList() { return weaponList; }
	public List<Weapon> GetOwnedWeapons() { return spawnedweaponlist; }
	public bool GetGameActive() { return gameActive; }
	public void SetGameActive(bool value) { gameActive = value; }
	public static bool GetIsGameActive() { return GameManagerScript.GetGMScript().GetSpawnController().GetGameActive(); }

	private AudioClip pickupSpawned;

    public float GetTimeSinceLastKill() { return timeSinceLastKill; }
	public void SetTimeSinceLastKill(float value) { timeSinceLastKill = value; }

	public List<PassiveBonus> GetSpawnedPassives() { return spawnedpalist; }
	public List<Weapon> GetSpawnedWeapons() { return spawnedweaponlist; }

	public int GetNextWeaponCost() { return nextWeaponCost; }
	public int GetNextPassiveCost() { return nextPassiveCost; }

	#endregion

	#region cost matrix setup

	internal int upgradeLevel = 0;
	private int[,] costMatrix;
	private int[,] starterCostMatrix = new int[,] 
	{ 
		{ 500, 1000, 1500 }, 
		{ 1250, 2500, 3500 },
		{ 3500, 5000, 6500 },
		{ 4500, 6000, 7000 },
		{ 6000, 7500, 9000 },
		{ 7000, 8500, 10000 },
		{ 8000, 9500, 10500 },
		{ 9000, 10500, 12000 },
		{ 10000, 11500, 13000 } 
	};

	private string[] corporealTree = new string[]
	{
		"Shotgun",
		"Snubnose",
		"Elite Summons",
		"Crucifier",
		"Judgement",
		"Assimilation",
		"Adjure",
		"Boiler",
		"Void Cannon"
	};

	private string[] holyTree = new string[]
	{
		"Banshee",
		"Penance",
		"Mass Summons",
		"Longinus",
		"Reaver",
		"Teleport",
		"Praetorian",
		"Godwind",
		"Medusa"
	};

	private string[] bloodTree = new string[]
	{
		"Sledgehammer",
		"Hand Cannon",
		"Juggernaut",
		"Triptikoss",
		"Goregun",
		"Viper",
		"Lacerator",
		"Bloodbath",
		"Devourer"
	};

	internal string[] corporealTreeDemo = new string[]
	{
		"Shotgun",
		"Snubnose",
		"Elite Summons",
		"Crucifier",
		"Judgement",
		"Void Cannon"
	};

	internal string[] holyTreeDemo = new string[]
	{
		"Banshee",
		"Penance",
		"Mass Summons",
		"Longinus",
		"Reaver",
		"Medusa"
	};

	internal string[] bloodTreeDemo = new string[]
	{
		"Sledgehammer",
		"Hand Cannon",
		"Juggernaut",
		"Triptikoss",
		"Goregun",
		"Devourer"
	};

	internal int[] corpCostTree = new int[9];
	internal int[] holyCostTree = new int[9];
	internal int[] bloodCostTree = new int[9];

    #endregion

    #region startup

    void Start()
	{
		Initialise();
		LoadSpawnsets();
		InitialiseUsedSpawnlists();
		ClearNameAndNumTexts();
		PopulateSpawnlists();
		RemoveSkippedUnits();
		UpdateLastDemoTier();
		//DebugCostMatrix();
	}

    private void UpdateLastDemoTier()
    {
        if(gops.GetIsDemomode())
            for (int i = 0; i < 3; i++)
            {
				starterCostMatrix[corporealTreeDemo.Length - 1, i] = 9999999;
			}
	}

    private void Initialise()
	{
		gops = GameOptions.GetGOPS();
		playerUnit = GameManagerScript.GetGMScript().GetPlayer().GetComponent<PlayerUnit>();
		statl = GameManagerScript.GetGMScript().GetStatl();
		pickupSpawned = GameManagerScript.GetGMScript().GetSoundLibrary().GetPickupSpawned();
		sl = GameManagerScript.GetGMScript().GetSoundLibrary();
		spawnPoints = FindObjectsOfType<SpawnPoint>();
		maxSpawnPoints = spawnPoints.Length;
		staticGreenHologram = greenHologram;
		staticRedHologram = redHologram;
		staticGreyHologram = greyHologram;
		ringRenderers = ringGO.GetComponentsInChildren<MeshRenderer>().ToList();
	}

	private void LoadSpawnsets()
	{
		uic = GetComponent<UIController>();
		es = SpawnsetEnemies.Load(epath);
		egl = SpawnsetEnemies.Load(eglpath);
		//ws = WeaponSpawnset.Load(wpath);
		pus = PowerupSpawnset.Load(poupath);
		//pas = PassiveSpawnset.Load(papath);
	}

	private void InitialiseUsedSpawnlists()
	{
		usedSpawnList = new List<Spawn>();
		usedEGLSpawnList = new List<Spawn>();
	}

	private void PopulateSpawnlists()
	{
		spawnList = es.spawns;
		//weaponList = ws.weapons;
		//paList = pas.passives;
		pulist = pus.powerups;
		EGLSpawnList = egl.spawns;

		if(gops.GetIsTutorialMode())
        {
            StartCoroutine(DelayAddSummonUpgrades());
        }

        if (gops.GetDebugSpawns())
		{
			DebugSpawnListsAndTimings();
		}
	}

	#endregion

	#region updates

	private void Update()
	{
		if (gops)
		{
			if (gops.GetIsTutorialMode())
				return;
			UpdateCanAffords();
			CheckGodhand();
			CheckUnitSpawn();
			CheckPowerupTiming();
			CheckLichAdvent();
			CheckHighlighting();
		}
	}

	private void UpdateCanAffords()
	{
		if (!firstUpgradesSpawned)
			return;
		canAffordCorp = statl.GetSouls() >= corpCostTree[upgradeLevel];
		canAffordHoly = statl.GetSouls() >= holyCostTree[upgradeLevel];
		canAffordBlood = statl.GetSouls() >= bloodCostTree[upgradeLevel];
		canAffordAny = canAffordCorp || canAffordHoly || canAffordBlood;


        if (gops.GetIsDemomode())
            if (upgradeLevel + 1 == corporealTreeDemo.Length)
            {
				canAffordCorp = false;
				canAffordHoly = false;
				canAffordBlood = false;
				canAffordAny = false;
			}
	}

	private void CheckGodhand()
	{
		if (gameActive && !gops.GetIsDemomode())
		{
			if (Altar.GetTimeSinceStart() > godhandtimer && !godhandgained && !playerUnit.GetIsDead())
			{
				ClearPUbench();
				AddGodhand();
				godhandgained = true;
			}
		}
	}

	private void CheckUnitSpawn()
	{
		//Debug.Log("enemy count = " + enemyCollection.transform.childCount);
		if (gameActive && enemyCollection.transform.childCount < maxregularspawns)
		{
			HandleEGLOrNot();
		}
		else if (gameActive && enemyCollection.transform.childCount >= maxregularspawns)
		{
			//LogSpawnMax();
		}
	}

	void CheckPowerupTiming()
	{
		if (gameActive)
		{
			foreach (Powerup powerup in pulist)
			{
				//remove existing powerup
				if (Time.timeSinceLevelLoad > puTimestamp && !spawnedpulist.Contains(powerup) && !playerUnit.GetIsDead() && pubench.GetComponentInChildren<Pickup>())
					Destroy(pubench.GetComponentInChildren<Pickup>().gameObject);


				if (Time.timeSinceLevelLoad > puTimestamp && !spawnedpulist.Contains(powerup) && !playerUnit.GetIsDead())
				{
					SpawnPowerup(powerup);
					spawnedpulist.Add(powerup);
					puTimestamp = Time.timeSinceLevelLoad + powerup.spawnDelay;
				}
			}
			if (pulist.Count == spawnedpulist.Count && !godhandgained)
				spawnedpulist.Clear();
		}
	}

	private void CheckLichAdvent()
	{
		if (gameActive)
			timeSinceLastKill += Time.deltaTime;

		if (timeSinceLastKill > timeTillAdvent && !adventspawned && gameActive && !playerUnit.GetIsDead())
		{
			DistributeAndSpawn(7, "Lich");
			adventspawned = true;
		}
	}

	private void CheckHighlighting()
	{
		HandleRingParticles();
        if (onCooldown)
            return;

        //Debug.Log($"{corporealTree[nextUpgradeLevel]} = {corpCostTree[nextUpgradeLevel]}");
        //Debug.Log($"{holyTree[nextUpgradeLevel]} = {holyCostTree[nextUpgradeLevel]}");
        //Debug.Log($"{bloodTree[nextUpgradeLevel]} = {bloodCostTree[nextUpgradeLevel]}");


        //used to make pickups call highlight green here when aiming was a thing
        var res = GameManagerScript.GetGMScript().GetResurrector();

		if (canAffordCorp)
			if (currentCorpPickup)
				if (currentCorpPickup.GetAvailableForPurchase() && !currentCorpPickup.bodyEnabled)
                {
					currentCorpPickup.HighlightGreen();
					res.ActivateShrinePickup("corp");
				}
		if (canAffordHoly)
			if (currentHolyPickup)
				if (currentHolyPickup.GetAvailableForPurchase() && !currentHolyPickup.bodyEnabled)
				{
					currentHolyPickup.HighlightGreen();
					res.ActivateShrinePickup("holy");
				}
		if (canAffordBlood)
			if (currentBloodPickup)
				if (currentBloodPickup.GetAvailableForPurchase() && !currentBloodPickup.bodyEnabled)
				{
					currentBloodPickup.HighlightGreen();
					res.ActivateShrinePickup("blood");
				}

	}

	private void HandleRingParticles()
	{
		if (powerupAvailable)
        {
            if (!ringParticles.isPlaying)
            {
                ringParticles.Play();
                return;
            }
        }
        else if (ringParticles.isPlaying)
        {
            ringParticles.Stop();
            return;
        }


    }

    #endregion

    #region cheats and other utility
    private IEnumerator DelayAddSummonUpgrades()
	{
		yield return new WaitForSeconds(0.2f);
		holyShrine.ApplyBonus("Summon Upgrade");
		holyShrine.ApplyBonus("Summon Upgrade");
	}

	private void DebugSpawnListsAndTimings()
	{
		float debugmultiplier = 1.0f;
		int counter = 0;
		foreach (Spawn spawn in spawnList)
        {
            DebugSingleSpawn(counter, spawn);
            counter++;
        }
        for (int i = 0; i < 20; i++)
		{
			foreach (Spawn spawn in EGLSpawnList)
			{
				var finalstring = new StringBuilder();
				finalstring.Append(String.Format("EGL {0,2:000} ] \t ({1,8:0000.0}s)    {2} {3}\n", counter, debugtimer, spawn.spawnCount, spawn.spawnUnit));
				Debug.Log(finalstring);

				debugtimer += spawn.spawnDelay * debugmultiplier;
				counter++;
			}
			if (debugmultiplier > EGLMinimum)
				debugmultiplier *= EGLShrinkrate;
		}
		debugtimer = 0;
	}

	private void DebugSingleSpawn(int counter, Spawn spawn, bool gameLog = false)
	{
		debugtimer += spawn.spawnDelay;
		var finalstring = new StringBuilder();
		if (gameLog)
		{
			finalstring.Append(String.Format("SPAWN {0,2:000} ] \t ({1,8:0.0}s)    {2} {3}\n", counter, Altar.GetTimeSinceStart(), spawn.spawnCount, spawn.spawnUnit));
			GameLog.Log(finalstring.ToString());
			Debug.Log(finalstring.ToString());
		}
		else
		{
			finalstring.Append(String.Format("SPAWN {0,2:000} ] \t ({1,8:0000.0}s)    {2} {3}\n", counter, debugtimer, spawn.spawnCount, spawn.spawnUnit));
			Debug.Log(finalstring.ToString());
		}
	}

	private void ClearNameAndNumTexts()
	{
		//TMPweaponnum.text = "";
		//TMPWeaponName.text = "";
		//TMPPaName.text = "";
		//TMPPuName.text = "";
	}

	private void RemoveSkippedUnits()
	{
		if (gops.GetSkipToSpawnIndex() != 0 && gops.GetSkipToSpawnIndex() + 1 < spawnList.Count)
		{
			for (int i = 0; i < gops.GetSkipToSpawnIndex(); i++)
			{
				usedSpawnList.Add(spawnList[i]);
			}
		}
	}

	internal GameObject ChooseSCSummon()
	{
		GameObject[] possibles = new GameObject[] { eliteNecromancer, lich, juggernaut, terrorNecromancer, dreadnought };
		int random = UnityEngine.Random.Range(0, possibles.Length - 1);
		return possibles[random];
	}

	private void AddGodhand()
	{
		//make the pickup
		GameObject pickup = Instantiate(godhandpickup, powerupPickupPos.transform.position, Quaternion.identity, pubench.transform);
		TMPPuName.text = pickup.GetComponent<Pickup>().GetPickupName();
		GameObject anim = Instantiate(weaponSpawnAnim, powerupPickupPos.transform.position, Quaternion.identity, pubench.transform);
		//ActivateBenchParticles(pubench.GetComponentsInChildren<ParticleSystem>());
		//waypointer.GetComponent<Waypointer>().ChooseAndTogglePointer("Blood", true);
		SoundLibrary.PlayFromTimedASO(sl.GetGodhand(), playerUnit.transform.position);
		Destroy(anim, 3.0f);
		pickup.name = "Godhand";
	}

	private void ClearPUbench()
	{
		if (pubench.GetComponentInChildren<Pickup>())
			Destroy(pubench.GetComponentInChildren<Pickup>().gameObject);
		foreach (Powerup powerup in pulist)
		{
			if (!spawnedpulist.Contains(powerup))
				spawnedpulist.Add(powerup);
		}
	}

	private void ActivateBenchParticles(ParticleSystem[] psarray)
	{
		foreach (ParticleSystem ps in psarray)
		{
			ps.Play();
			ps.GetComponentInChildren<Light>().range = 15;
		}
	}

	public void GiveAllUpgrades()
	{
		Player player = playerUnit.gameObject.GetComponent<Player>();
		foreach (Weapon weapon in weaponList)
		{
			spawnedweaponlist.Add(weapon);
			player.GetPlayerOwnedWeapons().Add(RollAndProvideWeapon(weapon.spawnName));
		}
		player.GetPlayerOwnedWeapons().Add(provideGameObject("Devourer"));
		player.GetPlayerOwnedWeapons().Add(provideGameObject("Void Cannon"));
		player.GetPlayerOwnedWeapons().Add(provideGameObject("Gauntlets"));
		uic.SetVoidOwned(true);
		uic.SetDevOwned(true);
		foreach (PassiveBonus pb in paList)
		{
			spawnedpalist.Add(pb);
		}
		holyShrine.ApplyBonus("Summon Upgrade");
		holyShrine.ApplyBonus("Friendly Juggernaut");
		holyShrine.ApplyBonus("Health Upgrade");
		holyShrine.ApplyBonus("Summon Upgrade");
		holyShrine.ApplyBonus("Elite Summons");
		holyShrine.ApplyBonus("Teleport");
		pubenchscript.ActivateWings();
		if (!gops.GetIsTutorialMode())
			player.GetPlayerOwnedWeapons().Add(provideGameObject("Godhand"));
	}

	private void ConfirmWeaponName(GameObject pickup)
	{
		if (pickup.GetComponent<Pickup>().GetPickupName() == "Snubnose")
			TMPWeaponName.text = "Leathian Snubnose";
		else
			TMPWeaponName.text = pickup.GetComponent<Pickup>().GetPickupName();
	}

	#endregion

	#region upgrade tree, cost matrix, purchasing and distribution

	public void GenerateRandomSeed()
	{
		int tempSeed = (int)DateTime.Now.Ticks;
		randomSeed = tempSeed;
		UnityEngine.Random.InitState(randomSeed);
		Debug.Log("Seed = " + randomSeed);
	}

	public void SetRandomSeed()
	{
		//throw new NotImplementedException();
	}

	public void HandleCostTrees()
	{
		costMatrix = RandomiseCostMatrix(starterCostMatrix);
		PopulateCostTrees();
	}

	private void PopulateCostTrees()
	{
		corpCostTree = PullCostTree(corpCostTree, 0);
		holyCostTree = PullCostTree(holyCostTree, 1);
		bloodCostTree = PullCostTree(bloodCostTree, 2);
	}

	private int[] PullCostTree(int[] costTree, int jIndex)
	{
		var tempTree = new int[costTree.Length];

		for (int i = 0; i < costTree.Length; i++)
		{
			tempTree[i] = costMatrix[i, jIndex];
		}

		//for (int i = 0; i < tempTree.Length; i++)
		//{
		//	Debug.Log("tempTree[i] = " + tempTree[i]);
		//}

		return tempTree;
	}

	private void DebugCostMatrix()
	{
		for (int i = 0; i < costMatrix.GetLength(0); i++)
		{
			Debug.Log(costMatrix[i, 0] + " " + costMatrix[i, 1] + " " + costMatrix[i, 2]);
		}
	}

	private int[,] RandomiseCostMatrix(int[,] inputMatrix)
	{
		int[,] resultMatrix = new int[inputMatrix.GetLength(0), inputMatrix.GetLength(1)];

		for (int i = 0; i < inputMatrix.GetLength(0); i++)
		{
			int[] trio = CreateAssignmentTrio();
			for (int j = 0; j < inputMatrix.GetLength(1); j++)
			{
				resultMatrix[i, j] = inputMatrix[i, trio[j]];
			}
			//Debug.Log("resultMatrix "+ i + " = " + resultMatrix[i,0] + " " + resultMatrix[i,1] + " " + resultMatrix[i,2]);
		}
		return resultMatrix;
	}

	private int[] CreateAssignmentTrio()
	{
		List<int> possible = new List<int>() { 0, 1, 2 };
		List<int> result = new List<int>();

		for (int i = 3; i > 0; i--)
		{
			int randomIndex = UnityEngine.Random.Range(0, i);
			result.Add(possible[randomIndex]);
			possible.RemoveAt(randomIndex);
		}
		return result.ToArray();
	}
	internal IEnumerator PlaceOnCooldown(float duration = 4)
    {
		onCooldown = true;
		yield return new WaitForSeconds(duration);
		onCooldown = false;

    }

	internal void DisplayNextPurchaseOptions(bool firstSpawn = false)
	{
		//Debug.Log("awaitingNextPurchase = " + awaitingNextPurchase);
		//Debug.Log("!firstSpawn = " + !firstSpawn);
		//Debug.Log("upgradeLevel = " + upgradeLevel);

		//if (awaitingNextPurchase && !firstSpawn)
		//	return;

		GameObject corpPickup;
		GameObject holyPickup;
		GameObject bloodPickup;

		DeleteAllPickups();

		//Demo mode currently implemented by empty pickups in the matrix, meaning providePickup is null

		//TODO: Make the UI towers and shrines show the next available pickup, but say that this is only available in full version. Need to build a thing that checks against a max upgrade level for demo, and doesn't allow banking past this point so that pickups can still spawn without the ability to purchase
		var ctree = gops.GetIsDemomode() ? corporealTreeDemo : corporealTree;
		var htree = gops.GetIsDemomode() ? holyTreeDemo : holyTree;
		var btree = gops.GetIsDemomode() ? bloodTreeDemo : bloodTreeDemo;

		if (providePickup(corporealTree[upgradeLevel]))
        {
            corpPickup = Instantiate(providePickup(ctree[upgradeLevel]), corpPickupPos.transform.position, Quaternion.identity, corpPickupPos.transform);
            currentCorpPickup = corpPickup.GetComponentInChildren<Pickup>();
            corpShrine.currentPickup = currentCorpPickup;
			currentCorpPickup.DisableComponents();
        }
        if (providePickup(holyTree[upgradeLevel]))
        {
            holyPickup = Instantiate(providePickup(htree[upgradeLevel]), holyPickupPos.transform.position, Quaternion.identity, holyPickupPos.transform);
            currentHolyPickup = holyPickup.GetComponentInChildren<Pickup>();
            holyShrine.currentPickup = currentHolyPickup;
			currentHolyPickup.DisableComponents();
        }
        if (providePickup(bloodTree[upgradeLevel]))
        {
            bloodPickup = Instantiate(providePickup(btree[upgradeLevel]), bloodPickupPos.transform.position, Quaternion.identity, bloodPickupPos.transform);
            currentBloodPickup = bloodPickup.GetComponentInChildren<Pickup>();
            bloodShrine.currentPickup = currentBloodPickup;
			currentBloodPickup.DisableComponents();
        }

		DisplayNextPurchaseTowers();

        if (firstSpawn)
            firstUpgradesSpawned = true;

    }

    private void DisplayNextPurchaseTowers()
    {
		uic.TMPCBankNameTower.text = corpShrine.currentPickup.pickupName;
		uic.TMPHBankNameTower.text = holyShrine.currentPickup.pickupName; 
		uic.TMPBBankNameTower.text = bloodShrine.currentPickup.pickupName;
	}

    internal void DeleteAllPickups()
    {
        var pickups = FindObjectsOfType<Pickup>();
        foreach (Pickup p in pickups)
        {
			if (!p)
				continue;
			if (p.pickupName == "Gauntlets")
				continue;
			if (p.pickupType == "Powerup")
				continue;
			Destroy(p.gameObject);
		}
		ClearAllTowerNames();
    }

    private void ClearAllTowerNames()
    {
		uic.TMPCBankNameTower.text = "";
		uic.TMPHBankNameTower.text = "";
		uic.TMPBBankNameTower.text = "";
	}

    internal void DeleteAllOtherPickups(Pickup pickup)
    {
		var pickups = FindObjectsOfType<Pickup>();
		foreach(Pickup p in pickups)
        {
			if (!p)
				continue;
			if (p.pickupName == "Gauntlets")
				continue;
			if (p.pickupType == "Powerup")
				continue;
			if (p != pickup)
				Destroy(p.gameObject);
        }
		ClearEmptyTowerNames();
    }

    private void ClearEmptyTowerNames()
	{
		if (corpShrine.currentPickup == null)
			uic.TMPCBankNameTower.text = "";
		if (holyShrine.currentPickup == null)
			uic.TMPHBankNameTower.text = "";
		if (bloodShrine.currentPickup == null)
			uic.TMPBBankNameTower.text = "";

	}

	#endregion

	#region unit spawning
	void RemoveNonDemoSpawns(Spawn spawn)
	{
		if (gops.GetIsDemomode() && (spawn.spawnUnit == "Vizier" || spawn.spawnUnit == "SoulCollector") && !usedSpawnList.Contains(spawn))
		{
			usedSpawnList.Add(spawn);
		}
	}

	private void LogSpawnMax()
	{
		Debug.Log("Maximum enemies reached. Unable to spawn.");
		Debug.Log("Current enemy count = " + enemyCollection.transform.childCount);
	}

	private void HandleEGLOrNot()
	{
		if (!isEGLActive)
		{
			if (usedSpawnList.Count >= spawnList.Count && spawnList.Count > 0 && usedSpawnList.Count > 0)
			{
				isEGLActive = true;
			}
			BeginSpawn(spawnList, usedSpawnList, isEGLActive);
		}
		else
		{
			CheckEGLCountandShrink();
			BeginSpawn(EGLSpawnList, usedEGLSpawnList, isEGLActive);
		}
	}

	private void CheckEGLCountandShrink()
	{
		if (usedEGLSpawnList.Count >= EGLSpawnList.Count)
		{
			usedEGLSpawnList.Clear();
			if (EGLMultiplier > EGLMinimum)
				EGLMultiplier *= EGLShrinkrate;
		}
	}

	private void BeginSpawn(List<Spawn> spawnList, List<Spawn> usedList, bool eglactive)
	{
		foreach (Spawn spawn in spawnList)
		{
			RemoveNonDemoSpawns(spawn);
			if (Time.timeSinceLevelLoad > spawnTimestamp && !usedList.Contains(spawn) && !playerUnit.GetIsDead())
			{
				DistributeAndSpawn(spawn);
				usedList.Add(spawn);
				spawnTimestamp = Time.timeSinceLevelLoad + ((gops.GetIsFastmode() ? spawn.spawnDelay / gops.GetFastModeFactor() : spawn.spawnDelay) * (eglactive ? EGLMultiplier : 1));
			}
		}
	}

	public void DistributeAndSpawn(Spawn spawn)
	{
		List<SpawnPoint> possiblePoints = new List<SpawnPoint>(spawnPoints);
		List<Transform> possiblePointTransforms = new List<Transform>();
		foreach (var point in spawnPoints)
		{
			possiblePointTransforms.Add(point.transform);
		}


		if (spawn.spawnCount > maxSpawnPoints)
			spawn.spawnCount = maxSpawnPoints;



		for (int i = 0; i < spawn.spawnCount; i++)
		{
			int random = UnityEngine.Random.Range(0, 100);
			int index = random < randomSpawnChancePercent ? GetNextClosestSpawnPoint(possiblePointTransforms) : GetRandomSpawnPoint(possiblePointTransforms);
			possiblePoints[index].SpawnUnit(provideGameObject(spawn.spawnUnit));
			possiblePoints.RemoveAt(index);
			possiblePointTransforms.RemoveAt(index);
		}


		//List<SpawnPoint> usedPoints = new List<SpawnPoint>();
		//int index = 0;


		if (GameOptions.GetGOPS().GetDebugSpawns())
        {
            spawnCounter++;
			DebugSingleSpawn(spawnCounter, spawn, true);
		}
	}

    private int GetRandomSpawnPoint(List<Transform> possiblePointTransforms)
    {
        return UnityEngine.Random.Range(0, possiblePointTransforms.Count);
	}

    private int GetNextClosestSpawnPoint(List<Transform> possiblePointTransforms)
    {
        return possiblePointTransforms.IndexOf(Unit.DetermineClosest(playerUnit.gameObject, possiblePointTransforms.ToArray()));
	}

	void DistributeAndSpawn(int count, string spawn)
    {
        List<SpawnPoint> possiblePoints = new List<SpawnPoint>(spawnPoints);
		List<SpawnPoint> usedPoints = new List<SpawnPoint>();
		int randomNr = UnityEngine.Random.Range(0, possiblePoints.Count);
		if (count > maxSpawnPoints)
			count = maxSpawnPoints;
		for (int i = 0; i < count; i++)
		{
			while (possiblePoints[randomNr] == null || usedPoints.Contains(possiblePoints[randomNr]))
			{
				randomNr = UnityEngine.Random.Range(0, possiblePoints.Count);
			}
			possiblePoints[randomNr].SpawnUnit(provideGameObject(spawn));
			usedPoints.Add(possiblePoints[randomNr]);
		}

	}
	#endregion

	#region weapon rolling (deprecated)

	private void ChooseAndSpawnPickup(Weapon weapon)
	{
		switch (weapon.spawnName)
		{
			case "Shotgun":
				RollWeaponChoice("Shotgun");
				break;
			case "Snubnose":
				RollWeaponChoice("Snubnose");
				break;
			case "Crucifier":
				RollWeaponChoice("Crucifier");
				break;
			case "Reaver":
				RollWeaponChoice("Reaver");
				break;
			default:
				SpawnWeaponPickup(weapon.spawnName);
				break;
		}
	}

	private void RollWeaponChoice(string weapontype)
	{
		int diceroll = UnityEngine.Random.Range(0, 3);
		int demodiceroll = UnityEngine.Random.Range(0, 10);
		int demoprobability = 4;
		if (gops.GetIsTutorialMode())
			diceroll = 0;
		if (gops.GetIsDemomode() && demodiceroll < demoprobability)
			RollAndSpawn(weapontype, diceroll);
		else if (gops.GetIsDemomode() && demodiceroll >= demoprobability)
		{
			diceroll = 0;
			RollAndSpawn(weapontype, diceroll);
		}
		else if (!gops.GetIsDemomode())
			RollAndSpawn(weapontype, diceroll);
	}

	private void RollAndSpawn(string weapontype, int diceroll)
	{
		switch (weapontype)
		{
			case "Shotgun":
				if (diceroll == 0)
					SpawnWeaponPickup("Shotgun");
				else if (diceroll == 1)
					SpawnWeaponPickup("Banshee");
				else if (diceroll == 2)
					SpawnWeaponPickup("Sledgehammer");
				break;
			case "Snubnose":
				if (diceroll == 0)
					SpawnWeaponPickup("Snubnose");
				else if (diceroll == 1)
					SpawnWeaponPickup("Penance");
				else if (diceroll == 2)
					SpawnWeaponPickup("Hand Cannon");
				break;
			case "Crucifier":
				if (diceroll == 0)
					SpawnWeaponPickup("Crucifier");
				else if (diceroll == 1)
					SpawnWeaponPickup("Triptikoss");
				else if (diceroll == 2)
					SpawnWeaponPickup("Longinus");
				break;
			case "Reaver":
				if (diceroll == 0)
					SpawnWeaponPickup("Reaver");
				else if (diceroll == 1)
					SpawnWeaponPickup("Goregun");
				else if (diceroll == 2)
					SpawnWeaponPickup("Judgement");
				break;
		}
	}

	private GameObject RollAndProvideWeapon(string weapontype)
	{
		int diceroll = UnityEngine.Random.Range(0, 3);
		switch (weapontype)
		{
			case "Shotgun":
				if (diceroll == 0)
					return provideGameObject("Shotgun");
				else if (diceroll == 1)
					return provideGameObject("Banshee");
				else if (diceroll == 2)
					return provideGameObject("Sledgehammer");
				break;
			case "Snubnose":
				if (diceroll == 0)
					return provideGameObject("Snubnose");
				else if (diceroll == 1)
					return provideGameObject("Penance");
				else if (diceroll == 2)
					return provideGameObject("Hand Cannon");
				break;
			case "Crucifier":
				if (diceroll == 0)
					return provideGameObject("Crucifier");
				else if (diceroll == 1)
					return provideGameObject("Triptikoss");
				else if (diceroll == 2)
					return provideGameObject("Longinus");
				break;
			case "Reaver":
				if (diceroll == 0)
					return provideGameObject("Reaver");
				else if (diceroll == 1)
					return provideGameObject("Goregun");
				else if (diceroll == 2)
					return provideGameObject("Judgement");
				break;
		}
		return provideGameObject("BoneBag");
	}

	#endregion

	#region old spawning
	internal void SpawnTutorialPowerup()
	{
		if (!tutpowerupspawned)
		{
			Powerup powerup = new Powerup();
			powerup.spawnName = "Annihilation";
			powerup.spawnIndex = 0;
			powerup.spawnDelay = 0;
			SpawnPowerup(powerup);
			tutpowerupspawned = true;
		}
	}

	void CheckPassiveReady()
	{
		int lowest = int.MaxValue;
		foreach (PassiveBonus passive in paList)
		{
			lowest = passive.spawnCost < lowest && !spawnedpalist.Contains(passive) ? passive.spawnCost : lowest;
			RemoveNonDemoPassive(passive);
			RemoveTutorialPassive(passive);
			FixTutTeleCost(passive);
			if (statl.GetSpiritBank() >= passive.spawnCost && !spawnedpalist.Contains(passive) && !pabench.GetComponentInChildren<Pickup>())
			{
				SpawnPassive(passive);
			}
		}
		nextPassiveCost = lowest;

	}

	private void SpawnPassive(PassiveBonus passive)
	{
		spawnedpalist.Add(passive);
		statl.SetSpiritBank(statl.GetSpiritBank() - passive.spawnCost);
		uic.DisplayOffering();
		uic.SetPaIconEnabled(true);
		//make the pickup
		GameObject pickup = Instantiate(providePickup(passive.spawnName), holyPickupPos.transform.position, Quaternion.identity, pabench.transform);
		TMPPaName.text = pickup.GetComponent<Pickup>().GetPickupName();
		GameObject anim = Instantiate(weaponSpawnAnim, holyPickupPos.transform.position, Quaternion.identity, pabench.transform);
		ActivateBenchParticles(pabench.GetComponentsInChildren<ParticleSystem>());
		Destroy(anim, 3.0f);
		pickup.name = passive.spawnName;
		if (statl.GetSpiritBank() < 30000)
			SoundLibrary.PlayFromTimedASO(pickupSpawned, playerUnit.transform.position);
	}

	void CheckWeaponReady()
	{
		int lowest = int.MaxValue;
		foreach (Weapon weapon in weaponList)
		{
			lowest = weapon.spawnCost < lowest && !spawnedweaponlist.Contains(weapon) ? weapon.spawnCost : lowest;
			RemoveNonDemoWeapon(weapon);
			if (statl.GetWeaponBank() >= weapon.spawnCost && !spawnedweaponlist.Contains(weapon) && !wbprops.GetComponentInChildren<Pickup>())
			{
				SpawnWeapon(weapon);
			}
		}
		nextWeaponCost = lowest;
	}

	private void SpawnWeapon(Weapon weapon)
	{
		spawnedweaponlist.Add(weapon);
		statl.SetWeaponBank(statl.GetWeaponBank() - weapon.spawnCost);
		uic.DisplayOffering();
		uic.SetWpIconEnabled(true);
		TMPweaponnum.text = "Key " + UIController.MakeRoman(weapon.defaultKey);
		ChooseAndSpawnPickup(weapon);
		GameObject anim = Instantiate(weaponSpawnAnim, corpPickupPos.transform.position, Quaternion.identity, wbench.transform);
		ActivateBenchParticles(wbench.GetComponentsInChildren<ParticleSystem>());
		Destroy(anim, 3.0f);
		if (statl.GetWeaponBank() < 30000)
			SoundLibrary.PlayFromTimedASO(pickupSpawned, playerUnit.transform.position);
	}
	private void SpawnWeaponPickup(String chosen)
	{
		GameObject pickup = Instantiate(providePickup(chosen), corpPickupPos.transform.position, Quaternion.identity, wbprops.transform);
		ConfirmWeaponName(pickup);
		pickup.name = chosen;
	}

	private void FixTutTeleCost(PassiveBonus pb)
	{
		if (!gops.GetIsTutorialMode())
			return;
		if (pb.spawnName == "Teleport" && pb.spawnCost != 500)
			pb.spawnCost = 500;
	}

	void RemoveTutorialPassive(PassiveBonus pb)
	{
		if (!gops.GetIsTutorialMode())
			return;
		if (pb.spawnName == "SummonPlus2")
		{
			spawnedpalist.Add(pb);
			//holyShrine.LockPip(0);
			//holyShrine.LockPip(3);
		}
	}
	void RemoveNonDemoPassive(PassiveBonus pb)
	{
		//if (gops.GetIsDemomode() && (pb.spawnName == "Scythe" || pb.spawnName == "Health") && !spawnedpalist.Contains(pb))
		//	spawnedpalist.Add(pb);
		//if (gops.GetIsDemomode())
		//{
		//	holyShrine.LockPip(5);
		//	holyShrine.LockPip(6);
		//}
	}
	void RemoveNonDemoWeapon(Weapon weapon)
	{
		if (gops.GetIsDemomode() && (weapon.spawnName == "Devourer" || weapon.spawnName == "Void Cannon") && !spawnedweaponlist.Contains(weapon))
			spawnedweaponlist.Add(weapon);
		//if (gops.GetIsDemomode())
		//{
		//	corpShrine.LockPip(5);
		//	corpShrine.LockPip(6);
		//	corpShrine.LockPip(7);
		//}
	}

	public void SpawnPowerup(Powerup powerup)
	{
		if (providePickup(powerup.spawnName))
		{
			GameObject pickup = Instantiate(providePickup(powerup.spawnName), powerupPickupPos.transform.position, Quaternion.identity, pubench.transform);
			TMPPuName.text = pickup.GetComponent<Pickup>().GetPickupName() + "  ready";
            GameObject anim = Instantiate(weaponSpawnAnim, powerupPickupPos.transform.position, Quaternion.identity, pubench.transform);
            pubenchscript.ActivateParticles();
            uic.SetPuIconEnabled(true);
            Destroy(anim, 3.0f);
            pickup.name = powerup.spawnName;
			SoundLibrary.PlayFromTimedASO(pickupSpawned, playerUnit.transform.position);
            powerupAvailable = true;
        }
    }

    #endregion
}