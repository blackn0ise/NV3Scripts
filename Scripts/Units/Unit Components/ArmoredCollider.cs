using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmoredCollider : MonoBehaviour
{
	[SerializeField] private GameObject dissipation = default;

	public void OnTriggerEnter(Collider other)
	{
		if ((other.CompareTag("FriendlyProjectile") && gameObject.CompareTag("Enemies")) ||
			(other.CompareTag("FriendlyProjectile") && gameObject.CompareTag("EnemyArmor")) ||
			(other.CompareTag("FriendlyProjectile") && gameObject.CompareTag("NecroSpawn")) ||
			(other.CompareTag("FriendlyProjectile") && gameObject.CompareTag("LichSpawn")) ||
			(other.CompareTag("EnemyProjectile") && (gameObject.CompareTag("Friendlies") || gameObject.CompareTag("Player"))))
			InstantiateDissipation(other.transform.position);
	}

	private void InstantiateDissipation(Vector3 position)
	{
		GameObject disinst = null;
		if (dissipation)
			disinst = Instantiate(dissipation, position, Quaternion.identity);
	}
}
