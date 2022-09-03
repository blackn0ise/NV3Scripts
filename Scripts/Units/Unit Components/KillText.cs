using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillText : MonoBehaviour
{
    [SerializeField] private string killTextPt2 = "";
    public string GetKillText() 
    {
        bool foundSteamName = SteamScript.instance.GetOwnName() != "";
        string killTextpt1 = foundSteamName ? SteamScript.instance.GetOwnName() + " " : "Player ";
        return killTextpt1 + killTextPt2; 
    }
}


//TODO



/*
 * Player was entombed by an Obsidian Demon
Player was sacrificed by a Vizier
Player was consumed by the Soul Collector
*/