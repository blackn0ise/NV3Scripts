using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MainMenuInput : MonoBehaviour
{
    [SerializeField] private GameObject mainMenu = default;
    [SerializeField] private GameObject optionsMenu = default;
    [SerializeField] private GameObject hotkeyMenu = default;
    [SerializeField] private GameObject resetMenu = default;
    [SerializeField] private GameObject leaderboard = default;
    [SerializeField] private GameObject loadingScreen = default;
    [SerializeField] private OptionsMenu optionScript = default;
    [SerializeField] private HotkeyMenu hotkeyScript = default;
    [SerializeField] private GameObject[] UIpanels = default;
    [SerializeField] private GameObject logoCanvas = default;
    private GameOptions gops;
	private bool menuUIOn = true;

    private void Start()
	{
        gops = GameOptions.GetGOPS();
	}

	public void OnBackButtonPressed()
    {
        if (resetMenu)
        {
            if (resetMenu.activeInHierarchy)
            {
                resetMenu.SetActive(false);
                optionsMenu.SetActive(true);
            }
            else if (leaderboard.activeInHierarchy)
            {
                leaderboard.SetActive(false);
                mainMenu.SetActive(true);
            }
            else if (hotkeyMenu.activeInHierarchy)
            {
                hotkeyMenu.SetActive(false);
                optionsMenu.SetActive(true);
            }
            else if (optionsMenu.activeInHierarchy)
            {
                optionsMenu.SetActive(false);
                mainMenu.SetActive(true);
                optionScript.SaveSettings();
            }
            else if (loadingScreen.activeInHierarchy)
            {
                mainMenu.GetComponent<MainMenu>().NextScene();
            }
        }
    }

    public void ToggleMenuUI()
    {
        if (gops.GetCameraDebugMode())
		{
            foreach(GameObject go in UIpanels)
			{
                go.SetActive(!menuUIOn);
			}
            logoCanvas.SetActive(!menuUIOn);
        }
        menuUIOn = !menuUIOn;
    }

    public void OnControlsChanged()
    {
        if (hotkeyMenu.activeInHierarchy)
            hotkeyScript.PopulateKeys();
    }
}
