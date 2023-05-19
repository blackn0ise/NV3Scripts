using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickup : MonoBehaviour
{
    [SerializeField] public string pickupName = default;
    [SerializeField] public string pickupType = "Weapon";
    [SerializeField] public GameObject pickupBody = default;
    [SerializeField] public GameObject bodyHighlight = default;
    [SerializeField] public GameObject pickupBodyHologram = default;
	[SerializeField] public AudioSource pickupAso = default;

    private Altar altarScript;
    private UIController uic;
    private StatLibrary statl;
    private MeshRenderer[] holorenderers;
    private SkinnedMeshRenderer[] sholorenderers;
    private GameManagerScript gm;
    internal SpawnController sc;
    private bool availableForPurchase = true;
    internal bool availableForAcquire = false;
    internal bool bodyEnabled = false;

    public string GetPickupName() { return pickupName; }
    public string GetPickupType() { return pickupType; }
    public GameObject GetPickupBody() { return pickupBody; }
    public GameObject GetPickupBodyHologram() { return pickupBodyHologram; }
    public AudioSource GetPickupAso() { return pickupAso; }
    public bool GetAvailableForPurchase() { return availableForPurchase; }

    private void Awake()
    {
        if (pickupBodyHologram)
        {
            holorenderers = pickupBodyHologram.GetComponentsInChildren<MeshRenderer>();
            sholorenderers = pickupBodyHologram.GetComponentsInChildren<SkinnedMeshRenderer>();
        }
    }

    // Start is called before the first frame update
    public virtual void Start()
	{
		Initialise();
	}

	public void Initialise()
    {
        gm = FindObjectOfType<GameManagerScript>();
        sc = gm.GetSpawnController();
        uic = gm.GetUIController();
        statl = gm.GetStatl();
        altarScript = gm.GetAltarScript();
        if (pickupBodyHologram)
            pickupBodyHologram.transform.localScale *= sc.hologramScale;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Spinner.Rotate(transform, 30.0f, 1);
    }

    public void DisableComponents()
    {
        if(pickupBody && bodyHighlight)
        {
            pickupBody.SetActive(false);
            bodyHighlight.SetActive(false);
        }
        if (holorenderers != null)
            foreach (MeshRenderer mr in holorenderers)
            {
                mr.material = SpawnController.GetGreyHologram();
            }
        if (sholorenderers != null)
            foreach (SkinnedMeshRenderer mr in sholorenderers)
            {
                mr.material = SpawnController.GetGreyHologram();
            }
        availableForPurchase = true;
        bodyEnabled = false;
    }

    public void HighlightGreen()
    {
        if (!GameOptions.GetGOPS().GetCustomSoulsEnabled())
            pickupAso.Play();
        if (holorenderers != null)
            foreach (MeshRenderer mr in holorenderers)
            {
                mr.material = SpawnController.GetGreenHologram();
            }
        if (sholorenderers != null)
            foreach (SkinnedMeshRenderer mr in sholorenderers)
            {
                mr.material = SpawnController.GetGreenHologram();
            }
        availableForPurchase = false;
        sc.purchaseAvailable = true;
    }

    public void HighlightRed()
    {
        if (holorenderers != null)
            foreach (MeshRenderer mr in holorenderers)
            {
                mr.material = SpawnController.GetRedHologram();
            }
        if (sholorenderers != null)
            foreach (SkinnedMeshRenderer mr in sholorenderers)
            {
                mr.material = SpawnController.GetRedHologram();
            }
    }

    public void MakeAvailable(string shrine)
    {
        if (!GameOptions.GetGOPS().GetCustomSoulsEnabled())
            pickupAso.PlayOneShot(gm.GetSoundLibrary().GetPickupSpawned());
        if (pickupBody && bodyHighlight)
        {
            pickupBody.SetActive(true);
            bodyHighlight.SetActive(true);
        }
        MeshRenderer[] renderers = pickupBodyHologram.GetComponentsInChildren<MeshRenderer>();
        availableForAcquire = true;
        sc.awaitingAcquire = true;
        altarScript.PulseAltarParticles(CheckTargetAndCost(shrine));
        bodyEnabled = true;
        UpdateTowerText(shrine);
	}

    private void UpdateTowerText(string shrine)
    {
        switch (shrine)
        {
            case "corp":
                uic.TMPCBankNameTower.text += " ready.";
                break;
            case "holy":
                uic.TMPHBankNameTower.text += " ready.";
                break;
            case "blood":
                uic.TMPBBankNameTower.text += " ready.";
                break;
        }
    }

    public int CheckTargetAndCost(string shrine)
    {
        switch (shrine)
        {
            case "corp":
                return sc.corpCostTree[sc.upgradeLevel];
            case "holy":
                return sc.holyCostTree[sc.upgradeLevel];
            case "blood":
                return sc.bloodCostTree[sc.upgradeLevel];
            default:
                Debug.Log("no look target to buy");
                return 0;
        }
    }
}
