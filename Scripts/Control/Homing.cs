using System;
using UnityEngine;

// this is a storage class only; no gameobject explicitly uses this class, only derived class and static functions

public class Homing : MonoBehaviour
{
	public bool hasUnitComponent { get; set; }
	public bool hasBulletComponent { get; set; }
	private Transform target;
	[SerializeField] private float turnspeed = 1.3f;
	[SerializeField] private float stoppingdistance = 0;
	[SerializeField] private float speed = 100f;
	[SerializeField] private Transform defaulttarget = default;
	public void SetTarget(Transform value) { target = value; }
	public Transform GetTarget() { return target; }
	public float GetSpeed() { return speed; }
	public void SetSpeed(float value) { speed = value; }
	public float GetStoppingDistance() { return stoppingdistance; }
	public void SetStoppingDistance(float value) { stoppingdistance = value; }
	public float GetTurnSpeed() { return turnspeed; }
	public void SetTurnSpeed(float value) { turnspeed = value; }
	public void SetDefaultTarget(Transform value) { defaulttarget = value; }
	public Transform GetDefaultTarget() { return defaulttarget; }

	public static Vector3 GetDirection(Transform target, Transform thisUnit, bool targetHasCollider)
	{
		//Vector3 targetDirection = (targetHasCollider ? target.GetComponentInChildren<Collider>().bounds.center : target.position) - thisUnit.position;
		Vector3 targetDirection = target.position - thisUnit.position;
		return targetDirection;
	}

	public static void TurnSelf(float turnspeed, Transform target, Transform thisUnit, bool targetHasCollider)
	{
		Vector3 targetDirection = GetDirection(target, thisUnit, targetHasCollider);
		DoTurn(turnspeed, thisUnit, targetDirection);
	}

	public static void TurnSelf(float turnspeed, Collider targetCollider, Transform thisUnit)
	{
		Vector3 targetDirection = targetCollider.bounds.center - thisUnit.position;
		DoTurn(turnspeed, thisUnit, targetDirection);
	}

	public static void TurnSelf(float turnspeed, Transform target, Transform thisUnit)
	{
		Vector3 targetDirection = target.position - thisUnit.position;
		DoTurn(turnspeed, thisUnit, targetDirection);
	}

	public static void TurnSelf(float turnspeed, Vector3 direction, Transform thisUnit)
	{
		DoTurn(turnspeed, thisUnit, direction);
	}

	public static void SlowYTurn(float turnspeed, Transform target, Transform thisUnit, float slowfactor, bool targetHasCollider)
	{
		Vector3 targetDirection = GetDirection(target, thisUnit, targetHasCollider);
		targetDirection.y /= slowfactor;
		DoTurn(turnspeed, thisUnit, targetDirection);
	}

	public static void SlowYTurn(float turnspeed, Collider targetCollider, Transform thisUnit, float slowfactor)
	{
		Vector3 targetDirection = targetCollider.bounds.center - thisUnit.position;
		targetDirection.y /= slowfactor;
		DoTurn(turnspeed, thisUnit, targetDirection);
	}

	public static void SlowYTurn(float turnspeed, Transform target, Transform thisUnit, float slowfactor)
	{
		Vector3 targetDirection = target.position - thisUnit.position;
		targetDirection.y /= slowfactor;
		DoTurn(turnspeed, thisUnit, targetDirection);
	}

	public static void SlowYTurn(float turnspeed, Vector3 direction, Transform thisUnit, float slowfactor)
	{
		Vector3 targetDirection = direction;
		targetDirection.y /= slowfactor;
		DoTurn(turnspeed, thisUnit, targetDirection);
	}

	public static void NoYTurn(float turnspeed, Transform target, Transform thisUnit, bool targetHasCollider)
	{
		Vector3 targetDirection = GetDirection(target, thisUnit, targetHasCollider);
		targetDirection.y = 0;
		DoTurn(turnspeed, thisUnit, targetDirection);
	}
	public static void NoYTurn(float turnspeed, Collider targetCollider, Transform thisUnit)
	{
		Vector3 targetDirection = targetCollider.bounds.center - thisUnit.position;
		targetDirection.y = 0;
		DoTurn(turnspeed, thisUnit, targetDirection);
	}
	public static void NoYTurn(float turnspeed, Transform target, Transform thisUnit)
	{
		Vector3 targetDirection = target.position - thisUnit.position;
		targetDirection.y = 0;
		DoTurn(turnspeed, thisUnit, targetDirection);
	}
	public static void NoYTurn(float turnspeed, Vector3 direction, Transform thisUnit)
	{
		Vector3 targetDirection = direction;
		targetDirection.y = 0;
		DoTurn(turnspeed, thisUnit, targetDirection);
	}

	public static bool ConfirmBadRange(Transform target, Transform thisUnit, bool targetHasCollider, float margin = 25)
	{
		float xdirabs = Mathf.Abs(GetDirection(target, thisUnit, targetHasCollider).x);
		float zdirabs = Mathf.Abs(GetDirection(target, thisUnit, targetHasCollider).z);
		bool badrange = zdirabs < margin && xdirabs < margin;
		if (badrange)
			Debug.Log("bad range detected by " + thisUnit + ". direction = " + GetDirection(target, thisUnit, targetHasCollider));
		return badrange;
	}

	public static void DoTurn(float turnspeed, Transform thisUnit, Vector3 direction)
	{
		float singleStep = turnspeed * Time.deltaTime;
		Vector3 newDirection = Vector3.RotateTowards(thisUnit.forward, direction, singleStep, 0.0f);
		newDirection.x = Mathf.Clamp(newDirection.x, -80f, 80f);
		Debug.DrawRay(thisUnit.position, newDirection, Color.red);
		thisUnit.rotation = Quaternion.LookRotation(newDirection);
	}

	public static void ChaseForward(float speed, Transform thisUnit, Transform target, float stoppingdistance)
	{
		if (target)
			if (Vector3.Distance(thisUnit.position, target.position) > stoppingdistance)
				thisUnit.position += thisUnit.forward * Time.deltaTime * speed;
	}

	public static void MoveForward(float speed, Transform thisUnit)
	{
		thisUnit.position += thisUnit.forward * Time.deltaTime * speed;
	}

	public static void ResetXRotation(float turnspeed, Transform thisUnit)
    {
		float singleStep = turnspeed * Time.deltaTime;
		Vector3 NoYForward = thisUnit.forward;
		NoYForward.y = 0;
		Vector3 newDirection = Vector3.RotateTowards(thisUnit.forward, NoYForward, singleStep, 0.0f);
		thisUnit.rotation = Quaternion.LookRotation(newDirection);
	}
}