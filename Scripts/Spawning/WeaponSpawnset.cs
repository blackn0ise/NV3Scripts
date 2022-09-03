using System.Collections.Generic;
using UnityEngine;
using System.Xml.Serialization;
using System.IO;

[XmlRoot("WeaponCollection")]
public class WeaponSpawnset
{
	[XmlArray("Weapons")]
	[XmlArrayItem("Weapon")]
	public List<Weapon> weapons = new List<Weapon>();


	public static WeaponSpawnset Load(string path)
	{
		TextAsset _xml = Resources.Load<TextAsset>(path);

		XmlSerializer serializer = new XmlSerializer(typeof(WeaponSpawnset));

		StringReader reader = new StringReader(_xml.text);

		WeaponSpawnset weapons = serializer.Deserialize(reader) as WeaponSpawnset;

		reader.Close();

		return weapons;
	}
}
