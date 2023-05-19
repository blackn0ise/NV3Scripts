using System.Collections.Generic;
using UnityEngine;

public interface IOwnershipTower
{
	GameObject GetGem(int value);
	List<Material> GetOriginalMaterials();
	List<MeshRenderer> GetGemRendererList();
	void UpdateMaterial(GameObject gem);
	void UpdateOwnership(Pickup pickup);
}