using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
	[SerializeField] private WeaponAnimator weaponanimator = default;
	[SerializeField] private Animator casterhandbody = default;
	[SerializeField] private PlayerUnit unit = default;
	[SerializeField] private PlayerMovement pm = default;
	[SerializeField] private PlayerInput playerInput = default;
	[SerializeField] private Resurrector resurrector = default;
	[SerializeField] private WeaponWheel weaponWheel = default;
	[SerializeField] private GameObject weaponWheelGO = default;
	[SerializeField] private AudioMixer audioMixer = default;
	[SerializeField] private PowerupBench powerupBench = default;
	private GameObject currentWeapon;
	private GameManagerScript gm;
	private UIController uic;
	private SpawnController sc;
	private GameOptions gops;
	private StatLibrary statl;
	private List<GameObject> ownedWeaponGOs;
	private Vector2 leftStickValues;
	private string previousWeapon;
	private string nextWeapon = "";
	private static bool shootingPressed = false;
	private int damageModifier = 1;
	private bool spawnHandled = false;
	private static bool isCasting = false;
	private static bool isSwapping = false;
	private bool isDashKeyPressed = false;
	private bool isWWheelpressed = false;
	private bool cachedIsWWheelGOActive = false;
	private bool currentWeaponSet = false;

	public GameObject GetCurrentWeapon() { return currentWeapon; }
	public List<GameObject> GetPlayerOwnedWeapons() { return ownedWeaponGOs; }
	public void SetIsSwapping(bool value) { isSwapping = value; }
	public static bool GetIsSwapping() { return isSwapping; }
	public static bool GetIsCasting() { return isCasting; }
	public void SetIsCasting(bool value) { isCasting = value; }
	public void SetDamageModifier(int value) { damageModifier = value; }
	public int GetDamageModifier() { return damageModifier; }
	public static bool GetShootingPressed() { return shootingPressed; }
	public void SetWWheelCooldown(bool value) { isWWheelpressed = value; }
	public bool GetIsWWheelpressed() { return isWWheelpressed; }
	public void SetLeftStickValues(Vector2 value) { leftStickValues = value; }

	private void Start()
	{
		InitialiseVariables();
		StartCoroutine(DelayPlayerInput());
	}

	private IEnumerator DelayPlayerInput()
	{
		yield return new WaitForSeconds(1);
		playerInput.enabled = true;
		if (playerInput.currentActionMap.name != "PlayerControls")
			playerInput.SwitchCurrentActionMap("PlayerControls");
	}

	private void Update()
	{
		CheckCurrentWeaponSet();
		HandleSpawn();
		if (!PlayerUnit.GetPlayerDead() && isDashKeyPressed)
			pm.AttemptDash();
		HandleFall();
		HandleTimeScale();
		SelectForWeaponWheel();
	}

	private void CheckCurrentWeaponSet()
	{
		if(!currentWeapon || !currentWeaponSet)
		{
			currentWeapon = GameObject.FindGameObjectWithTag("Weapon");
			currentWeaponSet = true;
		}

	}

	private void SelectForWeaponWheel()
	{
		//make a variable that only reflect true when the weapon wheel used to be active but no longer is
		weaponWheelGO.SetActive(isWWheelpressed);
		cachedIsWWheelGOActive = weaponWheelGO.activeInHierarchy != cachedIsWWheelGOActive;
		if (isWWheelpressed)
		{
			weaponWheel.ReadSelectionInput(leftStickValues);
			weaponWheel.CheckSelection();
			cachedIsWWheelGOActive = true;
		}
		if (cachedIsWWheelGOActive && !weaponWheelGO.activeInHierarchy)
		{
			weaponWheel.ConfirmSelection(isWWheelpressed);
			cachedIsWWheelGOActive = false; //reset
		}
	}

	private void HandleTimeScale()
	{
		if (isWWheelpressed && !powerupBench.GetIsInSlowdown())
			Time.timeScale = 0.2f;
		else if (!statl.GetPowerupActive())
		{
			Time.timeScale = 1;
			audioMixer.SetFloat("SFXPitch", 1);
		}
	}

	private void HandleSpawn()
	{
		if (!spawnHandled)
		{
			ProvideGauntletsIfFound();
			spawnHandled = true;
		}
	}

	private void HandleFall()
	{
		if (transform.position.y < -150)
			unit.KillUnit(true);
	}

	private void ProvideGauntletsIfFound()
	{
		if (gops.GetHasGauntlets())
			ownedWeaponGOs.Add(sc.provideGameObject("Gauntlets"));
	}

	private void InitialiseVariables()
	{
		gops = GameOptions.GetGOPS();
		gm = GameManagerScript.GetGMScript();
		sc = gm.GetSpawnController();
		uic = gm.GetUIController();
		statl = gm.GetStatl();
		ownedWeaponGOs = new List<GameObject>();
		currentWeapon = GameObject.FindGameObjectWithTag("Weapon");
		previousWeapon = currentWeapon.name;
		ownedWeaponGOs.Add(sc.provideGameObject("BoneBag"));
	}

	public void OnWeaponWheelCalled(InputAction.CallbackContext value)
	{
		isWWheelpressed = value.ReadValue<float>() != 0;

	}

	public void OnMouseScroll(InputAction.CallbackContext value)
	{
		if (value.ReadValue<Vector2>().y > 0)
		{
			AttemptSwapToNextWeapon();
		}
		else if (value.ReadValue<Vector2>().y < 0)
		{
			AttemptSwapToPreviousWeapon();
		}
	}

	public void OnShootPressed(InputAction.CallbackContext value)
	{
		shootingPressed = value.ReadValueAsButton();
	}

	public void OnControlsChanged()
	{
		Debug.Log("Device change detected.");
	}

	public void ToggleUI()
	{
		uic.SetUIOn(!uic.GetUIOn());
	}

	public void OnResetPressed()
	{
		gm.BeginFadeout();
	}

	public void OnExitPressed()
	{
		gm.ExitToMainMenu();
	}

	public void AttemptCycleWeapons()
	{
		if (!GetComponent<PlayerUnit>().IsDead())
			if (previousWeapon != currentWeapon.name)
				AnimateSwapWeapon(currentWeapon, previousWeapon);
	}

	public void AttemptSwapToNextWeapon()
	{
		if (!GetComponent<PlayerUnit>().IsDead())
			if (ownedWeaponGOs.Count > 1)
				SwapToNextWeapon();
	}

	public void AttemptSwapToPreviousWeapon()
	{
		if (!GetComponent<PlayerUnit>().IsDead())
			if (ownedWeaponGOs.Count > 1)
				SwapToPreviousWeapon();
	}

	public void OnW1Pressed()
	{
		if (gops.GetCameraDebugMode())
			pm.SetCdebugVelocityFactor(1);
		else if (CheckWeaponOwned("Gauntlets") && currentWeapon.name != "Gauntlets")
			AnimateSwapWeapon(currentWeapon, "Gauntlets");
	}
	public void OnW2Pressed()
	{
		if (gops.GetCameraDebugMode())
			pm.SetCdebugVelocityFactor(2);
		else if(CheckVariantsOwned("Snubnose") && CheckNotSameVariant("Snubnose", currentWeapon))
			AnimateSwapWeapon(currentWeapon, CheckTargetVariant("Snubnose"));
		else if (currentWeapon.name != "BoneBag" && CheckWeaponOwned("BoneBag"))
			AnimateSwapWeapon(currentWeapon, "BoneBag");
	}
	public void OnW3Pressed()
	{
		if (gops.GetCameraDebugMode())
			pm.SetCdebugVelocityFactor(3);
		else if(CheckVariantsOwned("Shotgun") && CheckNotSameVariant("Shotgun", currentWeapon))
			AnimateSwapWeapon(currentWeapon, CheckTargetVariant("Shotgun"));
	}
	public void OnW4Pressed()
	{
		if (gops.GetCameraDebugMode())
			pm.DecreaseWalkSpeed(15);
		else if (CheckVariantsOwned("Crucifier") && CheckNotSameVariant("Crucifier", currentWeapon))
			AnimateSwapWeapon(currentWeapon, CheckTargetVariant("Crucifier"));
	}

	public void OnW5Pressed()
	{
		if (gops.GetCameraDebugMode())
			pm.IncreaseWalkSpeed(15);
		else if (CheckVariantsOwned("Reaver") && CheckNotSameVariant("Reaver", currentWeapon))
			AnimateSwapWeapon(currentWeapon, CheckTargetVariant("Reaver"));
	}
	public void OnW6Pressed()
	{
		if (CheckWeaponOwned("Devourer") && currentWeapon.name != "Devourer")
			AnimateSwapWeapon(currentWeapon, "Devourer");
	}
	public void OnW7Pressed()
	{
		if (CheckWeaponOwned("Void Cannon") && currentWeapon.name != "Void Cannon")
			AnimateSwapWeapon(currentWeapon, "Void Cannon");
	}
	public void OnW8Pressed()
	{
		if (CheckWeaponOwned("Godhand") && currentWeapon.name != "Godhand")
			AnimateSwapWeapon(currentWeapon, "Godhand");
	}

	private bool CheckVariantsOwned(string basetype)
	{
		switch (basetype)
		{
			case "Shotgun":
				return CheckWeaponOwned("Shotgun") || CheckWeaponOwned("Banshee") || CheckWeaponOwned("Sledgehammer");
			case "Snubnose":
				return CheckWeaponOwned("Snubnose") || CheckWeaponOwned("Hand Cannon") || CheckWeaponOwned("Penance");
			case "Crucifier":
				return CheckWeaponOwned("Crucifier") || CheckWeaponOwned("Triptikoss") || CheckWeaponOwned("Longinus");
			case "Reaver":
				return CheckWeaponOwned("Reaver") || CheckWeaponOwned("Goregun") || CheckWeaponOwned("Judgement");
		}
		return false;
	}

	private bool CheckNotSameVariant(string basetype, GameObject currentWeapon)
	{
		switch (basetype)
		{
			case "Shotgun":
				return currentWeapon.name != "Shotgun" && currentWeapon.name != "Banshee" && currentWeapon.name != "Sledgehammer";
			case "Snubnose":
				return currentWeapon.name != "Snubnose" && currentWeapon.name != "Hand Cannon" && currentWeapon.name != "Penance";
			case "Crucifier":
				return currentWeapon.name != "Crucifier" && currentWeapon.name != "Triptikoss" && currentWeapon.name != "Longinus";
			case "Reaver":
				return currentWeapon.name != "Reaver" && currentWeapon.name != "Goregun" && currentWeapon.name != "Judgement";
		}
		return false;
	}

	public void OnDashKeyPressed(InputAction.CallbackContext value)
	{
		isDashKeyPressed = value.ReadValueAsButton();
	}

	public void OnSwapSummonPressed()
	{
		if (!unit.IsDead())
		{
			if (resurrector.GetAvailableSummonOptions().Count > 1)
			{
				resurrector.AnimateSwapSummon();
				resurrector.SetSwitchingSummons(true);
			}
		}
	}

	public void RaiseAndCastLHand()
	{
		casterhandbody.SetTrigger("Raise");
	}

	private void SwapToNextWeapon()
	{
		CheckRemoveBoneBag();
		int index = 0;
		index = DetermineNextWeaponIndex(index);
		AnimateSwapWeapon(currentWeapon, ownedWeaponGOs[index].name);
	}

	private void SwapToPreviousWeapon()
	{
		CheckRemoveBoneBag();
		int index = 0;
		index = DeterminePreviousWeaponIndex(index);
		AnimateSwapWeapon(currentWeapon, ownedWeaponGOs[index].name);
	}

	public void AnimateSwapWeapon(GameObject currentWeapon, string weaponName)
	{
		if (!isSwapping && !isCasting && !unit.IsDead())
		{
			CheckRemoveBoneBag();
			var array = FindObjectsOfType<MonoBehaviour>().OfType<IBasicWeapon>();
			var counter = 0;
			foreach (var item in array)
			{
				counter++;
			}
			if (counter > 1)
				previousWeapon = CheckTargetVariant("Snubnose");
			else
				previousWeapon = currentWeapon.name;
			nextWeapon = weaponName;
			weaponanimator.GetAnimator().SetTrigger("StartSwap");
			weaponanimator.GetAnimator().SetBool("IsShooting", false);
			isSwapping = true;
			uic.DisplayWeaponSelected(nextWeapon);
		}
	}

	private int DetermineNextWeaponIndex(int index)
	{
		for (int i = 0; i < ownedWeaponGOs.Count; i++)
		{
			if (currentWeapon.name == ownedWeaponGOs[i].name)
			{
				int container = i;
				if (i == ownedWeaponGOs.Count - 1)
					container = 0;
				else
					container = i + 1;
				index = container;
			}
		}

		return index;
	}

	private int DeterminePreviousWeaponIndex(int index)
	{
		for (int i = 0; i < ownedWeaponGOs.Count; i++)
		{
			if (currentWeapon.name == ownedWeaponGOs[i].name)
			{
				int container = i;
				if (i == 0)
					container = ownedWeaponGOs.Count - 1;
				else
					container = i - 1;
				index = container;
			}
		}

		return index;
	}

	public void CheckRemoveBoneBag()
	{
		bool hasbag = false;
		GameObject bag = null;
		foreach (GameObject go in ownedWeaponGOs)
		{
			if (go != null)
				if (go.name == "BoneBag")
				{
					hasbag = true;
					bag = go;
				}
		}

		if (ownedWeaponGOs.Contains(sc.provideGameObject(CheckTargetVariant("Snubnose"))) && hasbag)
		{
			GameObject variant = sc.provideGameObject(CheckTargetVariant("Snubnose"));
			ownedWeaponGOs.Remove(bag);
			ownedWeaponGOs.Remove(variant);
			List<GameObject> newlist = new List<GameObject>();
			newlist.Add(variant);
			foreach (GameObject go in ownedWeaponGOs)
			{
				newlist.Add(go);
			}
			ownedWeaponGOs = newlist;
		}
	}

	public void EquipWeapon()
	{
		if (!unit.IsDead())
		{
			if (nextWeapon == "Snubnose" || nextWeapon == "Hand Cannon" || nextWeapon == "Penance")
			{
				Destroy(currentWeapon);
				GameObject newWeapon = Instantiate(sc.provideGameObject(nextWeapon), FindObjectOfType<WeaponAnimator>().transform.position, FindObjectOfType<Camera>().transform.rotation, GameObject.Find("GunPosition").transform);
				GameObject newWeapon2 = Instantiate(sc.provideGameObject("BoneBag"), FindObjectOfType<WeaponAnimator>().transform.position, FindObjectOfType<Camera>().transform.rotation, GameObject.Find("GunPosition").transform);
				newWeapon.name = nextWeapon;
				newWeapon2.name = "BoneBag";
				nextWeapon = "";
				currentWeapon = newWeapon;
				ResetUICAmmoText();
			}
			else
			{
				foreach (GameObject weapon in GameObject.FindGameObjectsWithTag("Weapon"))
				{
					Destroy(weapon);
				}
				GameObject newWeapon = Instantiate(sc.provideGameObject(nextWeapon), FindObjectOfType<WeaponAnimator>().transform.position, FindObjectOfType<Camera>().transform.rotation, GameObject.Find("GunPosition").transform);
				newWeapon.name = nextWeapon;
				nextWeapon = "";
				currentWeapon = newWeapon;
				ResetUICAmmoText();
			}
		}
	}

	private void ResetUICAmmoText()
	{
		uic.SetAmmoUIEnabled(false);
		uic.SetBrokeUIEnabled(false);
		uic.SetCDUIEnabled(false);
	}

	public bool CheckWeaponOwned(string name)
	{
		foreach (GameObject weapon in ownedWeaponGOs)
		{
			if (weapon.name == name)
				return true;
		}
		return false;
	}

	public string CheckTargetVariant(string basetype)
	{
		switch (basetype)
		{
			case "Shotgun":
				if (CheckWeaponOwned("Shotgun"))
					return "Shotgun";
				else if (CheckWeaponOwned("Banshee"))
					return "Banshee";
				else if (CheckWeaponOwned("Sledgehammer"))
					return "Sledgehammer";
				break;
			case "Snubnose":
				if (CheckWeaponOwned("Snubnose"))
					return "Snubnose";
				else if (CheckWeaponOwned("Penance"))
					return "Penance";
				else if (CheckWeaponOwned("Hand Cannon"))
					return "Hand Cannon";
				break;
			case "Crucifier":
				if (CheckWeaponOwned("Crucifier"))
					return "Crucifier";
				else if (CheckWeaponOwned("Triptikoss"))
					return "Triptikoss";
				else if (CheckWeaponOwned("Longinus"))
					return "Longinus";
				break;
			case "Reaver":
				if (CheckWeaponOwned("Reaver"))
					return "Reaver";
				else if (CheckWeaponOwned("Judgement"))
					return "Judgement";
				else if (CheckWeaponOwned("Goregun"))
					return "Goregun";
				break;
		}
		return basetype;
	}
}
