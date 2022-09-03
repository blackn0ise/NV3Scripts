using Ara;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Souls : MonoBehaviour
{
    [SerializeField] private float destroydelay = 2;

    private void Start()
    {
        Destroy(gameObject, destroydelay);
    }

}
