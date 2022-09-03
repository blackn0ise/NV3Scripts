using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EpilepsyWarning : MonoBehaviour
{
    [SerializeField] private GameObject gamePersistent = default;
    GameObject gp;

    private void Start()
    {
        InitialiseGPIfNoneFound();
    }
    private void InitialiseGPIfNoneFound()
    {
        if (!FindObjectOfType<GameOptions>())
        {
            gp = Instantiate(gamePersistent);
            gp.name = gamePersistent.name;
        }
    }
}
