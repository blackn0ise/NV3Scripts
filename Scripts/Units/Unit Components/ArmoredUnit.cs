using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ArmoredUnit : MonoBehaviour, IUnit
{
	[Header("References")]
	[SerializeField] private GameObject deathAnim = default;
	[SerializeField] private GameObject souls = default;
	[SerializeField] private Weakpoint[] weakpoints = default;

	[Header("Materials")]
	[SerializeField] private Material flickerMaterial = default;
	[SerializeField] private Material deathMaterial = default;
	[SerializeField] private Material spawnInMaterial = default;

	[Header("Parameters")]
	[SerializeField] private int soulGains = default;
	[SerializeField] private int maxHealth = default;

	[ReadOnly] private int health;
	private GameManagerScript gm;
	private StatLibrary statl;
	private SpawnController sc;
	private AudioClip dead;
	private AudioClip hurt;
	private MeshRenderer[] renderers;
	private SkinnedMeshRenderer[] smrenderers;
	private List<Material> originalMaterials;
	private List<Material> originalSMRMaterials;
	private AudioSource aso;
	private AudioSource environmentaso;
	private bool hasDedSoundPlayed = false;
	private bool dying = false;
	private int remainingpoints = 0;

	public int GetHealth() { return health; }
	public int GetMaxHealth() { return maxHealth; }
	public int GetRemainingPoints() { return remainingpoints; }
	public void SetRemainingPoints(int value) { remainingpoints = value; }
	public void SetHealth(int value) { health = value; }
	public void SetMaxHealth(int value) { maxHealth = value; }
	public AudioClip GetDeadClip() { return dead; }
	public AudioClip GetHurtClip() { return hurt; }
	public void SetDeadClip(AudioClip value) { dead = value; }
	public void SetHurtClip(AudioClip value) { hurt = value; }

	public void Start()
	{
		Initialise();
		StartCoroutine(AnimateSpawnIn());
	}

	IEnumerator AnimateSpawnIn()
	{
		int animaterate = 2;
		SetRendererMaterials(spawnInMaterial);
		SetSMRendererMaterials(spawnInMaterial);
		float alphavalue = 1;
		while (alphavalue > 0)
		{
			alphavalue -= Time.deltaTime * animaterate;
			SetRendererAlphaClip(alphavalue);
			SetSMRendererAlphaClip(alphavalue);
			yield return new WaitForSeconds(Time.deltaTime);
		}
		ResetMRMaterials();
		ResetSMRMaterials();
	}

	private void Update()
	{
		CheckWeakpoints();
	}

	private void CheckWeakpoints()
	{
		bool stillalive = false;
		int alivecounter = 0;
		NewMethod(ref stillalive, ref alivecounter);
		remainingpoints = alivecounter;
		if (!stillalive)
			HandleDeath();
	}

	private void NewMethod(ref bool stillalive, ref int alivecounter)
	{
		foreach (Weakpoint point in weakpoints)
		{
			if (!point.IsDead())
			{
				stillalive = true;
				alivecounter += 1;
			}
		}
	}

	private void Initialise()
	{
		gm = GameManagerScript.GetGMScript();
		statl = gm.GetStatl();
		sc = gm.GetSpawnController();
		aso = GetComponent<AudioSource>();
		environmentaso = gm.GetEnvirionmentASO();
		health = maxHealth;
		saveOriginalMaterials();
		remainingpoints = weakpoints.Length;
	}

	public void saveOriginalMaterials()
	{
		originalMaterials = new List<Material>();
		originalSMRMaterials = new List<Material>();
		renderers = gameObject.GetComponentsInChildren<MeshRenderer>();
		smrenderers = gameObject.GetComponentsInChildren<SkinnedMeshRenderer>();
		foreach (MeshRenderer boi in renderers)
			originalMaterials.Add(boi.material);
		foreach (SkinnedMeshRenderer boi in smrenderers)
			originalSMRMaterials.Add(boi.material);
	}

	public void HandleDamageSoundsAndFlicker()
	{
		SoundLibrary.varySoundPitch(aso, 0.05f);
		aso.PlayOneShot(hurt);
		//no flicker for armored units, this is handled by weakpoints
		//StartCoroutine(DamageFlicker());
	}

	public IEnumerator DamageFlicker()
	{
		if (!IsDead())
		{
			FlickerMRMaterials();
			FlickerSMRMaterials();
		}
		yield return new WaitForSeconds(0.5f);
		if (!IsDead())
		{
			ResetMRMaterials();
			ResetSMRMaterials();
		}
	}

	private void ResetSMRMaterials()
	{
		for (int i = 0; i < originalSMRMaterials.Count; i++)
		{
			smrenderers[i].material = originalSMRMaterials[i];
		}
	}

	private void ResetMRMaterials()
	{
		for (int i = 0; i < originalMaterials.Count; i++)
		{
			renderers[i].material = originalMaterials[i];
		}
	}

	private void FlickerMRMaterials()
	{
		foreach (MeshRenderer mat in renderers)
		{
			if (!mat.gameObject.CompareTag("Wiggler"))
				mat.material = flickerMaterial;
		}
	}

	private void FlickerSMRMaterials()
	{
		foreach (SkinnedMeshRenderer mat in smrenderers)
		{
			if (!mat.gameObject.CompareTag("Wiggler"))
				mat.material = flickerMaterial;
		}
	}

	public void HandleDeath()
	{
		if(!dying)
		{
			dying = true;
			CreateDeathParticles();
			StartCoroutine(ApplyDeathMaterialAndAnimate());
			gameObject.layer = 10;
			GiveSouls();
			PlayDeathSound();
			if (souls)
			{
				GameObject soulsinst = Instantiate(souls, transform.position, gameObject.transform.rotation);
				Destroy(soulsinst, 2);
			}
			if (GetComponent<SoulCollector>())
				SoulCollector.soulCollectorActive = false;
			else if (GetComponent<Vizier>())
				Vizier.vizierActive = false;
		}
	}

	IEnumerator ApplyDeathMaterialAndAnimate()
	{
		int animaterate = 2;
		SetRendererMaterials(deathMaterial);
		SetSMRendererMaterials(deathMaterial);
		float alphavalue = 0;
		while (alphavalue < 1)
		{
			alphavalue += Time.deltaTime * animaterate;
			SetRendererAlphaClip(alphavalue);
			SetSMRendererAlphaClip(alphavalue);
			yield return new WaitForSeconds(Time.deltaTime);
		}
		HandleKill();
	}

	private void SetRendererAlphaClip(float alphavalue)
	{
		foreach (MeshRenderer rend in renderers)
		{
			if (rend)
				rend.material.SetFloat("_Alpha_Clip_Level", alphavalue);
		}
	}

	private void SetSMRendererAlphaClip(float alphavalue)
	{
		foreach (SkinnedMeshRenderer rend in smrenderers)
		{
			if (rend)
				rend.material.SetFloat("_Alpha_Clip_Level", alphavalue);
		}
	}

	private void SetRendererMaterials(Material material)
	{
		foreach (MeshRenderer rend in renderers)
		{
			if (rend)
			{
				rend.material = new Material(material);
				rend.material.SetFloat("_Alpha_Clip_Level", 0); 
			}
		}
	}

	private void SetSMRendererMaterials(Material material)
	{
		foreach (SkinnedMeshRenderer rend in smrenderers)
		{
			if (rend)
			{
				rend.material = new Material(material);
				rend.material.SetFloat("_Alpha_Clip_Level", 0); 
			}
		}
	}

	private void HandleKill()
	{
		if (gameObject.CompareTag("Enemies") || gameObject.CompareTag("EnemyArmor") || gameObject.CompareTag("NecroSpawn") || gameObject.CompareTag("LichSpawn"))
			KillUnit();
		if (gameObject.CompareTag("Friendlies"))
			KillFriendAndUpdateCount();
	}

	public void GiveSouls()
	{
		statl.SetSouls(statl.GetSouls() + soulGains);
		if (statl.GetPowerupActive())
			statl.SetBloodBank(statl.GetBloodBank() + soulGains);
	}

	public void KillFriendAndUpdateCount()
	{
		Destroy(gameObject);
		Resurrector.CountFriends("Friendlies");
	}

	public void KillUnit()
	{
		Destroy(gameObject);
		sc.SetTimeSinceLastKill(0);
	}

	public void CreateDeathParticles()
	{
		GameObject anim = Instantiate(deathAnim, transform.position, transform.rotation);
		ParticleSystem ps = anim.GetComponentInChildren<ParticleSystem>();
		Destroy(anim, ps.main.duration);
	}

	public void PlayDeathSound()
	{
		if (!hasDedSoundPlayed)
		{
			SoundLibrary.ResetPitch(environmentaso);
			SoundLibrary.varySoundPitch(environmentaso, 0.2f);
			environmentaso.PlayOneShot(dead);
			hasDedSoundPlayed = true;
		}
	}

	public bool IsDead()
	{
		if (GetHealth() <= 0)
			return true;
		return false;
	}
}


