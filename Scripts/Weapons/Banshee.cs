﻿using EZCameraShake;
using System.Collections;
using UnityEngine;

public class Banshee : MonoBehaviour, ICostingWeapon, IBasicWeapon, ISingleShotBlaster, IChargingWeapon
{
	//Basic Weapon properties
	[Header("Components")]
	[SerializeField] private GameObject projectilePrefab = default;
	[SerializeField] private GameObject mouth = default;
	[SerializeField] private GameObject muzzleFlash = default;
	[SerializeField] private GameObject muzzleParticlesPrefab = default;
	[SerializeField] private ParticleSystem succParticles = default;

	[Header("Parameters")]
	[SerializeField] private float shootRate = default;

	private Transform pjtsf;
	private AudioSource aso;
	private GameManagerScript gm;
	private StatLibrary statl;
	private PlayerUnit unit;
	private SoundLibrary sl;
	private UIController uic;
	private Camera playercamera;
	private WeaponAnimator weaponAnimator;
	public bool readytofire { get; set; }
	private float shootRateTimeStamp = 0f;
	private AudioClip chargeSound = default;
	private AudioSource playeraso;

	public void SetSRTS(float value) { shootRateTimeStamp = value; }
	public void SetUnit(PlayerUnit value) { unit = value; }
	public void SetSL(StatLibrary value) { statl = value; }
	public void SetASO(AudioSource value) { aso = value; }
	public void SetCamera(Camera value) { playercamera = value; }

	public Transform GetPjtsf() { return pjtsf; }
	public GameObject GetProjectilePrefab() { return projectilePrefab; }
	public void SetProjectilePrefab(GameObject value) { projectilePrefab = value; }
	public bool GetReadyToFire() { return readytofire; }
	public void SetReadyToFire(bool value) { readytofire = value; }
	public PlayerUnit GetUnit() { return unit; }
	public GameObject GetMuzzleFlash() { return muzzleFlash; }
	public GameObject GetMouth() { return mouth; }
	public float GetShootRate() { return shootRate; }
	public float GetSRTS() { return shootRateTimeStamp; }
	public AudioSource GetASO() { return aso; }
	public GameObject GetProjPref() { return projectilePrefab; }
	public Camera GetCamera() { return playercamera; }
	public StatLibrary GetStatLibrary() { return statl; }
	public void SetUIAmmoString(string value) { uic.SetAmmoString(value); }
	public void SetWeaponAnimatorEquip(IChargingWeapon value) { weaponAnimator.SetChargingWeaponEquipped(value); }


	//Charge Weapon properties
	[SerializeField] private AudioClip overheatSound = default;
	[SerializeField] private float overheatTime = default;
	[SerializeField] private float cooldownTime = default;
	private float delayTime; //this is as long as the charging sound

	public float cooldownTimer { get; set; }
	public float LMBtimedown { get; set; }
	public bool isChargeSoundPlaying { get; set; }
	public bool isOverheatSoundPlaying { get; set; }
	public bool isFiring { get; set; } //used to tell whether to play charge sound or not
	public float GetOverheatTime() { return overheatTime; }
	public float GetDelayTime() { return delayTime; }
	public float GetCoolDownTime() { return cooldownTime; }

	//Devourer properties
	[SerializeField] private int projCount = 10;
	[SerializeField] private float firerate = 0.25f;

	public void Start()
	{
		InitialiseChargingWeapon();
		InitialisePlayerWeapon();
	}

	public void Update()
	{
		SetUIAmmoString("Ammo : N/A");
		if (Player.GetShootingPressed() && !PlayerUnit.GetPlayerDead() && !Player.GetIsCasting() && !Player.GetIsSwapping())
			HandleShooting();
		else if (!isFiring)
			ResetCharge();

		if (cooldownTimer > 0)
		{
			cooldownTimer -= Time.deltaTime;
		}
	}

	public void InitialisePlayerWeapon()
	{
		gm = GameManagerScript.GetGMScript();
		playercamera = gm.GetSceneCamera();
		unit = gm.GetPlayer().GetComponent<PlayerUnit>();
		statl = gm.GetStatl();
		sl = gm.GetSoundLibrary();
		aso = gm.GetPlayer().GetComponent<AudioSource>();
		pjtsf = gm.GetPJTSF();
		uic = gm.GetUIController();
		playeraso = gm.GetPlayer().GetComponent<AudioSource>();
		weaponAnimator = gm.GetPlayer().GetComponentInChildren<WeaponAnimator>();
		SetWeaponAnimatorEquip(this);
	}

	public void InitialiseChargingWeapon()
	{
		cooldownTimer = 0;
		LMBtimedown = 0;
		isChargeSoundPlaying = false;
		isOverheatSoundPlaying = false;
		isFiring = false;
		delayTime = 1;
	}

	public void ResetCharge()
	{
		StopChargeSound();
		LMBtimedown = 0;
		isChargeSoundPlaying = false;
		if (succParticles.isPlaying)
			succParticles.Stop();
	}

	public void ResetState()
	{
		LMBtimedown = 0;
		isFiring = false;
		isChargeSoundPlaying = false;
	}

	public void PlayChargeSound()
	{
		if (!isChargeSoundPlaying)
		{
			chargeSound = SoundLibrary.ChooseRandomFromList(sl.GetBansheeChargeList());
			GetASO().clip = chargeSound;
			delayTime = chargeSound.length;
			GetASO().Play();
			isChargeSoundPlaying = true;
		}
	}

	public void StopChargeSound()
	{
		GetASO().clip = null;
		isChargeSoundPlaying = false;
	}

	public IEnumerator PlayOHSound()
	{
		if (!isOverheatSoundPlaying)
		{
			isOverheatSoundPlaying = true;
			GetASO().PlayOneShot(overheatSound);
			yield return new WaitForSeconds(overheatSound.length);
			isOverheatSoundPlaying = false;
		}
	}

	public IEnumerator StopFiring(float delay)
	{
		yield return new WaitForSeconds(delay);
		isFiring = false;
	}

	public void Overheat()
	{
		StartCoroutine("PlayOHSound");
		cooldownTimer = cooldownTime;
	}

	public virtual void ChargeShot()
	{
		if (!isFiring && cooldownTimer <= 0)
		{
			PlayChargeSound();
			if (!succParticles.isPlaying)
				succParticles.Play();
		}
	}

	public virtual void BeginFireProjectile()
	{
		GameObject projectileInstance = InstantiateProjectile();
		playeraso.PlayOneShot(SoundLibrary.ChooseRandomFromList(sl.GetBansheeFireList()));
		weaponAnimator.AnimateOneSingleshot();
		shootRateTimeStamp = Time.time + shootRate;
		aimProjectile(projectileInstance);
		CreateMuzzleFlash();
		CreateMuzzleParticles();
		CameraShaker.Instance.ShakeOnce(4, 4, 0.2f, 0.2f);
	}

	public GameObject InstantiateProjectile()
	{
		Vector3 projPosition = mouth.transform.position;
		GameObject projectileInstance = Instantiate(projectilePrefab, projPosition, Quaternion.identity, pjtsf);
		projectileInstance.name = projectilePrefab.name;
		if (projectileInstance.GetComponent<Bullet>())
			projectileInstance.GetComponent<Bullet>().SetParent(GameManagerScript.GetGMScript().GetPlayer().gameObject);
		else if (projectileInstance.GetComponent<Laser>())
			projectileInstance.GetComponent<Laser>().SetParent(mouth);
		return projectileInstance;
	}

	public GameObject InstantiateProjectile(GameObject projectile)
	{
		Vector3 projPosition = mouth.transform.position;
		GameObject projectileInstance = Instantiate(projectile, projPosition, Quaternion.identity, pjtsf);
		projectileInstance.name = projectile.name;
		if (projectileInstance.GetComponent<Bullet>())
			projectileInstance.GetComponent<Bullet>().SetParent(GameManagerScript.GetGMScript().GetPlayer().gameObject);
		else if (projectileInstance.GetComponent<Laser>())
			projectileInstance.GetComponent<Laser>().SetParent(mouth);
		return projectileInstance;
	}

	public void CreateMuzzleFlash()
	{
		if (GetMuzzleFlash())
		{
			GameObject flash = Instantiate(GetMuzzleFlash(), transform.position, Quaternion.identity, GetMouth().transform);
			aimProjectile(flash);
			flash.transform.Rotate(Vector3.forward, UnityEngine.Random.Range(0, 360));
			Destroy(flash, 0.05f);
		}
	}

	public void aimProjectile(GameObject projectile)
	{
		projectile.transform.forward = playercamera.transform.forward;
	}

	public void HandleShooting()
	{
		LMBtimedown += Time.deltaTime;
		ChargeShot();
		if (LMBtimedown > GetDelayTime() && LMBtimedown < GetOverheatTime() && cooldownTimer <= 0 && !isFiring)
		{
			DoShoot();
			if (succParticles.isPlaying)
				succParticles.Stop();
		}
		else if (LMBtimedown >= GetOverheatTime() && !isOverheatSoundPlaying)
		{
			Overheat();
			StartCoroutine(StopFiring(0.8f));
		}
	}

	public void DoShoot()
	{
		StartCoroutine(GetEmBoi());
		isFiring = true;
	}

	public IEnumerator GetEmBoi()
	{
		for (int i = 0; i < projCount; i++)
		{
			yield return new WaitForSeconds(firerate);
			BeginFireProjectile();
		}
		ResetState();
		isFiring = false;
	}

	public void CreateMuzzleParticles()
	{
		GameObject mpinst = Instantiate(muzzleParticlesPrefab, mouth.transform.position, transform.rotation, transform.parent);
		mpinst.name = muzzleParticlesPrefab.name;
	}

}
