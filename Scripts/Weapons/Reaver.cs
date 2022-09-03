using EZCameraShake;
using System;
using System.Collections;
using UnityEngine;

public class Reaver : MonoBehaviour, IBasicWeapon, IChargingWeapon, IRepeatingBlaster, ISmokeSeparator
{
	//Basic Weapon properties
	[Header("Main Components")]
	[SerializeField] private GameObject projectilePrefab = default;
	[SerializeField] private GameObject mouth = default;
	[SerializeField] private GameObject muzzleFlash = default;
	[Header("Weak emitter")]
	[SerializeField] private ParticleSystem weakEmitter = default;
	[SerializeField] private GameObject weakSmokeEmitterPrefab = default;
	[SerializeField] private ParticleSystem weakSmokeEmitter = default;
	[Header("Strong emitter")]
	[SerializeField] private GameObject strongSmokeEmitterPrefab = default;
	[SerializeField] private ParticleSystem strongEmitter = default;
	[SerializeField] private ParticleSystem strongSmokeEmitter = default;

	[Header("Parameters")]
	[Tooltip("Generalised shoot rate. This is more for how often the reaver can fire in general (I think? don't mess with this)")]
	[SerializeField] private float shootRate = default;

	private Transform pjtsf;
	private GameManagerScript gm;
	private AudioSource aso;
	private UIController uic;
	private StatLibrary statl;
	private PlayerUnit unit;
	private Camera playercamera;
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
	public float GetOverheatTime() { return overheatTime; }
	public float GetDelayTime() { return delayTime; }
	public float GetCoolDownTime() { return cooldownTime; }
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

	//Reaver properties
	[SerializeField] private AudioClip fireclip = default;
	[SerializeField] private GameObject weakblast = default;
	[SerializeField] private int projCount = 50;
	[Tooltip("Shoot rate for (invisible) strong projectiles")]
	[SerializeField] private float firerate = 0.25f;
	[Tooltip("Shoot rate for (invisible) weak projectiles")]
	[SerializeField] private float weakshootrate = 0.3f;
	private float weakSRTS = 0;

	public void Start()
	{
		InitialiseChargingWeapon();
		InitialisePlayerWeapon();
		ReparentEmitter();
	}

	public void Update()
	{
		SetUIAmmoString("Ammo : N/A");
		InstantiateMPIfNone();
		if (Player.GetShootingPressed() && !PlayerUnit.GetPlayerDead() && !Player.GetIsCasting() && !Player.GetIsSwapping())
			HandleShooting(); 
		else if (!isFiring)
		{
			ResetCharge();
			StopEmitters();
		}
		if (cooldownTimer > 0)
		{
			cooldownTimer -= Time.deltaTime;
			LMBtimedown = 0;
			uic.SetCDUIEnabled(true);
		}
		else
			uic.SetCDUIEnabled(false);
	}

	public void HandleShooting()
	{
		LMBtimedown += Time.deltaTime;
		ChargeShot();
		if (LMBtimedown > GetDelayTime() && LMBtimedown < GetOverheatTime() && cooldownTimer <= 0 && !isFiring)
		{
			BigShoot();
		}
	}

	public void ChargeShot()
	{
		if (!isFiring && cooldownTimer <= 0)
		{
			FireWeakEmitter();
			CreateMuzzleParticles();
			if (Time.time > weakSRTS && !Player.GetIsCasting())
			{
				FireWeakProjectile();
				StartCoroutine("PlayChargeSound");
			}
		}
	}

	public void ConfirmReadyToFire()
	{
		readytofire = (Time.time > shootRateTimeStamp) && !Player.GetIsCasting();
	}

	public virtual void BeginFireProjectile()
	{
		GameObject projectileInstance = InstantiateProjectile();
		shootRateTimeStamp = Time.time + shootRate;
		aimProjectile(projectileInstance);
		CreateMuzzleFlash();
	}

	public void aimProjectile(GameObject projectile)
	{
		projectile.transform.forward = playercamera.transform.forward;
	}

	public void FireWeakProjectile()
	{
		GameObject projectileInstance = InstantiateProjectile(weakblast);
		SetSRTS(Time.time + weakshootrate);
		aimProjectile(projectileInstance);
		CameraShaker.Instance.ShakeOnce(0.5f, 0.5f, 0.2f, 0.2f);
	}

	private void BigShoot()
	{
		if (weakEmitter.isPlaying)
			weakEmitter.Stop();
		if (!strongEmitter.isPlaying)
			strongEmitter.Play();
		if (strongSmokeEmitter)
			strongSmokeEmitter.Play();
		StartCoroutine(ReleaseTheBoys());
		isFiring = true;
	}

	public IEnumerator ReleaseTheBoys()
	{
		GetASO().PlayOneShot(fireclip);
		for (int i = 0; i < projCount; i++)
		{
			yield return new WaitForSeconds(firerate);
			BeginFireProjectile();
			BeginFireProjectile();
			BeginFireProjectile();
			CameraShaker.Instance.ShakeOnce(1.0f, 1.0f, 0.1f, 0.1f);
		}
		Overheat();
		ResetState();
	}

	public void Overheat()
	{
		StartCoroutine("PlayOHSound");
		cooldownTimer = cooldownTime;
		isFiring = false;
	}

	public void ResetState()
	{
		ResetSound();
		LMBtimedown = 0;
		isFiring = false;
		isChargeSoundPlaying = false;
		Debug.Log("strongSmokeEmitter = " + strongSmokeEmitter);
		if (strongEmitter.isPlaying)
			strongEmitter.Stop();
	}

	public void ResetCharge()
	{
		StopChargeSound();
		LMBtimedown = 0;
		isChargeSoundPlaying = false;
		if (weakEmitter.isPlaying)
			weakEmitter.Stop();
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

	private void StopEmitters()
	{
		if (weakSmokeEmitter.isPlaying)
			weakSmokeEmitter.Stop();
		if (strongSmokeEmitter.isPlaying)
			strongSmokeEmitter.Stop();
	}

	public void InstantiateMPIfNone()
	{
		if (weakSmokeEmitter == null)
		{
			//Debug.Log("instantiated weak smoker");
			GameObject mpinst = Instantiate(weakSmokeEmitterPrefab, transform.position, transform.rotation, transform.parent);
			mpinst.name = weakSmokeEmitterPrefab.name;
			weakSmokeEmitter = mpinst.GetComponent<ParticleSystem>();
		}
		if (strongSmokeEmitter == null)
		{
			//Debug.Log("instantiated strong smoker");
			GameObject mpinst = Instantiate(strongSmokeEmitterPrefab, transform.position, transform.rotation, transform.parent);
			mpinst.name = strongSmokeEmitterPrefab.name;
			strongSmokeEmitter = mpinst.GetComponent<ParticleSystem>();
		}
	}

	public void ReparentEmitter()
	{
		if (weakSmokeEmitter)
			ReparentEmitter(weakSmokeEmitter);
		if (strongSmokeEmitter)
			ReparentEmitter(strongSmokeEmitter);
	}

	public void ReparentEmitter(ParticleSystem emitter)
	{
		emitter.transform.parent = transform.parent;

	}

	private void FireWeakEmitter()
	{
		if (weakEmitter)
			if (!weakEmitter.isPlaying)
				weakEmitter.Play();
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

	private void CreateMuzzleParticles()
	{
		if (weakSmokeEmitter)
		{
			//DebugSmokeEmitters();
			if (!weakSmokeEmitter.isPlaying)
				weakSmokeEmitter.Play();
		}
	}

	private void DebugSmokeEmitters()
	{
		Debug.Log("weakSmokeEmitter = " + weakSmokeEmitter);
		Debug.Log("weakSmokeEmitterplaying = " + weakSmokeEmitter.isPlaying);
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

	public void ResetSound()
	{
		GetASO().clip = null;
		isChargeSoundPlaying = false;
	}

	public IEnumerator StopFiring(float delay)
	{
		yield return new WaitForSeconds(delay);
		isFiring = false;
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
}
