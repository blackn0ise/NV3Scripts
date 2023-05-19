using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ResetMenu : MonoBehaviour
{
    private GameOptions gops;
    [SerializeField] private AudioSource clickeraso = default;
    //[SerializeField] private GameObject gamePersistent = default;

    void Start()
    {
        gops = GameOptions.GetGOPS();
    }

    public void DeleteSettings()
    {
        //GameObject gp;

        SaveSystem.ClearSave();
        if (gops)
        {
            var steammanager = gops.GetComponentInChildren<SteamManager>();
            if (steammanager)
            {
                steammanager.transform.parent = null;
                DontDestroyOnLoad(steammanager.gameObject);
            }
            Destroy(gops.gameObject);

   //         gp = Instantiate(gamePersistent);
   //         gp.name = gamePersistent.name;
			//gops = gp.GetComponent<GameOptions>();
            Debug.Log("Old game options destroyed.");
        }
        Scene scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.name);
        SceneManager.SetActiveScene(scene);
    }

    public void PlayClickSound()
    {
        clickeraso.Play();
    }

}
