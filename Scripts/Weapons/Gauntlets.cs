using EZCameraShake;
using System.Collections;
using UnityEngine;

public class Gauntlets : MonoBehaviour, IBasicWeapon, IBasicBulletShooter, ISmokeSeparator
{
	[Header("Components")]
	[SerializeField] private GameObject projectilePrefab = default;
	[SerializeField] private GameObject mouth = default;
	[SerializeField] private GameObject muzzleFlash = default;
	[SerializeField] private GameObject muzzleParticlesPrefab = default;
	[SerializeField] private GameObject aura = default;
	[SerializeField] private ParticleSystem muzzleParticles = default;
	[SerializeField] private AudioClip shoot = default;

	[Header("Parameters")]
	[SerializeField] private float shootRate = default;

	private GameObject aurainstance = null;
	private Transform pjtsf;
	private AudioSource aso;
	private Animator animator;
	private StatLibrary sl;
	private PlayerUnit unit;
	private UIController uic;
	private Camera playercamera;
	private WeaponAnimator weaponAnimator;
	private GameManagerScript gm;
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
		gm = GameManagerScript.GetGMScript();
		animator = GetComponentInChildren<Animator>();
		InitialisePlayerWeapon();
		GetASO().PlayOneShot(shoot);
		ReparentEmitter();
		uic = GameManagerScript.GetGMScript().GetUIController();
		weaponAnimator = gm.GetPlayer().GetComponentInChildren<WeaponAnimator>();
		SetWeaponAnimatorEquip(null);
	}

	public void Update()
	{
		SetUIAmmoString("Ammo : N/A");
		ConfirmReadyToFire();
		if (Player.GetShootingPressed() && !PlayerUnit.GetPlayerDead() && !Player.GetIsCasting() && !Player.GetIsSwapping())
			HandleShooting();
		if (aurainstance != null && !Player.GetShootingPressed())
			Deactivate();
		InstantiateMPIfNone();
	}

	public void InstantiateMPIfNone()
	{
		if (!muzzleParticles)
		{
			Debug.Log("instantiated new particle system");
			GameObject mpinst = Instantiate(muzzleParticlesPrefab, transform.position, transform.rotation, transform.parent);
			mpinst.name = muzzleParticlesPrefab.name;
			muzzleParticles = mpinst.GetComponent<ParticleSystem>();
		}
	}

	public void ReparentEmitter()
	{
		muzzleParticles.transform.parent = transform.parent;
	}

	private void Deactivate()
	{
		if (animator.GetBool("IsAttacking") != false)
			animator.SetBool("IsAttacking", false);
		Destroy(aurainstance);
		aurainstance = null;
		if (muzzleParticles)
			if (muzzleParticles.isPlaying)
				muzzleParticles.Stop();
	}

	public void HandleShooting()
	{
		if (GetReadyToFire())
		{
			BeginFireProjectile();
			if (Player.GetShootingPressed() && aurainstance == null)
				Activate();
		}
	}

	private void Activate()
	{
		animator.SetBool("IsAttacking", true);
		GetASO().PlayOneShot(shoot);
		aurainstance = Instantiate(aura, GetMouth().transform.position, Quaternion.identity, transform);
		aurainstance.transform.forward = GetCamera().transform.forward;
		if (muzzleParticles)
			muzzleParticles.Play();
	}

	public void InitialisePlayerWeapon()
	{
		playercamera = gm.GetSceneCamera();
		unit = GameManagerScript.GetGMScript().GetPlayer().GetComponent<PlayerUnit>();
		sl = GameManagerScript.GetGMScript().GetStatl();
		aso = GameManagerScript.GetGMScript().GetPlayer().GetComponent<AudioSource>();
		pjtsf = GameObject.Find("Projectiles").transform;
	}

	public void ConfirmReadyToFire()
	{
		readytofire = Time.time > shootRateTimeStamp && !Player.GetIsCasting();
	}

	public virtual void BeginFireProjectile()
	{
		GameObject projectileInstance = InstantiateProjectile();
		shootRateTimeStamp = Time.time + shootRate;
		aimProjectile(projectileInstance);
		CreateMuzzleFlash();
		CameraShaker.Instance.ShakeOnce(1.5f, 1.5f, 0.2f, 0.2f);
	}

	public GameObject InstantiateProjectile()
	{
		Vector3 projPosition = mouth.transform.position;
		GameObject projectileInstance = Instantiate(projectilePrefab, projPosition, Quaternion.identity, pjtsf);
		projectileInstance.name = projectilePrefab.name;
		if (projectileInstance.GetComponent<Bullet>())
			projectileInstance.GetComponent<Bullet>().SetParent(GameManagerScript.GetGMScript().GetPlayer().gameObject);
		return projectileInstance;
	}

	public GameObject InstantiateProjectile(GameObject projectile)
	{
		Vector3 projPosition = mouth.transform.position;
		GameObject projectileInstance = Instantiate(projectile, projPosition, Quaternion.identity, pjtsf);
		projectileInstance.name = projectile.name;
		if (projectileInstance.GetComponent<Bullet>())
			projectileInstance.GetComponent<Bullet>().SetParent(GameManagerScript.GetGMScript().GetPlayer().gameObject);
		return projectileInstance;
	}

	public void CreateMuzzleFlash()
	{
		if (GetMuzzleFlash())
		{
			GameObject flash = Instantiate(GetMuzzleFlash(), transform.position, Quaternion.identity, GetMouth().transform);
			aimProjectile(flash);
			flash.transform.Rotate(Vector3.forward, Random.Range(0, 360));
			Destroy(flash, 0.05f);
		}
	}

	public void aimProjectile(GameObject projectile)
	{
		projectile.transform.forward = playercamera.transform.forward;
	}
}
