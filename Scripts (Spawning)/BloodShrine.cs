using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BloodShrine : MonoBehaviour
{
    [SerializeField] private SoundLibrary sl = default;
    [SerializeField] private GameObject shrineBody = default;
    [SerializeField] private SpawnController sc = default;
    [SerializeField] private UIController uic = default;
    [SerializeField] private ParticleSystem[] pickupParticles = default;
    private Player player;
    private List<MeshRenderer> shrineRenderers = default;
    internal Pickup currentPickup;
    private Resurrector resurrector;
    private PlayerUnit unit;
    private StatLibrary statl;

    void Start()
    {
        shrineRenderers = new List<MeshRenderer>(shrineBody.GetComponentsInChildren<MeshRenderer>());
        player = GameManagerScript.GetGMScript().GetPlayer().GetComponent<Player>();
        resurrector = GameManagerScript.GetGMScript().GetResurrector();
        unit = player.GetComponent<PlayerUnit>();
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
        //Debug.Log(" pickup.CheckTargetAndCost(\"blood\") = " + pickup.CheckTargetAndCost("blood"));
        statl.SetSouls(statl.GetSouls() - pickup.CheckTargetAndCost("blood"));
        DeactivateParticles();
        GameManagerScript.GetGMScript().GetCorpShrine().DeactivateParticles();
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
            GetComponent<AudioSource>().PlayOneShot(sl.GetPickupGet());
            Destroy(pickup.gameObject);
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
        uic.SetWpIconEnabled(false);
    }

    private void CheckHighlighting()
    {
        if (MouseLook.LookTargetShrine != "blood")
            MouseLook.RemoveHighlighting(shrineRenderers);
        else
            MouseLook.DoHighlighting(shrineRenderers);
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
