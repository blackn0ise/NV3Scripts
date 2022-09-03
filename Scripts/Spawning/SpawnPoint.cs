using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
	[SerializeField] private AudioClip spawnSound = default;
	[SerializeField] private GameObject spawnAnim = default;
	[SerializeField] private GameObject enemyCollection = default;
	[SerializeField] private GameObject maincollection = default;
	[SerializeField] private AudioSource environmentaso = default;

	private GameObject player;
	private AudioSource aso;
	private List<Spawn> spawnList = new List<Spawn>();
	public List<Spawn> GetSpawnList() { return spawnList; }

	void Start()
	{
		player = GameObject.Find("Player");
		aso = GetComponent<AudioSource>();
	}

	public void SpawnUnit(GameObject spawn)
	{
		SoundLibrary.ResetPitch(environmentaso);
		GameObject newUnit = Instantiate(spawn, transform.position, Quaternion.identity, enemyCollection.transform);
		newUnit.transform.LookAt(player.transform);
		GameObject anim = Instantiate(spawnAnim, transform.position, Quaternion.identity, maincollection.transform);
		Destroy(anim, 1.0f);
		newUnit.name = spawn.name;
		SoundLibrary.varySoundPitch(aso, 0.4f);
		aso.PlayOneShot(spawnSound);
	}

}
