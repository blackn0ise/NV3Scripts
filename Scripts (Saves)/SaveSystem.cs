using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System;
using UnityEngine.Audio;

public static class SaveSystem

{
	public static void SaveGame(float version, int score, GameOptions gops, float ms, float mvol, float svol, int rwidth, int rheight, int gqual, float fov, string json, bool ppEnabled, bool middle, bool fgauntlets, bool swgauntlets)
	{
		BinaryFormatter formatter = new BinaryFormatter();
		string path = Application.persistentDataPath + "/save.necro";
		FileStream stream = new FileStream(path, FileMode.Create);

		SaveData data = new SaveData(version, score, gops, ms, mvol, svol, rwidth, rheight, gqual, fov, json, ppEnabled, middle, fgauntlets, swgauntlets);

		formatter.Serialize(stream, data);
		Debug.Log("Saved data to " + path + " successfully.");
		stream.Close();
	}
	public static void SaveGame(GameOptions gops, AudioMixer audioMixer)
	{
		BinaryFormatter formatter = new BinaryFormatter();
		string path = Application.persistentDataPath + "/save.necro";
		FileStream stream = new FileStream(path, FileMode.Create);

		float mvol;
		float svol;
		audioMixer.GetFloat("MusicVolume", out mvol);
		audioMixer.GetFloat("SFXVolume", out svol);

		SaveData data = new SaveData(gops.saveVersion, gops.GetSave().highScore, gops, gops.GetSave().inputSensitivity, mvol, svol, Screen.currentResolution.width, Screen.currentResolution.height, QualitySettings.GetQualityLevel(), gops.GetSave().sfov, gops.GetSave().inputOverrideJson, gops.GetSave().postProcessingEnabled, gops.GetSave().startInMiddle, gops.GetSave().hasFoundGauntlets, gops.GetSave().startWithGauntlets);

		formatter.Serialize(stream, data);
		Debug.Log("Saved data to " + path + " successfully.");
		stream.Close();
	}
	public static void SaveGameDefault(GameOptions gops)
	{
		BinaryFormatter formatter = new BinaryFormatter();
		string path = Application.persistentDataPath + "/save.necro";
		FileStream stream = new FileStream(path, FileMode.Create);

        SaveData data = new SaveData(gops.saveVersion, 0, gops, 2, -5.74778f, -5.74778f, Screen.currentResolution.width, Screen.currentResolution.height, QualitySettings.GetQualityLevel(), 90, "", true, false, false, false);

        formatter.Serialize(stream, data);
		Debug.Log("Saved data to " + path + " successfully.");
		stream.Close();
	}

	public static SaveData LoadGame()
	{
		string path = Application.persistentDataPath + "/save.necro";
		if (File.Exists(path))
		{
			BinaryFormatter formatter = new BinaryFormatter();
			FileStream stream = new FileStream(path, FileMode.Open);

			SaveData data = formatter.Deserialize(stream) as SaveData;
			Debug.Log("Loaded data from " + path + " successfully.");
			//Debug.Log("highscore = " + data.highScore);

			stream.Close();
			return data;
		}
		else
		{
			Debug.Log("Save file not found at " + path);
			return null;
		}
	}

	public static void ClearSave()
	{
		string path = Application.persistentDataPath + "/save.necro";
		if (File.Exists(path))
			File.Delete(path);
		Debug.Log("File deleted from " + path);
	}
}
