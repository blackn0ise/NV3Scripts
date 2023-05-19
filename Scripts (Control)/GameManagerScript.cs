using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManagerScript : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private GameObject gamePersistent = default;
    [SerializeField] private GameObject altar = default;
    [SerializeField] private Altar altarScript = default;
    [SerializeField] private GameObject player = default;
    [SerializeField] private GameObject enemyCollection = default;
    [SerializeField] private GameObject friendlyCollection = default;
    [SerializeField] private GameObject nspawnCollection = default;
    [SerializeField] private StatLibrary statl = default;
    [SerializeField] private SoundLibrary sl = default;
    [SerializeField] private SpawnController sc = default;
    [SerializeField] private UIController uic = default;
    [SerializeField] private Transform pjtsf = default;
    [SerializeField] private BoidController bc = default;
    [SerializeField] private TutorialScript ts = default;
    [SerializeField] private Camera sceneCamera = default;
    [SerializeField] private PowerupBench powerupBench = default;
    [SerializeField] private CorporealShrine corpShrine = default;
    [SerializeField] private HolyShrine holyShrine = default;
    [SerializeField] private BloodShrine bloodShrine = default;
    [SerializeField] private Resurrector resurrector = default;
    [SerializeField] private PlayerMovement playerMovement = default;
    [SerializeField] private MouseLook mouseLook = default;
    [SerializeField] private Player playerScript = default;
    [SerializeField] internal Vector3 tutStartPosition = Vector3.zero;
    [SerializeField] internal Vector3 tutStartRotation = Vector3.zero;
    [SerializeField] internal Vector3 middleStartPosition = Vector3.zero;
    [SerializeField] internal Vector3 middleStartRotation = Vector3.zero;
    private static GameManagerScript instance;
    private SteamScript steamo;
    private GameOptions gops;
    private PlayerUnit playerUnit;
    private Animator fwanimator;
    internal MouseLook GetMouseLook(){ return mouseLook; }
    public StatLibrary GetStatl() { return statl; }
    public SoundLibrary GetSoundLibrary() { return sl; }
    public SpawnController GetSpawnController() { return sc; }
    public UIController GetUIController() { return uic; }
    public Transform GetPJTSF() { return pjtsf; }
    public GameObject GetAltar() { return altar; }
    public Altar GetAltarScript() { return altarScript; }
    public GameObject GetPlayer() { return player; }
    public PlayerUnit GetPlayerUnit() { return playerUnit; }
    public Player GetPlayerScript() { return playerScript; }
    public PlayerMovement GetPlayerMovement() { return playerMovement; }
    public Resurrector GetResurrector() { return resurrector; }
    public Camera GetSceneCamera() { return sceneCamera; }
    public BoidController GetBoidController() { return bc; }
    public SteamScript GetSteamo() { return steamo; }
    public TutorialScript GetTutorialScript() { return ts; }
    public GameObject GetEnemies() { return enemyCollection; }
    public GameObject GetFriendlies() { return friendlyCollection; }
    public GameObject GetNSpawns() { return nspawnCollection; }
    public PowerupBench GetPowerupBench() { return powerupBench; }
    public CorporealShrine GetCorpShrine() { return corpShrine; }
    public HolyShrine GetHolyShrine() { return holyShrine; }
    public BloodShrine GetBloodShrine() { return bloodShrine; }
    public static GameManagerScript GetGMScript() { return instance;  }
    internal bool hasSetHealth;
    internal int hitCounter;

    private void Awake()
    {
        CheckForGOPS();
    }

    private void CheckForGOPS()
    {
        var alreadyGops = FindObjectOfType<GameOptions>();
        if (!alreadyGops)
            gops = Instantiate(gamePersistent).GetComponent<GameOptions>();
        else
            gops = FindObjectOfType<GameOptions>();
        instance = this;
    }

    void Start()
    {
        Initialise();
    }

    private void Initialise()
    {
        CheckForGOPS();

        fwanimator = GameObject.FindGameObjectWithTag("FadeWall").GetComponent<Animator>();
        SoulCollector.soulCollectorActive = false;
        Vizier.vizierActive = false;

        var findss = FindObjectOfType<SteamScript>();

        if (GameOptions.GetGOPS() != null)
        {
            if (gops.GetStartWithAllUpgrades())
                statl.SetSouls(statl.GetSouls() + 1);
            StartCoroutine(HandlePlayerPos());
            gops.musicPlayer.Play();
        }
        if (findss)
            steamo = findss;
	}

    private void Update()
    {
        SetHealthForPlayer();
    }

    private IEnumerator HandlePlayerPos()
    {
        //doing this weird shit because of a seemingly unreliable physics engine bug that means the values aren't properly set sometimes
        yield return new WaitForSeconds(0.2f);
        for (int i = 0; i < 15; i++)
        {
            if (gops.GetSave().startInMiddle)
            {
                if (player.transform.position != middleStartPosition)
                {
                    player.transform.position = middleStartPosition;
                    player.transform.eulerAngles = middleStartRotation;
                    playerMovement.viperEnabled = true;
                }
                else
                    break;
            }
            else
            {
                if (player.transform.position != tutStartPosition)
                {
                    player.transform.position = tutStartPosition;
                    player.transform.eulerAngles = tutStartRotation;
                }
                else
                    break;
            }
            yield return new WaitForSeconds(0.01f);
        }
    }

    public void ExitToMainMenu()
    {
        if (fwanimator && SceneManager.GetActiveScene().name != "Main Menu")
        {
            fwanimator.SetTrigger("FadeToBlack"); 
        }
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        SceneManager.LoadScene("Main Menu");
    }

	internal Camera GetCamera()
	{
        return sceneCamera;
	}

	public void BeginFadeout()
    {
        fwanimator.SetTrigger("FadeToBlack");
    }

    public void DoRestartGame()
    {
        GameLog.Clear();
        Scene scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.name);
        SceneManager.SetActiveScene(scene);
    }

    private void SetHealthForPlayer()
    {
        if (!hasSetHealth && GetPlayer() && gops && gops.GetSave() != null)
        {
			if (gops.GetSaveHealthEnabled())
			{
                gops.SetMaxHealth(gops.GetSave().defaultMaxHealth);
                playerUnit = GetPlayer().gameObject.GetComponent<PlayerUnit>();
                playerUnit.SetMaxHealth(gops.GetSave().defaultMaxHealth);
                playerUnit.SetHealth(gops.GetSave().defaultMaxHealth);
            }
			else
			{
                gops.SetMaxHealth(gops.GetDefaultMaxHealth());
                playerUnit = GetPlayer().gameObject.GetComponent<PlayerUnit>();
                playerUnit.SetMaxHealth(gops.GetDefaultMaxHealth());
                playerUnit.SetHealth(gops.GetDefaultMaxHealth());

            }

            hasSetHealth = true;
        }
    }

    public static IEnumerable<T> FindInterfacesOfType<T>()
    {
		return SceneManager.GetActiveScene().GetRootGameObjects()
				.SelectMany(go => go.GetComponentsInChildren<T>());
	}

	public static bool HasInterfacesOfType<T>()
    {
        foreach (var implementation in FindInterfacesOfType<T>())
        {
            if (implementation != null)
                return true;
        }
        return false;
    }
}
