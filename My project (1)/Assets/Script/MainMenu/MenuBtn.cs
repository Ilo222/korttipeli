using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class MenuBtn : MonoBehaviour
{
    [Header("Skenejen nimet")]
    public string mainSceneName = "Cutscene";
    public string howToPlaySceneName = "HowtoplayScene";
    public string mainMenuSceneName = "MainMenu";
    public string CreditsSceneName = "Credits";
    public string endSceneName = "End";

    [Header("Camera Positions")]
    public Transform startCameraPosition;
    public Transform targetCameraPosition;

    [Header("Scene Select")]
    public GameObject sceneSelectPanel;

    [Header("Kameran nopeus")]
    public float cameraSpeed = 3f;

    private Camera mainCamera;
    private bool movingCamera = false;
    private Coroutine activeMoveCoroutine;

    void Start()
    {
        mainCamera = Camera.main;

        if (startCameraPosition != null && mainCamera != null)
        {
            mainCamera.transform.position = startCameraPosition.position;
            mainCamera.transform.rotation = startCameraPosition.rotation;
        }

        if (sceneSelectPanel != null)
        {
            sceneSelectPanel.SetActive(false);
        }
    }

    public void PlayButton()
    {
        if (targetCameraPosition == null) return;

        if (sceneSelectPanel != null)
        {
            sceneSelectPanel.SetActive(true);
        }

        MoveCameraTo(targetCameraPosition);
    }

    public void BackButton()
    {
        if (startCameraPosition == null) return;

     
        MoveCameraTo(startCameraPosition);
    }

    void MoveCameraTo(Transform target)
    {
        if (activeMoveCoroutine != null)
        {
            StopCoroutine(activeMoveCoroutine);
        }

        activeMoveCoroutine = StartCoroutine(SmoothMove(target.position, target.rotation));
    }

    IEnumerator SmoothMove(Vector3 targetPosition, Quaternion targetRotation)
    {
        movingCamera = true;

        while (
            Vector3.Distance(mainCamera.transform.position, targetPosition) > 0.01f ||
            Quaternion.Angle(mainCamera.transform.rotation, targetRotation) > 0.5f
        )
        {
            mainCamera.transform.position = Vector3.Lerp(
                mainCamera.transform.position,
                targetPosition,
                cameraSpeed * Time.deltaTime
            );

            mainCamera.transform.rotation = Quaternion.Slerp(
                mainCamera.transform.rotation,
                targetRotation,
                cameraSpeed * Time.deltaTime
            );

            yield return null;
        }

        mainCamera.transform.position = targetPosition;
        mainCamera.transform.rotation = targetRotation;

        movingCamera = false;
        activeMoveCoroutine = null;
    }

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

    public void GotoEndScene()
    {
        SceneManager.LoadScene(endSceneName);
    }

    public void ExitMenu()
    {
        Application.Quit();
    }

    void Update()
    {
        if (
            Keyboard.current != null &&
            Keyboard.current.escapeKey.wasPressedThisFrame
        )
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }
    }
}