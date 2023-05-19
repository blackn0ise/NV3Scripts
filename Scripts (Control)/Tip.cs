using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using System.Xml.Serialization;

public class Tip
{
	[XmlAttribute("imgNumber")]
	public int imgNumber;
	[XmlElement("text")]
	public string text;
	[XmlElement("pbRequired")]
	public int pbRequired;
}
