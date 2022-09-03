using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml.Serialization;
using System.IO;

[XmlRoot("PassiveCollection")]
public class PassiveSpawnset
{
	[XmlArray("PassiveBonuses")]
	[XmlArrayItem("PassiveBonus")]
	public List<PassiveBonus> passives = new List<PassiveBonus>();


	public static PassiveSpawnset Load(string path)
	{
		TextAsset _xml = Resources.Load<TextAsset>(path);

		XmlSerializer serializer = new XmlSerializer(typeof(PassiveSpawnset));

		StringReader reader = new StringReader(_xml.text);

		PassiveSpawnset passives = serializer.Deserialize(reader) as PassiveSpawnset;

		reader.Close();

		return passives;
	}
}
