using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Weakpoint : MonoBehaviour, IUnit
{
	[ReadOnly] private int health;
	[SerializeField] private int maxHealth = default;
	[SerializeField] private float damageIFrameRate = default;
	[SerializeField] private int soulGains = default;
	[SerializeField] private GameObject deathAnim = default;
	[SerializeField] private GameObject souls = default;
	[SerializeField] private Material flickerMaterial = default;
	[SerializeField] private Material deathMaterial = default;
	private float damageTimeStamp = 0f;
	private bool hasDedSoundPlayed = false;
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
	private bool fromSC;

	public int GetHealth() { return health; }
	public int GetMaxHealth() { return maxHealth; }
	public void SetHealth(int value) { health = value; }
	public void SetMaxHealth(int value) { maxHealth = value; }
	public AudioClip GetDeadClip() { return dead; }
	public AudioClip GetHurtClip() { return hurt; }
	public void SetDeadClip(AudioClip value) { dead = value; }
	public void SetHurtClip(AudioClip value) { hurt = value; }

	private void Awake()
	{
		saveOriginalMaterials();
	}

	public void Start()
	{
		Initialise();
	}

	private void Initialise()
	{
		gm = GameManagerScript.GetGMScript();
		fromSC = GetComponentInParent<SoulCollector>();
		statl = gm.GetStatl();
		sc = gm.GetSpawnController();
		aso = GetComponentInParent<AudioSource>();
		environmentaso = gm.GetEnvirionmentASO();
		health = maxHealth;
	}

	public void OnTriggerEnter(Collider other)
	{
		if ((other.CompareTag("FriendlyProjectile") && gameObject.CompareTag("Enemies")) ||
			(other.CompareTag("FriendlyProjectile") && gameObject.CompareTag("NecroSpawn")) ||
			(other.CompareTag("FriendlyProjectile") && gameObject.CompareTag("LichSpawn")) ||
			(other.CompareTag("EnemyProjectile") && (gameObject.CompareTag("Friendlies") || gameObject.CompareTag("Player"))))
			HandleDamage(other);
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

	public void HandleDamage(Collider co)
	{
		SoundLibrary.ResetPitch(aso);

		if (!IsDead())
		{
			if (Time.time >= damageTimeStamp)
			{
				IBullet projGo = co.GetComponentInParent<IBullet>();
				SetHealth(GetHealth() - projGo.GetDamage());
				damageTimeStamp = Time.time + damageIFrameRate;
				HandleDamageSoundsAndFlicker();
				HandleDeath(co);
			}
		}

	}

	public void HandleDamageSoundsAndFlicker()
	{
		SoundLibrary.varySoundPitch(aso, 0.05f);
		aso.PlayOneShot(hurt);
		StartCoroutine(DamageFlicker());
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

	public void HandleDeath(Collider col)
	{
		if (IsDead())
		{
			if (!gameObject.CompareTag("Player"))
			{
				CreateDeathParticles();
				StartCoroutine(ApplyDeathMaterialAndAnimate());
				gameObject.layer = 10;
				GiveSouls(col);
			}
			PlayDeathSound();
			if (souls)
			{
				GameObject soulsinst = Instantiate(souls, transform.position, gameObject.transform.rotation);
				Destroy(soulsinst, 2);
			}
		}
	}

	IEnumerator ApplyDeathMaterialAndAnimate()
	{
		int animaterate = 2;
		SetRendererMaterials();
		SetSMRendererMaterials();
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
			rend.material.SetFloat("_Alpha_Clip_Level", alphavalue);
		}
	}

	private void SetSMRendererAlphaClip(float alphavalue)
	{
		foreach (SkinnedMeshRenderer rend in smrenderers)
		{
			rend.material.SetFloat("_Alpha_Clip_Level", alphavalue);
		}
	}

	private void SetRendererMaterials()
	{
		foreach (MeshRenderer rend in renderers)
		{
			rend.material = new Material(deathMaterial);
			rend.material.SetFloat("_Alpha_Clip_Level", 0);
		}
	}

	private void SetSMRendererMaterials()
	{
		foreach (SkinnedMeshRenderer rend in smrenderers)
		{
			rend.material = new Material(deathMaterial);
			rend.material.SetFloat("_Alpha_Clip_Level", 0);
		}
	}

	private void HandleKill()
	{
		if (gameObject.CompareTag("Enemies") || gameObject.CompareTag("NecroSpawn") || gameObject.CompareTag("LichSpawn"))
			KillUnit();
		if (gameObject.CompareTag("Friendlies"))
			KillFriendAndUpdateCount();
	}

	public void GiveSouls(Collider col)
	{
		statl.SetSouls(statl.GetSouls() + soulGains);

		if (col.name == "GauntletShotCol")
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
		if (!fromSC)
			Destroy(gameObject);
		else
			gameObject.SetActive(false);
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


