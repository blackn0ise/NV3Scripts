using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour, IBullet
{
	[Header("Components")]
	[SerializeField] private GameObject dissipation = default;

	[Header("Parameters")]
	[SerializeField] private int damage = default;
	[SerializeField] private bool hasInaccurateFire = default;
	[SerializeField] private bool isPiercing = default;
	[SerializeField] private bool hasKnockback = false;
	[SerializeField] private float projDecay = default;
	[SerializeField] private float inaccuracyOffset = default;
	[SerializeField] private float knockbackFactor = 5;

	private GameObject parent;
	private AudioSource aso;
	private AudioSource playeraso;
	public float GetProjDecay() { return projDecay; }
	public float GetKnockFactor() { return knockbackFactor; }
	public bool GetHasKnockback() { return hasKnockback; }
	public GameObject GetParent() { return parent; }
	public void SetParent(GameObject value) { parent = value; }
	private bool dissipating = false;
	private TrailRenderer tr;

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

	public void Initialise()
	{
		aso = GetComponent<AudioSource>();
		playeraso = GameObject.Find("Player").GetComponent<AudioSource>();
		tr = GetComponentInChildren<TrailRenderer>();
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