using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class TutorialScript : MonoBehaviour
{
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
	private AudioSource easo;
	private SoundLibrary sl;
	private PlayerMovement pm;
	private int cachedSouls = 0;

	[SerializeField] private TextMeshProUGUI TMPTutReminder = default;
	[SerializeField] private GameObject wbprops = default;
	[SerializeField] private GameObject paprops = default;
	[SerializeField] private GameObject puprops = default;
	[SerializeField] private PlayerInput playerInput = default;

	private void Start()
	{
		gm = GameManagerScript.GetGMScript();
		gops = GameOptions.GetGOPS();
		uic = gm.GetUIController();
		statl = gm.GetStatl();
		sl = gm.GetSoundLibrary();
		player = gm.GetPlayer();
		easo = gm.GetEnvirionmentASO();
		pm = player.GetComponent<PlayerMovement>();
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
				StartCoroutine(SpawnTutorialEnemies(6.5f));
				StartCoroutine(DisplayTip(1.3f, number));
				StartCoroutine(ClearTutorialTip(13.0f));
				StartCoroutine(SetIsCheckingRequirements(6.5f, true));
				break;
			case 2:
				StartCoroutine(DisplayTip(0, number));
				StartCoroutine(ClearTutorialTip(6.5f));
				StartCoroutine(SetIsCheckingRequirements(7.8f, true));
				break;
			case 3:
				StartCoroutine(DisplayTip(0, number));
				StartCoroutine(ClearTutorialTip(15.5f));
				StartCoroutine(SetIsCheckingRequirements(16.8f, true));
				break;
			case 4:
				StartCoroutine(DisplayTip(0, number));
				StartCoroutine(ClearTutorialTip(15.5f));
				StartCoroutine(SetIsCheckingRequirements(16.5f, true));
				break;
			case 5:
				StartCoroutine(DisplayTip(0, number));
				StartCoroutine(ClearTutorialTip(10.5f));
				StartCoroutine(SetIsCheckingRequirements(11.8f, true));
				break;
			case 6:
				StartCoroutine(DisplayTip(0, number));
				StartCoroutine(ClearTutorialTip(14.5f));
				StartCoroutine(SetIsCheckingRequirements(15.5f, true));
				break;
			case 7:
				StartCoroutine(DisplayTip(0, number));
				StartCoroutine(ClearTutorialTip(9.5f));
				StartCoroutine(SetIsCheckingRequirements(10.8f, true));
				break;
			case 8:
				StartCoroutine(DisplayTip(0, number));
				StartCoroutine(ClearTutorialTip(12.5f));
				StartCoroutine(SetIsCheckingRequirements(13.8f, true));
				break;
			case 9:
				sc.SpawnTutorialPowerup();
				StartCoroutine(DisplayTip(0, number));
				StartCoroutine(ClearTutorialTip(14.5f));
				StartCoroutine(SetIsCheckingRequirements(15.8f, true));
				break;
			case 10:
				cachedcurrentweapon = playerScript.GetCurrentWeapon();
				StartCoroutine(DisplayTip(5, number));
				StartCoroutine(ClearTutorialTip(17.5f));
				StartCoroutine(SetIsCheckingRequirements(18.8f, true));
				break;
			case 11:
				cachedSouls = statl.GetSouls();
				sc.GiveAllUpgrades();
				uic.HandleTutorialPips();
				StartCoroutine(SpawnTutorialEnemies(3.0f));
				playerScript.AnimateSwapWeapon(playerScript.GetCurrentWeapon(), "Devourer");
				FindObjectOfType<WeaponBench>().ActivatePip(5);
				SoundLibrary.ResetPitch(easo);
				easo.PlayOneShot(sl.GetPickupGet());
				StartCoroutine(DisplayTip(0, number));
				StartCoroutine(ClearTutorialTip(25.5f));
				StartCoroutine(SetIsCheckingRequirements(27.8f, true));
				break;
			case 12:
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
				tutorialTip.text = $"You can fully heal all damage with ({UIController.GetKeyString("Heal", playerInput)}). However, this costs souls, and cost increases over time and with every use.";
				break;
			case 3:
				tutorialTip.text = $"You can summon friendly units with ({UIController.GetKeyString("Summon", playerInput)}). \n\n This also costs souls, and cost increases over time. \n\n There is a limit to how many you can summon, but eventually your limit will increase and you'll gain access to new units.";
				break;
			case 4:
				tutorialTip.text = $"Bank souls into the weapon bank using ({UIController.GetKeyString("WeaponBank", playerInput)}). Banking into the weapon bank will eventually spawn upgrades at the weapon altar.";
				break;
			case 5:
				tutorialTip.text = $"Pick up new weapons from the Flesh and Bone (Gold) Altar.";
				break;
			case 6:
				tutorialTip.text = $"Bank souls into the spirit bank using ({UIController.GetKeyString("SpiritBank", playerInput)}). Banking into the spirit bank will eventually spawn upgrades at the spirit altar.";
				break;
			case 7:
				tutorialTip.text = $"Pick up new passive upgrades from the Holy (Blue) Altar.";
				break;
			case 8:
				tutorialTip.text = $"One passive upgrade is the Teleport ability. Hold ({UIController.GetKeyString("Teleport", playerInput)}) to aim this, and release ({UIController.GetKeyString("Teleport", playerInput)}) to teleport.";
				break;
			case 9:
				tutorialTip.text = $"Sometimes temporary powerups will spawn at the Blood (Red) Altar. Acquire them for a temporary bonus.";
				break;
			case 10:
				tutorialTip.text = $"If your weapon runs out of ammo, switch it out with {UIController.GetKeyString("CycleWeapons", playerInput)} (cycle weapons), {UIController.GetKeyString("NextWeapon", playerInput)} (next weapon) or the weapon's number key. \n\n Ammo will be refilled when you swap back.";
				break;
			case 11:
				tutorialTip.text = $"Eventually you'll gain access to superweapons such as the Devourer (key 6). \n\n The Devourer and the Void Cannon are extremely powerful, yet cost souls to use. \n\n Hold down {UIController.GetKeyString("Shoot", playerInput)} to fire.";
				break;
			case 12:
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
				TMPTutReminder.text = $"Heal all damage with ({UIController.GetKeyString("Heal", playerInput)}).";
				break;
			case 3:
				TMPTutReminder.text = $"Summon friendly units with ({UIController.GetKeyString("Summon", playerInput)}).";
				break;
			case 4:
				TMPTutReminder.text = $"Bank souls into the weapon bank using ({UIController.GetKeyString("WeaponBank", playerInput)}).";
				break;
			case 5:
				TMPTutReminder.text = $"Pick up new weapons from the Gold Altar.";
				break;
			case 6:
				TMPTutReminder.text = $"Bank souls into the spirit bank using ({UIController.GetKeyString("SpiritBank", playerInput)}).";
				break;
			case 7:
				TMPTutReminder.text = $"Pick up new passive upgrades from the Blue Altar.";
				break;
			case 8:
				TMPTutReminder.text = $"Hold ({UIController.GetKeyString("Teleport", playerInput)}) to aim the teleport, and release ({UIController.GetKeyString("Teleport", playerInput)}) to teleport.";
				break;
			case 9:
				TMPTutReminder.text = $"Pick up temporary powerups from the Red Altar.";
				break;
			case 10:
				TMPTutReminder.text = $"Swap weapons with ({UIController.GetKeyString("CycleWeapons", playerInput)}) (cycle weapons), ({UIController.GetKeyString("NextWeapon", playerInput)}) (next weapon) or the weapon's number key to reload.";
				break;
			case 11:
				TMPTutReminder.text = $"Switch to Devourer with ({UIController.GetKeyString("NextWeapon", playerInput)}). Fire by holding down the shoot button.";
				break;
			case 12:
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
					if (playerunit.GetHealth() == playerunit.GetMaxHealth())
						requirementsmet = true;
					break;
				case 3:
					if (Resurrector.CountFriends("Friendlies") > 0)
						requirementsmet = true;
					break;
				case 4:
					if (wbprops.GetComponentInChildren<Pickup>() || sc.GetTutWeaponSpawned())
						requirementsmet = true;
					break;
				case 5:
					if (!wbprops.GetComponentInChildren<Pickup>())
						requirementsmet = true;
					break;
				case 6:
					if (paprops.GetComponentInChildren<Pickup>() || sc.GetTutPassiveSpawned())
						requirementsmet = true;
					break;
				case 7:
					if (!paprops.GetComponentInChildren<Pickup>())
						requirementsmet = true;
					break;
				case 8:
					if (pm.GetTeleportDone())
						requirementsmet = true;
					break;
				case 9:
					if (!puprops.GetComponentInChildren<Pickup>())
						requirementsmet = true;
					break;
				case 10:
					if (cachedcurrentweapon != playerScript.GetCurrentWeapon())
						requirementsmet = true;
					break;
				case 11:
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

	private IEnumerator SpawnTutorialEnemies(float delay)
	{
		KillFriendlies();
		yield return new WaitForSeconds(delay);
		Spawn spawn = new Spawn();
		spawn.spawnDelay = 0;
		spawn.spawnCount = 5;
		spawn.spawnUnit = "TutorialRevenant";
		sc.distributeAndSpawn(spawn);
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
