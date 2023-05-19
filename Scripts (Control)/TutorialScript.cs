using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class TutorialScript : MonoBehaviour
{
	[SerializeField] private TextMeshProUGUI TMPTutReminder = default;
	[SerializeField] private GameObject wbprops = default;
	[SerializeField] private GameObject paprops = default;
	[SerializeField] private GameObject puprops = default;
	[SerializeField] private PlayerInput playerInput = default;

	private bool isCheckingRequirements = false;
	private int phaseNumber = 0;
	private GameOptions gops;
	private UIController uic;
	private SpawnController sc;
	private GameManagerScript gm;
	private TextMeshProUGUI tutorialTip;
	private Animator tipTextAnimator;
	private GameObject player;
	private StatLibrary statl;
	private Player playerScript;
	private PlayerUnit playerunit;
	private GameObject cachedcurrentweapon;
	//private AudioSource easo;
	private SoundLibrary sl;
	private PlayerMovement pm;
    private GameObject enemyCollection;
	private string firstUpgradeSpawned;
	private int cachedSouls = 0;

	public int GetPhaseNumber() { return phaseNumber; }

    private void Start()
	{
		gm = GameManagerScript.GetGMScript();
		gops = GameOptions.GetGOPS();
		uic = gm.GetUIController();
		statl = gm.GetStatl();
		sl = gm.GetSoundLibrary();
		player = gm.GetPlayer();
		//easo = gm.GetEnvirionmentASO();
		pm = player.GetComponent<PlayerMovement>();
		enemyCollection = gm.GetEnemies();
		playerScript = player.GetComponent<Player>();
		playerunit = player.GetComponent<PlayerUnit>();
		sc = gm.GetSpawnController();
		if (uic && gops)
		{
			tutorialTip = uic.GetTMPTutorialPanel().GetComponentInChildren<TextMeshProUGUI>();
			tipTextAnimator = uic.GetTMPTutorialPanel().GetComponent<Animator>();
		}
		if (gops && tutorialTip)
			if (gops.GetIsTutorialMode())
				tipTextAnimator.SetBool("TutOn", true);
		TMPTutReminder.text = "";
	}

	internal void BeginTutorial()
	{
		phaseNumber = 1;
		ApplyAction(phaseNumber);
	}

	void Update()
	{
		CheckRequirements();
	}

	private void ApplyAction(int number)
	{
		switch (number)
		{
			case 1:
				StartCoroutine(ClearTutorialTip(0));
				StartCoroutine(SpawnTutorialEnemies(6.5f, 3));
				StartCoroutine(DisplayTip(1.3f, number));
				StartCoroutine(ClearTutorialTip(13.0f));
				StartCoroutine(SetIsCheckingRequirements(6.5f, true));
				break;
			case 2:
				StartCoroutine(DisplayTip(0, number));
				StartCoroutine(ClearTutorialTip(10.5f));
				StartCoroutine(SetIsCheckingRequirements(11.8f, true));
				break;
			case 3:
				StartCoroutine(DisplayTip(0, number));
				StartCoroutine(ClearTutorialTip(15.5f));
				StartCoroutine(SetIsCheckingRequirements(16.5f, true));
				break;
			case 4:
				StartCoroutine(DisplayTip(0, number));
				StartCoroutine(ClearTutorialTip(10.5f));
				StartCoroutine(SetIsCheckingRequirements(11.8f, true));
				break;
			case 5:
				StartCoroutine(DisplayTip(0, number));
				StartCoroutine(ClearTutorialTip(14.5f));
				StartCoroutine(SetIsCheckingRequirements(5.5f, true));
				break;
			case 6:
				StartCoroutine(DisplayTip(0, number));
				StartCoroutine(ClearTutorialTip(9.5f));
				StartCoroutine(SetIsCheckingRequirements(10.8f, true));
				break;
			case 7:
				StartCoroutine(DisplayTip(0, number));
				StartCoroutine(ClearTutorialTip(12.5f));
				StartCoroutine(SetIsCheckingRequirements(13.8f, true));
				break;
			case 8:
				sc.SpawnTutorialPowerup();
				StartCoroutine(DisplayTip(0, number));
				StartCoroutine(ClearTutorialTip(14.5f));
				StartCoroutine(SetIsCheckingRequirements(15.8f, true));
				break;
			case 9:
				cachedcurrentweapon = playerScript.GetCurrentWeapon();
				StartCoroutine(DisplayTip(5, number));
				StartCoroutine(ClearTutorialTip(17.5f));
				StartCoroutine(SetIsCheckingRequirements(18.8f, true));
				break;
			case 10:
				cachedSouls = statl.GetSouls();
				sc.GiveAllUpgrades();
				//uic.HandleTutorialPips();
				InvokeRepeating("SpawnEnemiesRepeating", 3, 20);
				playerScript.AnimateSwapWeapon(playerScript.GetCurrentWeapon(), "Devourer");
				//FindObjectOfType<CorporealShrine>().ActivatePip(5);
				//SoundLibrary.ResetPitch(easo);
				//easo.PlayOneShot(sl.GetPickupGet());
				StartCoroutine(DisplayTip(0, number));
				StartCoroutine(ClearTutorialTip(25.5f));
				StartCoroutine(SetIsCheckingRequirements(27.8f, true));
				break;
			case 11:
				StartCoroutine(DisplayTip(4, number));
				StartCoroutine(ClearTutorialTip(15.5f));
				break;
			default:
				break;
		}
	}

	public void SetTipText(int number)
	{
		switch (number)
		{
			case 1:
				tutorialTip.text = "Kill enemies to gain souls. Souls are the universal currency of this realm.";
				break;
			case 2:
				tutorialTip.text = $"A soul will remain when some units die. Summon friendly units by aiming at this and pressing ({UIController.GetKeyString("Summon", playerInput)}).";
				break;
			case 3:
				tutorialTip.text = $"As you collect souls, some of these souls are sacrificed to the circle in the middle. When you have collected enough souls, upgrades will spawn.\n\n Sacrifices are stronger closer to the middle circle, and will go to the nearest altar. Move around the arena to see how this works.";
				break;
			case 4:
				tutorialTip.text = firstUpgradeSpawned == "weapon" ? $"Pick up new weapons from the Corporeal (Gold) Altar." : firstUpgradeSpawned == "passive" ? $"Pick up new passive upgrades from the Holy (Blue) Altar." : "null";
				break;
			case 5:
				tutorialTip.text = firstUpgradeSpawned == "weapon" ? $"Move closer to the Holy (blue) altar to spawn passive upgrades. \n\n They'll spawn faster the closer you are to the middle circle." : firstUpgradeSpawned == "passive" ? $"Move closer to the Corporeal (Gold) altar to spawn new weapons. \n\n They'll spawn faster the closer you are to the middle circle." : "null";
				break;
			case 6:
				tutorialTip.text = firstUpgradeSpawned == "weapon" ? $"Pick up new passive upgrades from the Holy (Blue) Altar." : firstUpgradeSpawned == "passive" ? $"Pick up new weapons from the Corporeal (Gold) Altar." : "null";
				break;
			case 7:
				tutorialTip.text = $"One passive upgrade is the Teleport ability. Hold ({UIController.GetKeyString("Teleport", playerInput)}) to aim this, and release ({UIController.GetKeyString("Teleport", playerInput)}) to teleport.";
				break;
			case 8:
				tutorialTip.text = $"Sometimes temporary powerups will spawn at the Blood (Red) Altar. Acquire them for a temporary bonus. \n\n You can also acquire extra lives from this altar, but can only hold one extra life at a time.";
				break;
			case 9:
				tutorialTip.text = $"If your weapon runs out of ammo, switch it out with {UIController.GetKeyString("CycleWeapons", playerInput)} (cycle weapons), {UIController.GetKeyString("NextWeapon", playerInput)} (next weapon) or the weapon's number key. \n\n Ammo will be refilled when you swap back.";
				break;
			case 10:
				tutorialTip.text = $"Eventually you'll gain access to superweapons such as the Devourer (key 6). \n\n The Devourer and the Void Cannon are extremely powerful, yet cost souls to use. \n\n Hold down {UIController.GetKeyString("Shoot", playerInput)} to fire.";
				break;
			case 11:
				tutorialTip.text = $"This concludes the tutorial. Explore the arena and familiarise yourself with it. \n\n Lore and advanced mechanical tips are shown in the pre-game tip screen. \n\n Good luck, Necromancer.";
				break;
			case 99:
				tutorialTip.text = "Thank you for playing the Necromancer Demo! \n\n Godhood awaits in the full version...";
				break;
			default:
				break;
		}
	}

	private void SetReminderText(int number)
	{
		switch (number)
		{
			case 1:
				TMPTutReminder.text = "Kill all enemies to gain souls.";
				break;
			case 2:
				TMPTutReminder.text = $"Summon friendly units with ({UIController.GetKeyString("Summon", playerInput)}).";
				break;
			case 3:
				TMPTutReminder.text = $"Move around the arena and observe how soul sacrifices work.";
				break;
			case 4:
				TMPTutReminder.text = firstUpgradeSpawned == "weapon" ? $"Pick up new weapons from the Gold Altar." : firstUpgradeSpawned == "passive" ? $"Pick up new passive upgrades from the Blue Altar." : "null";
				break;
			case 5:
				TMPTutReminder.text = firstUpgradeSpawned == "weapon" ? $"Move closer to the Blue altar to spawn passive upgrades." : firstUpgradeSpawned == "passive" ? $"Move closer to the Gold altar to spawn new weapons." : "null";
				break;
			case 6:
				TMPTutReminder.text = firstUpgradeSpawned == "weapon" ? $"Pick up new passive upgrades from the Blue Altar." : firstUpgradeSpawned == "passive" ? $"Pick up new weapons from the Gold Altar." : "null"; break;
			case 7:
				TMPTutReminder.text = $"Hold ({UIController.GetKeyString("Teleport", playerInput)}) to aim the teleport, and release ({UIController.GetKeyString("Teleport", playerInput)}) to teleport.";
				break;
			case 8:
				TMPTutReminder.text = $"Pick up temporary powerups from the Red Altar.";
				break;
			case 9:
				TMPTutReminder.text = $"Swap weapons with ({UIController.GetKeyString("CycleWeapons", playerInput)}) (cycle weapons), ({UIController.GetKeyString("NextWeapon", playerInput)}) (next weapon) or the weapon's number key to reload.";
				break;
			case 10:
				TMPTutReminder.text = $"Switch to Devourer with ({UIController.GetKeyString("NextWeapon", playerInput)}). Fire by holding down the shoot button.";
				break;
			case 11:
				TMPTutReminder.text = $"Press ({UIController.GetKeyString("Exit", playerInput)}) to return to the main menu.";
				break;
			default:
				break;
		}
	}

	private void CheckRequirements()
	{
		if (isCheckingRequirements)
		{
			bool requirementsmet = false;
			switch (phaseNumber)
			{
				case 1:
					if (Unit.GetAllEnemies(player).Length == 0)
						requirementsmet = true;
					break;
				case 2:
					if (Resurrector.CountFriends("Friendlies") > 0)
						requirementsmet = true;
					break;
				case 3:
					//if (wbprops.GetComponentInChildren<Pickup>() || sc.GetTutWeaponSpawned())
					firstUpgradeSpawned = CheckPickupSpawned();
					if (firstUpgradeSpawned != null)
						requirementsmet = true;
					break;
				case 4:
					switch (firstUpgradeSpawned)
                    {
						case "weapon":
							if (!wbprops.GetComponentInChildren<Pickup>())
								requirementsmet = true;
							break;
						case "passive":
							if (!paprops.GetComponentInChildren<Pickup>())
								requirementsmet = true;
							break;
					}
					break;
				case 5:
					switch (firstUpgradeSpawned)
                    {
						case "weapon":
							if (paprops.GetComponentInChildren<Pickup>())
								requirementsmet = true;
							break;
						case "passive":
							if (wbprops.GetComponentInChildren<Pickup>())
								requirementsmet = true;
							break;
                    }
					break;
				case 6:
					switch (firstUpgradeSpawned)
					{
						case "weapon":
							if (!paprops.GetComponentInChildren<Pickup>())
								requirementsmet = true;
							break;
						case "passive":
							if (!wbprops.GetComponentInChildren<Pickup>())
								requirementsmet = true;
							break;
					}
					break;
				case 7:
					if (pm.GetTeleportDone())
						requirementsmet = true;
					break;
				case 8:
					if (!puprops.GetComponentInChildren<Pickup>())
						requirementsmet = true;
					break;
				case 9:
					if (cachedcurrentweapon != playerScript.GetCurrentWeapon())
						requirementsmet = true;
					break;
				case 10:
					if (cachedSouls != statl.GetSouls())
						requirementsmet = true;
					break;
				default:
					break;
			}
			if (requirementsmet)
			{
				phaseNumber++;
				ApplyAction(phaseNumber);
				isCheckingRequirements = false;
			} 
		}
	}

    private string CheckPickupSpawned()
    {
		Pickup wb = wbprops.GetComponentInChildren<Pickup>();
		Pickup passive = paprops.GetComponentInChildren<Pickup>();

		if (wb != null)
			return "weapon";
		else if (passive != null)
			return "passive";

		return null;
	}

	private IEnumerator SpawnTutorialEnemies(float delay, int count = 5)
	{
		KillFriendlies();
		yield return new WaitForSeconds(delay);
		Spawn spawn = new Spawn();
		spawn.spawnDelay = 0;
		spawn.spawnCount = count;
		spawn.spawnUnit = "TutorialRevenant";
		sc.DistributeAndSpawn(spawn);
	}

	private void SpawnEnemiesRepeating()
	{
		if (enemyCollection.transform.childCount > 20)
			return;
		Spawn spawn = new Spawn();
		spawn.spawnDelay = 0;
		spawn.spawnCount = 5;
		spawn.spawnUnit = "TutorialRevenant";
		sc.DistributeAndSpawn(spawn);
	}

	private static void KillFriendlies()
	{
		Revenant[] friendlies = FindObjectsOfType<Revenant>();
		foreach (Revenant revenant in friendlies)
			Destroy(revenant.gameObject);
	}

	private IEnumerator SetIsCheckingRequirements(float delay, bool value)
	{
		yield return new WaitForSeconds(delay);
		isCheckingRequirements = value;
	}

	public IEnumerator ClearTutorialTip(float delay)
	{
		yield return new WaitForSeconds(delay);
		tipTextAnimator.SetBool("TutOn", false);
	}

	private IEnumerator DisplayTip(float delay, int number)
	{
		yield return new WaitForSeconds(delay);
		SetTipText(number);
		SetReminderText(number);
		tipTextAnimator.SetBool("TutOn", true);
	}

	public IEnumerator DisplayTip(int number)
	{
		yield return new WaitForSeconds(1);
		SetTipText(number);
		tipTextAnimator.SetBool("TutOn", true);
	}
}
