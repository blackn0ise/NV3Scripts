using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Altar : MonoBehaviour
{
	[Header("Components")]
	[SerializeField] private AudioClip startGameClip = default;
	[SerializeField] private GameObject startGameLight = default;
	[SerializeField] private MeshRenderer gemRenderer = default;
	[SerializeField] private MeshRenderer gemRenderer2 = default;
	[SerializeField] private Material startMaterial = default;
	
	private static float startTime;
	private bool volumefading = false;
	private bool tutorialStarted = false;
	private AudioSource envaso;
	private AudioSource thisaso;
	private SpawnPoint[] spawnlocs;
	private SpawnController sc;
	private SoundLibrary sl;
	private GameOptions gops;
	private GameManagerScript gm;
	private AudioSource musicplayer;
	private UIController uic;
	private Player player;
	private TutorialScript ts;
	private List<Material> originalMaterials;
	private List<MeshRenderer> renderers;

	public static float GetStartTime() { return startTime; }
	public static float GetTimeSinceStart() { return Time.timeSinceLevelLoad - startTime; }
	public List<Material> GetOriginalMaterials() { return originalMaterials; }
	public List<MeshRenderer> GetRendererList() { return renderers; }
	public void SetStartTime(float value) { startTime = value; }

	void Start()
	{
		gm = GameManagerScript.GetGMScript();
		musicplayer = GameObject.Find("MusicPlayer").GetComponent<AudioSource>();
		player = gm.GetPlayer().GetComponent<Player>();
		uic = gm.GetUIController();
		sl = gm.GetSoundLibrary();
		spawnlocs = FindObjectsOfType<SpawnPoint>();
		envaso = gm.GetEnvirionmentASO();
		thisaso = GetComponent<AudioSource>();
		sc = gm.GetSpawnController();
		ts = gm.GetTutorialScript();
		gops = GameOptions.GetGOPS();
		originalMaterials = new List<Material>();
		renderers = new List<MeshRenderer>(GetComponentsInChildren<MeshRenderer>());
		FindObjectOfType<Detrilight>().saveOriginalMaterials(originalMaterials, gameObject);
	}

	private void Update()
	{
		if (volumefading && thisaso.volume > 0)
		{
			thisaso.volume -= (1.0f * Time.deltaTime);
		}
		else if (thisaso.volume <= 0)
		{
			thisaso.Stop();
			volumefading = false;
		}

	}

	private void OnTriggerEnter(Collider other)
	{
		if (!gops.GetIsTutorialMode())
		{
			if (!sc.GetGameActive() && other.gameObject.CompareTag("Player"))
			{
				StartGame();
				StartCoroutine(AnimateGem());
			}
		}
		else if (!tutorialStarted)
		{
			StartTutorial();
		}
	}

	IEnumerator AnimateGem()
	{
		int animaterate = 2;
		gemRenderer.material = startMaterial;
		gemRenderer2.material = startMaterial;
		float alphavalue = 0;
		while (alphavalue < 1)
		{
			alphavalue += Time.deltaTime * animaterate;
			gemRenderer.material.SetFloat("_Alpha_Clip_Level", alphavalue);
			gemRenderer2.material.SetFloat("_Alpha_Clip_Level", alphavalue);
			yield return new WaitForSeconds(Time.deltaTime);
		}
		Destroy(gemRenderer.gameObject);
		Destroy(gemRenderer2.gameObject);
	}

	private void StartGame()
	{
		sc.SetGameActive(true);
		gops.SetIsNewSession(false);
		envaso.PlayOneShot(startGameClip);
		volumefading = true;
		StopParticles();
		StartCoroutine(PlayMusic());
		StartCoroutine(DoDemoMessage());
		SetStartTime(Time.timeSinceLevelLoad);
		GameObject flash = Instantiate(startGameLight, transform.position, Quaternion.identity, transform);
		Destroy(flash, 1.7f);
		foreach (SpawnPoint sp in spawnlocs)
		{
			sp.gameObject.GetComponentInChildren<ParticleSystem>().Play();
		}
	}

	private IEnumerator DoDemoMessage()
	{
		if (gops.GetIsDemomode())
		{
			yield return new WaitForSeconds(gops.GetDemoMsgDelay());
			StartCoroutine(ts.DisplayTip(99));
			StartCoroutine(ts.ClearTutorialTip(15)); 
		}
	}

	private void StartTutorial()
	{
		sc.SetGameActive(true);
		gops.SetIsNewSession(false);
		envaso.PlayOneShot(startGameClip);
		volumefading = true;
		StopParticles();
		StartCoroutine(PlayMusic(sl.GetTutorialTrack()));
		SetStartTime(Time.timeSinceLevelLoad);
		GameObject flash = Instantiate(startGameLight, transform.position, Quaternion.identity, transform);
		Destroy(flash, 1.7f);
		foreach (SpawnPoint sp in spawnlocs)
		{
			sp.gameObject.GetComponentInChildren<ParticleSystem>().Play();
		}
		ts.BeginTutorial();
		tutorialStarted = true;
	}

	private void StopParticles()
	{
		foreach (ParticleSystem ps in GetComponentsInChildren<ParticleSystem>())
		{
			ps.Stop();
		}
	}

	IEnumerator PlayMusic()
	{
		MusicPlayer mp = musicplayer.gameObject.GetComponent<MusicPlayer>();
		mp.GetComponent<AudioSource>().volume = 1;
		yield return new WaitForSeconds(1.8f);
		musicplayer.clip = sl.GetTrack(mp);
		musicplayer.Play();
	}

	IEnumerator PlayMusic(AudioClip track)
	{
		MusicPlayer mp = musicplayer.gameObject.GetComponent<MusicPlayer>();
		mp.GetComponent<AudioSource>().volume = 1;
		yield return new WaitForSeconds(1.8f);
		musicplayer.clip = track;
		musicplayer.Play();
	}

	public static GameObject ResurrectUnit(float offsetamount, GameObject resUnit, GameObject spawnAnim, GameObject summontracer, Vector3 reslocation, Vector3 tracerstartloc, Quaternion tracerrotation, Transform parentGO)
	{
		Vector3 randomoffset = new Vector3(Random.Range(-offsetamount, offsetamount), 0, Random.Range(-offsetamount, offsetamount));
		GameObject newUnit = Instantiate(resUnit, reslocation + randomoffset, Quaternion.identity, parentGO);
		GameObject anim = Instantiate(spawnAnim, reslocation + randomoffset, Quaternion.identity, parentGO);
		GameObject tracer = Instantiate(summontracer, tracerstartloc, tracerrotation);
		tracer.GetComponent<HomingProjectile>().SetDefaultTarget(newUnit.transform);
		Destroy(anim, 1.0f);
		Destroy(tracer, 2.0f);
		newUnit.name = resUnit.name;
		return newUnit;
	}
}

