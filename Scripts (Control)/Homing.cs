using UnityEngine;

// this is a storage class only; no gameobject explicitly uses this class, only derived class and static functions

public class Homing : MonoBehaviour
{
	public bool hasUnitComponent { get; set; }
	public bool hasBulletComponent { get; set; }
	private Transform target;
	[Tooltip("Higher turnspeed means a stronger turn. This is used as a multiplier for a RotateTowards maxRadiansDelta.")] 
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

	public static Vector3 GetDirection(Transform target, Transform thisUnit)
	{
		Vector3 targetDirection = target.position - thisUnit.position;
		return targetDirection;
	}

	public static Vector3 GetDirectionShaky(Transform target, Transform thisUnit, float targetVariance = 1)
	{
		Vector3 targetDirection = target.position - thisUnit.position;
		targetDirection += new Vector3(Random.Range(-targetVariance, targetVariance), Random.Range(-targetVariance, targetVariance), Random.Range(-targetVariance, targetVariance));
		return targetDirection;
	}

	public static void TurnSelf(float turnspeed, Transform target, Transform thisUnit, bool targetHasCollider)
	{
		Vector3 targetDirection = GetDirection(target, thisUnit);
		DoTurn(turnspeed, thisUnit, targetDirection);
	}

	public static void TurnSelfShaky(float turnspeed, Transform target, Transform thisUnit, float targetVariance = 1)
	{
		Vector3 targetDirection = GetDirectionShaky(target, thisUnit, targetVariance);
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

    public static void SlowYTurn(float turnspeed, Transform target, Transform thisUnit, float slowfactor)
    {
        Vector3 targetDirection = GetDirection(target, thisUnit);
        targetDirection.y /= slowfactor;
        if (ConfirmBadRange(target, thisUnit))
        {
            targetDirection.y = 0;
            targetDirection.x = 1;
            targetDirection.z = 1;
        }
        else
        {
            //Debug.Log("Range not bad");
        }
        targetDirection = targetDirection.normalized;
        DoTurn(turnspeed, thisUnit, targetDirection);
    }

    public static void SlowYTurn(float turnspeed, Vector3 direction, Transform thisUnit, float slowfactor)
    {
        Vector3 targetDirection = direction;
        targetDirection.y /= slowfactor;
        if (ConfirmBadRange(direction, thisUnit))
        {
            targetDirection.y = 0;
            targetDirection.x = 1;
            targetDirection.z = 1;
        }
        else
        {
            //Debug.Log("Range not bad");
        }
        DoTurn(turnspeed, thisUnit, targetDirection);
    }

    public static bool ConfirmBadRange(Transform target, Transform thisUnit, float margin = 0.2f)
    {
        float xdirabs = Mathf.Abs(GetDirection(target, thisUnit).normalized.x);
        float zdirabs = Mathf.Abs(GetDirection(target, thisUnit).normalized.z);
        bool badrange = zdirabs < margin && xdirabs < margin;
        /*if (thisUnit.name == "Tower2debug")
            if (badrange)
            {
                Debug.Log("bad range detected by " + thisUnit.name + ". direction = " + GetDirection(target, thisUnit));
                Debug.Log("direction.normalized = " + GetDirection(target, thisUnit).normalized);
                Debug.Log("xdirabs = " + xdirabs);
                Debug.Log("zdirabs = " + zdirabs);
            }*/
        return badrange;
    }

    public static bool ConfirmBadRange(Vector3 direction, Transform thisUnit, float margin = 0.2f)
    {
        float xdirabs = Mathf.Abs(direction.normalized.x);
        float zdirabs = Mathf.Abs(direction.normalized.z);
        bool badrange = zdirabs < margin && xdirabs < margin;
        /*if (badrange)
		{
			Debug.Log("bad range detected by " + thisUnit.name + ". direction = " + direction);
			Debug.Log("direction.normalized = " + direction.normalized);
			Debug.Log("xdirabs = " + xdirabs);
			Debug.Log("zdirabs = " + zdirabs);
		}*/
        return badrange;
	}

    internal static void DoTurn(object p, Transform transform, Vector3 vector3)
    {
        throw new System.NotImplementedException();
    }

    public static void NoYTurn(float turnspeed, Transform target, Transform thisUnit)
	{
		Vector3 targetDirection = GetDirection(target, thisUnit);
		targetDirection.y = 0;
		DoTurn(turnspeed, thisUnit, targetDirection);
	}
	public static void NoYTurn(float turnspeed, Collider targetCollider, Transform thisUnit)
	{
		Vector3 targetDirection = targetCollider.bounds.center - thisUnit.position;
		targetDirection.y = 0;
		DoTurn(turnspeed, thisUnit, targetDirection);
	}

	public static void NoYTurn(float turnspeed, Vector3 direction, Transform thisUnit)
	{
		Vector3 targetDirection = direction;
		targetDirection.y = 0;
		DoTurn(turnspeed, thisUnit, targetDirection);
	}

	public static void DoTurn(float turnspeed, Transform thisUnit, Vector3 direction)
	{
		float singleStep = turnspeed * Time.deltaTime;
		Vector3 newDirection = Vector3.RotateTowards(thisUnit.forward, direction, singleStep, 0.0f);
		Debug.DrawRay(thisUnit.position, newDirection, Color.red, 0.2f);
		Vector3 lea = thisUnit.localEulerAngles;
		//if (thisUnit.name == "Tower2debug")
		//	Debug.Log("lea = " + lea);
		lea.x = Mathf.Clamp(lea.x, -30, 60);
        thisUnit.localEulerAngles = lea;
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