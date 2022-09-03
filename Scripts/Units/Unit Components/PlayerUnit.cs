using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerUnit : MonoBehaviour, IUnit
{
	[SerializeField] private int maxHealth = default;
	[SerializeField] private float damageIFrameRate = default;
	[SerializeField] private Animator hurtAnim = default;
	[SerializeField] private PlayerMovement playerMovement = default;
	[SerializeField] private PlayerInput playerInput = default;
	[SerializeField] private GameObject deathimage = default;

	private UIController uic;
	private SoundLibrary sl;
	private SpawnController sc;
	private AudioClip dead;
	private AudioClip hurt;
	private GameManagerScript gm;
	private GameOptions gops;
	private float damageTimeStamp = 0f;
	private bool hasDedSoundPlayed = false;
	private int health;
	private static bool playerDead;

	public int GetHealth() { return health; }
	public int GetMaxHealth() { return maxHealth; }
	public void SetHealth(int value) { health = value; }
	public void SetMaxHealth(int value) { maxHealth = value; }

	public AudioClip GetDeadClip() { return dead; }
	public AudioClip GetHurtClip() { return hurt; }
	public void SetDeadClip(AudioClip value) { dead = value; }
	public void SetHurtClip(AudioClip value) { hurt = value; }

	public static bool GetPlayerDead() { return playerDead; }

	public void Start()
	{
		gm = GameManagerScript.GetGMScript();
		sl = gm.GetSoundLibrary();
		sc = gm.GetSpawnController();
		uic = gm.GetUIController();
		gops = GameOptions.GetGOPS();
		SetDeadClip(sl.GetPlayerDead());
	}

	private void Update()
	{
		playerDead = health <= 0;
	}

	public void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("EnemyProjectile") && gameObject.CompareTag("Player") && !gops.GetGodModeEnabled())
			HandleDamage(other);
	}

	public void HandleDamage(Collider co)
	{
		SoundLibrary.ResetPitch(GetComponent<AudioSource>());

		if (!IsDead())
		{
			if (Time.time >= damageTimeStamp)
			{
				IBullet projGo = co.GetComponentInParent<IBullet>();
				SetHealth(GetHealth() - projGo.GetDamage());
				damageTimeStamp = Time.time + damageIFrameRate;
				hurtAnim.SetTrigger("Hurt");
				HandleDamageSoundsAndFlicker();
				HandleKnockback(co, projGo);
				HandleDeath(co);
			}
		}

	}

	private void HandleKnockback(Collider co, IBullet projGo)
	{
		if (projGo.GetHasKnockback())
		{
			Vector3 velocity = Vector3.zero;
			Vector3 direction = -1 * (co.transform.position - transform.position);
			velocity = direction * projGo.GetKnockFactor() * Time.deltaTime;
			playerMovement.SetVelocity(velocity);
		}
	}

	public void HandleDamageSoundsAndFlicker()
	{
		GetComponent<AudioSource>().PlayOneShot(sl.ChoosePlayerHurt());
	}

	public void HandleDeath(Collider col)
	{
		if (IsDead())
		{
			KillUnit();
			if (playerInput.currentActionMap.name != "PlayerControls")
				playerInput.SwitchCurrentActionMap("PlayerControls");
			KillText text = col.GetComponent<KillText>();
			if (text != null)
				GameLog.Log(text.GetKillText());


		}
	}

	public void KillUnit(bool fall = false)
	{
		health = 0;
		//set layer to corpses layer
		foreach (Transform child in gameObject.GetComponentsInChildren<Transform>())
		{
			child.gameObject.layer = 10;
		}
		ActivateDeathUI();
		PlayDeathSound();
		sc.SetGameActive(false);
		if(fall)
        {
			bool foundSteamName = SteamScript.instance.GetOwnName() != "";
			string killTextpt1 = foundSteamName ? SteamScript.instance.GetOwnName() + " " : "Player ";
			string killTextpt2 = "fell into the abyss.";
			GameLog.Log(killTextpt1 + killTextpt2, 30);
		}
	}

	public void ActivateDeathUI()
	{
		DisableWeaponRenderers();
		deathimage.GetComponent<Animator>().SetTrigger("dead");
		if (sc.GetGameActive())
		{
			uic.DisplayUIStats();
			uic.DisplayAndUploadHighScore();
		}
	}

	public void PlayDeathSound()
	{
		if (!hasDedSoundPlayed)
		{
			SoundLibrary.ResetPitch(GameObject.Find("Environment").GetComponent<AudioSource>());
			SoundLibrary.varySoundPitch(GameObject.Find("Environment").GetComponent<AudioSource>(), 0.2f);

			GameObject.Find("Environment").GetComponent<AudioSource>().clip = dead;
			GameObject.Find("Environment").GetComponent<AudioSource>().PlayOneShot(dead);
			hasDedSoundPlayed = true;
		}
	}

	public static void DisableWeaponRenderers()
	{
		if (GameObject.FindGameObjectsWithTag("Weapon").Length > 0)
		{
			foreach (GameObject weapon in GameObject.FindGameObjectsWithTag("Weapon"))
			{
				weapon.SetActive(false);
				foreach (MeshRenderer renderer in weapon.gameObject.GetComponentsInChildren<MeshRenderer>())
				{
					renderer.enabled = false;
				}
			}
		}
		if (FindObjectOfType<Gauntlets>())
			foreach (SkinnedMeshRenderer renderer in FindObjectOfType<Gauntlets>().gameObject.GetComponentsInChildren<SkinnedMeshRenderer>())
			{
				renderer.enabled = false;
			}
	}

	public bool IsDead()
	{
		if (GetHealth() <= 0)
			return true;
		return false;
	}
}


