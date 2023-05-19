using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CorporealShrine : MonoBehaviour
{
	[Header("Components")]
	[SerializeField] private SpawnController sc = default;
	[SerializeField] private UIController uic = default;
	[SerializeField] private SoundLibrary sl = default;
	[SerializeField] private Material onMaterial = default;
	[SerializeField] private GameObject shrineBody = default;
	[SerializeField] private ParticleSystem[] pickupParticles = default;

	[Header("Ownership Gameobjects")]
	[SerializeField] private GameObject[] OwnershipPips = default;

	private List<Material> originalMaterials;
	private List<MeshRenderer> shrineRenderers = default;
	public List<Material> GetOriginalMaterials() { return originalMaterials; }
	private Player player;
	internal Pickup currentPickup;
    private Resurrector resurrector;
	private StatLibrary statl;

	void Start()
	{
		player = GameManagerScript.GetGMScript().GetPlayer().GetComponent<Player>();
		originalMaterials = new List<Material>();
		resurrector = GameManagerScript.GetGMScript().GetResurrector();
		shrineRenderers = new List<MeshRenderer>(shrineBody.GetComponentsInChildren<MeshRenderer>());
		if (FindObjectOfType<Detrilight>())
			FindObjectOfType<Detrilight>().saveOriginalMaterials(originalMaterials, gameObject);
		statl = GameManagerScript.GetGMScript().GetStatl();
	}

	void Update()
    {
		CheckHighlighting();
	}

	private void OnTriggerStay(Collider other)
	{
		Pickup pickup = GetComponentInChildren<Pickup>();
		bool playerhere = player && other.gameObject.GetComponent<Player>();
		bool pickuphere = currentPickup != null;
		if (!playerhere)
			return;
		if (!pickuphere)
			return;
		if (!currentPickup.availableForAcquire)
			return;
		if (sc.onCooldown)
			return;
		//Debug.Log(" pickup.CheckTargetAndCost(\"corp\") = " + pickup.CheckTargetAndCost("corp"));
		statl.SetSouls(statl.GetSouls() - pickup.CheckTargetAndCost("corp"));
		DeactivateParticles();
		GameManagerScript.GetGMScript().GetBloodShrine().DeactivateParticles();
		GameManagerScript.GetGMScript().GetHolyShrine().DeactivateParticles();
		HandleWeaponGet(pickup);
		HandlePassiveGet(other, pickup);
		HandleNextPurchase();
	}

	private void HandleNextPurchase()
	{
		sc.awaitingAcquire = false;
        sc.upgradeLevel++;
        uic.ActivateGem(sc.upgradeLevel);
        sc.DisplayNextPurchaseOptions();
        StartCoroutine(sc.PlaceOnCooldown());
        //sc.awaitingNextPurchase = true;
    }

    private void HandleWeaponGet(Pickup pickup)
    {
        if (pickup.pickupType != "Weapon")
            return;
        pickup = GetComponentInChildren<Pickup>();
        player.GetPlayerOwnedWeapons().Add(sc.provideGameObject(pickup.GetPickupName()));
        UpdateOwnership(pickup);
        player.AnimateSwapWeapon(player.GetCurrentWeapon(), pickup.GetPickupName());
        GetComponent<AudioSource>().PlayOneShot(sl.GetPickupGet());
        //DisableEmission();
        uic.SetWpIconEnabled(false);
    }

    private void HandlePassiveGet(Collider other, Pickup pickup)
    {
        if (pickup.pickupType != "Augment")
            return;
        if (player && other.gameObject.GetComponent<Player>() && GetComponentInChildren<Pickup>())
        {
            pickup = GetComponentInChildren<Pickup>();
			string pickupname = pickup.GetPickupName();
			ApplyBonus(pickupname);
			UpdateOwnership(pickup);
			GetComponent<AudioSource>().PlayOneShot(sl.GetPickupGet());
			Destroy(pickup.gameObject);
			//DisableEmission();
			//uic.SetPaIconEnablead(false);
			//TMPPaName.text = "";
		}
	}

	public void ApplyBonus(string pickupname)
	{
		switch (pickupname)
		{
			case "Mass Summons":
				resurrector.SetResMax(resurrector.GetResMax() + 5);
				Resurrector.resSoulDropRate = 2;
				break;
			case "Friendly Juggernaut":
				resurrector.ReplaceSummon(0, sc.provideGameObject("Friendly Juggernaut"));
				resurrector.SetSwitchingSummons(true);
				resurrector.AnimateSwapSummon();
				resurrector.SetResMax(resurrector.GetResMax() + 2);
				break;
			case "Elite Summons":
				resurrector.ReplaceSummon(0, sc.provideGameObject("Friendly Elite Revenant"));
				resurrector.SetSwitchingSummons(true);
				resurrector.AnimateSwapSummon();
				resurrector.SetResMax(resurrector.GetResMax() + 2);
				break;
			case "Teleport":
				player.GetComponentInChildren<PlayerMovement>().SetTeleportEnabled(true);
				break;
			case "Assimilation":
				player.GetComponentInChildren<PlayerMovement>().SetAssimilationEnabled(true);
				break;
			case "Viper":
				player.GetComponentInChildren<PlayerMovement>().SetViperEnabled(true);
				break;
		}
	}

	private void CheckHighlighting()
	{
		if (MouseLook.LookTargetShrine != "corp")
			MouseLook.RemoveHighlighting(shrineRenderers);
		else
			MouseLook.DoHighlighting(shrineRenderers);
	}

	private void UpdateUI(GameObject gameObject)
	{
		gameObject.SetActive(true);

	}

	public void UpdateMaterial(GameObject gem)
	{
		gem.GetComponent<MeshRenderer>().material = onMaterial;
	}

    public void ActivateParticles()
	{
		foreach (ParticleSystem ps in pickupParticles)
		{
			if (ps == null)
				continue;
			ps.Play();
		}
	}

	public void DeactivateParticles()
	{
		foreach (ParticleSystem ps in pickupParticles)
		{
			if (ps == null)
				continue;
			ps.Stop();
		}
	}

	#region unused

	internal void ActivatePip(int pip)
	{
		OwnershipPips[pip].SetActive(true);
	}

	internal void LockPip(int pip)
	{
		//OwnershipPips[pip].SetActive(true);
		//OwnershipPips[pip].GetComponent<Image>().color = Color.white;
		//OwnershipPips[pip].GetComponent<Image>().sprite = lockedPipSprite;
	}

	public void UpdateOwnership(Pickup pickup)
	{
		//switch (pickup.GetPickupName())
		//{
		//	case "Shotgun":
		//	case "Banshee":
		//	case "Sledgehammer":
		//		UpdateUI(OwnershipPips[1]);
		//		break;
		//	case "Snubnose":
		//	case "Hand Cannon":
		//	case "Penance":
		//		UpdateUI(OwnershipPips[2]);
		//		break;
		//	case "Crucifier":
		//	case "Triptikoss":
		//	case "Longinus":
		//		UpdateUI(OwnershipPips[3]);
		//		break;
		//	case "Reaver":
		//	case "Judgement":
		//	case "Goregun":
		//		UpdateUI(OwnershipPips[4]);
		//		break;
		//	case "Devourer":
		//		UpdateUI(OwnershipPips[5]);
		//		break;
		//	case "Void Cannon":
		//		UpdateUI(OwnershipPips[6]);
		//		break;
		//}
		//UpdateUICSWOwnership();
	}


	public void UpdateUICSWOwnership()
	{
		if (GetComponentInChildren<Pickup>().GetPickupName() == "Void Cannon")
			uic.SetVoidOwned(true);
		else if (GetComponentInChildren<Pickup>().GetPickupName() == "Devourer")
			uic.SetDevOwned(true);
	}
	#endregion
}
