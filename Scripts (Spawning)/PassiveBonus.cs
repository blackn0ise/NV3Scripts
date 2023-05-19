using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using System.Xml.Serialization;

public class PassiveBonus
{
	[XmlAttribute("spawnIndex")]
	public int spawnIndex;
	[XmlElement("spawnCost")]
	public int spawnCost;
	[XmlElement("spawnName")]
	public string spawnName;
}
