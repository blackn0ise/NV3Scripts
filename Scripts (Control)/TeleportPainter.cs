using System.Collections;
using UnityEngine;

public class TeleportPainter : MonoBehaviour
{
	[SerializeField] private LineRenderer lr = default;
    [SerializeField] private GameObject hologram = default;
    private Vector3 hitpoint;
    public Vector3 GetHitpoint() { return hitpoint; }
    public Vector3 GetHologramPosition() { return hologram.transform.position; }


    private void Update()
	{
        lr.SetPosition(0, transform.position);


        int layerMask = 1 << LayerMask.NameToLayer("Bullets") | 1 << LayerMask.NameToLayer("TriggerGround");
        layerMask = ~layerMask;

        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, 50000f, layerMask))
        {
            if (hit.collider)
            {
                hitpoint = hit.point;
                lr.SetPosition(1, hit.point);
            }
        }
        else
        {
            lr.SetPosition(1, transform.forward * 100f);
            hitpoint = transform.forward * 100f;
        }
        hologram.transform.position = hitpoint;
    }

}