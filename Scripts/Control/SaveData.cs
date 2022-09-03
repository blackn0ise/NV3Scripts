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


	public SaveData(int score, GameOptions go, float ms, float mvol, float svol, int rwidth, int rheight, int gqual, float fov, string json, bool ppEnabled)
    {
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
		defaultMaxHealth = go.GetMaxHealth();

		//InputController info
		inputSensitivity = ms;

		inputOverrideJson = json;
	}
}
