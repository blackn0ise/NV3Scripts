using System.Collections;
using UnityEngine;

public class VoidRip : Laser
{
    [SerializeField] private GameObject singularity = default;

    public override IEnumerator DestroySelf()
    {
        yield return new WaitForSeconds(GetShotLength());
        if (GameManagerScript.HasInterfacesOfType<IChargingWeapon>())
        {
            foreach (var cw in GameManagerScript.FindInterfacesOfType<IChargingWeapon>())
                cw.ResetState();
        }
        SpawnSingularity();
    }

    private void SpawnSingularity()
    {
        GameObject hb = Instantiate(singularity, GetHitPoint() , Quaternion.identity);
        hb.name = hb.name;
        if (hb.GetComponent<Bullet>())
            hb.GetComponent<Bullet>().SetParent(GetParent());
        Destroy(gameObject);
    }
}
