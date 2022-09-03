using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WeaponBench : MonoBehaviour, IOwnershipTower
{
	[Header("Components")]
	[SerializeField] private SpawnController sc = default;
	[SerializeField] private UIController uic = default;
	[SerializeField] private SoundLibrary sl = default;
	[SerializeField] private GameObject wbmanager = default;
	[SerializeField] private TextMeshPro TMPWeaponNum = default;
	[SerializeField] private TextMeshPro TMPWeaponName = default;
	[SerializeField] private Sprite lockedPipSprite = default;
	[SerializeField] private Material onMaterial = default;

	[Header("Ownership Gameobjects")]
	[SerializeField] private GameObject[] OwnershipGems = default;
	[SerializeField] private GameObject[] OwnershipPips = default;

	List<Material> originalMaterials;
	List<MeshRenderer> renderers;
	public List<Material> GetOriginalMaterials() { return originalMaterials; }
	public GameObject GetGem(int value) { return OwnershipGems[value]; }
	public List<MeshRenderer> GetGemRendererList() { return renderers; }
	private Player player;
	private AudioSource aso;

	void Start()
	{
		aso = GetComponent<AudioSource>();
		player = GameManagerScript.GetGMScript().GetPlayer().GetComponent<Player>();
		originalMaterials = new List<Material>();
		renderers = new List<MeshRenderer>(GetComponentsInChildren<MeshRenderer>());
		if (FindObjectOfType<Detrilight>())
			FindObjectOfType<Detrilight>().saveOriginalMaterials(originalMaterials, gameObject);

	}

	private void OnTriggerStay(Collider other)
	{
		if (player && other.gameObject.GetComponent<Player>() && wbmanager.GetComponentInChildren<Pickup>())
		{
			Pickup pickup = wbmanager.GetComponentInChildren<Pickup>();
			player.GetPlayerOwnedWeapons().Add(sc.provideGameObject(pickup.GetPickupName()));
			UpdateOwnership(pickup);
			player.AnimateSwapWeapon(player.GetCurrentWeapon(), pickup.GetPickupName());
			GetComponent<AudioSource>().PlayOneShot(sl.GetPickupGet());
			Destroy(pickup.gameObject);
			DisableEmission();
			uic.SetWpIconEnabled(false);
			ClearText();

		}
	}

	internal void ActivatePip(int pip)
	{
		OwnershipPips[pip].SetActive(true);
	}

	internal void LockPip(int pip)
	{
		OwnershipPips[pip].SetActive(true);
		OwnershipPips[pip].GetComponent<Image>().color = Color.white;
		OwnershipPips[pip].GetComponent<Image>().sprite = lockedPipSprite;
	}

	public void UpdateOwnership(Pickup pickup)
	{
		switch (pickup.GetPickupName())
		{
			case "Shotgun":
			case "Banshee":
			case "Sledgehammer":
				UpdateMaterial(OwnershipGems[1]);
				UpdateUI(OwnershipPips[1]);
				break;
			case "Snubnose":
			case "Hand Cannon":
			case "Penance":
				UpdateMaterial(OwnershipGems[2]);
				UpdateUI(OwnershipPips[2]);
				break;
			case "Crucifier":
			case "Triptikoss":
			case "Longinus":
				UpdateMaterial(OwnershipGems[3]);
				UpdateUI(OwnershipPips[3]);
				break;
			case "Reaver":
			case "Judgement":
			case "Goregun":
				UpdateMaterial(OwnershipGems[4]);
				UpdateUI(OwnershipPips[4]);
				break;
			case "Devourer":
				UpdateMaterial(OwnershipGems[5]);
				UpdateUI(OwnershipPips[5]);
				break;
			case "Void Cannon":
				UpdateMaterial(OwnershipGems[6]);
				UpdateUI(OwnershipPips[6]);
				break;
		}
		UpdateUICSWOwnership();
	}

	private void UpdateUI(GameObject gameObject)
	{
		gameObject.SetActive(true);
	}

	public void UpdateMaterial(GameObject gem)
	{
		gem.GetComponent<MeshRenderer>().material = onMaterial;
	}

	public void UpdateUICSWOwnership()
	{
		if (wbmanager.GetComponentInChildren<Pickup>().GetPickupName() == "Void Cannon")
			uic.SetVoidOwned(true);
		else if (wbmanager.GetComponentInChildren<Pickup>().GetPickupName() == "Devourer")
			uic.SetDevOwned(true);
	}

	private void ClearText()
	{
		TMPWeaponNum.text = "";
		TMPWeaponName.text = "";
	}

	private void DisableEmission()
	{
		foreach (ParticleSystem ps in GetComponentsInChildren<ParticleSystem>())
		{
			ps.Stop();
			ps.GetComponentInChildren<Light>().range = 0;
			sc.GetWaypointer().GetComponent<Waypointer>().ChooseAndTogglePointer("FNB", false);
		}
	}
}
