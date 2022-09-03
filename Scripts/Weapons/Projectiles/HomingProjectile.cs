using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HomingProjectile : Homing
{
	private bool targetHasCollider;
	private bool GetTargetHasCollider() { return targetHasCollider; }
	private void SetTargetHasCollider(bool value) { targetHasCollider = value; }

	private void Start()
	{
		hasBulletComponent = GetComponent<Bullet>();
		hasUnitComponent = GetComponent<IUnit>() != null;

		if (!GetDefaultTarget())
			SetTarget(GameObject.Find("Player").transform);
		else
			SetTarget(GetDefaultTarget());

		SyncIfBullet();
	}

	public void Update()
	{
		if (GetTarget())
			TurnSelf(GetTurnSpeed(), GetTarget(), transform, targetHasCollider);
		else
			SyncIfBullet();
		MoveForward(GetSpeed(), transform);
	}

	private void SyncIfBullet()
	{
		targetHasCollider = false;
		if (hasBulletComponent)
			if (GetComponent<Bullet>().GetParent())
				SyncTargets();
	}

	private void SyncTargets()
	{
		if (GetComponent<Bullet>().GetParent().GetComponent<Revenant>())
			SetTarget(GetComponent<Bullet>().GetParent().GetComponent<Revenant>().GetTarget());
		if (GetComponent<Bullet>().GetParent().GetComponent<Juggernaut>() && GetComponent<Bullet>().GetParent().CompareTag("Friendlies"))
			SetTarget(GetComponent<Bullet>().GetParent().GetComponent<Juggernaut>().GetTarget());
		if (GetTarget())
			targetHasCollider = GetTarget().GetComponentInChildren<Collider>();
	}
}
