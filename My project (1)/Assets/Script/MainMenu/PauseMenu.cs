using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [Header("UI Paneeli")]
    public GameObject pauseMenuUI;

    [Header("Napit")]
    public RectTransform resumeButton;
    public RectTransform restartButton;
    public RectTransform mainMenuButton;

    [Header("Canvas")]
    public Canvas pauseCanvas;

    [Header("Skenejen nimet")]
    public string restartSceneName = "MiskaMain";
    public string mainMenuSceneName = "MainMenu";

    private bool isMenuOpen = false;

    void Start()
    {
        Time.timeScale = 1f;

        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(false);
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
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

        if (isMenuOpen)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

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

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Debug.Log("Cursor lock: " + Cursor.lockState);
        Debug.Log("Cursor visible: " + Cursor.visible);
    }

    public void Resume()
    {
        Debug.Log("RESUME TOIMII");

        isMenuOpen = false;

        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(false);
        }

        Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void Restart()
    {
        Debug.Log("RESTART TOIMII");

        Time.timeScale = 1f;

        SceneManager.LoadScene(restartSceneName);
    }

    public void MainMenu()
    {
        Debug.Log("MAIN MENU TOIMII");

        Time.timeScale = 1f;

        SceneManager.LoadScene(mainMenuSceneName);
    }
}