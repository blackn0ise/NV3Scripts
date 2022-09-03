using System.Collections.Generic;
using UnityEngine;
using System.Xml.Serialization;
using System.IO;

[XmlRoot("TipCollection")]
public class TipSet
{
	[XmlArray("Tips")]
	[XmlArrayItem("Tip")]
	public List<Tip> tips = new List<Tip>();


	public static TipSet Load(string path)
	{
		TextAsset _xml = Resources.Load<TextAsset>(path);

		XmlSerializer serializer = new XmlSerializer(typeof(TipSet));

		StringReader reader = new StringReader(_xml.text);

		TipSet tips = serializer.Deserialize(reader) as TipSet;

		reader.Close();

		return tips;
	}
}
