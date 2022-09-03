using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Refacer : MonoBehaviour
{
    [SerializeField] private float focusTurnSpeed = 3;
    [SerializeField] private float resetTurnSpeed = 3;

    public void FocusTowards(Transform target)
    {
        Homing.TurnSelf(focusTurnSpeed, target, transform);
    }

    public void ResetFocusing(Transform parent)
    {
        if (transform.forward != parent.forward)
            Homing.SlowYTurn(focusTurnSpeed, parent.forward, transform, resetTurnSpeed);
    }
}
