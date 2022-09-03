using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Tooth : MonoBehaviour, IBullet
{
	[Header("Components")]
	[SerializeField] private GameObject dissipation = default;
	[SerializeField] private GameObject body = default;
	[SerializeField] private AudioSource aso = default;
	[SerializeField] private Rigidbody rb = default;

	[Header("Parameters")]
	[SerializeField] private int damage = default;
	[SerializeField] private bool hasInaccurateFire = default;
	[SerializeField] private bool isPiercing = default;
	[SerializeField] private bool hasKnockback = false;
	[SerializeField] private float projSpeed = default;
	[SerializeField] private float projDecay = default;
	[SerializeField] private float inaccuracyOffset = default;
	[SerializeField] private float rotationOffset = default;
	[SerializeField] private float initialRotationMultiplier = default;
	[SerializeField] private float knockbackFactor = 5;

	private GameObject parent;
	private GameObject player;
	private Player playerScript;
	private AudioSource playeraso;
	private GameManagerScript gmscript;
	public float GetProjDecay() { return projDecay; }
	public float GetKnockFactor() { return knockbackFactor; }
	public bool GetHasKnockback() { return hasKnockback; }
	public GameObject GetParent() { return parent; }
	public void SetParent(GameObject value) { parent = value; }
	private Vector3 rotatedirection;

	public int GetDamage() { return damage; }

	public void Start()
	{
		if (parent)
		{
			Initialise();
			HandleQuadDamage();
			IgnoreParentBulletCollisions();
			HandleAccuracy();
			RandomiseRotation();
			RandomiseDamage();
			if (aso)
				SoundLibrary.varySoundPitch(aso, 0.2f);
			Destroy(gameObject, projDecay);
		}
	}

	private void RandomiseDamage()
	{
		damage += Random.Range(-damage / 2, damage / 2);
	}

	public void FixedUpdate()
	{
		Move();
	}

	private void Move()
	{
		Spin();
		rb.velocity = transform.forward * projSpeed * Time.deltaTime;
	}

	private void Spin()
	{
		body.transform.Rotate(rotatedirection);
	}

	public void Initialise()
	{
		gmscript = GameManagerScript.GetGMScript();
		player = gmscript.GetPlayer();
		playerScript = player.GetComponent<Player>();
		playeraso = player.GetComponent<AudioSource>();
	}

	private void RandomiseRotation()
	{
		rotatedirection = new Vector3(Random.Range(-rotationOffset, rotationOffset), Random.Range(-rotationOffset, rotationOffset), Random.Range(-rotationOffset, rotationOffset));
		body.transform.Rotate(rotatedirection * initialRotationMultiplier);
	}

	public void HandleQuadDamage()
	{
		if (parent == player || parent == player)
		{
			damage *= playerScript.GetDamageModifier();
			if (playerScript.GetDamageModifier() > 1)
			{
				HandleQuadSound();
			}
		}
	}

	public void HandleAccuracy()
	{
		if (hasInaccurateFire)
		{
			transform.forward += new Vector3(Random.Range(-inaccuracyOffset, inaccuracyOffset), Random.Range(-inaccuracyOffset, inaccuracyOffset), Random.Range(-inaccuracyOffset, inaccuracyOffset));
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
		if (FindObjectOfType<Shotgun>())
			playeraso.PlayOneShot(gmscript.GetSoundLibrary().GetQuadBullet(), 0.05f);
		else
			playeraso.PlayOneShot(gmscript.GetSoundLibrary().GetQuadBullet(), 0.33f);

	}

	public void OnTriggerEnter(Collider other)
	{
		bool friendlyFire = CheckFriendlyFire(other);

		if (!isPiercing && other.gameObject != parent && (other.gameObject.tag != gameObject.tag) && !friendlyFire)
		{
			if (gameObject.CompareTag("EnemyProjectile") && dissipation)
				Instantiate(dissipation, transform.position, Quaternion.identity);
			Destroy(gameObject, 0.02f);
		}
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