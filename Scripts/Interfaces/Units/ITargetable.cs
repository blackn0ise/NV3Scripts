using UnityEngine;

public interface ITargetable
{
    void ConfirmAndChaseTarget(GameObject thisUnit);
    Transform GetTarget();
}
