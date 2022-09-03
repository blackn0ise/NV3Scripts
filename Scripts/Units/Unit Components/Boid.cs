// special thanks to Seb Lague

/*
MIT License

Copyright (c) 2019 Sebastian Lague

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using System.Collections.Generic;
using UnityEngine;

public class Boid : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Rigidbody rb = default;
    [SerializeField] private Unit unit = default;

    [Header("Speed Controls")]
    [SerializeField] private float startspeed = 250;
    [SerializeField] private float minSpeed = 50;
    [SerializeField] private float maxSpeed = 500;
    [SerializeField] private float maxSteerForce = 150;

    [Header("Weightings")]
    [SerializeField] private float separationWeight = 4;
    [SerializeField] private float alignmentWeight = 0.4f;
    [SerializeField] private float cohesionWeight = 0.4f;
    [SerializeField] private float collisionWeight = 4;
    [SerializeField] private float targetWeight = 2;
    [SerializeField] private float upwardFloatingWeight = 1;

    [Header("Additional Parameters")]
    [SerializeField] private float separationDistance = 80;
    [SerializeField] private float col_sphereCastRadius = 30;
    [SerializeField] private float col_SphereCastMaxDist = 50;
    [SerializeField] private int numViewDirections = 100;

    private List<Boid> boidlist;
    private BoidController boidController;
    private Transform cachedTransform;
    private Vector3 targetdirection = Vector3.zero;
    private Vector3 acceleration = Vector3.zero;
    private Vector3 tempVelocity;
    private Vector3 position;
    private Vector3 forward;
    private int groundMask = default;
    private float floatRandomSeed = 0;
    private float floatRandomSeed2 = 0;

    public void SetBoidController(BoidController value) { boidController = value; }
    public Unit GetUnit() { return unit; }
    public Rigidbody GetRB() { return rb; }
    public void SetSpeed(float value) { startspeed = value; }
    public float GetSpeed() { return startspeed; }
    public void SetMaxSpeed(float value) { maxSpeed = value; }
    public float GetMaxSpeed() { return maxSpeed; }

    private void Awake()
    {
        cachedTransform = transform;
    }
    void Start()
    {
        position = cachedTransform.position;
        forward = cachedTransform.forward;
        boidlist = new List<Boid>();
        groundMask = 1 << LayerMask.NameToLayer("Ground") | 1 << LayerMask.NameToLayer("HighGround");
        rb.velocity = transform.forward * startspeed;
        tempVelocity = rb.velocity;
        floatRandomSeed = Random.value;
        floatRandomSeed2 = Random.value;
    }

    void Update()
    {
        if (boidController)
        {
            boidlist = boidController.GetBoidList();
        }
        RemoveFromListIfDead();
    }

    private void RemoveFromListIfDead()
    {
        if (unit.IsDead())
            if (boidlist.Contains(this))
                boidlist.Remove(this);
    }

    public void DoMovement(Vector3 target)
    {
        if (!unit.IsDead())
        {
            if (target != null)
                FollowTarget(target);
            if (DetermineNeighbouringBoids(this, boidlist).Count != 0)
                FlockWithNeighbours();
            if (IsHeadingForCollision())
                AvoidCollisions();
            AddUpwardForce();
            FinaliseRigidbodyDirAndVel();
        }

    }

    public void DoUnfacedMovement(Vector3 target)
    {
        if (!unit.IsDead())
        {
            if (target != null)
                FollowTarget(target);
            if (DetermineNeighbouringBoids(this, boidlist).Count != 0)
                FlockWithNeighbours();
            if (IsHeadingForCollision())
                AvoidCollisions();
            AddUpwardForce();
            FinaliseRigidbodyVel(target);
        }

    }

    private void FinaliseRigidbodyDirAndVel()
    {
        tempVelocity += acceleration * Time.deltaTime;
        float speed = tempVelocity.magnitude;
        Vector3 direction = tempVelocity / speed;
        speed = Mathf.Clamp(speed, minSpeed, maxSpeed);
        tempVelocity = direction * speed;

        cachedTransform.position += tempVelocity * Time.deltaTime;
        cachedTransform.forward = direction;
        transform.position = cachedTransform.position;
        position = cachedTransform.position;
        forward = direction;
        if (rb)
            rb.velocity = tempVelocity;
    }

    private void FinaliseRigidbodyVel(Vector3 target)
    {
        tempVelocity += acceleration * Time.deltaTime;
        float speed = tempVelocity.magnitude;
        Vector3 direction = tempVelocity / speed;
        speed = Mathf.Clamp(speed, minSpeed, maxSpeed);
        tempVelocity = direction * speed;

        cachedTransform.position += tempVelocity * Time.deltaTime;
        transform.position = cachedTransform.position;
        position = cachedTransform.position;
        if (rb)
            rb.velocity = tempVelocity;
    }

    private void FollowTarget(Vector3 target)
    {
        Vector3 offsetToTarget = (target - transform.position);
        acceleration = SteerTowards(offsetToTarget) * targetWeight;
    }

    private void FlockWithNeighbours()
    {
        List<Boid> nearestboids = DetermineNeighbouringBoids(this, boidlist);

        var alignmentForce = SteerTowards(GetAlignmentDir(nearestboids)) * alignmentWeight;
        var cohesionForce = SteerTowards(GetCohesionDir(nearestboids)) * cohesionWeight;
        var seperationForce = SteerTowards(GetSeparationDir(nearestboids)) * separationWeight;

        acceleration += alignmentForce;
        acceleration += cohesionForce;
        acceleration += seperationForce;
    }

    private void AvoidCollisions()
    {
        Vector3 collisionAvoidDir = GetCollisionAvoidDir();
        Vector3 collisionAvoidForce = SteerTowards(collisionAvoidDir) * collisionWeight;
        acceleration += collisionAvoidForce;
    }
    private void AddUpwardForce()
    {
        var upwardForce = SteerTowards(GetUpwardBaseValue()) * upwardFloatingWeight;
        acceleration += upwardForce;
    }

    private Vector3 GetUpwardBaseValue()
    {
        float floatSine = Mathf.Sin(Time.time + floatRandomSeed)/2;
        float floatSine2 = Mathf.Sin(Time.time + floatRandomSeed2)/3;
        return Vector3.up * (floatSine + floatSine2);
    }

    Vector3 SteerTowards(Vector3 vector)
    {
        Vector3 v = vector.normalized * maxSpeed - tempVelocity;
        return Vector3.ClampMagnitude(v, maxSteerForce);
    }

    public void SetStartingDirection(Transform target)
    {
        if (target)
        {
            targetdirection = (target.position - transform.position);
            targetdirection = targetdirection.normalized;
        }
    }

    bool IsHeadingForCollision()
    {
        RaycastHit hit;
        if (Physics.SphereCast(transform.position, col_sphereCastRadius, transform.forward, out hit, col_SphereCastMaxDist, groundMask))
        {
            return true;
        }
        return false;
    }

    private Vector3 GetCollisionAvoidDir()
    {
        Vector3[] rayDirections = CalculateViewDirections(numViewDirections);

        for (int i = 0; i < rayDirections.Length; i++)
        {
            Vector3 dir = cachedTransform.TransformDirection(rayDirections[i]);
            Ray ray = new Ray(position, dir);
            if (!Physics.SphereCast(ray, col_sphereCastRadius, col_SphereCastMaxDist, groundMask))
                return dir;
        }
        return forward;
    }

    private Vector3[] CalculateViewDirections(int numViewDirections)

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

    private Vector3 GetSeparationDir(List<Boid> nearestboids)
    {
        Vector3 separationdir = Vector3.zero;
        foreach (Boid boid in nearestboids)
        {
            if (boid)
            {
                Vector3 dir = boid.transform.position - transform.position;
                separationdir += dir;
            }
        }
        separationdir /= nearestboids.Count;
        separationdir *= -1;
        separationdir = separationdir.normalized;
        return separationdir;
    }

    private Vector3 GetAlignmentDir(List<Boid> nearestboids)
    {
        Vector3 alignmentDir = Vector3.zero;
        foreach (Boid boid in nearestboids)
        {
            if (boid)
            {
                Vector3 dir = boid.GetRB().velocity;
                alignmentDir += dir;
            }
        }
        alignmentDir /= nearestboids.Count;
        alignmentDir = alignmentDir.normalized;
        return alignmentDir;
    }

    private Vector3 GetCohesionDir(List<Boid> nearestboids)
    {
        Vector3 cohesiondir = Vector3.zero;
        foreach (Boid boid in nearestboids)
        {
            if (boid)
            {
                Vector3 pos = boid.transform.position;
                cohesiondir += pos;
            }
        }
        cohesiondir /= nearestboids.Count;
        cohesiondir *= -1;
        cohesiondir = cohesiondir.normalized;
        return cohesiondir;
    }

    public List<Boid> DetermineNeighbouringBoids(Boid thisboid, List<Boid> allboids)
    {
        List<Boid> allboidsexceptthis = new List<Boid>();
        allboidsexceptthis.AddRange(allboids);
        allboidsexceptthis.Remove(thisboid);

        List<Boid> neighbours = new List<Boid>();
        foreach (Boid friendboid in allboidsexceptthis)
        {
            if (friendboid != null)
            {
                bool isfboiddead = false;
                if (friendboid && (friendboid.GetUnit() != null))
                    isfboiddead = friendboid.GetUnit().IsDead();
                Vector3 currentPos = thisboid.transform.position;
                float dist = Vector3.Distance(friendboid.transform.position, currentPos);

                if ((dist < separationDistance) && friendboid && (friendboid.GetUnit() != null) && !isfboiddead)
                {
                    neighbours.Add(friendboid);
                }
            }
        }
        return neighbours;
    }
}
