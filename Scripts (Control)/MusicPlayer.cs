using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicPlayer : MonoBehaviour
{
    [SerializeField] private AudioClip mainMenu = default;
    [SerializeField] private AudioClip game = default;
    [SerializeField] private AudioSource aso = default;
    [SerializeField] public bool dontCycle = true;

    public AudioClip GetMainMenu() { return mainMenu; }
    public AudioClip GetGameClip() { return game; }
    private Scene activescene;

    private int lasttrack = 0;
    private bool fadeStop = false;

    public void SetLastTrack(int value) { lasttrack = value; }
    public int GetLastTrack() { return lasttrack; }

    private void Start()
    {
        aso = GetComponent<AudioSource>();
        activescene = SceneManager.GetActiveScene();
        lasttrack = SoundLibrary.trackCount - 1;
    }
    private void Update()
    {
        if (activescene != SceneManager.GetActiveScene() && SceneManager.GetActiveScene().name != "Main Menu")
        {
            aso.Stop();
            activescene = SceneManager.GetActiveScene();
        }

        if (aso.volume <= 0)
        {
            aso.Stop();
            fadeStop = false;
            aso.volume = 1;
        }

        if (fadeStop && aso.volume > 0)
        {
            if (SceneManager.GetActiveScene().name == "Main Menu")
            {
                aso.volume = 1;
                fadeStop = false;
                return;
            }
            aso.volume -= Time.deltaTime;
        }
    }
    public void Stop()
    {
        fadeStop = true;
    }
    public void Play()
    {
        aso.Play();
    }

}
