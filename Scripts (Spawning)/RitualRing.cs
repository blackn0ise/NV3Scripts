using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RitualRing : MonoBehaviour
{
    public static bool isInRing { get; private set; } = false;

    private void OnTriggerStay(Collider other)
    {
        isInRing = true;
    }

    private void OnTriggerExit(Collider other)
    {
        isInRing = false;
    }
}
