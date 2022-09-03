using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//for units only, not for bullets / projectiles

public class YLockedHomer : MonoBehaviour
{
	private Transform target;
	private bool targetHasCollider = false;
	[SerializeField] private float turnspeed = 0.3f;
	[SerializeField] private Transform defaulttarget = default;

	public float GetTurnSpeed() { return turnspeed; }
	public void SetTurnSpeed(float value) { turnspeed = value; }
	public void SetTarget(Transform value) { target = value; if (target) targetHasCollider = target.GetComponentInChildren<Collider>(); }
	public Transform GetTarget() { return target; }
	public bool GetTargetHasCollider() { return targetHasCollider; }
	public void SetTargetHasCollider(bool value) { targetHasCollider = value; }

	private void Start()
	{
		if (!defaulttarget)
			target = GameObject.Find("Player").transform;
		else
			target = defaulttarget;

	}

	public virtual void Update()
	{
		//chase and turn if target
		if (target && target.GetComponent<IUnit>() != null)
		{
			if (!target.GetComponent<IUnit>().IsDead())
			{
				Homing.NoYTurn(GetTurnSpeed(), target, transform, targetHasCollider);
			}
		}
		//updates target if no target
		else
			ConfirmTarget(gameObject);
	}

	public void ConfirmTarget(GameObject thisUnit)
	{
		if (!target)
		{
			GetNewTarget(thisUnit);
		}
		else if (target.GetComponent<IUnit>() != null && target.GetComponent<IUnit>().IsDead())
		{
			GetNewTarget(thisUnit);
		}
		else if (defaulttarget)
			target = defaulttarget;
		else
			target = GameObject.Find("Player").transform;
	}

	private void GetNewTarget(GameObject thisUnit)
	{
		targetHasCollider = false;
		target = Unit.GetClosestEnemy(thisUnit);
		if (target)
			targetHasCollider = target.GetComponentInChildren<Collider>();
	}
}
