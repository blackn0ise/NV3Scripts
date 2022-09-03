using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System;

public static class SaveSystem

{
	public static void SaveGame(int score, GameOptions go, float ms, float mvol, float svol, int rwidth, int rheight, int gqual, float fov, string json, bool ppEnabled)
	{
		BinaryFormatter formatter = new BinaryFormatter();
		string path = Application.persistentDataPath + "/save.necro";
		FileStream stream = new FileStream(path, FileMode.Create);

		SaveData data = new SaveData(score, go, ms, mvol, svol, rwidth, rheight, gqual, fov, json, ppEnabled);

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
