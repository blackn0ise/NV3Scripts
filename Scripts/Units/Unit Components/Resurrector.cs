using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Resurrector : MonoBehaviour
{
	[Header("Components")]
	[SerializeField] private GameObject soultracer = default;
	[SerializeField] private GameObject resUnit = default;
	[SerializeField] private GameObject spawnAnim = default;
	[SerializeField] private GameObject summontracer = default;
	[SerializeField] private GameObject spelleffect = default;
	[SerializeField] private GameObject fjugghost = default;
	[SerializeField] private GameObject revghost = default;
	[SerializeField] private GameObject elrevghost = default;
	[SerializeField] private GameObject spendtracer = default;
	[SerializeField] private GameObject reslocation = default;
	[SerializeField] private GameObject tracerlocation = default;
	[SerializeField] private Player player = default;
	[SerializeField] private PlayerUnit unit = default;
	[SerializeField] private Detrilight detrilight = default;
	[SerializeField] private Transform friendliestransform = default;
	[SerializeField] private Transform ghostPos = default;
	[SerializeField] private Transform WBenchtransform = default;
	[SerializeField] private Transform PABenchtransform = default;
	[SerializeField] private Transform spellpos = default;
	[SerializeField] private Animator gunpositionanim = default;
	[SerializeField] private Animator lefthandanim = default;

	[Header("Parameters")]
	[SerializeField] private int costlimit = 250;
	[SerializeField] private int usePenaltyIncreaseNumber = 50;
	[SerializeField] private int baserevcost = 50;
	[SerializeField] private int basejugcost = 150;
	[SerializeField] private int resRandomMax = 2;
	[SerializeField] private int healTimeFactor = 1;
	[SerializeField] private int resTimeFactor = 2;
	[SerializeField] private float resPositionVariance = 200;
	private int baserescost = 50;
	private int resmax = 1;
	private int rescost = 50;
	private GameManagerScript gm;
	private GameOptions gops;
	private SoundLibrary sl;
	private UIController uic;
	private SpawnController sc;
	private StatLibrary statl;
	private AudioSource aso;
	private bool spellavailable = false;
	private bool healavailable = false;
	private bool summonavailable = false;
	private bool wbankavailable = false;
	private bool sbankavailable = false;
	private bool switchingsummons = false;
	private bool upgradesGiven = false;
	private float temphealthcost;
	private static int healthcost;
	private bool swarmsAvailable = false;
	public string castSelected { get; set; }
	private List<GameObject> availableSummonOptions;
	private int usePenalty;

	public static int GetHealthCost() { return healthcost; }
	public int GetResCost() { return rescost; }
	public void SetResCost(int value) { rescost = value; }
	public int GetResMax() { return resmax; }
	public void SetResMax(int value) { resmax = value; }
	public int GetResRandomMax() { return resRandomMax; }
	public void SetResRandomMax(int value) { resRandomMax = value; }
	public GameObject GetResUnit() { return resUnit; }
	public void SetResUnit(GameObject value) { resUnit = value; }
	public List<GameObject> GetAvailableSummonOptions() { return availableSummonOptions; }
	public bool GetHealAvailable() { return healavailable; }
	public bool GetSpellAvailable() { return spellavailable; }
	public bool GetSummonAvailable() { return summonavailable; }
	public bool GetWBankAvailable() { return wbankavailable; }
	public bool GetSBankAvailable() { return sbankavailable; }
	public void SetSwitchingSummons(bool value) { switchingsummons = value; }
	public bool GetSwarmsAvailable() { return swarmsAvailable; }

	public void SetSwarmsAvailable(bool value) { swarmsAvailable = value; }

	void Start()
	{
		InitialiseVariables();
	}

	private void InitialiseVariables()
	{
		gm = GameManagerScript.GetGMScript();
		gops = GameOptions.GetGOPS();
		sc = gm.GetSpawnController();
		aso = GetComponent<AudioSource>();
		statl = gm.GetStatl();
		sl = gm.GetSoundLibrary();
		uic = gm.GetUIController();
		availableSummonOptions = new List<GameObject>();
		availableSummonOptions.Add(sc.provideGameObject("Friendly Revenant"));
	}

	private void Update()
	{
		CalculateCosts();
	}

	public void OnHealPressed()
	{
		castSelected = "Heal";
		BeginDTSpend();
	}

	public void OnSummonPressed()
	{
		castSelected = "Summon";
		BeginDTSpend();
	}

	public void OnWBankPressed()
	{
		castSelected = "WeaponBank";
		if (!CheckAllAcquired("Weapons"))
			BeginDTSpend();
	}

    public void OnSBankPressed()
    {
        castSelected = "SpiritBank";
        if (!CheckAllAcquired("Passives"))
            BeginDTSpend();
	}

	private bool CheckAllAcquired(string type)
	{
		switch (type)
		{
			case "Weapons":
				foreach (Weapon weapon in sc.GetSpawnedWeapons())
					Debug.Log(weapon.spawnName);
				int maxNeeded = 6;
				if (sc.GetSpawnedWeapons().Count < maxNeeded)
					return false;
				else
					return true;
			case "Passives":
				foreach (PassiveBonus pb in sc.GetSpawnedPassives())
					Debug.Log(pb.spawnName);
				int pmaxNeeded = 7;
				if (sc.GetSpawnedPassives().Count < pmaxNeeded)
					return false;
				else
					return true;
			default:
				return false;
		}

	}

	internal void AddToInventory(GameObject gameObject)
	{
		availableSummonOptions.Add(gameObject);
	}

	internal void ReplaceSummon(int replaceindex, GameObject replacement)
	{
		GetAvailableSummonOptions()[replaceindex] = replacement;
	}

	private void CalculateCosts()
	{
		CalculateHealSummonFromTime();
		ConfirmAvailabilityBools();
	}

	private void CalculateHealSummonFromTime()
	{
		CalculateHealthCost(healTimeFactor);
		CalculateResCost(resTimeFactor);
	}

	private void CalculateResCost(int timefactor)
	{
		rescost = Mathf.Clamp(baserescost + Mathf.FloorToInt(Altar.GetTimeSinceStart() / timefactor), 0, costlimit);
	}

	private void CalculateHealthCost(int timefactor)
	{
		GetHealthLossPercentage();
		temphealthcost += Altar.GetTimeSinceStart() / timefactor;
		healthcost = Mathf.FloorToInt(Mathf.Clamp(temphealthcost + usePenalty, 0, costlimit));
	}

	private void GetHealthLossPercentage()
	{
		temphealthcost = unit.GetMaxHealth() - unit.GetHealth();
		temphealthcost /= unit.GetMaxHealth();
		temphealthcost *= 100.0f;
	}

	public void BeginDTSpend()
	{
		bool prereqsmet = CheckPrereqs();
		if (prereqsmet)
		{
			if (!GetComponent<PlayerUnit>().IsDead())
			{
				AnimateCast();
				CountFriends("Friendlies");
			}
			if (gops.GetStartWithPowerup())
			{
				sc.spawnPowerup(sc.GetFirstPowerup());
				gops.SetStartWithPowerup(false);
			}
			if (gops.GetStartWithAllUpgrades() && !upgradesGiven)
			{
				sc.GiveAllUpgrades();
				upgradesGiven = true;
			}
		}
	}

	private bool CheckPrereqs()
	{
		if (castSelected == "Heal" && healavailable)
			return true;
		else if (castSelected == "Summon" && summonavailable)
			return true;
		else if (castSelected == "WeaponBank" && wbankavailable)
			return true;
		else if (castSelected == "SpiritBank" && sbankavailable)
			return true;
		else if (gops)
			if (gops.GetIsTutorialMode())
			{
				if (castSelected == "WeaponBank")
					return true;
				if (castSelected == "SpiritBank")
					return true;
			}
		return false;
	}

	private void AnimateCast()
	{
		if (!Player.GetIsSwapping() && !Player.GetIsCasting() && spellavailable)
		{
			player.SetIsCasting(true);
			gunpositionanim.SetTrigger("SpellCast");
		}
	}

	public void AnimateSwapSummon()
	{
		if (!Player.GetIsSwapping() && !Player.GetIsCasting() && switchingsummons)
		{
			player.SetIsCasting(true);
			gunpositionanim.SetTrigger("SpellCast");
		}
	}

	internal bool CheckSummonOwned(GameObject requestedsummon)
	{
		return availableSummonOptions.Contains(requestedsummon);
	}

	public static int CountFriends(string group)
	{
		var allies = new List<GameObject>();
		foreach (GameObject ally in GameObject.FindGameObjectsWithTag(group))
		{
			if (ally.GetComponent<IUnit>() != null)
			{
				if (ally && !ally.GetComponent<IUnit>().IsDead())
					allies.Add(ally);
			}
		}
		return allies.Count;
	}

	public void SelectAndApplyCast()
	{
		if (!GetComponent<PlayerUnit>().IsDead())
		{
			if (switchingsummons)
			{
				DoSwitchSummon(resUnit);
			}
			else if (healavailable && castSelected == "Heal")
			{
				DoHeal();
            }
            else if (summonavailable && castSelected == "Summon")
            {
                DoSummon();
            }
            else if (wbankavailable && castSelected == "WeaponBank")
            {
                DoWBankDeposit();
            }
            else if (sbankavailable && castSelected == "SpiritBank")
            {

                DoSBankDeposit();
            }
            if (gops)
                if (gops.GetIsTutorialMode())
                {
                    if (castSelected == "WeaponBank")
					{
						sc.SpawnTutorialWeapon();
						statl.SetSouls(5000);
					}
					if (castSelected == "SpiritBank")
					{
						sc.SpawnTutorialPassive();
						statl.SetSouls(5000);
					}
				}
		}
	}

    private void DoSwitchSummon(GameObject currentsummon)
	{
		GameObject nextsummon = DetermineNextSummon(currentsummon, GetAvailableSummonOptions());
		SetResUnit(nextsummon);
		lefthandanim.SetTrigger("Bank");
		aso.PlayOneShot(sl.GetDetriHeal());
		detrilight.CreateSpellEffect("Summon", spelleffect, spellpos);
		GameObject ghost = null;
		switch (resUnit.name)
		{
			case "FriendlyRevenant":
				ghost = Instantiate(revghost, ghostPos.position, ghostPos.rotation, ghostPos);
				UpdateSummonStats("FriendlyRevenant");
				break;
			case "FriendlyJuggernaut":
				ghost = Instantiate(fjugghost, ghostPos.position, ghostPos.rotation, ghostPos);
				UpdateSummonStats("FriendlyJuggernaut");
				break;
			case "FriendlyEliteRevenant":
				ghost = Instantiate(elrevghost, ghostPos.position, ghostPos.rotation, ghostPos);
				UpdateSummonStats("FriendlyJuggernaut");
				break;
		}
		Debug.Log("switched to summon " + resUnit.name);
	}

	private void UpdateSummonStats(string summonname)
	{
		switch (summonname)
		{
			case "FriendlyRevenant":
				baserescost = baserevcost;
				break;
			case "FriendlyEliteRevenant":
				baserescost = baserevcost;
				break;
			case "FriendlyJuggernaut":
				baserescost = basejugcost;
				break;
		}
	}

	private void DoHeal()
	{
		int temphealthcost = healthcost;
		lefthandanim.SetTrigger("Heal");
		unit.SetHealth(unit.GetMaxHealth());
		statl.SetSouls(statl.GetSouls() - temphealthcost);
		usePenalty += usePenaltyIncreaseNumber;
		uic.DisplaySoulCost(temphealthcost);
		aso.PlayOneShot(sl.GetDetriHeal());
		detrilight.CreateSpellEffect("Heal", spelleffect, spellpos);
	}

	private void DoSummon()
	{
		int temprescost = rescost;
		lefthandanim.SetTrigger("Summon");
		aso.PlayOneShot(sl.GetDetriRes());
		detrilight.CreateSpellEffect("Summon", spelleffect, spellpos);
		int finalrescount = CalculateFinalResCount();
		for (int i = 0; i < finalrescount; i++)
		{
			if (statl.GetSouls() > temprescost && CountFriends("Friendlies") < finalrescount)
			{
				statl.SetSouls(statl.GetSouls() - temprescost);
				uic.DisplaySoulCost(temprescost);
				Altar.ResurrectUnit(resPositionVariance, resUnit, spawnAnim, summontracer, reslocation.transform.position, tracerlocation.transform.position, transform.rotation, friendliestransform);
			}
		}
	}

	private int CalculateFinalResCount()
	{
		bool swarmable = swarmsAvailable && (resUnit.name == "FriendlyRevenant" || resUnit.name == "FriendlyEliteRevenant");
		int finalrescount;
		if (swarmable)
			finalrescount = GetResMax() + UnityEngine.Random.Range(1, resRandomMax);
		else
			finalrescount = GetResMax();
		return finalrescount;
	}

	public GameObject DetermineNextSummon(GameObject currentsummon, List<GameObject> summonlist)
	{
		int index = 0;

		for (int i = 0; i < summonlist.Count; i++)
		{
			if (currentsummon.name == summonlist[i].name)
			{
				int container = i;
				if (i == summonlist.Count - 1)
					container = 0;
				else
					container = i + 1;
				index = container;
			}
		}
		return summonlist[index];
	}

	private void DoWBankDeposit()
	{
		int tempbank = statl.GetSouls();
		lefthandanim.SetTrigger("Bank");
		statl.SetWeaponBank(statl.GetWeaponBank() + (gops.GetIsFastmode() || gops.GetBigGainsMode() ? statl.GetSouls() * gops.GetSoulMultiplier() : statl.GetSouls()));
		statl.SetSouls(0);
		uic.DisplaySoulCost(tempbank);
		uic.DisplayBankAdd(tempbank, "Weapon");
		aso.PlayOneShot(sl.GetDetriBank());
		detrilight.CreateSpellEffect("Bank", spelleffect, spellpos);
		GameObject tracer = Instantiate(spendtracer, tracerlocation.transform.position, transform.rotation);
		tracer.GetComponent<HomingProjectile>().SetDefaultTarget(WBenchtransform);
		Destroy(tracer, 4);
	}

	private void DoSBankDeposit()
	{
		int tempbank = statl.GetSouls();
		lefthandanim.SetTrigger("Bank");
		statl.SetSpiritBank(statl.GetSpiritBank() + (gops.GetIsFastmode() || gops.GetBigGainsMode() ? statl.GetSouls() * gops.GetSoulMultiplier() : statl.GetSouls()));
		statl.SetSouls(0);
		uic.DisplaySoulCost(tempbank);
		uic.DisplayBankAdd(tempbank, "Spirit");
		aso.PlayOneShot(sl.GetDetriBank());
		detrilight.CreateSpellEffect("Bank", spelleffect, spellpos);
		GameObject tracer = Instantiate(soultracer, tracerlocation.transform.position, transform.rotation);
		tracer.GetComponent<HomingProjectile>().SetDefaultTarget(PABenchtransform);
		Destroy(tracer, 4);
	}

	private void ConfirmAvailabilityBools()
	{
		if (unit.GetHealth() < unit.GetMaxHealth() && statl.GetSouls() > healthcost && healthcost != 0)
		{
			spellavailable = true;
			healavailable = true;
		}
		else
		{
			healavailable = false;
		}
		if (statl.GetSouls() > rescost && CountFriends("Friendlies") < GetResMax())
		{
			spellavailable = true;
			summonavailable = true;
		}
		else
		{
			summonavailable = false;
		}
		if (statl.GetSouls() > 0)
		{
			spellavailable = true;
			wbankavailable = true;
			sbankavailable = true;
		}
		else
		{
			wbankavailable = false;
			sbankavailable = false;
		}

		if (!healavailable && !summonavailable && !wbankavailable && !sbankavailable)
			spellavailable = false;

	}
}
