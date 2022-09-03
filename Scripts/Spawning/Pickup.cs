using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickup : MonoBehaviour
{
	[SerializeField] private string pickupName = default;
	public string GetPickupName() { return pickupName; }
	private List<Material> originalMaterials;
	// Start is called before the first frame update
	public virtual void Start()
	{
		Initialise();
	}

	public void Initialise()
	{
		originalMaterials = new List<Material>();
		MeshRenderer[] renderers = gameObject.GetComponentsInChildren<MeshRenderer>();
		foreach (MeshRenderer boi in renderers)
			originalMaterials.Add(boi.material);
	}

	// Update is called once per frame
	void FixedUpdate()
	{
		Spinner.Rotate(transform, 30.0f, 1);
	}
}
