using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml.Serialization;
using System.IO;

[XmlRoot("PowerupCollection")]
public class PowerupSpawnset
{
	[XmlArray("Powerups")]
	[XmlArrayItem("Powerup")]
	public List<Powerup> powerups = new List<Powerup>();


	public static PowerupSpawnset Load(string path)
	{
		TextAsset _xml = Resources.Load<TextAsset>(path);

		XmlSerializer serializer = new XmlSerializer(typeof(PowerupSpawnset));

		StringReader reader = new StringReader(_xml.text);

		PowerupSpawnset powerups = serializer.Deserialize(reader) as PowerupSpawnset;

		reader.Close();

		return powerups;
	}
}
