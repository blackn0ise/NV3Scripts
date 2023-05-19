using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Portal : MonoBehaviour
{
	[SerializeField] private GameObject exit = default;
	[SerializeField] private GameObject exit2 = default;
	[SerializeField] private AudioClip portaled = default;
	private CharacterController cc;

	private void OnTriggerEnter(Collider other)
    {
		cc = other.gameObject.GetComponent<CharacterController>();

		if (other.gameObject.GetComponent<Player>() && other.gameObject.GetComponent<CharacterController>())
		{
			cc.enabled = false;
			Vector3 position = SoulCollector.soulCollectorActive ? exit2.transform.position : exit.transform.position;
			other.gameObject.transform.position = position;
			GetComponent<AudioSource>().PlayOneShot(portaled);
			cc.enabled = true;

		}
	}
}
