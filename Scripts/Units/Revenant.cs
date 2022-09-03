using System.Collections;
using UnityEngine;

public class Revenant : MonoBehaviour, ITargetableBoid, IShooter, IChaser, IMeleeAttacker, IRandomTargeter
{

	[Header("Components")]
	[SerializeField] private Boid boid = default;
	[SerializeField] private AudioSource aso = default;
	[SerializeField] private Unit unit = default;
	[SerializeField] private GameObject projectilePrefab = default;
	[SerializeField] private GameObject meleeProjectilePrefab = default;
	[SerializeField] private GameObject fireParticles = default;
	[SerializeField] private Transform mouth = default;
	[SerializeField] private Animator animator = default;
	[SerializeField] private Refacer refacer = default;

	[Header("Moving")]
	[SerializeField] private float stoppingDistance = 15;
	[SerializeField] private int randomTargetPercentChance = 30;
	[SerializeField] private float strafeVectorStrength = 1;

	[Header("Shooting")]
	[SerializeField] private float shootdelay = 4f;
	[SerializeField] private float shootRate = 1.3f;
	[SerializeField] private float shootRandomiserMax = 1.5f;
	[SerializeField] private float meleeRandomiserMax = 1.5f;

	private Transform target;
	private SoundLibrary sl;
	private Transform pjtsf;
	private GameManagerScript gm;
	private float confirmedStrafeDuration = 3;
	private float strafeTime = 0;
	private float shootRateTimeStamp = 0f;
	private bool isStrafing = false;
	private Vector3 strafeDirection = Vector3.zero;
	private float targetCooldown = 0;
	private float hoverRandomSeedx = 0;
	private float hoverRandomSeedy = 0;
	private float hoverRandomSeedz = 0;

    public GameObject GetProjPrefab() { return projectilePrefab; }
	public void SetSRTS(float value) { shootRateTimeStamp = value; }
	public float GetShootRate() { return shootRate; }
	public float GetStoppingDistance() { return stoppingDistance; }
	public AudioSource GetASO() { return aso; }
	public Unit GetUnit() { return unit; }
	public Transform GetTarget() { return target; }
	public SoundLibrary GetSL() { return sl; }
	public void SetSL(SoundLibrary value) { sl = value; }
	public void SetUnit(Unit value) { unit = value; }
	public void SetShootRate(float value) { shootRate = value; }
	public void SetASO(AudioSource value) { aso = value; }

	void Start()
	{
		InitialiseRevenant();

	}

	private void InitialiseRevenant()
	{
		gm = GameManagerScript.GetGMScript();
		sl = gm.GetSoundLibrary();
		pjtsf = gm.GetPJTSF();
		shootRateTimeStamp = Time.time + shootdelay;
		hoverRandomSeedx = Random.value;
		hoverRandomSeedy = Random.value;
		hoverRandomSeedz = Random.value;

		switch (unit.name) // switch only necessary if different revenants are going to have different sounds; not currently built
		{
			case "Revenant":
				unit.SetDeadClip(sl.GetRevenantDead());
				unit.SetHurtClip(sl.GetRevenantHurt());
				break;
			case "TutorialRevenant":
				unit.SetDeadClip(sl.GetRevenantDead());
				unit.SetHurtClip(sl.GetRevenantHurt());
				break;
			case "EliteRevenant":
				unit.SetDeadClip(sl.GetRevenantDead());
				unit.SetHurtClip(sl.GetRevenantHurt());
				break;
			case "FriendlyRevenant":
				unit.SetDeadClip(sl.GetRevenantDead());
				unit.SetHurtClip(sl.GetRevenantHurt());
				break;
		}
	}

	void Update()
	{
		ConfirmAndChaseTargetAsBoid(gameObject);
		if (!unit.IsDead())
			AttackTarget();
	}

	public void ConfirmAndChaseTargetAsBoid(GameObject thisUnit)
	{
		if (targetCooldown > 0)
			targetCooldown -= Time.deltaTime;
		if (!target)
		{
			RollPlayerRandomOrClosest(thisUnit);
			refacer.ResetFocusing(transform);
			Homing.ResetXRotation(1, transform);
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
			boid.DoMovement(target.position + 0.1f * FindRandomisedVector());
			refacer.FocusTowards(target);
		}
		else if (Vector3.Distance(transform.position, target.position) > stoppingDistance)
		{
			boid.DoMovement(target.position);
			refacer.ResetFocusing(transform);
		}
	}

    private Vector3 FindRandomisedVector()
	{
		float hoveroffsetx = Mathf.Clamp(3*Mathf.Sin(Time.time + hoverRandomSeedx), 0.8f, 1);
		float hoveroffsety = Mathf.Abs(Mathf.Sin(Time.time + hoverRandomSeedy));
		float hoveroffsetz = -Mathf.Abs(Mathf.Sin(Time.time + hoverRandomSeedz));

		Vector3 backward = -1 * gameObject.transform.forward;

		Vector3 offset = new Vector3(hoveroffsetx, hoveroffsety, hoveroffsetz);
		Vector3 newDirection = backward + offset;

		newDirection = newDirection.normalized;
		//Debug.Log("newDirection * strafeVectorStrength = " + newDirection * strafeVectorStrength);
		return newDirection * strafeVectorStrength;
	}


	public void StrafeInDirection(Vector3 direction)
	{
		if (strafeTime < confirmedStrafeDuration)
		{
			boid.DoMovement(strafeDirection);
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

	public void AttackTarget()
	{
		if (target)
		{
			if (target.GetComponent<IUnit>() != null && !target.GetComponent<IUnit>().IsDead())
			{
				if (Time.time > shootRateTimeStamp)
				{

					if (Vector3.Distance(transform.position, target.position) <= stoppingDistance)
					{
						AnimateFireMeleeProjectile();
					}
					else if (Vector3.Distance(transform.position, target.position) > stoppingDistance)
					{
						AnimateFireProjectile();
					}
				}
			}
		}
	}

	public void AnimateFireMeleeProjectile()
	{
		shootRateTimeStamp = Time.time + shootRate + Random.Range(0, meleeRandomiserMax);
		if (animator)
			animator.SetTrigger("Fire");
		StartCoroutine(FireMeleeProjectile());
	}

	public void AnimateFireProjectile()
	{
		shootRateTimeStamp = Time.time + shootRate + Random.Range(0, shootRandomiserMax);
		if (animator)
			animator.SetTrigger("Fire");
		StartCoroutine(FireProjectile());
	}

	public virtual IEnumerator FireProjectile()
	{
		float animdelay = 0.6f;
		yield return new WaitForSeconds(animdelay);
		GameObject instance = Instantiate(projectilePrefab, mouth.position, Quaternion.identity, pjtsf);
		GameObject fp = Instantiate(fireParticles, mouth.position, Quaternion.identity, pjtsf);
		instance.transform.LookAt(target);
		Destroy(fp, 3.0f);
		if (instance.GetComponent<IBullet>() != null)
			instance.GetComponent<IBullet>().SetParent(gameObject);
		instance.name = projectilePrefab.name;
	}

	public virtual IEnumerator FireMeleeProjectile()
	{
		float animdelay = 0.6f;
		yield return new WaitForSeconds(animdelay);
		GameObject instance = Instantiate(meleeProjectilePrefab, mouth.position, Quaternion.identity, pjtsf);
		GameObject fp = Instantiate(fireParticles, mouth.position, Quaternion.identity, pjtsf);
		instance.transform.LookAt(target);
		Destroy(fp, 3.0f);
		if (instance.GetComponent<IBullet>() != null)
			instance.GetComponent<IBullet>().SetParent(gameObject);
		instance.name = projectilePrefab.name;
	}
}
