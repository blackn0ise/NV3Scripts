using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Nightmare : MonoBehaviour
{
	[SerializeField] private float stoppingdistance = 0;
	//[SerializeField] private int totaldamage;
	[SerializeField] private float speed = 100f;
	[SerializeField] private float turnspeed = 1.3f;
	[SerializeField] private float updateoffset = 2.0f;
	[SerializeField] private float updatefrequency = 6.0f;
	[SerializeField] private AudioSource aso = default;
	private GameManagerScript gm;
	private Transform target;
	private float updateTimestamp;
	private GameObject altar;
	private GameObject player;
	private bool targetHasCollider = false;
	private Collider targetCollider = null;

	public float GetSpeed() { return speed; }
	public void SetSpeed(float value) { speed = value; }

	private void Start()
	{
		InitialiseAndLaugh();
	}

	public void Update()
	{
		ChaseTarget();
		PeriodicUpdate();
	}

	private void InitialiseAndLaugh()
	{
		gm = GameManagerScript.GetGMScript();
		altar = gm.GetAltar();
		player = gm.GetPlayer();
		target = altar.transform;
		aso.PlayOneShot(gm.GetSoundLibrary().ChooseLichSpawn());
	}

	private void PeriodicUpdate()
	{
		if (!target)
		{
			GetNewTarget();
		}
		if (target == altar.transform)
		{
			if (Time.time > updateTimestamp)
			{
				GetNewTarget();
				updateTimestamp += updatefrequency + Random.Range(-updateoffset, updateoffset);
			}
		}
		if (SoulCollector.soulCollectorActive)
			ExcludeSCFromTargeting();
		if (Vizier.vizierActive)
			ExcludeViziersFromTargeting();
	}

	private void ExcludeViziersFromTargeting()
	{
		List<GameObject> viziers = new List<GameObject>();
		foreach (Vizier vizier in FindObjectsOfType<Vizier>())
		{
			viziers.Add(vizier.gameObject);
		}
		List<Transform> vizierChildren = new List<Transform>();
		foreach (GameObject vizier in viziers)
		{
			foreach (Transform child in vizier.transform)
			{
				vizierChildren.Add(child);
			} 
		}
		GetNewTargetExcluding(vizierChildren);
		updateTimestamp += updatefrequency + Random.Range(-updateoffset, updateoffset);
	}

	private void ExcludeSCFromTargeting()
	{
		GameObject soulcollector = FindObjectOfType<SoulCollector>().gameObject;
		List<Transform> scchildren = new List<Transform>();
		foreach (Transform child in soulcollector.transform)
		{
			scchildren.Add(child);
		}
		GetNewTargetExcluding(scchildren);
		updateTimestamp += updatefrequency + Random.Range(-updateoffset, updateoffset);
	}

	private void GetNewTargetExcluding(List<Transform> exceptions)
	{
		targetHasCollider = false;
		targetCollider = null;
		GameObject newTarget = Unit.ChooseRandomEnemyExcluding(player, exceptions);
		if (newTarget != null)
			target = newTarget.transform;
		else
			target = altar.transform;
		targetCollider = target.GetComponentInChildren<Collider>();
		targetHasCollider = targetCollider;
	}

	private void GetNewTarget()
	{
		targetHasCollider = false;
		targetCollider = null;
		Transform newTarget = Unit.GetRandomEnemy(player);
		if (newTarget != null)
			target = newTarget;
		else
			target = altar.transform;
		targetCollider = target.GetComponentInChildren<Collider>();
		targetHasCollider = targetCollider;
	}

	private void ChaseTarget()
	{
		if (target)
		{
			if (targetHasCollider)
			{
				if (targetCollider)
					Homing.TurnSelf(turnspeed, targetCollider, transform);
			}
			else
			{
				Homing.TurnSelf(turnspeed, target, transform);
			}
			Homing.ChaseForward(speed, transform, target, stoppingdistance);
		}
	}
}
