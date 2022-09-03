using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class BulletPillar : MonoBehaviour, IBullet
{
	[Header("Components")]
	[SerializeField] private GameObject dissipation = default;

	[Header("Parameters")]
	[SerializeField] private int damage = default;
	[SerializeField] private bool hasInaccurateFire = default;
	[SerializeField] private bool isPiercing = default;
	[SerializeField] private bool hasKnockback = false;
	[SerializeField] private float projSpeed = default;
	[SerializeField] private float projDecay = default;
	[SerializeField] private float inaccuracyOffset = default;
	[SerializeField] private float knockbackFactor = 5;

	private GameObject parent;
	private AudioSource aso;
	private AudioSource playeraso;
	private GameObject player;
	private Player playerScript;
	private GameManagerScript gameManagerScript;
	public float GetProjDecay() { return projDecay; }
	public float GetKnockFactor() { return knockbackFactor; }
	public bool GetHasKnockback() { return hasKnockback; }
	public GameObject GetParent() { return parent; }
	public void SetParent(GameObject value) { parent = value; }

	private TrailRenderer tr;
	private bool hashomingcomponent;
	private bool hasrigidbody;
	private bool dissipating = false;

	public int GetDamage() { return damage; }

	public void Start()
	{
		if (parent)
		{
			Initialise();
			HandleQuadDamage();
			HandleAccuracy();
			if (aso)
				SoundLibrary.varySoundPitch(aso, 0.2f);
 			HandleDestruction();
		}
	}

	public void Initialise()
	{
		hashomingcomponent = GetComponent<Homing>();
		hasrigidbody = transform.GetComponent<Rigidbody>();
		SyncTargetForRevenants();
		SyncTargetForFJuggs();
		aso = GetComponent<AudioSource>();
		gameManagerScript = GameManagerScript.GetGMScript();
		player = gameManagerScript.GetPlayer();
		playerScript = player.GetComponent<Player>();
		playeraso = player.GetComponent<AudioSource>();
		tr = GetComponentInChildren<TrailRenderer>();
	}

	public void HandleQuadDamage()
	{
		if (parent == player || parent == player)
		{
			damage *= playerScript.GetDamageModifier();
			if (playerScript.GetDamageModifier() > 1)
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
			transform.forward += (new Vector3(Random.Range(-inaccuracyOffset, inaccuracyOffset), 0, Random.Range(-inaccuracyOffset, inaccuracyOffset)));
		}
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
	}

	public void FixedUpdate()
	{
		if (!hashomingcomponent)
			if (hasrigidbody)
				transform.GetComponent<Rigidbody>().velocity = transform.forward * projSpeed * Time.deltaTime;
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

	public void SyncTargetForFJuggs()
	{
		var homing = GetComponent<HomingProjectile>();
		var juggParented = parent.GetComponent<Juggernaut>();
		var friendly = parent.CompareTag("Friendlies");
		if (homing && juggParented && friendly)
		{
			homing.SetTarget(juggParented.GetTarget());
			transform.forward = parent.transform.forward;
		}
	}

	public void SyncTargetForRevenants()
	{
		var homing = GetComponent<Homing>();
		var revenantParented = parent.GetComponent<Revenant>();
		if (homing && revenantParented)
		{
			homing.SetTarget(revenantParented.GetTarget());
			transform.LookAt(revenantParented.GetTarget());
		}
	}

	public void HandleQuadSound()
	{
		if (FindObjectOfType<Reaver>())
		{
			playeraso.PlayOneShot(gameManagerScript.GetSoundLibrary().GetQuadBullet(), 0.05f);
		}

		else if (FindObjectOfType<Snubnose>())
		{
			playeraso.PlayOneShot(gameManagerScript.GetSoundLibrary().GetQuadBullet(), 0.33f);
		}

		else if (FindObjectOfType<Shotgun>())
		{
			playeraso.PlayOneShot(gameManagerScript.GetSoundLibrary().GetQuadBullet(), 0.05f);
		}

		else if(FindObjectOfType<Crucifier>())
		{
			playeraso.PlayOneShot(gameManagerScript.GetSoundLibrary().GetQuadBullet(), 1.0f);
		}

		else
		{
			playeraso.PlayOneShot(gameManagerScript.GetSoundLibrary().GetQuadBullet(), 0.33f);
		}
	}

	public void OnTriggerEnter(Collider other)
	{
		bool friendlyFire = CheckFriendlyFire(other);

		if (!isPiercing && other.gameObject != parent && (other.gameObject.tag != gameObject.tag) && !friendlyFire  && !dissipating)
		{
			dissipating = true;
			DissipateCollision();
		}
	}

	private void DissipateCollision()
	{
		CreateAndParentDissipation();
		Destroy(gameObject, 0.02f);
	}

	private void DestroySelf()
	{
		Destroy(gameObject);
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

}