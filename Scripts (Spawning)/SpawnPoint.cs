using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
	[SerializeField] private AudioClip spawnSound = default;
	[SerializeField] private GameObject spawnAnim = default;
	[SerializeField] private GameObject enemyCollection = default;
	[SerializeField] private GameObject maincollection = default;
	[SerializeField] private GameObject spawnParticles = default;

	private GameObject player;
	private List<Spawn> spawnList = new List<Spawn>();
	public List<Spawn> GetSpawnList() { return spawnList; }

	void Start()
	{
		player = GameObject.Find("Player");
	}

	public void SpawnUnit(GameObject spawn)
	{
		GameObject newUnit = Instantiate(spawn, transform.position, Quaternion.identity, enemyCollection.transform);
		newUnit.transform.LookAt(player.transform);

		//animation
		GameObject anim = Instantiate(spawnAnim, transform.position, Quaternion.identity, maincollection.transform);
		Destroy(anim, 1.0f);
		GameObject parti = Instantiate(spawnParticles, transform.position, Quaternion.identity, maincollection.transform);
		Destroy(parti, 3.0f);


		newUnit.name = spawn.name;
		SoundLibrary.PlayFromTimedASO(spawnSound, transform.position, 1, 0.4f);
	}

}
