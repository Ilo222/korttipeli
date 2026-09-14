using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class MenuBtn : MonoBehaviour
{
    [Header("Skenejen nimet")]
    public string mainSceneName = "MiskaMain";
    public string howToPlaySceneName = "HowtoplayScene";
    public string mainMenuSceneName = "MainMenu";

    public void GotoMainScene()
    {
        SceneManager.LoadScene(mainSceneName);
    }

    public void GotoHowToPlay()
    {
        SceneManager.LoadScene(howToPlaySceneName);
    }

    public void GotoMainMenu()
    {
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void ExitMenu()
    {
        Application.Quit();
    }

    void Update()
    {
        // ESC-n‰pp‰in palauttaa p‰‰valikkoon ilman virheit‰
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }
    }
}