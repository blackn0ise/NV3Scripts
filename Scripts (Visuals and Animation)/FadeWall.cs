using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeWall : MonoBehaviour
{
    public void reloadLevel()
    {
        Debug.Log("reloaded");
        GameManagerScript.GetGMScript().DoRestartGame();
    }
}
