using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ara;

public class Aiming : MonoBehaviour
{
    public static int NoGroundLayerMask()
    {
        // Bit shift the index of the Ground + TriggerGround + HighGround layer to get a bit mask
        int layerMask = 1 << LayerMask.NameToLayer("Ground") | 1 << LayerMask.NameToLayer("TriggerGround") |
    1 << LayerMask.NameToLayer("HighGround");
        // This would cast rays only against colliders in the ground layers.
        // But instead we want to collide against everything except these layers. The ~ operator does this, it inverts a bitmask.
        layerMask = ~layerMask;
        return layerMask;
    }
    public static int GroundLayerMask()
    {
        // Bit shift the index of the Ground + TriggerGround + HighGround layer to get a bit mask
        int layerMask = 1 << LayerMask.NameToLayer("Ground") | 1 << LayerMask.NameToLayer("TriggerGround") |
    1 << LayerMask.NameToLayer("HighGround");
        // Cast rays only against colliders in the ground layers.
        return layerMask;
    }

    public static Transform CheckDirectLineOfSight(Transform thisTransform)
    {
        if (Physics.Raycast(thisTransform.position, thisTransform.forward, out RaycastHit hit, 50000f, NoGroundLayerMask()))
            if (hit.collider.CompareTag("Enemies") || hit.transform.CompareTag("Enemies"))
                return hit.transform;
        return null;
    }

    public static RaycastHit[] RayCastEnemiesAll(Vector3 origin, Vector3 direction, float maxDistance)
    {
        RaycastHit[] rayCastHits = Physics.RaycastAll(origin, direction, maxDistance, NoGroundLayerMask());
        return rayCastHits;
    }

    public static RaycastHit RayCastEnemies(Vector3 origin, Vector3 direction, float maxDistance)
    {
        Physics.Raycast(origin, direction, out RaycastHit hit, maxDistance, NoGroundLayerMask());
        return hit;
    }

    public static RaycastHit[] RayCastEnemiesRandomSpread(Vector3 origin, Vector3 direction, float maxDistance, int rayCount, float offsetRange)
    {
        List<RaycastHit> rayCastHits = new List<RaycastHit>();
        for (int i = 0; i < rayCount; i++)
        {
            var newDirection = direction;

            newDirection.x += Random.Range(-offsetRange, offsetRange);
            newDirection.y += Random.Range(-offsetRange, offsetRange);
            newDirection.z += Random.Range(-offsetRange, offsetRange);
            Debug.Log("origin = " + origin);
            Debug.Log("newDirection = " + newDirection);
            Debug.DrawLine(origin, newDirection * maxDistance, Color.blue, 2.5f);

            Physics.Raycast(origin, newDirection, out RaycastHit hit, maxDistance, NoGroundLayerMask());

            if (hit.transform == null || hit.collider == null)
                continue;

            if (hit.collider.CompareTag("Enemies") || hit.transform.CompareTag("Enemies"))
                rayCastHits.Add(hit);
        }
        return rayCastHits.ToArray();
    }

    public static RaycastHit[] RayCastEnemiesEvenSpread(Vector3 origin, Vector3 direction, float maxDistance, int rayCount)
    {/* 
      * incomplete
      * 
        List<RaycastHit> rayCastHits = new List<RaycastHit>();
        Vector3[] rayDirections = CalculateEvenSpreadDirections(rayCount);
        for (int i = 0; i < rayCount; i++)
        {
            Debug.DrawLine(origin, rayDirections[i] * maxDistance, Color.blue, 2.5f);

            Physics.Raycast(origin, rayDirections[i], out RaycastHit hit, maxDistance, NoGroundLayerMask());

            if (hit.transform == null || hit.collider == null)
                continue;

            if (hit.collider.CompareTag("Enemies") || hit.transform.CompareTag("Enemies"))
                rayCastHits.Add(hit);
        }
    */
        return null;
    }

    public static RaycastHit[] ConeCastAll(Vector3 origin, Vector3 direction, float maxDistance, float coneAngle, float maxRadius, int layerMask = ~0)
    {
        RaycastHit[] sphereCastHits = Physics.SphereCastAll(origin - new Vector3(0, 0, maxRadius), maxRadius, direction, maxDistance, layerMask);
        List<RaycastHit> coneCastHitList = new List<RaycastHit>();

        if (sphereCastHits.Length > 0)
        {
            for (int i = 0; i < sphereCastHits.Length; i++)
            {
                //sphereCastHits[i].collider.gameObject.GetComponent<Renderer>().material.color = new Color(1f, 1f, 1f);
                Vector3 hitPoint = sphereCastHits[i].point;
                Vector3 directionToHit = hitPoint - origin;
                float angleToHit = Vector3.Angle(direction, directionToHit);

                if (angleToHit > coneAngle)
                    continue;

                coneCastHitList.Add(sphereCastHits[i]);
                //Debug.Log("collider = " + sphereCastHits[i].collider.transform.name);
                //Debug.DrawLine(origin, sphereCastHits[i].transform.position, Color.green);
            }
        }

        RaycastHit[] coneCastHits = new RaycastHit[coneCastHitList.Count];
        coneCastHits = coneCastHitList.ToArray();

        return coneCastHits;
    }

    public static RaycastHit[] RayCastAll(Vector3 origin, Vector3 direction, float maxDistance, int layerMask = ~0)
    {
        RaycastHit[] rayCastHits = Physics.RaycastAll(origin, direction, maxDistance, layerMask);
        return rayCastHits;
    }

    public static RaycastHit DetermineClosestHit(GameObject source, RaycastHit[] hitList)
    {
        RaycastHit closest = hitList[hitList.Length-1];
        float minDist = Mathf.Infinity;
        foreach (RaycastHit target in hitList)
        {
            Vector3 currentPos = source.transform.position;
            float dist = Vector3.Distance(target.transform.position, currentPos);

            if ((dist < minDist))
            {
                closest = target;
                minDist = dist;
            }
        }
        return closest;
    }

    public static RaycastHit[] ConeCastEnemiesAll(Vector3 origin, Vector3 direction, float maxDistance, float coneAngle, float maxRadius)
    {
        RaycastHit[] sphereCastHits = Physics.SphereCastAll(origin - new Vector3(0, 0, maxRadius), maxRadius, direction, maxDistance, NoGroundLayerMask());
        List<RaycastHit> coneCastHitList = new List<RaycastHit>();

        if (sphereCastHits.Length > 0)
        {
            for (int i = 0; i < sphereCastHits.Length; i++)
            {
                Vector3 hitPoint = sphereCastHits[i].point;
                Vector3 directionToHit = hitPoint - origin;
                float angleToHit = Vector3.Angle(direction, directionToHit);

                if (angleToHit > coneAngle)
                    continue;
                coneCastHitList.Add(sphereCastHits[i]);
                Debug.DrawLine(origin, sphereCastHits[i].transform.position, Color.red);

            }
        }

        RaycastHit[] coneCastHits = new RaycastHit[coneCastHitList.Count];
        coneCastHits = coneCastHitList.ToArray();

        return coneCastHits;
    }

    public static bool CheckInRange(GameObject gameObject, Vector3 origin, Vector3 destination, float hitRangeMax)
    {
        float distanceToTarget = Vector3.Distance(origin, destination);
        return distanceToTarget < hitRangeMax;
    }

    public static Vector3[] CalculateViewDirections(int numViewDirections)
    {
        Vector3[] directions = new Vector3[numViewDirections];

        float goldenRatio = (1 + Mathf.Sqrt(5)) / 2;
        float angleIncrement = Mathf.PI * 2 * goldenRatio;

        for (int i = 0; i < numViewDirections; i++)
        {
            float t = (float)i / numViewDirections;
            float inclination = Mathf.Acos(1 - 2 * t);
            float azimuth = angleIncrement * i;

            float x = Mathf.Sin(inclination) * Mathf.Cos(azimuth);
            float y = Mathf.Sin(inclination) * Mathf.Sin(azimuth);
            float z = Mathf.Cos(inclination);

            directions[i] = new Vector3(x, y, z);
        }
        return directions;
    }

    public static Vector3[] CalculateEvenSpreadDirections(int numViewDirections)
    {
        Vector3[] directions = new Vector3[numViewDirections];

        float goldenRatio = (1 + Mathf.Sqrt(5)) / 2;
        float angleIncrement = Mathf.PI * 2 * goldenRatio;

        for (int i = 0; i < numViewDirections; i++)
        {
            float t = (float)i / numViewDirections;
            float inclination = Mathf.Acos(1 - 2 * t);
            float azimuth = angleIncrement * i;

            float x = Mathf.Sin(inclination) * Mathf.Cos(azimuth);
            float y = Mathf.Sin(inclination) * Mathf.Sin(azimuth);
            float z = Mathf.Cos(inclination);

            directions[i] = new Vector3(x, y, z);
        }
        return directions;
    }

    public static Vector3[] PlotCirclePoints(Vector3 origin, int numPoints, float radius, float startingOffset = 0)
    {
        List<Vector3> list = new List<Vector3>();
        float angleSplit = 360.0f / numPoints;
        for (int i = 0; i < numPoints; i++)
        {
            var x = radius * Mathf.Sin(Mathf.PI * 2 * (angleSplit * i + startingOffset) / 360);
            var y = radius * Mathf.Cos(Mathf.PI * 2 * (angleSplit * i + startingOffset) / 360);

            list.Add(new Vector3(origin.x + x, origin.y + y, origin.z));
        }
        return list.ToArray();
    }
}
