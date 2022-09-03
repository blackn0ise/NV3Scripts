using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EZCameraShake;

public class BoneBag : MonoBehaviour, IBasicWeapon, IBasicBulletShooter, ISmokeSeparator
{
	[Header("Components")]
	[SerializeField] private GameObject projectilePrefab = default;
	[SerializeField] private GameObject mouth = default;
	[SerializeField] private GameObject muzzleFlash = default;
	[SerializeField] private GameObject muzzleParticlesPrefab = default;
	[SerializeField] private ParticleSystem muzzleParticles = default;
	[SerializeField] private Transform flashPoint = default;

	[Header("Parameters")]
	[SerializeField] private float shootRate = default;
	[SerializeField] private float flashDecay = 0.05f;

	private Transform pjtsf;
	private AudioSource aso;
	private Animator animator;
	private StatLibrary sl;
	private PlayerUnit unit;
	private UIController uic;
	private Camera playercamera;
	private WeaponAnimator weaponAnimator;
	private GameManagerScript gm;
	private GameOptions gops;
	private PlayerMovement pm;
	public bool readytofire { get; set; }
	private float shootRateTimeStamp = 0f;
	private bool snubnoseActive = false;

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
	public void SetUIAmmoString(string value) { uic.SetAmmoString(value); }
	public void SetWeaponAnimatorEquip(IChargingWeapon value) { weaponAnimator.SetChargingWeaponEquipped(value); }

	public void Start()
	{
		gops = GameOptions.GetGOPS();
		animator = GetComponentInChildren<Animator>();
		InitialisePlayerWeapon();
		ReparentEmitter();
		uic = GameManagerScript.GetGMScript().GetUIController();
		if (FindObjectOfType<Snubnose>())
			snubnoseActive = true;

		CheckCameraDebugMode();
	}

	private void CheckCameraDebugMode()
	{
		if (gops.GetCameraDebugMode())
		{
			foreach (MeshRenderer mr in GetComponentsInChildren<MeshRenderer>())
			{
				mr.enabled = false;
			}
			foreach (SkinnedMeshRenderer mr in GetComponentsInChildren<SkinnedMeshRenderer>())
			{
				mr.enabled = false;
			}
		}
	}

	public void Update()
	{
		if(!snubnoseActive)
			SetUIAmmoString("Ammo : N/A");
		ConfirmReadyToFire();
		if (Player.GetShootingPressed() && !PlayerUnit.GetPlayerDead() && !Player.GetIsCasting() && !Player.GetIsSwapping())
			HandleShooting();
		InstantiateMPIfNone();
		if (!Player.GetShootingPressed())
		{
			if (animator.GetBool("IsAttacking") != false)
				animator.SetBool("IsAttacking", false);
			StopEmitters();
		}
	}

	private void StopEmitters()
	{
		if (muzzleParticles)
			if (muzzleParticles.isPlaying)
				muzzleParticles.Stop();
	}

	public void InstantiateMPIfNone()
	{
		if (!muzzleParticles && !gops.GetCameraDebugMode())
		{
			GameObject mpinst = Instantiate(muzzleParticlesPrefab, mouth.transform.position, mouth.transform.rotation, mouth.transform);
			mpinst.name = muzzleParticlesPrefab.name;
			muzzleParticles = mpinst.GetComponent<ParticleSystem>();
		}
	}

	public void ReparentEmitter()
	{
		muzzleParticles.transform.parent = transform.parent;
	}

	public void InitialisePlayerWeapon()
	{
		gm = GameManagerScript.GetGMScript();
		playercamera = gm.GetSceneCamera();
		unit = gm.GetPlayer().GetComponent<PlayerUnit>();
		sl = gm.GetStatl();
		aso = gm.GetPlayer().GetComponent<AudioSource>();
		pjtsf = gm.GetPJTSF();
		pm = gm.GetPlayer().GetComponentInChildren<PlayerMovement>();
		weaponAnimator = gm.GetPlayer().GetComponentInChildren<WeaponAnimator>();
		SetWeaponAnimatorEquip(null);
	}

	public void ConfirmReadyToFire()
	{
		readytofire = Time.time > shootRateTimeStamp && !Player.GetIsCasting();
	}

	public virtual void HandleShooting()
	{
		if (gops.GetCameraDebugMode())
			pm.DecreaseYVelocity(pm.GetCdebugVelocityFactor());
		else if (readytofire)
		{
			BeginFireProjectile();
			AnimateShooting();
		}
	}

	private void AnimateShooting()
	{
		if (Player.GetShootingPressed())
		{
			animator.SetBool("IsAttacking", true);
			if (muzzleParticles)
				muzzleParticles.Play();
		}
	}

	public virtual void BeginFireProjectile()
	{
		GameObject projectileInstance = InstantiateProjectile();
		shootRateTimeStamp = Time.time + shootRate;
		aimProjectile(projectileInstance);
		CreateMuzzleFlash();
		CameraShaker.Instance.ShakeOnce(0.8f, 0.3f, 0.2f, 0.2f);
	}

	public GameObject InstantiateProjectile()
	{
		Vector3 projPosition = mouth.transform.position;
		GameObject projectileInstance = Instantiate(projectilePrefab, projPosition, Quaternion.identity, pjtsf);
		projectileInstance.name = projectilePrefab.name;
		projectileInstance.GetComponent<IBullet>().SetParent(playercamera.gameObject);
		return projectileInstance;
	}

	public GameObject InstantiateProjectile(GameObject projectile)
	{
		Vector3 projPosition = mouth.transform.position;
		GameObject projectileInstance = Instantiate(projectile, projPosition, Quaternion.identity, pjtsf);
		projectileInstance.name = projectile.name;
		IBullet bullet = projectileInstance.GetComponent<IBullet>();
		if (bullet != null)
			bullet.SetParent(gm.GetPlayer().gameObject);
		return projectileInstance;
	}

	public void CreateMuzzleFlash()
	{
		if (GetMuzzleFlash())
		{
			GameObject flash = Instantiate(GetMuzzleFlash(), flashPoint.position, Quaternion.identity);
			aimProjectile(flash);
			flash.transform.Rotate(Vector3.forward, UnityEngine.Random.Range(0, 360));
			Destroy(flash, flashDecay);
		}
	}

	public void aimProjectile(GameObject projectile)
	{
		projectile.transform.forward = playercamera.transform.forward;
	}
}
