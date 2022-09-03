using EZCameraShake;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Snubnose : MonoBehaviour, IBasicWeapon, IBasicBulletShooter, IShotgunBlaster, IAmmoUser
{
	[Header("Components")]
	[SerializeField] private GameObject projectilePrefab = default;
	[SerializeField] private GameObject mouth = default;
	[SerializeField] private GameObject muzzleFlash = default;
	[SerializeField] private GameObject muzzleParticlesPrefab = default;
	[SerializeField] private Transform pjtsf = default;
	[SerializeField] private AudioClip reloadSound = default;
	[SerializeField] private AudioClip outOfAmmo = default;
	[SerializeField] private List<AudioClip> shotSoundList = default;

	[Header("Parameters")]
	[SerializeField] private float shootRate = default;
	[SerializeField] private float reloadDelay = 0.3f;
	[SerializeField] private int bulletNumber = 15;
	[SerializeField] private int ammoCount = default;

	private GameObject player;
	private AudioSource aso;
	private Animator animator;
	private StatLibrary sl;
	private PlayerUnit unit;
	private UIController uic;
	private GameManagerScript gm;
	private Camera playercamera;
	private WeaponAnimator weaponAnimator;
	public bool readytofire { get; set; }
	private bool outOfAmmoClipPlaying = false;
	private float shootRateTimeStamp = 0f;
	private int startingAmmo = 0;

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
		player = gm.GetPlayer().gameObject;
		startingAmmo = ammoCount;

		if (GetComponent<Animator>())
			animator = GetComponent<Animator>();

		weaponAnimator = gm.GetPlayer().GetComponentInChildren<WeaponAnimator>();
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
			BeginFireProjectile();
			ammoCount--;
		}
		if (ammoCount <= 0 && !outOfAmmoClipPlaying)
		{
			StartCoroutine(PlayOOAClip());
			TriggerAmmoUI();
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

	public void BeginFireProjectile()
	{
		InstantiateProjectile();
		GetASO().PlayOneShot(SoundLibrary.ChooseRandomFromList(shotSoundList));
		SetSRTS(Time.time + GetShootRate());
		CameraShaker.Instance.ShakeOnce(1.5f, 1.5f, 0.1f, 0.2f);
	}

	public void InstantiateProjectile()
	{
		Vector3 projPosition = GetMouth().transform.position;
		for (int i = 0; i < bulletNumber; i++)
		{
			GameObject projectileInstance = Instantiate(GetProjPref(), projPosition, Quaternion.identity, GetPjtsf());
			projectileInstance.name = GetProjPref().name;
			projectileInstance.GetComponent<IBullet>().SetParent(player);
			aimProjectile(projectileInstance);
		}
		CreateMuzzleFlash();
		CreateMuzzleParticles();
	}

	public void CreateMuzzleFlash()
	{
		if (GetMuzzleFlash())
		{
			GameObject flash = Instantiate(GetMuzzleFlash(), transform.position, Quaternion.identity, GetMouth().transform);
			GameObject pellets = null;
			aimProjectile(flash);
			if (pellets != null)
				aimProjectile(pellets);
			flash.transform.Rotate(Vector3.forward, Random.Range(0, 360));

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

	IEnumerator ReloadAnimation()
	{
		yield return new WaitForSeconds(reloadDelay);
		animator.SetTrigger("Reload");
		yield return new WaitForSeconds(0.2f);
		GetASO().PlayOneShot(reloadSound);
	}
}
