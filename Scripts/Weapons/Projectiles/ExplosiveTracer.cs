using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosiveTracer : MonoBehaviour
{
    [SerializeField] private GameObject floorShadow = default;
    public GameObject parent { get; set; }
    private GameObject floorTracerInstance;
    private float defaultDecay = 10;

    void Start()
    {
        if (floorShadow)
            floorTracerInstance = Instantiate(floorShadow, transform.position, transform.rotation, transform);
        Destroy(gameObject, defaultDecay);
    }

    void Update()
    {
        FollowParent();
        CheckFloor();
    }

    private void FollowParent()
    {
        if (parent)
            transform.position = parent.transform.position;
    }

    public void CheckFloor()
    {
        int layerMask = 1 << LayerMask.NameToLayer("Bullets") | 1 << LayerMask.NameToLayer("TriggerGround");
        layerMask = ~layerMask;
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.up * -1, out hit, 50000f, layerMask))
        {
            MoveShadow(hit);
        }
        else
        {
            ShadowDefault();
        }
    }

    private void MoveShadow(RaycastHit hit)
    {
        if (hit.collider)
        {
            if (floorTracerInstance)
                floorTracerInstance.transform.position = hit.point;
        }
    }

    private void ShadowDefault()
    {
        Vector3 loweredpos = transform.position;
        loweredpos.y = -10000;
        if (floorTracerInstance)
            floorTracerInstance.transform.position = loweredpos;
    }
}
