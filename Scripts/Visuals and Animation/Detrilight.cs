using Ara;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Detrilight : MonoBehaviour, IMaterialChanger
{
	private GameObject detriLight;
	private GameObject dsprite;

	[SerializeField] private GameObject dspriteposition = default;
	[SerializeField] private GameObject dspritepf = default;
	
	private StatLibrary sl;
	private Altar altar;
	private WeaponBench wb;
	List<Material> originalMaterials;
	[SerializeField] private PlayerUnit playerunit = default;
	[SerializeField] private Resurrector resurrector = default;
	[SerializeField] private bool colouringEnabled = true;
	[SerializeField] private Light dLight = default;
	[SerializeField] private float maxdspritethickness = 2.0f;

	public static Color healcolor { get; set; }
	public static Color rescolor { get; set; }
	public static Color bankcolor { get; set; }
	public static Color blankcolor { get; set; }
	public static Color purplecolor { get; set; }

	void Start()
	{
		altar = GameManagerScript.GetGMScript().GetAltar().GetComponent<Altar>();
		wb = FindObjectOfType<WeaponBench>();
		sl = GameManagerScript.GetGMScript().GetStatl();
		detriLight = gameObject;
		dLight = detriLight.GetComponent<Light>();
		SetDTColours();
		saveOriginalMaterials();
	}

	void Update()
    {
		if (colouringEnabled)
			ColourDetrizideSpell();
	}

	public void SetDTColours()
	{
		healcolor = new Color(0, 255, 0, 255);
		rescolor = new Color(0, 0, 100, 255);
		bankcolor = new Color(255, 161, 8, 255);
		blankcolor = new Color(0, 0, 0, 0);
		purplecolor = new Color(150, 0, 150, 255);
	}

	public void CreateSpellEffect(string type, GameObject effect, Transform pos)
	{
		GameObject instance = effect;
		ParticleSystem ps = effect.GetComponentInChildren<ParticleSystem>();
		var main = ps.main;
		switch (type)
		{
			case "Heal":
				instance.GetComponentInChildren<Light>().color = healcolor;
				main.startColor = healcolor;
				break;
			case "Summon":
				instance.GetComponentInChildren<Light>().color = rescolor;
				main.startColor = rescolor;
				break;
			case "Bank":
				instance.GetComponentInChildren<Light>().color = bankcolor;
				main.startColor = bankcolor;
				break;
		}
		instance = Instantiate(effect, pos.position, playerunit.transform.rotation, playerunit.transform);
		Destroy(instance, 0.5f);

	}

	public void ColourDetrizideSpell()
	{
		if (resurrector.GetSpellAvailable())
			AnimateDsprite();
		else
			DeleteDSprite();
		if (dsprite)
			if (dsprite.GetComponentInChildren<AraTrail>())
				dsprite.GetComponentInChildren<AraTrail>().thickness = 0.25f + Mathf.Clamp((float)sl.GetSouls() / 200, 0, maxdspritethickness);
	}

	private void DeleteDSprite()
	{
		if (dsprite)
			Destroy(dsprite);
	}

	private void AnimateDsprite()
	{
		if (!dsprite)
			dsprite = Instantiate(dspritepf, dspriteposition.transform.position, transform.rotation, dspriteposition.transform);
	}

	public void saveOriginalMaterials()
	{
		originalMaterials = new List<Material>();
		MeshRenderer[] renderers = gameObject.GetComponentsInChildren<MeshRenderer>();
		foreach (MeshRenderer boi in renderers)
			originalMaterials.Add(boi.material);
	}

	public void saveOriginalMaterials(List<Material> list, GameObject go)
	{
		MeshRenderer[] renderers = go.GetComponentsInChildren<MeshRenderer>();
		foreach (MeshRenderer boi in renderers)
			list.Add(boi.material);
	}

	public void overwriteMaterials(Material mat, GameObject go)
	{
		MeshRenderer[] renderers = go.GetComponentsInChildren<MeshRenderer>();
		foreach (MeshRenderer boi in renderers)
			boi.material = mat;
	}

	public void ResetMaterials(GameObject go)
	{
		if (go.GetComponent<WeaponBench>())
		{
			for (int i = 0; i < go.GetComponent<WeaponBench>().GetOriginalMaterials().Count; i++)
			{
				go.GetComponent<WeaponBench>().GetComponentsInChildren<MeshRenderer>()[i].material = go.GetComponent<WeaponBench>().GetOriginalMaterials()[i];
			}
		}
		else if (go.GetComponent<Altar>())
		{
			for (int i = 0; i < go.GetComponent<Altar>().GetOriginalMaterials().Count; i++)
			{
				go.GetComponent<Altar>().GetComponentsInChildren<MeshRenderer>()[i].material = go.GetComponent<Altar>().GetOriginalMaterials()[i];
			}
		}
	}
}
