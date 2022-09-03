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
    [SerializeField] private GameObject player = default;
    [SerializeField] private GameObject enemyCollection = default;
    [SerializeField] private GameObject friendlyCollection = default;
    [SerializeField] private GameObject nspawnCollection = default;
    [SerializeField] private StatLibrary statl = default;
    [SerializeField] private SoundLibrary sl = default;
    [SerializeField] private SpawnController sc = default;
    [SerializeField] private UIController uic = default;
    [SerializeField] private AudioSource easo = default;
    [SerializeField] private Transform pjtsf = default;
    [SerializeField] private BoidController bc = default;
    [SerializeField] private TutorialScript ts = default;
    [SerializeField] private Camera sceneCamera = default;
    private static GameManagerScript instance;
    private SteamScript steamo;
    private GameOptions gops;
    private PlayerUnit playerUnit;
    private Animator fwanimator;
    public StatLibrary GetStatl() { return statl; }
    public SoundLibrary GetSoundLibrary() { return sl; }
    public SpawnController GetSpawnController() { return sc; }
    public UIController GetUIController() { return uic; }
    public AudioSource GetEnvirionmentASO() { return easo; }
    public Transform GetPJTSF() { return pjtsf; }
    public GameObject GetAltar() { return altar; }
    public GameObject GetPlayer() { return player; }
    public PlayerUnit GetPlayerUnit() { return playerUnit; }
    public Camera GetSceneCamera() { return sceneCamera; }
    public BoidController GetBoidController() { return bc; }
    public SteamScript GetSteamo() { return steamo; }
    public TutorialScript GetTutorialScript() { return ts; }
    public GameObject GetEnemies() { return enemyCollection; }
    public GameObject GetFriendlies() { return friendlyCollection; }
    public GameObject GetNSpawns() { return nspawnCollection; }
    public static GameManagerScript GetGMScript() { return instance;  }
    bool hasSetHealth;

	private void Awake()
    {
        if (!FindObjectOfType<GameOptions>())
            gops = Instantiate(gamePersistent).GetComponent<GameOptions>();
        instance = this;
    }
    void Start()
    {
        Initialise();
    }

    private void Initialise()
    {
        fwanimator = GameObject.FindGameObjectWithTag("FadeWall").GetComponent<Animator>();
        SoulCollector.soulCollectorActive = false;
        Vizier.vizierActive = false;

        var findss = FindObjectOfType<SteamScript>();

        if (GameOptions.GetGOPS() != null)
        {
            gops = GameOptions.GetGOPS();
            if (gops.GetStartWithAllUpgrades())
				statl.SetSouls(statl.GetSouls() + 1);
		}
		if (findss)
			steamo = findss;
	}

    private void Update()
    {
        SetHealthForPlayer();
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
            if (gops)
                if (gops.GetIsTutorialMode())
                    playerUnit.SetHealth(playerUnit.GetHealth() - 20);
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
