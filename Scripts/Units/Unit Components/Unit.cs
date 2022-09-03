using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Unit : MonoBehaviour, IUnit
{
	[Header("References")]
	[SerializeField] private GameObject deathAnim = default;
	[SerializeField] private GameObject souls = default;

	[Header("Materials")]
	[SerializeField] private Material flickerMaterial = default;
	[SerializeField] private Material deathMaterial = default;
	[SerializeField] private Material spawnInMaterial = default;

	[Header("Parameters")]
	[SerializeField] private int soulGains = default;
	[SerializeField] private int maxHealth = default;
	[SerializeField] private float damageIFrameRate = default;
	[ReadOnly] private int health;

	private float damageTimeStamp = 0f;
	private bool hasDedSoundPlayed = false;
	private GameManagerScript gm;
	private StatLibrary statl;
	private SpawnController sc;
	private AudioClip dead;
	private AudioClip hurt;
	private MeshRenderer[] renderers;
	private SkinnedMeshRenderer[] smrenderers;
	private List<Material> originalMaterials;
	private List<Material> originalSMRMaterials;
	private AudioSource aso;
	private AudioSource environmentaso;
	private float strayKillDistance = 4000;
	private float lowKillDistance = 0;
	public AudioClip GetDeadClip() { return dead; }
	public AudioClip GetHurtClip() { return hurt; }
	public void SetDeadClip(AudioClip value) { dead = value; }
	public void SetHurtClip(AudioClip value) { hurt = value; }
	public int GetHealth() { return health; }
	public int GetMaxHealth() { return maxHealth; }
	public void SetHealth(int value) { health = value; }
	public void SetMaxHealth(int value) { maxHealth = value; }

	public void Start()
	{
		Initialise();
		StartCoroutine(AnimateSpawnIn());
	}

	private void Initialise()
	{
		gm = GameManagerScript.GetGMScript();
		statl = gm.GetStatl();
		sc = gm.GetSpawnController();
		aso = GetComponent<AudioSource>();
		environmentaso = gm.GetEnvirionmentASO();
		health = maxHealth;
		saveOriginalMaterials();
	}

	private void Update()
	{
		if (Vector3.Distance(transform.position, gm.GetPlayer().transform.position) > strayKillDistance)
			KillUnit(); 
		if (transform.position.y < lowKillDistance)
			KillUnit();
	}

	IEnumerator AnimateSpawnIn()
	{
		int animaterate = 2;
		SetRendererMaterials(spawnInMaterial);
		SetSMRendererMaterials(spawnInMaterial);
		float alphavalue = 1;
		while (alphavalue > 0)
		{
			alphavalue -= Time.deltaTime * animaterate;
			SetRendererAlphaClip(alphavalue);
			SetSMRendererAlphaClip(alphavalue);
			yield return new WaitForSeconds(Time.deltaTime);
		}
		ResetMRMaterials();
		ResetSMRMaterials();
	}

	public void OnTriggerEnter(Collider other)
	{
		if ((other.CompareTag("FriendlyProjectile") && gameObject.CompareTag("Enemies")) ||
			(other.CompareTag("FriendlyProjectile") && gameObject.CompareTag("NecroSpawn")) ||
			(other.CompareTag("FriendlyProjectile") && gameObject.CompareTag("LichSpawn")) ||
			(other.CompareTag("EnemyProjectile") && (gameObject.CompareTag("Friendlies") || gameObject.CompareTag("Player"))))
			HandleDamage(other);
	}

	public void saveOriginalMaterials()
	{
		originalMaterials = new List<Material>();
		originalSMRMaterials = new List<Material>();
		renderers = gameObject.GetComponentsInChildren<MeshRenderer>();
		smrenderers = gameObject.GetComponentsInChildren<SkinnedMeshRenderer>();
		foreach (MeshRenderer boi in renderers)
			originalMaterials.Add(boi.material);
		foreach (SkinnedMeshRenderer boi in smrenderers)
			originalSMRMaterials.Add(boi.material);
	}

	public void HandleDamage(Collider co)
	{
		SoundLibrary.ResetPitch(aso);

		if (!IsDead())
		{
			if (Time.time >= damageTimeStamp)
			{
				IBullet projGo = co.GetComponentInParent<IBullet>();
				SetHealth(GetHealth() - projGo.GetDamage());
				damageTimeStamp = Time.time + damageIFrameRate;
				HandleDamageSoundsAndFlicker();
				HandleDeath(co);
			}
		}

	}

	public void HandleDamageSoundsAndFlicker()
	{
		SoundLibrary.varySoundPitch(aso, 0.05f);
		aso.PlayOneShot(hurt);
		StartCoroutine(DamageFlicker());
	}

	public IEnumerator DamageFlicker()
	{
		if (!IsDead())
		{
			FlickerMRMaterials();
			FlickerSMRMaterials();
		}
		yield return new WaitForSeconds(0.5f);
		if (!IsDead())
		{
			ResetMRMaterials();
			ResetSMRMaterials();
		}
	}

	private void ResetSMRMaterials()
	{
		for (int i = 0; i < originalSMRMaterials.Count; i++)
		{
			smrenderers[i].material = originalSMRMaterials[i];
		}
	}

	private void ResetMRMaterials()
	{
		for (int i = 0; i < originalMaterials.Count; i++)
		{
			renderers[i].material = originalMaterials[i];
		}
	}

	private void FlickerMRMaterials()
	{
		foreach (MeshRenderer mat in renderers)
		{
			if (!mat.gameObject.CompareTag("Wiggler"))
				mat.material = flickerMaterial;
		}
	}

	private void FlickerSMRMaterials()
	{
		foreach (SkinnedMeshRenderer mat in smrenderers)
		{
			if (!mat.gameObject.CompareTag("Wiggler"))
				mat.material = flickerMaterial;
		}
	}

	public void HandleDeath(Collider col)
	{
		if (IsDead())
		{
			if (!gameObject.CompareTag("Player"))
			{
				CreateDeathParticles();
				StartCoroutine(ApplyDeathMaterialAndAnimate());
				gameObject.layer = 10;
				GiveSouls(col);
			}
			PlayDeathSound();
			if (souls)
			{
				GameObject soulsinst = Instantiate(souls, transform.position, gameObject.transform.rotation);
				Destroy(soulsinst, 2);
			}
		}
	}

	IEnumerator ApplyDeathMaterialAndAnimate()
	{
		int animaterate = 2;
		SetRendererMaterials(deathMaterial);
		SetSMRendererMaterials(deathMaterial);
		float alphavalue = 0;
		while (alphavalue < 1)
		{
			alphavalue += Time.deltaTime * animaterate;
			SetRendererAlphaClip(alphavalue);
			SetSMRendererAlphaClip(alphavalue);
			yield return new WaitForSeconds(Time.deltaTime);
		}
		HandleKill();
	}

	private void SetRendererAlphaClip(float alphavalue)
	{
		foreach (MeshRenderer rend in renderers)
		{
			rend.material.SetFloat("_Alpha_Clip_Level", alphavalue);
		}
	}

	private void SetSMRendererAlphaClip(float alphavalue)
	{
		foreach (SkinnedMeshRenderer rend in smrenderers)
		{
			rend.material.SetFloat("_Alpha_Clip_Level", alphavalue);
		}
	}

	private void SetRendererMaterials(Material material)
	{
		foreach (MeshRenderer rend in renderers)
		{
			rend.material = new Material(material);
			rend.material.SetFloat("_Alpha_Clip_Level", 0);
		}
	}

	private void SetSMRendererMaterials(Material material)
	{
		foreach (SkinnedMeshRenderer rend in smrenderers)
		{
			rend.material = new Material(material);
			rend.material.SetFloat("_Alpha_Clip_Level", 0);
		}
	}

	private void HandleKill()
	{
		if (gameObject.CompareTag("Enemies") || gameObject.CompareTag("NecroSpawn") || gameObject.CompareTag("LichSpawn"))
			KillUnit();
		if (gameObject.CompareTag("Friendlies"))
			KillFriendAndUpdateCount();
	}

	public void GiveSouls(Collider col)
	{
		statl.SetSouls(statl.GetSouls() + soulGains);

		if (col.name == "GauntletShot")
			statl.SetSouls(statl.GetSouls() + soulGains);

		if (statl.GetPowerupActive())
			statl.SetBloodBank(statl.GetBloodBank() + soulGains);
	}

	public void KillFriendAndUpdateCount()
	{
		Destroy(gameObject);
		Resurrector.CountFriends("Friendlies");
	}

	public void KillUnit()
	{
		Destroy(gameObject);
		sc.SetTimeSinceLastKill(0);
	}

	public void CreateDeathParticles()
	{
		GameObject anim = Instantiate(deathAnim, transform.position, transform.rotation);
		ParticleSystem ps = anim.GetComponentInChildren<ParticleSystem>();
		Destroy(anim, ps.main.duration);
	}

	public void PlayDeathSound()
	{
		if (!hasDedSoundPlayed)
		{
			SoundLibrary.ResetPitch(environmentaso);
			SoundLibrary.varySoundPitch(environmentaso, 0.2f);
			environmentaso.PlayOneShot(dead);
			hasDedSoundPlayed = true;
		}
	}

	public bool IsDead()
	{
		if (GetHealth() <= 0)
			return true;
		return false;
	}

	public static Transform GetClosestEnemy(GameObject thisUnit)
	{
		if (thisUnit)
		{
			Transform[] enemTransArray = PopulateEnemyTransArray(thisUnit);
			return DetermineClosestEnemy(thisUnit, enemTransArray);
		}
		else
			Debug.LogError("No Source Gameobject to check from. Gameobject is null.");
		return null;
	}

	public static Transform DetermineClosestEnemy(GameObject thisUnit, Transform[] enemTransArray)
	{
		Transform closest = null;
		float minDist = Mathf.Infinity;
		foreach (Transform enemy in enemTransArray)
		{
			Vector3 currentPos = thisUnit.transform.position;
			float dist = Vector3.Distance(enemy.position, currentPos);

			if ((dist < minDist) && enemy)
			{
				closest = enemy;
				minDist = dist;
			}
		}
		return closest;
	}

	public static Transform GetRandomEnemy(GameObject thisUnit)
	{
		if (thisUnit)
		{
			Transform[] enemTransArray = PopulateEnemyTransArray(thisUnit);
			try
			{
				return DetermineRandomEnemy(thisUnit, enemTransArray);
			}
			catch (IndexOutOfRangeException e)
			{
				Debug.Log("Null enemy in array.");
				Debug.Log(e);
				return null;
			}
		}
		else
			Debug.LogError("No Source Gameobject to check from. Gameobject is null.");
		return null;
	}

    private static Transform DetermineRandomEnemy(GameObject thisUnit, Transform[] enemTransArray)
    {
		int random = UnityEngine.Random.Range(0, enemTransArray.Length);
		return enemTransArray[random];
	}

    public static Transform[] PopulateEnemyTransArray(GameObject thisUnit)
	{
		var enemylist = new List<GameObject>();
		GameObject[] enemyArray = null;
		enemyArray = GetEnemyGameObjects(thisUnit, enemylist, enemyArray);
		Transform[] enemTransArray = new Transform[enemyArray.Length];

		for (int i = 0; i < enemyArray.Length; i++)
		{
			enemTransArray[i] = enemyArray[i].transform;
		}

		return enemTransArray;
	}

	public static GameObject ChooseRandomEnemyExcluding(GameObject thisUnit, List<Transform> exceptions)
	{
		GameObject[] enemylist = GetAllEnemies(thisUnit);
		bool multipleenemies = enemylist.Length > 1;
		if (multipleenemies)
		{
			List<GameObject> templist = new List<GameObject>();
			templist.AddRange(enemylist);
			foreach (Transform exception in exceptions)
			{
				if (templist.Contains(exception.gameObject))
					templist.Remove(exception.gameObject);
			}
			int random = UnityEngine.Random.Range(0, templist.Count);
			try
			{
				return templist[random];
			}
			catch (IndexOutOfRangeException e)
			{
				Debug.Log("IndexOutOfRangeException triggered by ChooseRandomEnemy.");
				Debug.Log(e);
				return null;
			}
		}
		Debug.Log("Unable to find replacement target for " + thisUnit + ". returning null.");
		return null;
	}

	/// <summary>
	/// Returns all enemies as an array of gameObjects. Uses the GetEnemyGameObjects() method.
	/// </summary>
	public static GameObject[] GetAllEnemies(GameObject thisUnit)
	{
		var enemylist = new List<GameObject>();
		GameObject[] enemyArray = null;
		enemyArray = GetEnemyGameObjects(thisUnit, enemylist, enemyArray);
		return enemyArray;
	}

	/// <summary>
	/// Returns all enemies as an array of gameObjects, after having removed an array of exceptions from that list. Uses the GetEnemyGameObjects() method.
	/// </summary>
	public static GameObject[] GetAllEnemiesExcluding(GameObject thisUnit, List<GameObject> exceptions)
	{
		var enemylist = new List<GameObject>();
		var templist = new List<GameObject>();
		GameObject[] enemyArray = null;
		enemyArray = GetEnemyGameObjects(thisUnit, enemylist, enemyArray);
		foreach (var item in enemylist)
		{
			templist.Add(item);
		}
		foreach (var item in exceptions)
		{
			if (templist.Contains(item.gameObject))
				templist.Remove(item.gameObject);
		}
		return templist.ToArray();
	}

    /// <summary>
    /// Returns any units tagged by criteria based on the given unit. Also cleans out dead and unitless objects.
    /// </summary>
    public static GameObject[] GetEnemyGameObjects(GameObject thisUnit, List<GameObject> enemylist, GameObject[] enemyArray)
    {
        var templist = new List<GameObject>();

        if (thisUnit.CompareTag("Player"))
        {
			templist.AddRange(GameObject.FindGameObjectsWithTag("Enemies"));
			templist.AddRange(GameObject.FindGameObjectsWithTag("NecroSpawn"));
			templist.AddRange(GameObject.FindGameObjectsWithTag("LichSpawn"));
		}
		else if (thisUnit.CompareTag("Friendlies"))
		{
			templist.AddRange(GameObject.FindGameObjectsWithTag("Enemies"));
			templist.AddRange(GameObject.FindGameObjectsWithTag("NecroSpawn"));
			templist.AddRange(GameObject.FindGameObjectsWithTag("LichSpawn"));
		}
		else if (thisUnit.CompareTag("Enemies") || thisUnit.CompareTag("EnemyArmor") || thisUnit.CompareTag("NecroSpawn") || thisUnit.CompareTag("LichSpawn"))
		{
			templist.AddRange(GameObject.FindGameObjectsWithTag("Friendlies"));
			if (GameObject.FindGameObjectWithTag("Player"))
				templist.AddRange(GameObject.FindGameObjectsWithTag("Player"));
		}
		foreach (GameObject go in templist)
        {
            IUnit unit = go.GetComponent<IUnit>();
            bool isEnemyDead = false;
            if (go && (unit != null))
                isEnemyDead = unit.IsDead();
            if (unit != null && !isEnemyDead)
                enemylist.Add(go);
        }
		enemyArray = enemylist.ToArray();
		return enemyArray;
	}

	/// <summary>
	/// Confirms strings for tags that would be considered enemies by a given unit.
	/// </summary>
	public static List<string> ConfirmEnemyTypes(GameObject thisUnit)
	{
		List<string> enemytypes = new List<string>();
		if (thisUnit.CompareTag("Player"))
		{
			enemytypes.Add("Enemies");
			enemytypes.Add("NecroSpawn");
			enemytypes.Add("LichSpawn");
		}
		else if (thisUnit.CompareTag("Friendlies"))
		{
			enemytypes.Add("Enemies");
			enemytypes.Add("NecroSpawn");
			enemytypes.Add("LichSpawn");
		}
		else if (thisUnit.CompareTag("Enemies") || thisUnit.CompareTag("EnemyArmor") || thisUnit.CompareTag("NecroSpawn") || thisUnit.CompareTag("LichSpawn"))
		{
			enemytypes.Add("Friendlies");
			enemytypes.Add("Player");
		}
		return enemytypes;
	}

	/// <summary>
	/// Confirms strings for tags that would be considered friendlies by a given unit.
	/// </summary>
	public static List<string> ConfirmFriendTypes(GameObject thisUnit)
	{
		List<string> friendtypes = new List<string>();
		if (thisUnit.CompareTag("Player"))
		{
			friendtypes.Add("Friendlies");
			friendtypes.Add("Player");
		}
		else if (thisUnit.CompareTag("Friendlies"))
		{
			friendtypes.Add("Player");
			friendtypes.Add("Friendlies");
		}
		else if (thisUnit.CompareTag("Enemies") || thisUnit.CompareTag("EnemyArmor") || thisUnit.CompareTag("NecroSpawn") || thisUnit.CompareTag("LichSpawn"))
		{
			friendtypes.Add("Enemies");
			friendtypes.Add("EnemyArmor");
			friendtypes.Add("NecroSpawn");
			friendtypes.Add("LichSpawn");
		}
		return friendtypes;
	}
}


