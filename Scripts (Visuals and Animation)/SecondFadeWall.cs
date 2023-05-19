using UnityEngine;
using UnityEngine.SceneManagement;

public class SecondFadeWall : MonoBehaviour
{
    public void StartMenu()
    {
        string activescene = SceneManager.GetActiveScene().name;
        if (activescene == "VoidSplash")
            SceneManager.LoadScene("Main Menu");
        else if (activescene == "EpilepsyWarning")
            SceneManager.LoadScene("VoidSplash");
    }

}
