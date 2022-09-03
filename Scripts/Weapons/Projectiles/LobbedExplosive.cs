using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbedExplosive : MonoBehaviour , IBullet
{
	[Header("Components")]
	[SerializeField] private GameObject dissipation = default;
	[SerializeField] private GameObject tracerPrefab = default;

	[Header("Parameters")]
	[SerializeField] private int damage = default;
	[SerializeField] private bool hasInaccurateFire = default;
	[SerializeField] private bool isPiercing = default;
	[SerializeField] private bool hasKnockback = false;
	[SerializeField] private float upwardsVelocity = 30.0f;
	[SerializeField] private float projSpeed = default;
	[SerializeField] private float projDecay = default;
	[SerializeField] private float inaccuracyOffset = default;
	[SerializeField] private float knockbackFactor = 5;

	[SerializeField] private float gravityfactor = 0.81f;
	private GameObject parent;
	private AudioSource aso;
	private AudioSource playeraso;
	public float GetProjDecay() { return projDecay; }
	public float GetKnockFactor() { return knockbackFactor; }
	public bool GetHasKnockback() { return hasKnockback; }
	public GameObject GetParent() { return parent; }
	public void SetParent(GameObject value) { parent = value; }
	private Transform pjtsf;
	private TrailRenderer tr;
	private GameObject tracerInstance;
	private bool dissipating = false;

	public int GetDamage() { return damage; }

	public void Start()
	{
		if (parent)
		{
			Initialise();
			HandleQuadDamage();
			IgnoreParentBulletCollisions();
			HandleAccuracy();
			if (aso)
				SoundLibrary.varySoundPitch(aso, 0.2f);
			HandleDestruction();
		}
	}

	public void Initialise()
	{
		SyncTargetForRevenants();
		aso = GetComponent<AudioSource>();
		playeraso = GameObject.Find("Player").GetComponent<AudioSource>();
		tr = GetComponentInChildren<TrailRenderer>();
		upwardsVelocity += Random.Range(-inaccuracyOffset, inaccuracyOffset);
		pjtsf = GameObject.Find("Projectiles").transform;
		CreateTracer(gameObject);
	}

	private void HandleDestruction()
	{
		StartCoroutine(AutoDissipate());
	}

	public IEnumerator AutoDissipate()
	{
		yield return new WaitForSeconds(projDecay);
		CreateAndParentDissipation();
		Destroy(gameObject);
		if (tracerInstance)
			Destroy(tracerInstance);
	}

	public void CreateAndParentDissipation()
	{
		GameObject disinst = null;
		if (dissipation)
			disinst = Instantiate(dissipation, transform.position, Quaternion.identity);
		if (disinst)
			if (disinst.GetComponent<IBullet>() != null)
				if (parent)
					disinst.GetComponent<IBullet>().SetParent(parent);
	}
	
	public void FixedUpdate()
	{
		MovePosition();
	}

	private void MovePosition()
	{
		upwardsVelocity -= gravityfactor * Time.deltaTime;
		Vector3 nextposition = transform.forward;
		nextposition.y = upwardsVelocity;
		if (!GetComponent<Homing>())
			transform.GetComponent<Rigidbody>().velocity = nextposition * projSpeed * Time.deltaTime;
	}

	public void HandleQuadDamage()
	{
		if (parent == GameManagerScript.GetGMScript().GetPlayer().gameObject || parent == GameManagerScript.GetGMScript().GetPlayer().gameObject)
		{
			damage *= GameManagerScript.GetGMScript().GetPlayer().GetComponent<Player>().GetDamageModifier();
			if (GameManagerScript.GetGMScript().GetPlayer().GetComponent<Player>().GetDamageModifier() > 1)
			{
				HandleQuadSound();
				if (tr && tr.time < 0.5f)
					GetComponentInChildren<TrailRenderer>().time = 0.5f;
			}
		}
	}

	public void HandleAccuracy()
	{
		if (hasInaccurateFire)
		{
			transform.forward += (new Vector3(Random.Range(-inaccuracyOffset, inaccuracyOffset), Random.Range(-inaccuracyOffset, inaccuracyOffset), Random.Range(-inaccuracyOffset, inaccuracyOffset)));
			transform.Rotate(new Vector3(0, 0, Random.Range(0, 180)));
		}
	}

	public void IgnoreParentBulletCollisions()
	{
		if (parent.gameObject.GetComponentInChildren<Collider>() && gameObject.GetComponentInChildren<Collider>())
			Physics.IgnoreCollision(gameObject.GetComponentInChildren<Collider>(), parent.gameObject.GetComponentInChildren<Collider>(), true);

		if (parent.gameObject.GetComponentInChildren<CharacterController>() && gameObject.GetComponentInChildren<Collider>())
			Physics.IgnoreCollision(parent.gameObject.GetComponentInChildren<CharacterController>(), gameObject.GetComponentInChildren<Collider>(), true);
	}



	public void SyncTargetForRevenants()
	{
		if (GetComponent<Homing>() && parent.GetComponent<Revenant>())
		{
			GetComponent<Homing>().SetTarget(parent.GetComponent<Revenant>().GetTarget());
			transform.forward = parent.transform.forward;
		}
	}

	public void HandleQuadSound()
	{
		if (GameObject.Find("Shotgun"))
			playeraso.PlayOneShot(GameManagerScript.GetGMScript().GetSoundLibrary().GetQuadBullet(), 0.15f);
		else if (FindObjectOfType<Crucifier>())
			playeraso.PlayOneShot(GameManagerScript.GetGMScript().GetSoundLibrary().GetQuadBullet());
		else
			playeraso.PlayOneShot(GameManagerScript.GetGMScript().GetSoundLibrary().GetQuadBullet(), 0.33f);
	}

	public void OnTriggerEnter(Collider other)
	{
		bool friendlyFire = CheckFriendlyFire(other);

		if (!isPiercing && other.gameObject != parent && (other.gameObject.tag != gameObject.tag) && !friendlyFire && !dissipating)
		{
			dissipating = true;
			DissipateCollision();
			if(tracerInstance)
				Destroy(tracerInstance);
		}
	}

	private void DissipateCollision()
	{
		CreateAndParentDissipation();
		Destroy(gameObject, 0.02f);
	}

	public bool CheckFriendlyFire(Collider other)
	{
		bool friendlyFire = false;
		List<string> friendtypes = null;
		if (parent)
			friendtypes = Unit.ConfirmFriendTypes(parent);
		if (friendtypes != null)
			foreach (string type in friendtypes)
			{
				if (other.tag == type)
					friendlyFire = true;
			}
		return friendlyFire;
	}

	public void CreateTracer(GameObject parent)
	{
		GameObject instance;
		if (tracerPrefab && parent)
		{
			instance = Instantiate(tracerPrefab, parent.transform.position, Quaternion.identity, pjtsf);
			if (instance.GetComponent<ExplosiveTracer>())
				instance.GetComponent<ExplosiveTracer>().parent = parent;
			tracerInstance = instance;
		}
	}

}