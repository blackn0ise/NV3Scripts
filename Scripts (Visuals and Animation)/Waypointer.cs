using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Waypointer : MonoBehaviour
{
    [SerializeField] private ParticleSystem holyTracer = default;
    [SerializeField] private ParticleSystem fnbTracer = default;
    [SerializeField] private ParticleSystem bloodTracer = default;
    [SerializeField] private ParticleSystem holyAura = default;
    [SerializeField] private ParticleSystem fnbAura = default;
    [SerializeField] private ParticleSystem bloodAura = default;
    [SerializeField] private Light holylight = default;
    [SerializeField] private Light fnblight = default;
    [SerializeField] private Light bloodlight = default;
    private List<ParticleSystem> emitters;
	private List<Light> lights;

	// Start is called before the first frame update
	void Start()
	{
		emitters = new List<ParticleSystem>();
		lights = new List<Light>();
		emitters.AddRange(new ParticleSystem[] { holyTracer, fnbTracer, bloodTracer, holyAura, fnbAura, bloodAura });
		lights.AddRange(new Light[] { holylight, fnblight, bloodlight });
        if (SceneManager.GetActiveScene().name == "Game")
            DeactivateAll();
	}

	private void DeactivateAll()
    {
        foreach (ParticleSystem emitter in emitters)
        {
            emitter.Stop();
        }
        foreach (Light light in lights)
        {
            light.range = 0;
        }
    }

    public void ChooseAndTogglePointer(string bench, bool istobeactive)
    {
        switch (bench)
        {
            case "Holy":
                if (istobeactive)
                    ActivatePointer(holylight, holyTracer, holyAura);
                else
                    DeactivatePointer(holylight, holyTracer, holyAura);
                break;

            case "FNB":
                if (istobeactive)
                    ActivatePointer(fnblight, fnbTracer, fnbAura);
                else
                    DeactivatePointer(fnblight, fnbTracer, fnbAura);
                break;

            case "Blood":
                if (istobeactive)
                    ActivatePointer(bloodlight, bloodTracer, bloodAura);
                else
                    DeactivatePointer(bloodlight, bloodTracer, bloodAura);
                break;

        }
    }

    private void DeactivatePointer(Light light, ParticleSystem tracer, ParticleSystem aura)
    {
        tracer.Stop();
        aura.Stop();
        light.range = 0;
    }

    private void ActivatePointer(Light light, ParticleSystem tracer, ParticleSystem aura)
    {
        tracer.Play();
        aura.Play();
        light.range = 40;
    }
}
