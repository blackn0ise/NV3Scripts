using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicPlayer : MonoBehaviour
{
    [SerializeField] private AudioClip mainMenu = default;
    [SerializeField] private AudioClip game = default;
    [SerializeField] public bool dontCycle = true;

    public AudioClip GetMainMenu() { return mainMenu; }
    public AudioClip GetGameClip() { return game; }
    private AudioSource aso;
    private Scene activescene;
    //private float chosenVolume = 1;
    //public float GetChosenVolume() { return chosenVolume; }
    //public void SetChosenVolume(float value) { chosenVolume = value; }

    private int lasttrack = 0;
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
        //Debug.Log("SceneManager.GetActiveScene = " + SceneManager.GetActiveScene().name);
        //Debug.Log("activescene = " + activescene.name);
        if (activescene != SceneManager.GetActiveScene() && SceneManager.GetActiveScene().name != "Main Menu")
        {
            aso.Stop();
            activescene = SceneManager.GetActiveScene();
        }
    }

}
