using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using UnityEngine.EventSystems;
using UnityEngine.Rendering.Universal;
using UnityEngine.InputSystem;

public class MainMenu : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private GameObject gamePersistent = default;
    [SerializeField] private GameObject loadingScreen = default;
    [SerializeField] private GameObject startConfirmButton = default;
    [SerializeField] private GameObject startTutConfirmButton = default;
    [SerializeField] private GameObject tipButton = default;
    [SerializeField] private GameObject esFirstSelected = default;
    [SerializeField] private GameObject demoPanel = default;
    [SerializeField] private Camera _camera = default;
    [SerializeField] private AudioClip startclip = default;
    [SerializeField] private TextMeshProUGUI tipText = default;
    [SerializeField] private TextMeshProUGUI TMPHSText = default;
    [SerializeField] private Image tipImg = default;
    [SerializeField] private AudioSource clickeraso = default;
    [SerializeField] private EventSystem eventSystem = default;
    [SerializeField] private PlayerInput playerInput = default;

    [Header("Parameters")]
    [SerializeField] private float startDelay = default;

    private AudioSource mpaso;
    private TipController tctrl;
    private MusicPlayer mp;
    private GameObject gopsgo;
    private GameOptions gops;
    bool volumefading = false;
    bool saveResultsSet = false;
    bool mvolupdated = false;
    AsyncOperation operation;

    private void OnEnable()
    {
        eventSystem.SetSelectedGameObject(esFirstSelected);
    }

    private void Start()
    {
        InitialiseGPIfNoneFound();
        InitialiseMPandPlay();
        ResetMenu();
        StartCoroutine(LoadAsynchronously());
        tctrl = GetComponent<TipController>();
        SetDemoPanel();
    }

    private void Update()
    {
        CheckShowStartConfirm();
        CheckIfFirstGame();
        UseSaveOptionsOnceFound();
        FadeVolume(); 
        ResetMPASOVolume();
    }

	private void SetDemoPanel()
    {
        if (!gops.GetIsDemomode())
            demoPanel.SetActive(false);

    }

    private void ResetMenu()
    {
        volumefading = false;
    }

    private void ResetMPASOVolume()
	{
        if (!mvolupdated)
        {
            mpaso.volume = 1;
            mvolupdated = true;
        }
    }

    private void InitialiseGPIfNoneFound()
    {
        if (!FindObjectOfType<GameOptions>())
        {
            gopsgo = Instantiate(gamePersistent);
            gopsgo.name = gamePersistent.name;
            gops = gopsgo.GetComponent<GameOptions>();
        }
        else
		{
            gops = GameOptions.GetGOPS();
        }
    }

    private void InitialiseMPandPlay()
    {
        mp = FindObjectOfType<MusicPlayer>();
        mpaso = mp.GetComponent<AudioSource>();
        mpaso.clip = mp.GetMainMenu();
		mpaso.Play();
	}

	private void UseSaveOptionsOnceFound()
	{
		if (!saveResultsSet && gops.GetSave() != null)
		{
            SteamScript steamScript = FindObjectOfType<SteamScript>();
            GameObject.Find("TMPHSParent").GetComponentInChildren<TextMeshProUGUI>().text = "PERSONAL BEST - " +
				(steamScript.GetFoundOwnResult() ? (steamScript.GetOwnScore() / 1000.0f).ToString() :
				(gops.GetSave().highScore / 1000.0f).ToString());
			saveResultsSet = true;
            _camera.GetUniversalAdditionalCameraData().renderPostProcessing = gops.GetSave().postProcessingEnabled;
        }
    }

	private void CheckIfFirstGame()
	{
		if (gops)
		{
            if (gops.GetIsNewSession() == false)
                gops.SetIsNewSession(true);
        }
    }

	private void CheckShowStartConfirm()
	{
        if (operation != null)
        {
            var done = operation.progress > 0.89;
            if (done && loadingScreen.activeInHierarchy == true)
			{
				if (gops)
				{
                    if (gops.GetSave().highScore != 0 && !gops.GetIsTutorialMode())
					{
                        //normal start, both buttons fast
						tipButton.SetActive(true);
                        startConfirmButton.SetActive(true);
                    }
                    else if (gops.GetIsTutorialMode())
                    {
                        //tutorial mode, don't show tutorial button
                        StartCoroutine(DelayedShowStartAndTutButton(3.5f, false));
                    }
                    else if (gops.GetIsFreshSave())
                    {
                        //first start, no other times, slow show
                        StartCoroutine(DelayedShowStartAndTutButton(3.5f));
                    }
                }
				else
                {
                    //show anyway even if no gops
                    CentreStartConfirmButton();
                    startConfirmButton.SetActive(true);
                }
            }
		}

    }

    private void CentreStartConfirmButton()
    {
        Vector2 anchoredPosition = startConfirmButton.GetComponent<RectTransform>().anchoredPosition;
        anchoredPosition.x = 0;
        startConfirmButton.GetComponent<RectTransform>().anchoredPosition = anchoredPosition;
    }

    private IEnumerator DelayedShowStartAndTutButton(float delay = 0, bool showTutButton = true)
	{
        yield return new WaitForSeconds(delay);
        startConfirmButton.SetActive(true);
        startTutConfirmButton.SetActive(showTutButton);
        if (!showTutButton)
            CentreStartConfirmButton();
    }

    private void FadeVolume()
	{
        if (volumefading && mpaso.volume > 0)
			mpaso.volume -= (0.5f * Time.deltaTime);
    }

    public void RefreshHighScore()
    {
        if (gops.GetSave() != null)
		{
			TMPHSText.text = "PERSONAL BEST: " + gops.GetSave().highScore.ToString();
			Debug.Log("Save reset successfully.");
		}
	}

	public void LoadGame()
    {
        clickeraso.PlayOneShot(startclip, 0.6f);
        loadingScreen.SetActive(true);
        eventSystem.SetSelectedGameObject(startConfirmButton);
        FormatStringAndSetTip();
        StartFade();
    }

    private void StartFade()
    {
        FindObjectOfType<MenuFadeWall>().StartGame();
        StartCoroutine(WaitForFadeAndLoad());
        volumefading = true;
    }

    public void FormatStringAndSetTip()
    {
        Tip tip = tctrl.GetRandomTip();
        string text = tip.text;
        text = Regex.Unescape(text);
        text = ReplaceTipKeyMarkers(text);
        if (tipText)
            tipText.text = text;
        if (tipImg)
            tipImg.sprite = tctrl.FindSpriteByNumber(tip.imgNumber);
    }

    private string ReplaceTipKeyMarkers(string text)
    {
        playerInput.SwitchCurrentActionMap("PlayerControls");
        string temp = text;
        if (temp.Contains("+ic.Heal+"))
            temp = temp.Replace("+ic.Heal+", UIController.GetKeyString("Heal", playerInput));
        if (temp.Contains("+ic.Summon+"))
            temp = temp.Replace("+ic.Summon+", UIController.GetKeyString("Summon", playerInput));
        if (temp.Contains("+ic.SpiritBank+"))
            temp = temp.Replace("+ic.SpiritBank+", UIController.GetKeyString("SpiritBank", playerInput));
        if (temp.Contains("+ic.WeaponBank+"))
            temp = temp.Replace("+ic.WeaponBank+", UIController.GetKeyString("WeaponBank", playerInput));
        if (temp.Contains("+ic.Dash+"))
            temp = temp.Replace("+ic.Dash+", UIController.GetKeyString("Dash", playerInput));
        if (temp.Contains("+ic.SwitchSummons+"))
            temp = temp.Replace("+ic.SwitchSummons+", UIController.GetKeyString("SwitchSummons", playerInput));
        if (temp.Contains("+ic.CycleWeapons+"))
            temp = temp.Replace("+ic.CycleWeapons+", UIController.GetKeyString("CycleWeapons", playerInput));
        if (text.Contains("+ic.NextWeapon+"))
            temp = temp.Replace("+ic.NextWeapon+", UIController.GetKeyString("NextWeapon", playerInput));
        playerInput.SwitchCurrentActionMap("MenuControls");
        return temp;
    }

	public void QuitGame()
	{
		Application.Quit();
	}

    public void NextScene()
    {
        operation.allowSceneActivation = true;
    }

    IEnumerator WaitForFadeAndLoad()
	{
		yield return new WaitForSeconds(startDelay);
    }

    IEnumerator LoadAsynchronously()
    {
        yield return new WaitForSeconds(0.1f);
        operation = SceneManager.LoadSceneAsync("Game");
        operation.allowSceneActivation = false;

        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / 0.9f);
            yield return null;
        }
        startConfirmButton.SetActive(true);
        tipButton.SetActive(true);
    }

    public void PlayClickSound()
	{
		clickeraso.Play();
	}

	public void SetTutorialMode(bool value)
	{
		if (gops)
            gops.SetTutorialMode(value);
	}
}
