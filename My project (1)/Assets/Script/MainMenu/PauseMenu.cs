using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [Header("UI Paneeli")]
    public GameObject pauseMenuUI;

    [Header("Skenejen nimet")]
    public string restartSceneName = "MiskaMain"; 
    public string mainMenuSceneName = "MainMenu"; 

    private bool isMenuOpen = false;

    void Start()
    {
        
        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(false);
        }
    }

    void Update()
    {
        
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (isMenuOpen)
            {
                Resume();
            }
            else
            {
                OpenMenu();
            }
        }
    }

    public void Resume()
    {
        isMenuOpen = false;
        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(false);
        }
    }

    void OpenMenu()
    {
        isMenuOpen = true;
        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(true);
        }
    }

    public void Restart()
    {
        
        SceneManager.LoadScene(restartSceneName);
    }

    public void MainMenu()
    {
        
        SceneManager.LoadScene(mainMenuSceneName);
    }
}