using EZCameraShake;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goregun : MonoBehaviour, IBasicWeapon, IBasicBulletShooter, IAmmoUser
{
	[Header("Components")]
	[SerializeField] private GameObject projectilePrefab = default;
	[SerializeField] private GameObject mouth = default;
	[SerializeField] private GameObject muzzleFlash = default;
	[SerializeField] private GameObject muzzleParticlesPrefab = default;
	[SerializeField] private AudioClip outOfAmmo = default;

	[Header("Parameters")]
	[SerializeField] private float shootRate = default;
	[SerializeField] public float staggerRate = 0.075f;
	[SerializeField] private int ammoCount = default;
	[SerializeField] private int lobCount = 5;

	private Transform pjtsf;
	private AudioSource aso;
	private StatLibrary sl;
	private PlayerUnit unit;
	private Camera playercamera;
	private UIController uic;
	private GameManagerScript gm;
	private WeaponAnimator weaponAnimator;
	private int startingAmmo = 0;
	private bool staggeringlobs = false;
	public bool readytofire { get; set; }
	private bool outOfAmmoClipPlaying = false;
	private float shootRateTimeStamp = 0f;

	public void SetSRTS(float value) { shootRateTimeStamp = value; }
	public void SetUnit(PlayerUnit value) { unit = value; }
	public void SetSL(StatLibrary value) { sl = value; }
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
	public StatLibrary GetStatLibrary() { return sl; }
	public int GetAmmoCount() { return ammoCount; }
	public void SetUIAmmoString(string value) { uic.SetAmmoString(value); }
	public void SetWeaponAnimatorEquip(IChargingWeapon value) { weaponAnimator.SetChargingWeaponEquipped(value); }

	public void Start()
	{
		InitialisePlayerWeapon();
	}

	public void Update()
	{
		SetUIAmmoString($"Ammo : [{ammoCount}/{startingAmmo}]");
		ConfirmReadyToFire();
		if (Player.GetShootingPressed() && !PlayerUnit.GetPlayerDead() && !Player.GetIsCasting() && !Player.GetIsSwapping())
			HandleShooting();
	}

	public void InitialisePlayerWeapon()
	{
		gm = GameManagerScript.GetGMScript();
		playercamera = gm.GetSceneCamera();
		unit = gm.GetPlayer().GetComponent<PlayerUnit>();
		sl = gm.GetStatl();
		aso = gm.GetPlayer().GetComponent<AudioSource>();
		pjtsf = gm.GetPJTSF();
		uic = gm.GetUIController();
		weaponAnimator = gm.GetPlayer().GetComponentInChildren<WeaponAnimator>();
		startingAmmo = ammoCount;
		SetWeaponAnimatorEquip(null);
	}

	public void ConfirmReadyToFire()
	{
		readytofire = Time.time > shootRateTimeStamp && ammoCount > 0 && !Player.GetIsCasting();
	}

	public virtual void HandleShooting()
	{
		if (readytofire)
		{
			StartCoroutine(StaggerLobs());
		}
		if (ammoCount <= 0 && !outOfAmmoClipPlaying)
		{
			StartCoroutine(PlayOOAClip());
			TriggerAmmoUI();
		}
	}

	private IEnumerator StaggerLobs()
	{
		if (!staggeringlobs)
		{
			staggeringlobs = true;
			for (int i = 0; i < lobCount; i++)
			{
				yield return new WaitForSeconds(staggerRate);
				BeginFireProjectile();
			}
			staggeringlobs = false;
			ammoCount--;
		}
	}

	public void TriggerAmmoUI()
	{
		uic.SetAmmoUIEnabled(true);
	}

	public IEnumerator PlayOOAClip()
	{
		outOfAmmoClipPlaying = true;
		aso.PlayOneShot(outOfAmmo);
		yield return new WaitForSeconds(outOfAmmo.length);
		outOfAmmoClipPlaying = false;
	}

	public virtual void BeginFireProjectile()
	{
		GameObject projectileInstance = InstantiateProjectile();
		weaponAnimator.AnimateOneSingleshot();
		shootRateTimeStamp = Time.time + shootRate;
		aimProjectile(projectileInstance);
		CreateMuzzleFlash();
		CreateMuzzleParticles();
		CameraShaker.Instance.ShakeOnce(2.0f, 2.0f, 0.1f, 0.2f);
	}

	public GameObject InstantiateProjectile()
	{
		Vector3 projPosition = mouth.transform.position;
		GameObject projectileInstance = Instantiate(projectilePrefab, projPosition, Quaternion.identity, pjtsf);
		projectileInstance.name = projectilePrefab.name;
		if (projectileInstance.GetComponent<IBullet>() != null)
			projectileInstance.GetComponent<IBullet>().SetParent(pjtsf.gameObject);
		else if (projectileInstance.GetComponent<Laser>())
			projectileInstance.GetComponent<Laser>().SetParent(mouth);
		return projectileInstance;
	}

	public GameObject InstantiateProjectile(GameObject projectile)
	{
		Vector3 projPosition = mouth.transform.position;
		GameObject projectileInstance = Instantiate(projectile, projPosition, Quaternion.identity, pjtsf);
		projectileInstance.name = projectile.name;
		if (projectileInstance.GetComponent<IBullet>() != null)
			projectileInstance.GetComponent<IBullet>().SetParent(pjtsf.gameObject);
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

	public void CreateMuzzleParticles()
	{
		GameObject mpinst = Instantiate(muzzleParticlesPrefab, transform.position, transform.rotation, transform.parent);
		mpinst.name = muzzleParticlesPrefab.name;
	}

	public void aimProjectile(GameObject projectile)
	{
		projectile.transform.forward = playercamera.transform.forward;
	}
}
