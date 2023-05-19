using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class VoidFadeWall : MonoBehaviour
{
    private bool isFading = false;
    private AudioSource aso;
    [SerializeField] private float fadeFactor = 1;

    private void Start()
    {
        aso = GetComponent<AudioSource>();
    }

    public void StartMenu()
    {
        LoadNextScene();
    }

    private static void LoadNextScene()
    {
        string activescene = SceneManager.GetActiveScene().name;
        if(activescene == "VoidSplash")
            SceneManager.LoadScene("Main Menu");
        else if(activescene == "EpilepsyWarning")
            SceneManager.LoadScene("VoidSplash");
    }

    private void Update()
    {
        if (isFading)
            aso.volume -= fadeFactor * Time.deltaTime;
    }

    public void OnSkipPressed()
    {
        FastFade();
        isFading = true;
    }

    public void FastFade()
    {
        GameObject.Find("SecondFadeWall").GetComponent<Animator>().SetTrigger("FastFade");
    }
}
