using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]

public class SaveData
{
    public int highScore;
	public float musicVolume;
	public float soundVolume;
	public int reswidth;
	public int resheight;
	public int gqualindex;
	public int defaultMaxHealth;
	public float inputSensitivity = 2;
	public float sfov = 60;
	public string inputOverrideJson = "";
	public float test;
	public bool postProcessingEnabled;
	public bool startInMiddle;
	public bool hasFoundGauntlets;
	public bool startWithGauntlets;
	public float saveVersion;


	public SaveData(float version, int score, GameOptions gops, float ms, float mvol, float svol, int rwidth, int rheight, int gqual, float fov, string json, bool ppEnabled, bool middle, bool foundGauntlets, bool swGauntlets)
    {
		saveVersion = version;
        highScore = score;
		//Options info
		musicVolume = mvol;
		soundVolume = svol;
		reswidth = rwidth;
		resheight = rheight;
		gqualindex = gqual;
		sfov = fov;
        postProcessingEnabled = ppEnabled;

        //GameOptions info;
        defaultMaxHealth = gops.GetMaxHealth();
        startInMiddle = middle;
		hasFoundGauntlets = foundGauntlets;
		startWithGauntlets = swGauntlets;

		//InputController info
		inputSensitivity = ms;

        inputOverrideJson = json;
	}
}
