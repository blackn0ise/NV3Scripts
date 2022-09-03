using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using System.Xml.Serialization;

public class Powerup
{
	[XmlAttribute("spawnIndex")]
	public int spawnIndex;
	[XmlElement("spawnDelay")]
	public int spawnDelay;
	[XmlElement("spawnName")]
	public string spawnName;
}
