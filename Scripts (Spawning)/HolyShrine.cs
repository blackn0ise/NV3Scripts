using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HolyShrine : MonoBehaviour
{
	[Header("Components")]
	[SerializeField] private SpawnController sc = default;
	[SerializeField] private SoundLibrary sl = default;
	[SerializeField] private StatLibrary statl = default;
	//[SerializeField] private GameObject newBonebag = default;
	[SerializeField] private GameObject shrineBody = default;
	[SerializeField] private ParticleSystem[] pickupParticles = default;

	private List<Material> originalMaterials = default;
	private List<MeshRenderer> shrineRenderers = default;
	private Player player;
	private PlayerUnit unit;
	private Resurrector resurrector;
	private UIController uic;
	internal Pickup currentPickup;

	public List<Material> GetOriginalMaterials() { return originalMaterials; }

	private void Start()
	{
		statl = GameManagerScript.GetGMScript().GetStatl();
		player = GameManagerScript.GetGMScript().GetPlayer().GetComponent<Player>();
		resurrector = GameManagerScript.GetGMScript().GetResurrector();
		sl = GameManagerScript.GetGMScript().GetSoundLibrary();
		unit = player.GetComponent<PlayerUnit>();
		uic = GameManagerScript.GetGMScript().GetUIController();
		shrineRenderers = new List<MeshRenderer>(shrineBody.GetComponentsInChildren<MeshRenderer>());
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
		//Debug.Log(" pickup.CheckTargetAndCost(\"holy\") = " + pickup.CheckTargetAndCost("holy"));
		statl.SetSouls(statl.GetSouls() - pickup.CheckTargetAndCost("holy"));
		DeactivateParticles();
		GameManagerScript.GetGMScript().GetCorpShrine().DeactivateParticles();
		GameManagerScript.GetGMScript().GetBloodShrine().DeactivateParticles();
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

	private void HandlePassiveGet(Collider other, Pickup pickup)
    {
        if (pickup.pickupType != "Augment")
            return;
        if (player && other.gameObject.GetComponent<Player>() && GetComponentInChildren<Pickup>())
        {
            pickup = GetComponentInChildren<Pickup>();
            string pickupname = pickup.GetPickupName();
            ApplyBonus(pickupname);
            //UpdateOwnership(pickup);
            GetComponent<AudioSource>().PlayOneShot(sl.GetPickupGet());
            Destroy(pickup.gameObject);
            //DisableEmission();
            uic.SetPaIconEnabled(false);
        }
    }

    private void HandleWeaponGet(Pickup pickup)
    {
        if (pickup.pickupType != "Weapon")
            return;
        pickup = GetComponentInChildren<Pickup>();
        player.GetPlayerOwnedWeapons().Add(sc.provideGameObject(pickup.GetPickupName()));
		player.AnimateSwapWeapon(player.GetCurrentWeapon(), pickup.GetPickupName());
		GetComponent<AudioSource>().PlayOneShot(sl.GetPickupGet());
		//DisableEmission();
		uic.SetWpIconEnabled(false);
		//ClearText();
	}

	private void CheckHighlighting()
	{
		if (MouseLook.LookTargetShrine != "holy")
			MouseLook.RemoveHighlighting(shrineRenderers);
		else
			MouseLook.DoHighlighting(shrineRenderers);
	}

	private void DisableEmission()
	{
		foreach (ParticleSystem ps in GetComponentsInChildren<ParticleSystem>())
		{
			ps.Stop();
			ps.GetComponentInChildren<Light>().range = 0;
			sc.GetWaypointer().GetComponent<Waypointer>().ChooseAndTogglePointer("Holy", false);
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

	private void ReplaceBonebag()
	{
		/*if (player.GetPlayerOwnedWeapons().Contains(sc.provideGameObject("Snubnose")))
			player.AnimateSwapWeapon(player.GetCurrentWeapon(), "Snubnose");
		else
			player.AnimateSwapWeapon(player.GetCurrentWeapon(), "BoneBag");
		sc.SetBoneBag(newBonebag);*/
	}

	//public void UpdateOwnership(Pickup pickup)
	//{
	//	OwnershipPips[owncounter].SetActive(true);
	//	owncounter += 1;
	//}

/*	public void UpdateMaterial(GameObject gem)
	{
		gem.GetComponent<MeshRenderer>().material = onMaterial;
	}*/

	internal void LockPip(int pip)
	{
		//OwnershipPips[pip].SetActive(true);
		//OwnershipPips[pip].GetComponent<Image>().color = Color.white;
		//OwnershipPips[pip].GetComponent<Image>().sprite = lockedPipSprite;
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
}
