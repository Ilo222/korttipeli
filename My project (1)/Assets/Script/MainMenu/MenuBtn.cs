using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class MenuBtn : MonoBehaviour
{
    [Header("Skenejen nimet")]
    public string mainSceneName = "Cutscene";
    public string howToPlaySceneName = "HowtoplayScene";
    public string mainMenuSceneName = "MainMenu";
    public string CreditsSceneName = "Credits";



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


    public void GotoCreditsScene()
    {
        SceneManager.LoadScene(CreditsSceneName);
    }

    public void ExitMenu()
    {
        Application.Quit();
    }

    void Update()
    {
        
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }
    }
}