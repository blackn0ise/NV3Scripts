using System.Collections;
using UnityEngine;

public class Laser : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private GameObject hitblast = default;
    [SerializeField] private GameObject muzzleflash = default;
    [SerializeField] private AudioClip shot = default;

    [Header("Parameters")]
    [SerializeField] private float laserdecay = 4.0f;

    private LineRenderer lr;
    private GameObject parent;
    private GameObject mouth;
    private Transform pjtsf;
    bool isparentplayermouth;
    public GameObject GetParent() { return parent; }
    public void SetParent(GameObject value) { parent = value; }
    public float GetLaserDecay() { return laserdecay; }

    //for Void Cannon
    Vector3 hitpoint;
    public Vector3 GetHitPoint() { return hitpoint; }
    public void SetHitPoint(Vector3 value) { hitpoint = value; }
    public float GetShotLength() { return shot.length; }

    void Start()
    {
        pjtsf = GameManagerScript.GetGMScript().GetPJTSF();
        mouth = GameObject.Find("Mouth");
        ConfirmIsPlayerMouth();
        if (isparentplayermouth)
        {
            var vc = parent.GetComponentInParent<VoidCannon>();
            var jg = parent.GetComponentInParent<Judgement>();
            if (vc)
            {
                vc.GetASO().PlayOneShot(shot);
                vc.GetASO().PlayOneShot(shot, 0.3f);
            }
            if (jg)
            {
                jg.GetASO().PlayOneShot(shot);
                jg.GetASO().PlayOneShot(shot, 0.3f);
            }
        }
        lr = GetComponent<LineRenderer>();
        StartCoroutine(DestroySelf());
    }

    private void ConfirmIsPlayerMouth()
    {
        if (parent == mouth)
            isparentplayermouth = true;
        else
            isparentplayermouth = false;
    }

    void LateUpdate()
    {
        if (mouth && parent)
        {
            GameObject mf = Instantiate(muzzleflash, mouth.transform.position, Quaternion.identity, pjtsf);
            Destroy(mf, 0.2f);

            lr.SetPosition(0, parent.transform.position);

            // Bit shift the index of the bullets + triggerground layer to get a bit mask
            int layerMask = 1 << LayerMask.NameToLayer("Bullets") | 1 << LayerMask.NameToLayer("TriggerGround");

            // This would cast rays only against colliders in the bullets + triggerground layer.
            // But instead we want to collide against everything except this layer. The ~ operator does this, it inverts a bitmask.
            layerMask = ~layerMask;

            RaycastHit hit;
            if (Physics.Raycast(parent.transform.position, parent.transform.forward, out hit, 50000f, layerMask))
            {
                if (hit.collider)
                {
                    hitpoint = hit.point;
                    lr.SetPosition(1, hit.point);
                    if(hitblast != null)
                    {
                        GameObject hb = Instantiate(hitblast, hit.point, Quaternion.identity, parent.transform);
                        hb.name = hb.name;
                        if (hb.GetComponent<Bullet>())
                            hb.GetComponent<Bullet>().SetParent(mouth);
                    }
                }
            }
            else
            {
                lr.SetPosition(1, parent.transform.forward * 50000f);
                hitpoint = parent.transform.position;
            }
        }
    }

    public static bool CheckLineOfSight(GameObject thisunit)
    {
        int layerMask = 1 << LayerMask.NameToLayer("Bullets") | 1 << LayerMask.NameToLayer("TriggerGround");
        // collide with everything except above layers
        layerMask = ~layerMask;
        RaycastHit hit;


        if (Physics.Raycast(thisunit.transform.position, thisunit.transform.forward, out hit, 50000f, layerMask))
            foreach (string str in Unit.ConfirmEnemyTypes(thisunit))
            {
                if (hit.transform.tag == str)
                    return true; 
            }
        return false;
    }

    public virtual IEnumerator DestroySelf()
    {
        yield return new WaitForSeconds(laserdecay);
        if(GameManagerScript.HasInterfacesOfType<IChargingWeapon>() && isparentplayermouth)
        {
            foreach (var cw in GameManagerScript.FindInterfacesOfType<IChargingWeapon>())
                cw.ResetState();
        }
        Destroy(gameObject);

    }
}