﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoulCollector : MonoBehaviour, ISummoner, ITargetableFocused, IChaser, ILazyHoverer, IRandomTargeter
{
	[Header("References")]
	[SerializeField] private AudioClip laughtrack = default;
	[SerializeField] private AudioClip summontrack = default;
	[SerializeField] private GameObject summontracer = default;
	[SerializeField] private GameObject spawnAnim = default;
	[SerializeField] private GameObject portal = default;
	[SerializeField] private GameObject playerPortal = default;
	[SerializeField] private ArmoredUnit unit = default;
	[SerializeField] private YLockedHomer homer = default;

	[Header("Moving")]
	[SerializeField] private float speed = 8;
	[SerializeField] private float stoppingDistance = 0;
	[SerializeField] private int randomTargetPercentChance = 30;

	[Header("Spawning")]
	[SerializeField] private int maxNecroSpawns = 20;
	[SerializeField] private int wavenumber = 6;
	[SerializeField] private float spawnrate = 25;
	[SerializeField] private float spawndelay = 4;

	private GameManagerScript gm;
	private Transform target;
	private SoundLibrary sl;
	private SpawnController sc;
	private PlayerMovement pm;
	private float spawnRateTimeStamp = 0f;
	private float targetCooldown = 0;
	public static bool soulCollectorActive = false;
	private int currentpoints;

	public int GetMaxSpawns() { return maxNecroSpawns; }
	public Transform GetTarget() { return target; }
	public void SetUnit(ArmoredUnit value) { unit = value; }

	public void Start()
	{
		gm = GameManagerScript.GetGMScript();
		pm = gm.GetPlayer().GetComponent<PlayerMovement>();
		spawnRateTimeStamp = Time.time + spawndelay;
		sl = gm.GetSoundLibrary();
		sc = gm.GetSpawnController();
		soulCollectorActive = true;
		currentpoints = GetComponentsInChildren<Weakpoint>().Length;
		unit.SetDeadClip(sl.GetVizierDead());
		unit.SetHurtClip(sl.GetVizierHurt());

		foreach (Weakpoint wp in GetComponentsInChildren<Weakpoint>())
		{
			wp.SetHurtClip(sl.GetVizierHurt());
		}
		SetPlayerTarget();
	}

	public void SetPlayerTarget()
	{
		target = gm.GetPlayer().transform;
		homer.SetTarget(target);
	}

	public void Update()
	{
		ConfirmAndChasePlayer(gameObject);
		if (!unit.IsDead())
		{
			CheckSummonTiming();
		}
		CheckPointDied();

	}

	private void CheckPointDied()
	{
		if (currentpoints != unit.GetRemainingPoints() && unit.GetRemainingPoints() != 0)
		{
			//StartCoroutine(TeleportPlayer());
			currentpoints = unit.GetRemainingPoints();
		}
	}

	private IEnumerator TeleportPlayer(float delay = 2.5f)
	{
		yield return new WaitForSeconds(delay);
		gm.GetPlayer().transform.position = playerPortal.transform.position;
		gm.GetPlayer().GetComponent<AudioSource>().PlayOneShot(sl.GetTeleported());
		gm.GetPlayer().transform.forward = playerPortal.transform.forward;

		Vector3 nv = playerPortal.transform.forward * 50;
		pm.SetVelocity(nv);
		pm.SetMDisabled(true);
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

	public IEnumerator BeginSummon()
	{
		yield return new WaitForSeconds(0);
		if (Resurrector.CountFriends("NecroSpawn") >= GetMaxSpawns())
			yield break;
		PlaySounds();
		DoSpawns();
	}

	public void DoSpawns()
	{
		for (int i = 0; i < wavenumber; i++)
		{
			GameObject chosensummon = sc.ChooseSCSummon();
			GameObject instance = Altar.ResurrectUnit(100, chosensummon, spawnAnim, summontracer, portal.transform.position, portal.transform.position, transform.rotation, GameManagerScript.GetGMScript().GetNSpawns().transform);
			instance.tag = "NecroSpawn";
			instance.name = chosensummon.name;
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

	public void ChaseTarget()
	{
		if (target)
		{
			if (Vector3.Distance(transform.position, target.position) <= stoppingDistance)
			{
				//melee would be here
			}
			else if (Vector3.Distance(transform.position, target.position) > stoppingDistance)
			{
				LazyHover();
			}
		}
	}

	public void LazyHover()
	{
		var tempposition = target.position;
		tempposition.y = 0;
		transform.position = Vector3.MoveTowards(transform.position, tempposition, speed * Time.deltaTime);
	}
}
