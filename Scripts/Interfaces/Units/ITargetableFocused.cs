using UnityEngine;

public interface ITargetableFocused
{
    void ConfirmAndChasePlayer(GameObject thisUnit);
    void SetPlayerTarget();
    Transform GetTarget();
}
