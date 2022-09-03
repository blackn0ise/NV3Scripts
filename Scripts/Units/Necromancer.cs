using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Necromancer : MonoBehaviour, ISummoner, ITargetable, IChaser, IStrafer, IHoverer, IRandomTargeter
{
	[Header("References")]
	[SerializeField] private Rigidbody rb = default;
	[SerializeField] private AudioClip laughtrack = default;
	[SerializeField] private AudioClip summontrack = default;
	[SerializeField] private GameObject summontracer = default;
	[SerializeField] private GameObject spawnAnim = default;
	[SerializeField] private GameObject unitPrefab = default;
	[SerializeField] private SlowTurningHomer homer = default;
	[SerializeField] private Unit unit = default;
	[SerializeField] private Animator animator = default;
	[SerializeField] private Transform resPosition = default;

	[Header("Moving")]
	[SerializeField] private float speed = 8;
	[SerializeField] private float stoppingDistance = 0;
	[SerializeField] private float ytarget = 35;
	[SerializeField] private float ythreshold = 70;
	[SerializeField] private float baseStrafeDuration = 5;
	[SerializeField] private float strafeDurationOffset = 2;
	[SerializeField] private float strafeChance = 80.0f;
	[SerializeField] private int randomTargetPercentChance = 30;

	[Header("Spawning")]
	[SerializeField] private int wavenumber = 6;
	[SerializeField] private float spawnrate = 25;
	[SerializeField] private float spawndelay = 4;
	[SerializeField] private float resPositionVariance = 150;

	private Vector3 strafeDirection = Vector3.zero;
	private float delayDuration = 0;
	private float strafeTime = 0;
	private float spawnRateTimeStamp = 0f;
	private float targetCooldown = 0;
	private bool isStrafing = false;
	private float confirmedStrafeDuration = 3;
	private bool canStartStrafe = true;
	private GameManagerScript gm;
	private Transform target;
	private SoundLibrary sl;
	private SpawnController sc;
	private BoidController bc;
    

    public Transform GetTarget() { return target; }

	public void Start()
	{
		gm = FindObjectOfType<GameManagerScript>();
		spawnRateTimeStamp = Time.time + spawndelay;
		sl = gm.GetSoundLibrary();
		bc = gm.GetBoidController();
		sc = gm.GetSpawnController();

		unit.SetDeadClip(sl.GetNecroDead());
		unit.SetHurtClip(sl.GetNecroHurt());
	}

	public void Update()
	{
		ConfirmAndChaseTarget(gameObject);
		if (!unit.IsDead())
			CheckSummonTiming();
	}

	public void ConfirmAndChaseTarget(GameObject thisUnit)
	{
		if (targetCooldown > 0)
			targetCooldown -= Time.deltaTime;
		if (!target || !homer.GetTarget())
		{   //no target
			RollPlayerRandomOrClosest(thisUnit);
		}
		else if (target.GetComponent<IUnit>() != null && target.GetComponent<IUnit>().IsDead())
		{   //target has no unit component or unit is dead
			RollPlayerRandomOrClosest(thisUnit);
		}
		else if (Homing.ConfirmBadRange(target, transform, homer.GetTargetHasCollider()))
		{   //try get a random target if bad range
			TryChooseOtherRandom();
		}
		else if (!unit.IsDead())
		{   //target is alive and available to chase
			ChaseTarget();
		}
		else return;
	}

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
					homer.SetTarget(target);
				}
				else
				{
					target = randomEnemy;
					homer.SetTarget(randomEnemy);
				}
			}
			else
			{
				target = Unit.GetClosestEnemy(thisUnit);
				homer.SetTarget(target);
			}
			if (target == originalTarget)
				ActivateTargetCooldown(0.5f);
		}
	}

	public void TryChooseOtherRandom()
	{
		if (targetCooldown <= 0)
		{
			GameObject originalTarget = target.gameObject;
			Transform randomEnemy = Unit.GetRandomEnemy(gameObject);
			if (randomEnemy && randomEnemy != originalTarget)
			{
				target = randomEnemy;
				homer.SetTarget(randomEnemy);
			}
			if (target == originalTarget)
				ActivateTargetCooldown(2.5f);
		}
	}

	private void ActivateTargetCooldown(float duration)
	{
		Debug.Log($"{gameObject.name} targeting on cooldown for .5s.");
		targetCooldown = duration;
	}

	public void CheckSummonTiming()
	{
		if (target)
		{
			if (target.GetComponent<IUnit>() != null && !target.GetComponent<IUnit>().IsDead() || target)
			{
				if (Time.time > spawnRateTimeStamp)
				{
					spawnRateTimeStamp = Time.time + spawnrate;
					StartCoroutine(BeginSummon());
				}
			}
		}
	}

	public IEnumerator BeginSummon()
	{
		yield return new WaitForSeconds(0);
		if (Resurrector.CountFriends("NecroSpawn") >= sc.getMaxNecroSpawns())
			yield break;
		PlaySounds();
		DoSpawns();
	}

	public void DoSpawns()
	{
		if (animator)
			animator.SetTrigger("Open");
		for (int i = 0; i < wavenumber; i++)
		{
			GameObject instance = Altar.ResurrectUnit(resPositionVariance, unitPrefab, spawnAnim, summontracer, resPosition.position, transform.position, transform.rotation, GameManagerScript.GetGMScript().GetNSpawns().transform);
			instance.tag = "NecroSpawn";
			instance.name = unitPrefab.name;
			if (bc)
				if (instance.GetComponent<Boid>())
				{
					Boid boid = instance.GetComponent<Boid>();
					boid.SetBoidController(bc);
					bc.AddToBoidList(boid);
				}
					
			instance.transform.forward = transform.forward;
		}
	}

	public void PlaySounds()
	{
		SoundLibrary.ResetPitch(gameObject.GetComponentInChildren<AudioSource>());
		SoundLibrary.varySoundPitch(gameObject.GetComponentInChildren<AudioSource>(), 0.5f);
		gameObject.GetComponentInChildren<AudioSource>().PlayOneShot(laughtrack);
		gameObject.GetComponentInChildren<AudioSource>().PlayOneShot(summontrack);
	}

	public void ChaseTarget()
	{
		if (target && !Singularity.isSingularityAwake)
		{
			if (!isStrafing)
			{
				ConsiderPositioning();
			}
			else
			{
				StrafeInDirection(strafeDirection);
			}
		}
	}

	public void StrafeInDirection(Vector3 direction)
	{
		if (strafeTime < confirmedStrafeDuration)
		{
			rb.velocity = direction * speed / 2 * Time.deltaTime;
			strafeTime += Time.deltaTime;
		}
		else
			StopStrafing();
	}

	public void ConsiderPositioning()
	{
		if (Vector3.Distance(transform.position, target.position) <= stoppingDistance)
		{
			TryStrafe();
		}
		else if (Vector3.Distance(transform.position, target.position) > stoppingDistance)
		{
			HoverTowards();
		}
	}

	public void HoverTowards()
	{
		Vector3 targetpos = target.position;
		if (targetpos.y < ythreshold)
			targetpos.y = ytarget;
		transform.position = Vector3.MoveTowards(transform.position, targetpos, speed * Time.deltaTime);
	}

	public void CoinflipStrafe()
	{
		float random = Random.Range(0, 100.0f);
		if (random < strafeChance)
			StartStrafe();
		else
			StrafeDelay(5.0f);
	}

	public void StartStrafe()
	{
		isStrafing = true;
		canStartStrafe = false;
		ChooseStrafeDirectionAndDuration();
	}

	public void StopStrafing()
	{
		isStrafing = false;
		strafeTime = 0;
		confirmedStrafeDuration = 0;
	}

	public void StrafeDelay(float possibletotal)
	{
		float random = Random.Range(0, possibletotal);
		delayDuration = random;
		canStartStrafe = false;
	}

	public void ChooseStrafeDirectionAndDuration()
	{
		Vector3 backward = -1 * gameObject.transform.forward;
		float offsetamount = 0.5f;
		Vector3 offset = new Vector3(Random.Range(-offsetamount, offsetamount), Random.Range(-offsetamount, offsetamount), Random.Range(-offsetamount, offsetamount));
		Vector3 newDirection = backward + offset;
		newDirection = newDirection.normalized;
		strafeDirection = newDirection;
		confirmedStrafeDuration = Random.Range(baseStrafeDuration - strafeDurationOffset, baseStrafeDuration + strafeDurationOffset);
	}

	public void TryStrafe()
	{
		if (canStartStrafe && !isStrafing)
		{
			CoinflipStrafe();
		}
		else if (delayDuration > 0)
			delayDuration -= Time.deltaTime;
		else
			canStartStrafe = true;
	}
}
