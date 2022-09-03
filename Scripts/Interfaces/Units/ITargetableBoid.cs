using UnityEngine;

public interface ITargetableBoid
{
    void ConfirmAndChaseTargetAsBoid(GameObject thisUnit);
    Transform GetTarget();
}
