using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Terror : MonoBehaviour, IChaser, ITargetableBoid, IRandomTargeter
{
    [SerializeField] private Boid boid = default;
    [SerializeField] private Unit unit = default;
    [SerializeField] private float acceleration = 2;
	[SerializeField] private int randomTargetPercentChance = 40;
	private GameManagerScript gm;
	private Transform target;
    private SoundLibrary sl;
	private float targetCooldown = 0;

	public Transform GetTarget() { return target; }

    void Start()
    {
		gm = GameManagerScript.GetGMScript();
		sl = GameManagerScript.GetGMScript().GetSoundLibrary();
        unit.SetDeadClip(sl.GetTerrorDead());
        unit.SetHurtClip(sl.GetTerrorHurt());
    }

    void Update()
    {
        if (!unit.IsDead())
        {
            boid.SetSpeed(boid.GetSpeed() + acceleration * Time.deltaTime);
            ConfirmAndChaseTargetAsBoid(gameObject);
        }
	}

    void FixedUpdate()
    {
        ChaseTarget();
    }

    public void ChaseTarget()
	{
        if (target && !Singularity.isSingularityAwake)
            //transform.position = Vector3.MoveTowards(transform.position, movement, speed * Time.deltaTime);
            boid.DoMovement(target.position);
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

}
