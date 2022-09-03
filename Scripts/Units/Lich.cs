using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lich : MonoBehaviour, IChaser, ISummoner, ITargetable, IHoverer, IRandomTargeter
{
	[Header("References")]
	[SerializeField] private AudioClip laughtrack = default;
	[SerializeField] private AudioClip summontrack = default;
	[SerializeField] private GameObject summontracer = default;
	[SerializeField] private GameObject spawnAnim = default;
	[SerializeField] private GameObject unitPrefab = default;
	[SerializeField] private Unit unit = default;
	[SerializeField] private AudioSource aso = default;
	[SerializeField] private SlowTurningHomer homer = default;
	[SerializeField] private Animator animator = default;
	[SerializeField] private ParticleSystem[] spellParticles = default;
	[SerializeField] private Transform mouth = default;
	[SerializeField] private GameObject projectilePrefab = default;
	[SerializeField] private GameObject wandererPrefab = default;

	[Header("Moving")]
	[SerializeField] private float speed = 8;
	[SerializeField] private float stoppingDistance = default;
	[SerializeField] private float ytarget = 30;
	[SerializeField] private float ythreshold = 70;
	[SerializeField] private int randomTargetPercentChance = 30;

	[Header("Spawning and Spells")]
	[SerializeField] private int maxNecroSpawns = 20;
	[SerializeField] private int wavenumber = 1;
	[SerializeField] private float spellCastRate = 10;
	[SerializeField] private float spawndelay = 4;
	[SerializeField] private int pillarCount = 3;
	[SerializeField] private float spellRandomisationOffset = 1.5f;

	private GameManagerScript gm;
	private Transform target;
	private SoundLibrary sl;
	private BoidController bc;
	private Transform pjtsf;
	private List<GameObject> wanderers;
	private Transform playerTransform;
	private float targetCooldown = 0;
	private float spellCastTimestamp = 0f;

    public int GetMaxSpawns() { return maxNecroSpawns; }
	public AudioClip GetLaugh() { return laughtrack; }
	public Transform GetTarget() { return target; }

	public void Start()
	{
		gm = GameManagerScript.GetGMScript();
		spellCastTimestamp = Time.time + spawndelay + Random.Range(-spellRandomisationOffset, spellRandomisationOffset);
		wanderers = new List<GameObject>();
		sl = gm.GetSoundLibrary();
		bc = gm.GetBoidController();
		pjtsf = gm.GetPJTSF();
		unit.SetDeadClip(sl.GetLichDead());
		unit.SetHurtClip(sl.GetLichHurt());
		playerTransform = gm.GetPlayer().transform;
		StartCoroutine(Lol());
	}

	public void Update()
	{
		ConfirmAndChaseTarget(gameObject);
		if (!unit.IsDead())
			CheckSummonTiming();
		else if (wanderers.Count > 0)
			foreach (GameObject wanderer in wanderers)
				wanderer.GetComponent<RandomWanderer>().AnimateDestroy();
	}

	public IEnumerator Lol()
	{
		yield return new WaitForSeconds(0.1f);
		aso.PlayOneShot(sl.ChooseLichSpawn());
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

	public void ChaseTarget()
	{
		if (target && !Singularity.isSingularityAwake)
		{
			if (Vector3.Distance(transform.position, target.position) <= stoppingDistance)
			{
				//melee would be here
			}
			else if (Vector3.Distance(transform.position, target.position) > stoppingDistance)
			{
				HoverTowards();
			}
		}
	}

	public void HoverTowards()
	{
		Vector3 targetpos = target.position;
		if (targetpos.y < ythreshold)
			targetpos.y = ytarget;
		transform.position = Vector3.MoveTowards(transform.position, targetpos, speed * Time.deltaTime);
	}

	public void CheckSummonTiming()
	{
		if (target)
		{
			if (target.GetComponent<IUnit>() != null && !target.GetComponent<IUnit>().IsDead() || target)
			{
				if (Time.time > spellCastTimestamp)
				{
					AnimateAndExecuteSpellCast();
				}
			}
		}
	}

	private void AnimateAndExecuteSpellCast()
    {
        DoAnimations();
		StartCoroutine(ChooseSpell());
    }

    private IEnumerator ChooseSpell()
	{
		float animdelay = 0.6f;
		yield return new WaitForSeconds(animdelay);
		int random = Random.Range(0, 3);
		switch (random)
        {
			case 0:
				StartCoroutine(BeginSummon());
				break;
			case 1:
				FirePillars();
				break;
			case 2:
				FireWanderer();
				break;
		}
    }

	private void FirePillars()
	{
		for (int i = 0; i < pillarCount; i++)
			FireProjectile();
	}

	public void FireProjectile()
	{
		GameObject instance = Instantiate(projectilePrefab, mouth.position, Quaternion.identity, pjtsf);
		instance.transform.LookAt(target);
		if (instance.GetComponent<IBullet>() != null)
			instance.GetComponent<IBullet>().SetParent(gameObject);
		instance.name = projectilePrefab.name;
	}
	public void FireWanderer()
	{
		GameObject instance = Instantiate(wandererPrefab, mouth.position, Quaternion.identity, pjtsf);
		instance.transform.LookAt(playerTransform);
		wanderers.Add(instance);
		if (instance.GetComponent<IBullet>() != null)
			instance.GetComponent<IBullet>().SetParent(gameObject);
		instance.name = projectilePrefab.name;
	}

	private void DoAnimations()
    {
        spellCastTimestamp = Time.time + spellCastRate + Random.Range(-spellRandomisationOffset, spellRandomisationOffset);
        if (animator)
            animator.SetTrigger("Spell");
        if (spellParticles.Length > 0)
            foreach (ParticleSystem ps in spellParticles)
            {
                ps.Play();
            }
    }

    public IEnumerator BeginSummon()
	{
		yield return new WaitForSeconds(0);
		PlaySounds();
		DoSpawns();
	}

	public void DoSpawns()
	{
		for (int i = 0; i < wavenumber; i++)
		{
			GameObject instance = Altar.ResurrectUnit(100, unitPrefab, spawnAnim, summontracer, transform.position, transform.position, transform.rotation, GameManagerScript.GetGMScript().GetNSpawns().transform);
			instance.tag = "LichSpawn";
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
}
