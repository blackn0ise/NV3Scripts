using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class SpawnController : MonoBehaviour
{
	[Header("Parameters")]
	[SerializeField] private int maxregularspawns = 20;
	[SerializeField] private int maxnecrospawns = 20;
	[SerializeField] private float timeTillAdvent = 30.0f;
	[SerializeField] private float EGLShrinkrate = default;
	[SerializeField] private float EGLMinimum = default;
	[SerializeField] private float godhandtimer = 10;

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
	[SerializeField] private GameObject weaponPickupPos = default;
	[SerializeField] private GameObject passivePickupPos = default;
	[SerializeField] private GameObject powerupPickupPos = default;
	[SerializeField] private GameObject waypointer = default;
	
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
	[SerializeField] private GameObject scythes = default;
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
	
	[Header("Other pickups")]
	[SerializeField] private GameObject quaddamagepickup = default;
	[SerializeField] private GameObject summonplus2pickup = default;
	[SerializeField] private GameObject scythepickup = default;
	[SerializeField] private GameObject healthpickup = default;
	[SerializeField] private GameObject godhandpickup = default;
	[SerializeField] private GameObject juggernautpickup = default;
	[SerializeField] private GameObject elitesummonspickup = default;
	[SerializeField] private GameObject teleportPickup = default;


	public int getMaxNecroSpawns() { return maxnecrospawns; }
	public bool GetTutWeaponSpawned() { return tutweaponspawned; }
	public bool GetTutPassiveSpawned() { return tutpassivespawned; }
	public bool GetTutPowerupSpawned() { return tutpowerupspawned; }
	public void SetBoneBag(GameObject value) { bonebag = value; }

	private float debugtimer = 0.0f;
	private bool isEGLActive = false;
	private bool gameActive = false;
	private bool godhandgained = false;
	private bool tutpowerupspawned = false;
	private bool tutweaponspawned = false;
	private bool tutpassivespawned = false;
	public const string epath = "spawns";
	public const string eglpath = "EGLspawns";
	public const string wpath = "weaponspawns";
	public const string papath = "passivespawns";
	public const string poupath = "powerupspawns";
	private float timeSinceLastKill = 0.0f;
	private bool adventspawned = false;
	private float spawnTimestamp = 0;
	private float puTimestamp = 0;
	private float EGLMultiplier = 1;
	private int maxSpawnPoints = 20;
	private SpawnPoint[] spawnPoints;
	private PlayerUnit playerUnit;
	private UIController uic;
	private GameOptions gops;
	private SpawnsetEnemies es;
	private SpawnsetEnemies egl;
	private WeaponSpawnset ws;
	private SoundLibrary sl;
	private PowerupSpawnset pus;
	private PassiveSpawnset pas;
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

	public Powerup GetFirstPowerup() { return pulist.ElementAt(1); }
	public GameObject GetWaypointer() { return waypointer; }

	public List<Weapon> GetWeaponList() { return weaponList; }
	public List<Weapon> GetOwnedWeapons() { return spawnedweaponlist; }
	public bool GetGameActive() { return gameActive; }
	public void SetGameActive(bool value) { gameActive = value; }
	private AudioSource aso;
	private AudioClip pickupSpawned;

	public float GetTimeSinceLastKill() { return timeSinceLastKill; }
	public void SetTimeSinceLastKill(float value) { timeSinceLastKill = value; }

	public List<PassiveBonus> GetSpawnedPassives() { return spawnedpalist; }

	public List<Weapon> GetSpawnedWeapons() { return spawnedweaponlist; }


	void Start()
	{
		Initialise();
		LoadSpawnsets();
		InitialiseUsedSpawnlists();
		ClearNameAndNumTexts();
		PopulateSpawnlists();
		RemoveSkippedSpawns();
	}

	private void Initialise()
	{
		gops = GameOptions.GetGOPS();
		playerUnit = GameManagerScript.GetGMScript().GetPlayer().GetComponent<PlayerUnit>();
		aso = GameManagerScript.GetGMScript().GetPlayer().GetComponent<AudioSource>();
		statl = GameManagerScript.GetGMScript().GetStatl();
		pickupSpawned = GameManagerScript.GetGMScript().GetSoundLibrary().GetPickupSpawned();
		sl = GameManagerScript.GetGMScript().GetSoundLibrary();
		spawnPoints = FindObjectsOfType<SpawnPoint>();
		maxSpawnPoints = spawnPoints.Length;
	}

	private void LoadSpawnsets()
	{
		uic = GetComponent<UIController>();
		es = SpawnsetEnemies.Load(epath);
		egl = SpawnsetEnemies.Load(eglpath);
		ws = WeaponSpawnset.Load(wpath);
		pus = PowerupSpawnset.Load(poupath);
		pas = PassiveSpawnset.Load(papath);
	}

	private void InitialiseUsedSpawnlists()
	{
		usedSpawnList = new List<Spawn>();
		usedEGLSpawnList = new List<Spawn>();
	}

	private void PopulateSpawnlists()
	{
		spawnList = es.spawns;
		weaponList = ws.weapons;
		paList = pas.passives;
		pulist = pus.powerups;
		EGLSpawnList = egl.spawns;
		if (gops.GetDebugSpawns())
		{
			DebugSpawnListsAndTimings();
		}
	}

	private void DebugSpawnListsAndTimings()
	{
		float debugmultiplier = 1.0f;
		int counter = 0;
		foreach (Spawn spawn in spawnList)
		{
			Debug.Log($"  {counter}  )\t {spawn.spawnCount} \t {spawn.spawnUnit} Spawns at \t{debugtimer} seconds");
			debugtimer += spawn.spawnDelay;
			counter++;
		}
		for (int i = 0; i < 8; i++)
		{
			foreach (Spawn spawn in EGLSpawnList)
			{
				Debug.Log($"  {counter}  )\t {spawn.spawnCount} \t EGL {spawn.spawnUnit} Spawns at \t{debugtimer} seconds");
				debugtimer += spawn.spawnDelay * debugmultiplier;
				counter++;
			}
			if (debugmultiplier > EGLMinimum)
				debugmultiplier *= EGLShrinkrate;
		}
	}

	private void ClearNameAndNumTexts()
	{
		TMPweaponnum.text = "";
		TMPWeaponName.text = "";
		TMPPaName.text = "";
		TMPPuName.text = "";
	}

	private void RemoveSkippedSpawns()
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

	private void Update()
	{
		if (gops)
		{
			if (!gops.GetIsTutorialMode())
			{
				CheckGodhand();
				CheckSpawnTimingAndCount();
				CheckWeaponReady();
				CheckPowerupTiming();
				CheckPassiveReady();
				if (gameActive)
					timeSinceLastKill += Time.deltaTime;
				CheckLichAdvent();
			}
		}
	}

	private void CheckGodhand()
	{
		if (gameActive && !gops.GetIsDemomode())
		{
			if (Altar.GetTimeSinceStart() > godhandtimer && !godhandgained && !playerUnit.IsDead())
			{
				ClearPUbench();
				AddGodhand();
				godhandgained = true;
			}
		}
	}

	private void AddGodhand()
	{
		//make the pickup
		GameObject pickup = Instantiate(godhandpickup, powerupPickupPos.transform.position, Quaternion.identity, pubench.transform);
		TMPPuName.text = pickup.GetComponent<Pickup>().GetPickupName();
		GameObject anim = Instantiate(weaponSpawnAnim, powerupPickupPos.transform.position, Quaternion.identity, pubench.transform);
		ActivateBenchParticles(pubench.GetComponentsInChildren<ParticleSystem>());
		//waypointer.GetComponent<Waypointer>().ChooseAndTogglePointer("Blood", true);
		aso.PlayOneShot(sl.GetGodhand());
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

	private void CheckLichAdvent()
	{
		if (timeSinceLastKill > timeTillAdvent && !adventspawned && gameActive && !playerUnit.IsDead())
		{
			distributeAndSpawn(7, "Lich");
			adventspawned = true;
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

	void CheckPowerupTiming()
	{
		if (gameActive)
		{
			foreach (Powerup powerup in pulist)
			{
				if (Time.timeSinceLevelLoad > puTimestamp && !spawnedpulist.Contains(powerup) && !playerUnit.IsDead() && !pubench.GetComponentInChildren<Pickup>())
				{
					spawnPowerup(powerup);
					spawnedpulist.Add(powerup);
					puTimestamp = Time.timeSinceLevelLoad + powerup.spawnDelay;
				}
			}
			if (pulist.Count == spawnedpulist.Count && !godhandgained)
				spawnedpulist.Clear();
		}
	}

	public void spawnPowerup(Powerup powerup)
	{
		if (providePickup(powerup.spawnName))
		{
			GameObject pickup = Instantiate(providePickup(powerup.spawnName), powerupPickupPos.transform.position, Quaternion.identity, pubench.transform);
			TMPPuName.text = pickup.GetComponent<Pickup>().GetPickupName();
			GameObject anim = Instantiate(weaponSpawnAnim, powerupPickupPos.transform.position, Quaternion.identity, pubench.transform);
			ActivateBenchParticles(pubench.GetComponentsInChildren<ParticleSystem>());
			//waypointer.GetComponent<Waypointer>().ChooseAndTogglePointer("Blood", true);
			uic.SetPuIconEnabled(true);
			Destroy(anim, 3.0f);
			pickup.name = powerup.spawnName;
			aso.PlayOneShot(pickupSpawned);
		}
	}

	internal void SpawnTutorialWeapon()
	{
		if (!tutweaponspawned)
		{
			Weapon weapon = new Weapon();
			weapon.spawnCost = 0;
			weapon.spawnIndex = 0;
			weapon.defaultKey = 3;
			weapon.spawnName = "Shotgun";
			SpawnWeapon(weapon);
			tutweaponspawned = true;
		}
	}

	internal void SpawnTutorialPassive()
	{
		if (!tutpassivespawned)
		{
			PassiveBonus passive = new PassiveBonus();
			passive.spawnCost = 0;
			passive.spawnIndex = 0;
			passive.spawnName = "Teleport";
			SpawnPassive(passive);
			tutpassivespawned = true;
		}
	}

	internal void SpawnTutorialPowerup()
	{
		if (!tutpowerupspawned)
		{
			Powerup powerup = new Powerup();
			powerup.spawnName = "QuadDamage";
			powerup.spawnIndex = 0;
			powerup.spawnDelay = 0;
			spawnPowerup(powerup);
			tutpowerupspawned = true;
		}
	}

	void CheckPassiveReady()
	{
		foreach (PassiveBonus passive in paList)
		{
			RemoveNonDemoPassive(passive);
			if (statl.GetSpiritBank() >= passive.spawnCost && !spawnedpalist.Contains(passive) && !pabench.GetComponentInChildren<Pickup>())
			{
				SpawnPassive(passive);
			}
		}

	}

	private void SpawnPassive(PassiveBonus passive)
	{
		spawnedpalist.Add(passive);
		statl.SetSpiritBank(statl.GetSpiritBank() - passive.spawnCost);
		uic.DisplayBankCost(passive.spawnCost, "Spirit");
		uic.DisplayOffering();
		uic.SetPaIconEnabled(true);
		//make the pickup
		GameObject pickup = Instantiate(providePickup(passive.spawnName), passivePickupPos.transform.position, Quaternion.identity, pabench.transform);
		TMPPaName.text = pickup.GetComponent<Pickup>().GetPickupName();
		GameObject anim = Instantiate(weaponSpawnAnim, passivePickupPos.transform.position, Quaternion.identity, pabench.transform);
		ActivateBenchParticles(pabench.GetComponentsInChildren<ParticleSystem>());
		Destroy(anim, 3.0f);
		pickup.name = passive.spawnName;
		if (statl.GetSpiritBank() < 30000)
			aso.PlayOneShot(pickupSpawned);
	}

	void CheckWeaponReady()
	{
		foreach (Weapon weapon in weaponList)
		{
			RemoveNonDemoWeapon(weapon);
			if (statl.GetWeaponBank() >= weapon.spawnCost && !spawnedweaponlist.Contains(weapon) && !wbprops.GetComponentInChildren<Pickup>())
			{
				SpawnWeapon(weapon);
			}
		}
	}

	private void SpawnWeapon(Weapon weapon)
	{
		spawnedweaponlist.Add(weapon);
		statl.SetWeaponBank(statl.GetWeaponBank() - weapon.spawnCost);
		uic.DisplayBankCost(weapon.spawnCost, "Weapon");
		uic.DisplayOffering();
		uic.SetWpIconEnabled(true);
		TMPweaponnum.text = "Key " + UIController.MakeRoman(weapon.defaultKey);
		ChooseAndSpawnPickup(weapon);
		GameObject anim = Instantiate(weaponSpawnAnim, weaponPickupPos.transform.position, Quaternion.identity, wbench.transform);
		ActivateBenchParticles(wbench.GetComponentsInChildren<ParticleSystem>());
		//waypointer.GetComponent<Waypointer>().ChooseAndTogglePointer("FNB", true);
		Destroy(anim, 3.0f);
		if (statl.GetWeaponBank() < 30000)
			aso.PlayOneShot(pickupSpawned);
	}

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

	private void SpawnWeaponPickup(String chosen)
	{
		GameObject pickup = Instantiate(providePickup(chosen), weaponPickupPos.transform.position, Quaternion.identity, wbprops.transform);
		ConfirmWeaponName(pickup);
		pickup.name = chosen;
	}

	public void GiveAllUpgrades()
	{
		Player player = playerUnit.gameObject.GetComponent<Player>();
		PassiveBench pa = pabench.GetComponent<PassiveBench>();
		PowerupBench pu = pubench.GetComponent<PowerupBench>();
		foreach (Weapon weapon in weaponList)
		{
			spawnedweaponlist.Add(weapon);
			player.GetPlayerOwnedWeapons().Add(RollAndProvideWeapon(weapon.spawnName));
		}
		player.GetPlayerOwnedWeapons().Add(provideGameObject("Devourer"));
		player.GetPlayerOwnedWeapons().Add(provideGameObject("Void Cannon"));
		player.GetPlayerOwnedWeapons().Add(provideGameObject("Gauntlets"));
		gops.SetHasGauntlets(true);
		uic.SetVoidOwned(true);
		uic.SetDevOwned(true);
		foreach (PassiveBonus pb in paList)
		{
			spawnedpalist.Add(pb);
		}
		pa.ApplyBonus("Summon Upgrade");
		pa.ApplyBonus("Friendly Juggernaut");
		pa.ApplyBonus("Death Scythe");
		pa.ApplyBonus("Health Upgrade");
		pa.ApplyBonus("Summon Upgrade");
		pa.ApplyBonus("Elite Summons");
		pa.ApplyBonus("Teleport");
		pu.ActivateWings();
		if(!gops.GetIsTutorialMode())
			player.GetPlayerOwnedWeapons().Add(provideGameObject("Godhand"));
	}

	private void ConfirmWeaponName(GameObject pickup)
	{
		if (pickup.GetComponent<Pickup>().GetPickupName() == "Snubnose")
			TMPWeaponName.text = "Leathian Snubnose";
		else
			TMPWeaponName.text = pickup.GetComponent<Pickup>().GetPickupName();
	}

	void RemoveNonDemoPassive(PassiveBonus pb)
	{
		if (gops.GetIsDemomode() && (pb.spawnName == "Scythe" || pb.spawnName == "Health") && !spawnedpalist.Contains(pb))
			spawnedpalist.Add(pb);
		PassiveBench bench = pabench.GetComponent<PassiveBench>();
		if (gops.GetIsDemomode())
		{
			bench.LockPip(5);
			bench.LockPip(6);
		}
	}
	void RemoveNonDemoWeapon(Weapon weapon)
	{
		if (gops.GetIsDemomode() && (weapon.spawnName == "Devourer" || weapon.spawnName == "Void Cannon") && !spawnedweaponlist.Contains(weapon))
			spawnedweaponlist.Add(weapon);
		WeaponBench bench = wbench.GetComponent<WeaponBench>();
		if (gops.GetIsDemomode())
		{
			bench.LockPip(5);
			bench.LockPip(6);
			bench.LockPip(7);
		}
	}
	void RemoveNonDemoSpawns(Spawn spawn)
	{
		if (gops.GetIsDemomode() && (spawn.spawnUnit == "Vizier" || spawn.spawnUnit == "SoulCollector") && !usedSpawnList.Contains(spawn))
		{
			usedSpawnList.Add(spawn);
		}
	}

	private void CheckSpawnTimingAndCount()
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
			if (Time.timeSinceLevelLoad > spawnTimestamp && !usedList.Contains(spawn) && !playerUnit.IsDead())
			{
				distributeAndSpawn(spawn);
				usedList.Add(spawn);
				spawnTimestamp = Time.timeSinceLevelLoad + ((gops.GetIsFastmode() ? spawn.spawnDelay / gops.GetFastModeFactor() : spawn.spawnDelay) * (eglactive ? EGLMultiplier : 1));
			}
		}
	}

	public void distributeAndSpawn(Spawn spawn)
	{
		List<SpawnPoint> possiblePoints = new List<SpawnPoint>(spawnPoints);
		List<SpawnPoint> usedPoints = new List<SpawnPoint>();
		int randomNr = UnityEngine.Random.Range(0, possiblePoints.Count);
		if (spawn.spawnCount > maxSpawnPoints)
			spawn.spawnCount = maxSpawnPoints;
		for (int i = 0; i < spawn.spawnCount; i++)
		{
			while (possiblePoints[randomNr] == null || usedPoints.Contains(possiblePoints[randomNr]))
			{
				randomNr = UnityEngine.Random.Range(0, possiblePoints.Count);
			}
			possiblePoints[randomNr].SpawnUnit(provideGameObject(spawn.spawnUnit));
			usedPoints.Add(possiblePoints[randomNr]);
		}

	}

	void distributeAndSpawn(int count, string spawn)
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
			case "Scythes":
				return scythes;
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
			case "Void Cannon":
				return voidcannonpickup;
			case "Crucifier":
				return crucifierpickup;
			case "Longinus":
				return longinuspickup;
			case "Triptikoss":
				return triptikosspickup;
			case "QuadDamage":
				return quaddamagepickup;
			case "SummonPlus2":
				return summonplus2pickup;
			case "Health":
				return healthpickup;
			case "Scythe":
				return scythepickup;
			case "Godhand":
				return godhandpickup;
			case "Juggernaut":
				return juggernautpickup;
			case "Elite Summons":
				return elitesummonspickup;
			case "Teleport":
				return teleportPickup;
			default:
				return null;
		}
	}
}