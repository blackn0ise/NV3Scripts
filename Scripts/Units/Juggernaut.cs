using System.Collections;
using UnityEngine;

public class Juggernaut : MonoBehaviour, ITargetableFocused, IShooter, IChaser, IStrafer, IHoverer, IRandomTargeter
{
	[Header("References")]
	[SerializeField] private GameObject mouth = default;
	[SerializeField] private AudioSource aso = default;
	[SerializeField] private Rigidbody rb = default;
	[SerializeField] private Unit unit = default;
	[SerializeField] private SlowTurningHomer homer = default;
	[SerializeField] private Animator animator = default;
	[SerializeField] private GameObject projectilePrefab = default;
	[SerializeField] private ParticleSystem[] fireParticles = default;

	[Header("Moving")]
	[SerializeField] private float speed = 30;
	[SerializeField] private float ytarget = 35;
	[SerializeField] private float ythreshold = 70;
	[SerializeField] private float stoppingDistance = 15;
	[SerializeField] private float baseStrafeDuration = 5;
	[SerializeField] private float strafeDurationOffset = 2;
	[SerializeField] private float strafeChance = 80.0f;
	[SerializeField] private int randomTargetPercentChance = 30;

	[Header("Shooting")]
	[SerializeField] private float shootdelay = 4f;
	[SerializeField] private float shootRate = 1.3f;
	[SerializeField] private int lobCount = 6;

	private Transform target;
	private SoundLibrary sl;
	private Transform pjtsf;
	private GameManagerScript gm;
	private Vector3 strafeDirection = Vector3.zero;
	private float confirmedStrafeDuration = 3;
	private float strafeTime = 0;
	private float shootRateTimeStamp = 0f;
	private float delayDuration = 0;
	private bool canStartStrafe = true;
	private bool isStrafing = false;
	private float targetCooldown = 0;

	public GameObject GetProjPrefab() { return projectilePrefab; }
	public void SetSRTS(float value) { shootRateTimeStamp = value; }
	public float GetShootRate() { return shootRate; }
	public float GetStoppingDistance() { return stoppingDistance; }
	public float GetSpeed() { return speed; }
	public AudioSource GetASO() { return aso; }
	public Unit GetUnit() { return unit; }
	public Transform GetTarget() { return target; }
	public SoundLibrary GetSL() { return sl; }
	public void SetSL(SoundLibrary value) { sl = value; }
	public void SetUnit(Unit value) { unit = value; }
	public void SetShootRate(float value) { shootRate = value; }
	public void SetSpeed(float value) { speed = value; }
	public void SetASO(AudioSource value) { aso = value; }

	void Start()
	{
		gm = GameManagerScript.GetGMScript();
		shootRateTimeStamp = Time.time + shootdelay;
		sl = gm.GetSoundLibrary();
        pjtsf = gm.GetPJTSF();

        unit.SetDeadClip(sl.GetJuggernautDead());
        unit.SetHurtClip(sl.GetJuggernautHurt());
        if (!gameObject.CompareTag("Friendlies"))
            SetPlayerTarget();
    }

    public void SetPlayerTarget()
	{
		target = gm.GetPlayer().transform;
		homer.SetTarget(target);
	}

	void Update()
	{
		ConfirmAndChasePlayer(gameObject);
		if (!unit.IsDead())
			AttackTarget();
	}

	public void ConfirmAndChasePlayer(GameObject thisUnit)
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

	public void AttackTarget()
	{
		if (target)
		{
			if (target.GetComponent<IUnit>() != null && !target.GetComponent<IUnit>().IsDead())
			{
				if (Time.time > shootRateTimeStamp)
				{
					animator.SetTrigger("Fire");
					for (int i = 0; i < lobCount; i++)
					{
						StartCoroutine(FireProjectile());
					}
					shootRateTimeStamp = Time.time + shootRate;
				}
			}
		}
	}

	public IEnumerator FireProjectile()
	{
		float animdelay = 0.6f;
		yield return new WaitForSeconds(animdelay);
		if (fireParticles.Length > 0)
			foreach (ParticleSystem ps in fireParticles)
			{
				ps.Play();
			}
		Vector3 spawnLoc = transform.position;
		GameObject instance = Instantiate(projectilePrefab, mouth.transform.position, Quaternion.identity, pjtsf);
		if (instance.GetComponent<IBullet>() != null)
			instance.GetComponent<IBullet>().SetParent(gameObject);
		instance.name = projectilePrefab.name;
		instance.transform.forward = transform.forward;
	}
}
