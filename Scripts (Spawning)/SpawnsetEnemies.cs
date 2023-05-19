using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml.Serialization;
using System.IO;

[XmlRoot("SpawnCollection")]
public class SpawnsetEnemies
{
	[XmlArray("Spawns")]
	[XmlArrayItem("Spawn")]
	public List<Spawn> spawns = new List<Spawn>();


	public static SpawnsetEnemies Load(string path)
	{
		TextAsset _xml = Resources.Load<TextAsset>(path);

		XmlSerializer serializer = new XmlSerializer(typeof(SpawnsetEnemies));

		StringReader reader = new StringReader(_xml.text);

		SpawnsetEnemies spawns = serializer.Deserialize(reader) as SpawnsetEnemies;

		reader.Close();

		return spawns;
	}
}
