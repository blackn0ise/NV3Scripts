using UnityEngine;

public interface IBullet
{
	bool CheckFriendlyFire(Collider other);
	int GetDamage();
	float GetProjDecay();
	float GetKnockFactor();
	bool GetHasKnockback();
	GameObject GetParent();
	void HandleAccuracy();
	void HandleQuadDamage();
	void HandleQuadSound();
	void Initialise();
	void OnTriggerEnter(Collider other);
	void SetParent(GameObject value);
	void Start();
}