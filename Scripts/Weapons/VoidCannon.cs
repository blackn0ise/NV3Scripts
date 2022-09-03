using EZCameraShake;
using System;
using System.Collections;
using UnityEngine;

public class VoidCannon : MonoBehaviour, IBasicWeapon, ISingleShotBlaster, ICostingWeapon, IChargingWeapon
{
	[Header("Components")]
	[SerializeField] private GameObject projectilePrefab = default;
	[SerializeField] private GameObject mouth = default;
	[SerializeField] private GameObject muzzleFlash = default;
	[SerializeField] private ParticleSystem succParticles = default;
	[SerializeField] private Animator animator = default;

	[Header("Parameters")]
	[SerializeField] private float shootRate = default;
	private Transform pjtsf;
	private AudioSource aso;
	private StatLibrary statl;
	private PlayerUnit unit;
	private GameManagerScript gm;
	private Camera playercamera;
	private UIController uic;
	private WeaponAnimator weaponAnimator;
	public bool readytofire { get; set; }
	private float shootRateTimeStamp = 0f;

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
	[SerializeField] private AudioClip chargeSound = default;
	[SerializeField] private AudioClip overheatSound = default;
	[SerializeField] private float overheatTime = default;
	[SerializeField] private float cooldownTime = default;
	private float delayTime;

	public float cooldownTimer { get; set; }
	public float LMBtimedown { get; set; }
	public bool isChargeSoundPlaying { get; set; }
	public bool isOverheatSoundPlaying { get; set; }
	public bool isFiring { get; set; } //used to tell whether to play charge sound or not
	public float GetOverheatTime() { return overheatTime; }
	public float GetDelayTime() { return delayTime; }
	public float GetCoolDownTime() { return cooldownTime; }

	//Void Cannon properties
	[SerializeField] private AudioClip fireclip = default;
	[SerializeField] private static int voidcost = 500;
	public static int GetVoidCost() { return voidcost; }

	public void Start()
	{
		InitialiseChargingWeapon();
		InitialisePlayerWeapon();
	}

	public void InitialisePlayerWeapon()
	{
		gm = GameManagerScript.GetGMScript();
		playercamera = gm.GetSceneCamera();
		unit = gm.GetPlayer().GetComponent<PlayerUnit>();
		statl = gm.GetStatl();
		aso = gm.GetPlayer().GetComponent<AudioSource>();
		pjtsf = gm.GetPJTSF();
		uic = gm.GetUIController();
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
		delayTime = chargeSound.length;
	}

	public void Update()
	{
		SetUIAmmoString(GetStatLibrary().GetSouls() > voidcost ? "Shot ready" : "Shot unavailable");
		if (Player.GetShootingPressed() && !PlayerUnit.GetPlayerDead() && !Player.GetIsCasting() && !Player.GetIsSwapping())
			HandleShooting();
		else if (!isFiring)
			ResetCharge();

		if (cooldownTimer > 0)
		{
			cooldownTimer -= Time.deltaTime;
		}
	}

	public void ResetCharge()
	{
		StopChargeSound();
		GetASO().Stop();
		LMBtimedown = 0;
		isChargeSoundPlaying = false;
		if (succParticles.isPlaying)
			succParticles.Stop();
	}

	public void ResetState()
	{
		StopChargeSound();
		GetASO().Stop();
		LMBtimedown = 0;
		isFiring = false;
		isChargeSoundPlaying = false;
	}

	public void PlayChargeSound()
	{
		if (!isChargeSoundPlaying)
		{
			GetASO().clip = chargeSound;
			GetASO().Play();
			isChargeSoundPlaying = true;
		}
	}

	public void StopChargeSound()
	{
		GetASO().Stop();
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
		isFiring = false;
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
		shootRateTimeStamp = Time.time + shootRate;
		aimProjectile(projectileInstance);
		CreateMuzzleFlash();
		CameraShaker.Instance.ShakeOnce(3, 3, 1.2f, 3.0f);
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
		}
	}

	public void DoShoot()
	{
		if (GetStatLibrary().GetSouls() > voidcost)
		{
			StartCoroutine(GetEmBoi());
			isFiring = true;
			GetStatLibrary().SetSouls(GetStatLibrary().GetSouls() - voidcost);
			voidcost *= 3;
		}
		else
		{
			StartCoroutine(StopFiring(0.8f));
			isFiring = true;
		}
	}

	public IEnumerator GetEmBoi()
	{
		BeginFireProjectile();
		GetASO().PlayOneShot(fireclip);
		animator.SetTrigger("Fire");
		yield return new WaitForSeconds(0);
	}
}
