﻿using Ara;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PowerupBench : MonoBehaviour
{
	#region variables

	[Header("Components")]
	[SerializeField] private AudioMixer audioMixer = default;
	[SerializeField] private SoundLibrary sl = default;
	[SerializeField] private StatLibrary statl = default;
	[SerializeField] private SpawnController sc = default;
	[SerializeField] private GameObject aura = default;
	[SerializeField] private AudioClip countdown = default;
	[SerializeField] private TextMeshPro TMPPuName = default;
	[SerializeField] private GameObject timerbar = default;
	[SerializeField] private MeshRenderer resGemRenderer = default;
	[SerializeField] private Material onMaterial = default;
	[SerializeField] private Material offMaterial = default;
	[SerializeField] private Image berserkborder = default;
	[SerializeField] private AudioSource aso = default;
	[SerializeField] private GameObject shrineBody = default;
	[SerializeField] private ParticleSystem[] pickupParticles = default;

	[Header("Parameters")]
	[SerializeField] private float putimeout = default;
	[SerializeField] private float slowdownamount = 0.2f;
	[SerializeField] private float rescalefactor = 5;
	[SerializeField] private float jumpmultiplyfactor = 2.75f;
	[SerializeField] private float movemultiplyfactor = 1.3f;
	[SerializeField] private int wingsthreshold = 3000;

	private bool isInSlowdown = false;
	private bool fadingin = false;
	private bool wingsspawned = false;
	private float fixedDeltaTime;
	private float powerupprogress = 0;
	private string currentPickup = "";
	private int countdowntime = 5;
	internal int resesAcquired = 0;
	private MeshRenderer timerrenderer;
	private GameObject aurainstance;
	private Player player;
	private PlayerMovement pm;
	private UIController uic;
	private CorporealShrine weaponBench;
	private GameObject ownershipGem;
	private GameOptions gops;
	private List<MeshRenderer> shrineRenderers = default;
	private Vector3 initialBarScale = Vector3.zero;

    public bool GetIsInSlowdown() { return isInSlowdown; }
	public void SetResOff() { resGemRenderer.material = offMaterial; }
	#endregion

	#region startup

	private void Awake()
	{
		// Make a copy of the fixedDeltaTime, it defaults to 0.02f, but it can be changed in the editor
		fixedDeltaTime = Time.fixedDeltaTime;
		initialBarScale = timerbar.transform.localScale;
		weaponBench = FindObjectOfType<CorporealShrine>();
		timerrenderer = timerbar.GetComponentInChildren<MeshRenderer>();
		//if (SceneManager.GetActiveScene().name == "Game")
		//	ownershipGem = weaponBench.GetGem(7);

	}

	private void Start()
	{
		uic = GameManagerScript.GetGMScript().GetUIController();
		player = GameManagerScript.GetGMScript().GetPlayer().GetComponent<Player>();
		gops = GameOptions.GetGOPS();
		if (player)
			pm = GameManagerScript.GetGMScript().GetPlayer().GetComponent<PlayerMovement>();
		shrineRenderers = new List<MeshRenderer>(shrineBody.GetComponentsInChildren<MeshRenderer>());
		TMPPuName.text = "Resurrections and\nPowerups spawn\nhere.";
	}

	#endregion

	#region updates

	private void Update()
	{
		CheckWingSpawn();
		UpdatePowerupElapsed();
		ConfirmFixedDeltaTime();
		RampSpeedUp();
		FadeOutBorder();
		FadeInBorder();
		CheckHighlighting();
	}

	private void CheckWingSpawn()
	{
		if (statl.GetBloodBank() > wingsthreshold && !wingsspawned && !gops.GetIsDemomode())
			ActivateWings();
	}

	private void UpdatePowerupElapsed()
	{
		if (statl.GetPowerupActive() && powerupprogress > 0)
		{
			powerupprogress -= Time.deltaTime;
			Vector3 newscale = initialBarScale;
			newscale.y *= (powerupprogress / putimeout);
			timerbar.transform.localScale = newscale;
			//DebugBar(newscale);
		}


	}

	private void ConfirmFixedDeltaTime()
	{
		Time.fixedDeltaTime = fixedDeltaTime * Time.timeScale;
	}

	private void RampSpeedUp()
	{
		float deltatime = Time.deltaTime;
		float pitch;
		audioMixer.GetFloat("SFXPitch", out pitch);
		bool underlevel = Time.timeScale < 1;
		bool doublelevel = Time.timeScale > 0.7f;
		Color color = berserkborder.color;
		if (isInSlowdown && underlevel)
		{
			ScaleTime(deltatime, doublelevel, color);
		}
		else if (isInSlowdown)
		{
			ResetTime(color);
		}

	}

	private void FadeOutBorder()
	{
		if (!isInSlowdown && berserkborder.color.a > 0 && statl.GetPowerupActive() == false)
		{
			Color color = berserkborder.color;
			color.a -= Time.deltaTime;
			berserkborder.color = color;
		}
	}

	private void FadeInBorder()
	{
		if (fadingin && berserkborder.color.a <= 1 && statl.GetPowerupActive() == true)
		{
			Color color = berserkborder.color;
			color.a += Time.deltaTime * 5;
			berserkborder.color = color;
		}
		else if (berserkborder.color.a >= 1)
		{
			fadingin = false;
		}
	}

	private void CheckHighlighting()
	{
		if (MouseLook.LookTargetShrine != "blood")
			MouseLook.RemoveHighlighting(shrineRenderers);
		else
			MouseLook.DoHighlighting(shrineRenderers);
	}

	#endregion

	#region update helpers

	private void ResetTime(Color color)
	{
		Time.timeScale = 1;
		audioMixer.SetFloat("SFXPitch", 1);
		isInSlowdown = false;
	}

	private void ScaleTime(float deltatime, bool doublelevel, Color color)
	{
		Time.timeScale += deltatime * (doublelevel ? rescalefactor * 3 : rescalefactor);
		audioMixer.SetFloat("SFXPitch", Time.timeScale);
		if (!fadingin)
		{
			color.a = 1.0f - Time.timeScale * 0.7f;
			berserkborder.color = color;
		}
	}

	#endregion

	#region trigger event (pickup get or res activate)

	private void OnTriggerStay(Collider other)
	{
		if (player && other.gameObject.GetComponent<Player>() && GetComponentInChildren<Pickup>())
		{
			Pickup pickup = GetComponentInChildren<Pickup>();
			currentPickup = pickup.GetPickupName();
			StartCoroutine(ApplyPowerup());
			Destroy(pickup.gameObject);
			//DisableEmission();
			uic.SetPuIconEnabled(false);
			TMPPuName.text = "Awaiting powerup...";
			DeactivateParticles();
			sc.powerupAvailable = false;
		}

	}

	IEnumerator ApplyPowerup()
	{
		Vector3 auraposition = player.transform.position;
		auraposition.z += 3;
		auraposition.y += 3;
		switch (currentPickup)
        {
            case "Annihilation":
                SlowDownTimeAndPitch();
                if (player.GetDamageModifier() == 1)
                {
                    ApplyAura(auraposition);
                    player.SetDamageModifier(4);
                    if (!aso.isPlaying)
                        aso.Play();

                    //countdown
                    statl.SetPowerupActive(true);
					timerrenderer.material = onMaterial;
					powerupprogress = putimeout;
					yield return new WaitForSeconds(putimeout - countdowntime);
					StartCoroutine("CountDown");
				}
				break;

			case "Resurrection":
				if (!aso.isPlaying)
                {
					aso.PlayOneShot(sl.GetPickupGet());
					aso.PlayOneShot(sl.pipGainedPing);
				}
				player.GetComponent<PlayerUnit>().SetHasResurrection(true);
                resesAcquired++;
                resGemRenderer.material = onMaterial;
                yield return new WaitForSeconds(0);
                break;

            case "Godhand":
                SlowDownTimeAndPitch();
                if (!aso.isPlaying)
                    aso.Play();
                player.GetPlayerOwnedWeapons().Add(sc.provideGameObject("Godhand"));
                uic.ActivateGem(10);
                player.AnimateSwapWeapon(player.GetCurrentWeapon(), "Godhand");
                Destroy(GetComponentInChildren<Pickup>().gameObject);
				//DisableEmission();
				TMPPuName.text = "";

				//countdown
				statl.SetPowerupActive(true);
				yield return new WaitForSeconds(10);
				statl.SetPowerupActive(false);
				break;
		}

	}

	IEnumerator CountDown()
	{
		for (int i = 0; i < countdowntime; i++)
		{
			aso.PlayOneShot(countdown);
			yield return new WaitForSeconds(1);
		}
		player.SetDamageModifier(1);
		statl.SetPowerupActive(false);
		if (timerbar.transform.localScale.y < 0.1f)
		{
			timerbar.transform.localScale = initialBarScale;
			timerrenderer.material = offMaterial;
		}
	}

	private void SlowDownTimeAndPitch()
	{
		if (Time.timeScale == 1)
		{
			Time.timeScale = slowdownamount;
			audioMixer.SetFloat("SFXPitch", slowdownamount);
			isInSlowdown = true;
			fadingin = true;
		}
	}

	public IEnumerator ApplyResurrectionBuffs()
	{
		//currently same as annihilation buff
		Vector3 auraposition = player.transform.position;
		auraposition.z += 3;
		auraposition.y += 3;
		SlowDownTimeAndPitch();
		if (player.GetDamageModifier() == 1)
			player.SetDamageModifier(4);

		ApplyAura(auraposition);
		//countdown
		statl.SetPowerupActive(true);
		timerrenderer.material = onMaterial;
		powerupprogress = putimeout;
		yield return new WaitForSeconds(putimeout - countdowntime);
		StartCoroutine("CountDown");
		Debug.Log("Resurrection buffs applied");
	}

	#endregion

	#region other utility

	private void DebugBar(Vector3 newscale)
	{
		Debug.Log(" powerupprogress = " + powerupprogress);
		Debug.Log("newscale = " + newscale);
		Debug.Log("powerupprogress / putimeout = " + powerupprogress / putimeout);
	}

	public void ActivateWings()
	{
		pm.SetJumpExtensionFactor(pm.GetJumpExtensionFactor() * jumpmultiplyfactor);
		pm.SetWalkSpeed(pm.GetWalkSpeed() * movemultiplyfactor);
		pm.SetWingsEnabled(true);
		if (!gops.GetStartWithAllUpgrades())
		{
			aso.PlayOneShot(sl.GetPickupGet());
			aso.PlayOneShot(sl.GetWingsReady());
		}
		wingsspawned = true;
	}

	private void ApplyAura(Vector3 auraposition)
	{
		aurainstance = Instantiate(aura, auraposition, Quaternion.identity, player.transform);
		aurainstance.name = aura.name;
		Destroy(aurainstance, putimeout);
	}

	#endregion

	#region particles

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

	#endregion
}
