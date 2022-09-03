using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
using UnityEngine;

public class Launchpad : MonoBehaviour
{
	[SerializeField] private AudioClip jump = default;
	[SerializeField] private float jumpHeight = default;
	[SerializeField] private float energy = 1;
	[SerializeField] private float energyregainfactor = 0.02f;
	[SerializeField] private float energlossfactor = 0.5f;
	private AudioSource aso;
	private GameManagerScript gm;
	private PlayerMovement pm;
	private Vector3 velocity;
	private ParticleSystem particles;
	private float origemissionrate;

	private void Start()
	{
		particles = GetComponentInChildren<ParticleSystem>();
		gm = GameManagerScript.GetGMScript();
		velocity = particles.transform.forward;
		velocity *= jumpHeight;
		origemissionrate = particles.emission.rateOverTimeMultiplier;

		if(GameManagerScript.GetGMScript().GetPlayer())
		{
			aso = gm.GetPlayer().GetComponent<AudioSource>();
			pm = gm.GetPlayer().GetComponent<PlayerMovement>();
		}
	}

	private void Update()
	{
		if (SoulCollector.soulCollectorActive && particles.isPlaying)
		{
			particles.Stop();
		}
		else if (!SoulCollector.soulCollectorActive && !particles.isPlaying)
		{
			particles.Play();
		}
		HandleEnergyAndEmission();
	}

	private void HandleEnergyAndEmission()
	{
		var newem = particles.emission;
		if (energy < 1)
		{
			energy += energyregainfactor * Time.deltaTime;
			newem.rateOverTimeMultiplier = origemissionrate * (energy/3);
		}
		else
		{
			energy = 1;
			if (newem.rateOverTimeMultiplier != origemissionrate)
				newem.rateOverTimeMultiplier = origemissionrate; 
		}
	}

	private void OnTriggerEnter(Collider other)
    {
		if (other.gameObject.GetComponent<Player>() && !SoulCollector.soulCollectorActive)
		{
			Vector3 nv = velocity * energy;
			pm.SetVelocity(nv);
			pm.SetLaunched(true);
			aso.PlayOneShot(jump);

			energy *= energlossfactor;
		}
    }
}
