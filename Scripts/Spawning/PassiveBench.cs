using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PassiveBench : MonoBehaviour, IOwnershipTower
{
	[Header("Components")]
	[SerializeField] private SpawnController sc = default;
	[SerializeField] private SoundLibrary sl = default;
	[SerializeField] private StatLibrary statl = default;
	[SerializeField] private TextMeshPro TMPPaName = default;
	[SerializeField] private Sprite lockedPipSprite = default;
	[SerializeField] private GameObject newBonebag = default;
	[SerializeField] private Material onMaterial = default;

	[Header("Parameters")]
	[SerializeField] private int resIncreaseFactor = default;

	[Header("Ownership Gameobjects")]
	[SerializeField] private GameObject[] OwnershipGems = default;
	[SerializeField] private GameObject[] OwnershipPips = default;

	List<Material> originalMaterials = default;
	List<MeshRenderer> gemrenderers;
	private Player player;
	private PlayerUnit unit;
	private Resurrector resurrector;
	private UIController uic;
	private int owncounter = 0;

	public List<Material> GetOriginalMaterials() { return originalMaterials; }
	public GameObject GetGem(int value) { return OwnershipGems[value]; }
	public List<MeshRenderer> GetGemRendererList() { return gemrenderers; }

	private void Start()
	{
		statl = GameManagerScript.GetGMScript().GetStatl();
		player = GameManagerScript.GetGMScript().GetPlayer().GetComponent<Player>();
		resurrector = GameManagerScript.GetGMScript().GetPlayer().GetComponent<Resurrector>();
		sl = GameManagerScript.GetGMScript().GetSoundLibrary();
		unit = player.GetComponent<PlayerUnit>();
		uic = GameManagerScript.GetGMScript().GetUIController();
		gemrenderers = new List<MeshRenderer>();
		foreach(GameObject gem in OwnershipGems)
		{
			gemrenderers.Add(gem.GetComponentInChildren<MeshRenderer>());
		}


	}
	private void OnTriggerStay(Collider other)
	{
		if (player && other.gameObject.GetComponent<Player>() && GetComponentInChildren<Pickup>())
		{
			Pickup pickup = GetComponentInChildren<Pickup>();
			string pickupname = pickup.GetPickupName();
			ApplyBonus(pickupname);
			UpdateOwnership(pickup);
			GetComponent<AudioSource>().PlayOneShot(sl.GetPickupGet());
			Destroy(pickup.gameObject);
			DisableEmission();
			uic.SetPaIconEnabled(false);
			TMPPaName.text = "";
		}
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
			case "Summon Upgrade":
				resurrector.SetResCost(resurrector.GetResCost() + resIncreaseFactor);
				resurrector.SetResMax(resurrector.GetResMax() + 1);
				resurrector.SetResRandomMax(resurrector.GetResMax() + 3);
				if (!resurrector.GetSwarmsAvailable())
					resurrector.SetSwarmsAvailable(true);
				break;
			case "Death Scythe":
				ReplaceBonebag();
				break;
			case "Health Upgrade":
				unit.SetMaxHealth(unit.GetMaxHealth() + 75);
				unit.SetHealth(unit.GetMaxHealth());
				break;
			case "Friendly Juggernaut":
				resurrector.AddToInventory(sc.provideGameObject("Friendly Juggernaut"));
				resurrector.SetSwitchingSummons(true);
				resurrector.AnimateSwapSummon();
				break;
			case "Elite Summons":
				resurrector.ReplaceSummon(0, sc.provideGameObject("Friendly Elite Revenant"));
				resurrector.SetSwitchingSummons(true);
				resurrector.AnimateSwapSummon();
				break;
			case "Teleport":
				player.GetComponentInChildren<PlayerMovement>().SetTeleportEnabled(true);
				break;
		}
	}

	private void ReplaceBonebag()
	{
		if (player.GetPlayerOwnedWeapons().Contains(sc.provideGameObject("Snubnose")))
			player.AnimateSwapWeapon(player.GetCurrentWeapon(), "Snubnose");
		else
			player.AnimateSwapWeapon(player.GetCurrentWeapon(), "BoneBag");
		sc.SetBoneBag(newBonebag);
	}

	public void UpdateOwnership(Pickup pickup)
	{
		gemrenderers[owncounter].material = onMaterial;
		OwnershipPips[owncounter].SetActive(true);
		owncounter += 1;
	}

	public void UpdateMaterial(GameObject gem)
	{
		gem.GetComponent<MeshRenderer>().material = onMaterial;
	}

	internal void LockPip(int pip)
	{
		OwnershipPips[pip].SetActive(true);
		OwnershipPips[pip].GetComponent<Image>().color = Color.white;
		OwnershipPips[pip].GetComponent<Image>().sprite = lockedPipSprite;
	}
}
