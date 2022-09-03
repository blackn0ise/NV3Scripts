using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Lacerator : MonoBehaviour, IShooter, ITargetableBoid, IChaser, IExploder, IRandomTargeter
{
	[Header("References")]
	[SerializeField] private AudioClip explodeClip = default;
	[SerializeField] private GameObject projectilePrefab = default;
	[SerializeField] private Unit unit = default;
	[SerializeField] private Boid boid = default;
	[Header("Parameters")]
	[SerializeField] private int shrapnelCount = default;
	[SerializeField] private float acceleration = 4;
	[SerializeField] private float stoppingDistance = 0;
	[SerializeField] private float speedincreasecdtime = 2;
	[SerializeField] private int randomTargetPercentChance = 40;
	private GameManagerScript gm;
	private AudioSource playeraso = default;
	private Transform target;
	private SoundLibrary sl;
	private Transform pjtsf;
	private float targetCooldown = 0;
	private bool inspeedcooldown;
	public Transform GetTarget() { return target; }

	public void Start()
	{
		gm = GameManagerScript.GetGMScript();
		sl = gm.GetSoundLibrary();
		playeraso = gm.GetPlayer().GetComponentInChildren<AudioSource>();
		unit.SetDeadClip(sl.GetLacerDead());
		unit.SetHurtClip(sl.GetLacerHurt());
		pjtsf = gm.GetPJTSF().transform;

	}

	public void Update()
	{
		if (!unit.IsDead())
		{
			StartCoroutine(IncreaseMaxSpeed());
			ConfirmAndChaseTargetAsBoid(gameObject);
		}
	}

	IEnumerator IncreaseMaxSpeed()
	{
		if (!inspeedcooldown)
		{
			inspeedcooldown = true;
			yield return new WaitForSeconds(speedincreasecdtime);
			boid.SetMaxSpeed(boid.GetMaxSpeed() + acceleration);
			inspeedcooldown = false; 
		}

	}

	public void ConfirmAndChaseTargetAsBoid(GameObject thisUnit)
	{
		if (targetCooldown > 0)
			targetCooldown -= Time.deltaTime;
		if (!target)
		{
			RollPlayerRandomOrClosest(thisUnit);
		}
		else if (target.GetComponent<IUnit>() != null && target.GetComponent<IUnit>().IsDead())
		{
			RollPlayerRandomOrClosest(thisUnit);
		}
		else if (!unit.IsDead())
		{
			ChaseTarget();
		}
		else return;
	}

	//no homing component for random rolling

	public void RollPlayerRandomOrClosest(GameObject thisUnit)
	{
		int rollPercent = Random.Range(0, 100);

		if (targetCooldown <= 0)
		{
			GameObject originalTarget = null;
			if (target)
				originalTarget = target.gameObject;
			Transform randomEnemy = Unit.GetRandomEnemy(gameObject);
			if (rollPercent < randomTargetPercentChance && randomEnemy && randomEnemy != originalTarget)
			{
				if (!gameObject.CompareTag("Friendlies") && !gm.GetPlayerUnit().IsDead())
				{
					target = gm.GetPlayer().transform;
				}
				else
				{
					target = randomEnemy;
				}
			}
			else
			{
				target = Unit.GetClosestEnemy(thisUnit);
			}
			if (target == originalTarget)
				ActivateTargetCooldown(0.5f);
		}
	}

	private void ActivateTargetCooldown(float duration)
	{
		Debug.Log($"{gameObject.name} targeting on cooldown for .5s.");
		targetCooldown = duration;
	}

	public void ChaseTarget()
	{
		if (target && !Singularity.isSingularityAwake)
		{
			if (Vector3.Distance(transform.position, target.position) <= stoppingDistance && !unit.IsDead())
			{
				Explode();
			}
			else if (Vector3.Distance(transform.position, target.position) > stoppingDistance)
			{
				boid.DoMovement(target.position);
			}
		}
	}

	public void Explode()
	{
		for (int i = 0; i < shrapnelCount; i++)
		{
			StartCoroutine(FireProjectile());
		}
		playeraso.PlayOneShot(explodeClip, 0.5f);
	}

	public IEnumerator FireProjectile()
	{
		float animdelay = 0f;
		yield return new WaitForSeconds(animdelay);
		Vector3 spawnLoc = transform.position;
		GameObject instance = Instantiate(projectilePrefab, spawnLoc, Quaternion.identity, pjtsf);
		if (instance.GetComponent<Bullet>())
			instance.GetComponent<Bullet>().SetParent(gameObject);
		instance.name = projectilePrefab.name;
		instance.transform.forward = transform.forward;
		Destroy(gameObject);
	}
}
