using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using System.Xml.Serialization;

public class Spawn
{
	[XmlAttribute("spawnDelay")]
	public float spawnDelay;
	[XmlElement("spawnCount")]
	public int spawnCount;
	[XmlElement("spawnUnit")]
	public string spawnUnit;

}
