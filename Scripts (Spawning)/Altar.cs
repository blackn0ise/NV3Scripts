using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Altar : MonoBehaviour
{
	[Header("Components")]
	[SerializeField] private AudioClip startGameClip = default;
	[SerializeField] private GameObject startGameLight = default;
	[SerializeField] private GameObject gemp = default;
	[SerializeField] private GameObject bigGem = default;
	[SerializeField] private MeshRenderer gemRenderer = default;
	[SerializeField] private MeshRenderer gemRenderer2 = default;
	[SerializeField] private Material startMaterial = default;
	[SerializeField] private Shitposter shitposter = default;

	[Header("Bank Pillar")]
	[SerializeField] private GameObject bankPillar = default;
	[SerializeField] private float pillarDecreaseFactor = 150;
	[SerializeField] private float pillarLerpFactor = 150;
	[SerializeField] private Color wBankPillarColor = Color.yellow;
	[SerializeField] private Color sBankPillarColor = Color.blue;
	[SerializeField] private Color bBankPillarColor = Color.red;
	[SerializeField] private Material wBankPillarMat = default;
	[SerializeField] private Material sBankPillarMat = default;
	[SerializeField] private Material bBankPillarMat = default;
	[SerializeField] private ParticleSystem bPillarParticles = default;
	[SerializeField] private MeshRenderer bPillarMRenderer = default;

	private float bPillarTargetY;
	private static float startTime;
	private bool volumefading = false;
	private bool tutorialStarted = false;
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
	private Vector3 originalBPScale = Vector3.zero;
	private Vector3 BPScale = Vector3.zero;

	public static float GetStartTime() { return startTime; }
	public static float GetTimeSinceStart() { return Time.timeSinceLevelLoad - startTime; }
	public static bool GetIsGameActive() { return GameManagerScript.GetGMScript().GetSpawnController().GetGameActive(); }
	public List<Material> GetOriginalMaterials() { return originalMaterials; }
	public List<MeshRenderer> GetRendererList() { return renderers; }
	public void SetStartTime(float value) { startTime = value; }

	void Start()
    {
        InitialiseVariables();
    }

    private void InitialiseVariables()
    {
        gm = GameManagerScript.GetGMScript();
        musicplayer = GameObject.Find("MusicPlayer").GetComponent<AudioSource>();
        player = gm.GetPlayer().GetComponent<Player>();
        uic = gm.GetUIController();
        sl = gm.GetSoundLibrary();
        spawnlocs = FindObjectsOfType<SpawnPoint>();
        thisaso = GetComponent<AudioSource>();
        sc = gm.GetSpawnController();
        ts = gm.GetTutorialScript();
        gops = GameOptions.GetGOPS();
        originalMaterials = new List<Material>();
        renderers = new List<MeshRenderer>(GetComponentsInChildren<MeshRenderer>());
        FindObjectOfType<Detrilight>().saveOriginalMaterials(originalMaterials, gameObject);
        bankPillar.SetActive(true);
        originalBPScale = bankPillar.transform.localScale;
        bankPillar.transform.localScale = Vector3.zero;
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
		HandleBPillarSize();
	}

	private void OnTriggerEnter(Collider other)
	{
		if (!gops.GetIsTutorialMode())
		{
			if (!sc.GetGameActive() && other.gameObject.CompareTag("Player"))
			{
				StartGame();
				StartCoroutine(AnimateGems());
			}
		}
		else if (!tutorialStarted)
		{
			StartGame(true);
			StartCoroutine(AnimateGems());
		}
	}


	private void StartGame(bool tutorial = false)
    {
		gm.hitCounter = 0;
        sc.SetGameActive(true);
        sc.GenerateRandomSeed();
        sc.HandleCostTrees();
        sc.DisplayNextPurchaseOptions(true);
        GameManagerScript.GetGMScript().GetPowerupBench().DeactivateParticles();
        gops.SetIsNewSession(false);
        if (gops.GetSave().highScore < 100)
        {
			gops.GetSave().startInMiddle = true;
			SaveSystem.SaveGame(gops, gops.audioMixer);
		}
		SoundLibrary.PlayFromTimedASO(startGameClip, transform.position);
        volumefading = true;
        StopGemParticles();
        if (!tutorial)
            StartCoroutine(PlayMusic());
		else
			StartCoroutine(PlayMusic(sl.GetTutorialTrack()));
		StartCoroutine(DoDemoThankMessage());
        StartCoroutine(SpawnPowerupCheat());
        SetStartTime(Time.timeSinceLevelLoad);
        GameObject flash = Instantiate(startGameLight, transform.position, Quaternion.identity, transform);
        Destroy(flash, 1.7f);
        foreach (SpawnPoint sp in spawnlocs)
        {
            sp.gameObject.GetComponentInChildren<ParticleSystem>().Play();
        }
		if(tutorial)
        {
			ts.BeginTutorial();
			tutorialStarted = true;
		}
		shitposter.FuckOff();
	}

	private IEnumerator SpawnPowerupCheat()
    {
		yield return new WaitForSeconds(2);
        if (gops.GetStartWithPowerup())
        {
            sc.SpawnPowerup(sc.GetFirstPowerup());
            gops.SetStartWithPowerup(false);
        }
    }

    private IEnumerator DoDemoThankMessage()
	{
		if (gops.GetIsDemomode())
		{
			yield return new WaitForSeconds(gops.GetDemoMsgDelay());
			StartCoroutine(ts.DisplayTip(99));
			StartCoroutine(ts.ClearTutorialTip(15)); 
		}
	}

	public static GameObject ResurrectUnit(float offsetamount, GameObject resUnit, GameObject spawnAnim, GameObject summontracer, GameObject summonParticles, Vector3 reslocation, Vector3 tracerstartloc, Quaternion tracerrotation, Transform parentGO)
	{
		Vector3 randomoffset = new Vector3(Random.Range(-offsetamount, offsetamount), 0, Random.Range(-offsetamount, offsetamount));
		GameObject newUnit = Instantiate(resUnit, reslocation + randomoffset, Quaternion.identity, parentGO);
		if (parentGO.name == "Friendlies")
			newUnit.transform.LookAt(GameManagerScript.GetGMScript().GetPlayer().transform);
		GameObject anim = Instantiate(spawnAnim, reslocation + randomoffset, Quaternion.identity, parentGO);
		GameObject tracer = Instantiate(summontracer, tracerstartloc, tracerrotation);
		tracer.GetComponent<HomingProjectile>().SetDefaultTarget(newUnit.transform);
		GameObject parti = Instantiate(summonParticles, newUnit.transform.position, Quaternion.identity, parentGO);
		Destroy(parti, 3.0f); 
		Destroy(anim, 1.0f);
		GameManagerScript.GetGMScript().GetAltarScript().BeginStopTracer(tracer);
		Destroy(tracer, 6.0f);
		newUnit.name = resUnit.name;
		return newUnit;
	}

	#region sound

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

	#endregion

	#region particles and effects

	IEnumerator AnimateGems()
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

		bigGem.SetActive(true);
	}

	private void StopGemParticles()
	{
		foreach (ParticleSystem ps in gemp.GetComponentsInChildren<ParticleSystem>())
		{
			ps.Stop();
		}
	}

	public void BeginStopTracer(GameObject tracer)
	{
		StartCoroutine(StopTracerParticles(tracer));
	}

	private IEnumerator StopTracerParticles(GameObject tracer, float delay = 3)
	{
		yield return new WaitForSeconds(delay);
		tracer.GetComponentInChildren<ParticleSystem>().Stop();
	}

	public void PulseAltarParticles(int count)
	{
		InitialiseBPScale();
		bPillarTargetY += originalBPScale.y * count;
		if (bPillarTargetY + (originalBPScale.y * count) > originalBPScale.y)
			bPillarTargetY = originalBPScale.y;

	}

	public void HandleBPillarColour(string type)
	{
		var main = bPillarParticles.main;
		switch (type)
		{
			case "corp":
				main.startColor = wBankPillarColor;
				bPillarMRenderer.material = wBankPillarMat;
				break;
			case "holy":
				main.startColor = sBankPillarColor;
				bPillarMRenderer.material = sBankPillarMat;
				break;
			case "blood":
				main.startColor = bBankPillarColor;
				bPillarMRenderer.material = bBankPillarMat;
				break;
		}
	}

	private void InitialiseBPScale()
	{
		if (bankPillar.transform.localScale.x != originalBPScale.x || bankPillar.transform.localScale.z != originalBPScale.z)
		{
			bankPillar.transform.localScale = originalBPScale;
			BPScale = originalBPScale;
		}

	}

	private void HandleBPillarSize()
	{
		if (bankPillar.transform.localScale.y > 0)
		{
			bPillarTargetY -= Time.deltaTime * pillarDecreaseFactor;
		}
		else
			bankPillar.transform.localScale = Vector3.zero;

		BPScale.y = Mathf.Lerp(BPScale.y, bPillarTargetY, pillarLerpFactor);
		bankPillar.transform.localScale = BPScale;
	}

	#endregion
}

