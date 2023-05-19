using System.Collections.Generic;
using UnityEngine;

public interface IMaterialChanger
{
	void saveOriginalMaterials();

	void saveOriginalMaterials(List<Material> list, GameObject go);

	void overwriteMaterials(Material mat, GameObject go);

	void ResetMaterials(GameObject go);

}
