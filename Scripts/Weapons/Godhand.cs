using EZCameraShake;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Godhand : MonoBehaviour, IBasicWeapon, IBasicBulletShooter
{
	[Header("Components")]
	[SerializeField] private GameObject projectilePrefab = default;
	[SerializeField] private GameObject mouth = default;
	[SerializeField] private GameObject muzzleFlash = default;
	[SerializeField] private AudioClip fireclip = default;
	[SerializeField] private GameObject muzzleParticlesPrefab = default;
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
	private GameManagerScript gm;
	private WeaponAnimator weaponAnimator;
	public bool readytofire { get; set; }
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
	public void SetUIAmmoString(string value) { uic.SetAmmoString(value); }
	public void SetWeaponAnimatorEquip(IChargingWeapon value) { weaponAnimator.SetChargingWeaponEquipped(value); }

	public void Start()
	{
		animator = GetComponentInChildren<Animator>();
		InitialisePlayerWeapon();
		uic = GameManagerScript.GetGMScript().GetUIController();
	}

	public void Update()
	{
		SetUIAmmoString("Ammo : N/A");
		ConfirmReadyToFire();
		if (Player.GetShootingPressed() && !PlayerUnit.GetPlayerDead() && !Player.GetIsCasting() && !Player.GetIsSwapping())
			HandleShooting();
		else
		{
			if (animator.GetBool("IsAttacking") != false)
				animator.SetBool("IsAttacking", false);
		}
	}

	public void InitialisePlayerWeapon()
	{
		gm = GameManagerScript.GetGMScript();
		playercamera = gm.GetSceneCamera();
		unit = gm.GetPlayer().GetComponent<PlayerUnit>();
		sl = gm.GetStatl();
		aso = gm.GetPlayer().GetComponent<AudioSource>();
		pjtsf = gm.GetPJTSF();
		weaponAnimator = gm.GetPlayer().GetComponentInChildren<WeaponAnimator>();
		SetWeaponAnimatorEquip(null);
	}

	public void ConfirmReadyToFire()
	{
		readytofire = Time.time > shootRateTimeStamp && !Player.GetIsCasting();
	}

	public virtual void HandleShooting()
	{
		if (readytofire)
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
		}
	}

	public virtual void BeginFireProjectile()
	{
		GameObject projectileInstance = InstantiateProjectile();
		shootRateTimeStamp = Time.time + shootRate;
		aimProjectile(projectileInstance);
		CreateMuzzleFlash();
		CreateMuzzleParticles();
		aso.PlayOneShot(fireclip);
		CameraShaker.Instance.ShakeOnce(5, 5, 0.2f, 0.5f);
	}

	public GameObject InstantiateProjectile()
	{
		Vector3 projPosition = mouth.transform.position;
		GameObject projectileInstance = Instantiate(projectilePrefab, projPosition, Quaternion.identity, pjtsf);
		projectileInstance.name = projectilePrefab.name;
		projectileInstance.GetComponent<IBullet>().SetParent(GameManagerScript.GetGMScript().GetPlayer().gameObject);
		return projectileInstance;
	}

	public GameObject InstantiateProjectile(GameObject projectile)
	{
		Vector3 projPosition = mouth.transform.position;
		GameObject projectileInstance = Instantiate(projectile, projPosition, Quaternion.identity, pjtsf);
		projectileInstance.name = projectile.name;
		if (projectileInstance.GetComponent<IBullet>() != null)
			projectileInstance.GetComponent<IBullet>().SetParent(GameManagerScript.GetGMScript().GetPlayer().gameObject);
		else if (projectileInstance.GetComponent<Laser>())
			projectileInstance.GetComponent<Laser>().SetParent(mouth);
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
