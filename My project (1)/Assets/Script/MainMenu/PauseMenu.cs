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
<<<<<<< HEAD

        if (isMenuOpen)
        {
            CheckMouseClick();
        }
    }

    void CheckMouseClick()
    {
        if (Mouse.current == null)
            return;

        if (!Mouse.current.leftButton.wasPressedThisFrame)
            return;

        Vector2 mousePosition = Mouse.current.position.ReadValue();

        Camera eventCamera = null;

        if (pauseCanvas != null)
        {
            if (pauseCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
            {
                eventCamera = pauseCanvas.worldCamera;
            }
        }

        if (resumeButton != null &&
            RectTransformUtility.RectangleContainsScreenPoint(
                resumeButton,
                mousePosition,
                eventCamera))
        {
            Resume();
            return;
        }

        if (restartButton != null &&
            RectTransformUtility.RectangleContainsScreenPoint(
                restartButton,
                mousePosition,
                eventCamera))
        {
            Restart();
            return;
        }

        if (mainMenuButton != null &&
            RectTransformUtility.RectangleContainsScreenPoint(
                mainMenuButton,
                mousePosition,
                eventCamera))
        {
            MainMenu();
            return;
        }
    }

    public void OpenMenu()
    {
        Debug.Log("PAUSE MENU TOIMII");

        isMenuOpen = true;

        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(true);
        }

        Time.timeScale = 0f;
=======
>>>>>>> parent of 1efd334 (PauseMenu)
    }

    public void Resume()
    {
        isMenuOpen = false;
        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(false);
        }
    }

<<<<<<< HEAD
        Time.timeScale = 1f;
=======
    void OpenMenu()
    {
        isMenuOpen = true;
        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(true);
        }
>>>>>>> parent of 1efd334 (PauseMenu)
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