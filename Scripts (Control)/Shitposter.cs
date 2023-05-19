using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class Shitposter : MonoBehaviour
{
    [SerializeField] private SlowTurningHomer homer = default;
    [SerializeField] private TextMeshPro tmp = default;
    [SerializeField] private float animDuration = default;
    [SerializeField] private float targetY = 5000;
    [SerializeField] private float targetZRot = 360;
    [SerializeField] private float targetScale = 0.001f;
    [ReadOnly] [SerializeField] private float t = 0;
    private bool fuckoff = false;
    private Vector3 cachedPosition;
    private float cachedScale;

    private string[] firstRunTips = {
        "You can do it. I totally believe in you.", 
        "Yeah buddy! Let's get this run done!",
        "Today's the day!",
        "Try not getting hit",
        "It's only up from here, I guess",
        "You're doing great, sweetie",
        "It's PB time!"
    };
    private string[] shitTalking = {
        "Don't worry, you'll get it next time",
        "I've seen better runs after eating Taco Bell",
        "You might as well go touch grass at this point, it's not like you're PBing anyway",
        "Make sure to aim at the bad guys",
        "Try not getting hit",
        "It could be worse",
        "You're not the worst at this but dammit you're trying",
        "I may not have any legs but at least I can do my job",
        "she took the kids man",
        "I'm condemned to eternity in this place watching ...this?",
        "99% of gamblers quit right before they hit it big",
        "go outside man",
        "git gud",
        "meme",
        "Maybe you should practice more before you stream",
        "You're doing great, sweetie",
        "At least you have a nice personality",
        "Wow, you really showed them who's boss",
        "Don't let the haters get to you",
        "You're a legend in your own mind",
        "You have -one- life left",
        "That was a close one",
        "You're a natural at this",
        "You should be proud of yourself",
        "You know there's a keybind for restarting, right?",
        "Skill issue",
        "that was impressive",
        "It's only up from here, I guess",
        "Are you perhaps, blind",
        "That one doesnt count",
        "noob",
        "lol. lmao.",
        "https://c.tenor.com/6jwgAV3gVBkAAAAC",
        "https://www.youtube.com/watch?v=dQw4w9WgXcQ",
        "bottom text"
    };

    private void Start()
    {
        bool firstGame = GameOptions.GetGOPS().GetIsNewSession();
        if (firstGame)
        {
            int index = Random.Range(0, firstRunTips.Length);
            tmp.text = firstRunTips[index];
        }
        else
        {
            int index = Random.Range(0, shitTalking.Length);
            tmp.text = shitTalking[index];
        }
    }

    private void Update()
    {
        UpdateFuckOff();
    }


    private void UpdateFuckOff()
    {
        if (!fuckoff)
            return;
        if (t >= 1)
        {
            gameObject.SetActive(false);
            fuckoff = false;
            return;
        }

        t += Time.deltaTime / animDuration;
        var newPosition = transform.position;
        newPosition.y = Mathf.Lerp(cachedPosition.y, targetY, t);
        transform.position = newPosition;


        var newRotation = transform.localEulerAngles;
        newRotation.z += (Time.deltaTime / animDuration) * targetZRot;
        transform.localEulerAngles = newRotation;


        var singleValueScale = transform.localScale.x;
        singleValueScale = Mathf.Lerp(cachedScale, targetScale, t);
        transform.localScale = new Vector3(singleValueScale, singleValueScale, singleValueScale);
    }

    internal void FuckOff()
    {
        homer.enabled = false;
        fuckoff = true;
        cachedPosition = transform.position;
        cachedScale = transform.localScale.x;
    }
}
