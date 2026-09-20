using System.Collections;
using TMPro;
using UnityEngine;
using Unity.Cinemachine;

public class TransitionManager : MonoBehaviour
{
    public static TransitionManager Instance;

    // =========================================================
    // MAIN GAME CINEMACHINE CAMERAS
    // =========================================================

    [Header("Main Game Cinemachine Cameras")]
    public CinemachineCamera topDownCamera;
    public CinemachineCamera wideCamera;

    // =========================================================
    // MINIGAME REGULAR UNITY CAMERAS
    // =========================================================

    [Header("Minigame Unity Cameras")]
    [Tooltip("Regular Unity Camera looking at the Knowledge minigame table.")]
    public Camera knowledgeCamera;

    [Tooltip("Regular Unity Camera looking at the Speed minigame table.")]
    public Camera speedCamera;

    [Tooltip("Regular Unity Camera looking at the Physical minigame table.")]
    public Camera physicalCamera;

    [Tooltip("Regular Unity Camera looking at the Luck minigame table.")]
    public Camera luckCamera;

    // =========================================================
    // MINIGAME TABLES
    // =========================================================

    [Header("Minigame Tables")]
    [Tooltip("Optional. Only assign these if you want TransitionManager to enable/disable them.")]
    public GameObject knowledgeTable;

    public GameObject speedTable;

    public GameObject physicalTable;

    public GameObject luckTable;

    // =========================================================
    // UI
    // =========================================================

    [Header("WarioWare UI")]
    public RectTransform minigamePanel;

    public TextMeshProUGUI instructionText;

    public TextMeshProUGUI countdownText;

    // =========================================================
    // TRANSITION SETTINGS
    // =========================================================

    [Header("Transition Settings")]
    public float wideViewDuration = 0.6f;

    public float panelSlideDuration = 0.25f;

    public float countdownStartDelay = 0.2f;

    public float countdownStepDuration = 0.7f;

    public float goDuration = 0.35f;

    public float returnDelay = 0.5f;

    // =========================================================
    // PANEL POSITIONS
    // =========================================================

    [Header("Panel Positions")]
    public Vector2 panelHiddenPosition = new Vector2(-2500f, 0f);

    public Vector2 panelShownPosition = Vector2.zero;

    // =========================================================
    // CINEMACHINE PRIORITIES
    // =========================================================

    [Header("Cinemachine Priorities")]
    public int topDownPriority = 100;

    public int widePriority = 110;

    // =========================================================
    // STATE
    // =========================================================

    private bool transitionRunning = false;

    private Camera activeMinigameCamera;

    // =========================================================
    // AWAKE
    // =========================================================

    private void Awake()
    {
        Instance = this;
    }

    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        SetupInitialState();
    }

    // =========================================================
    // INITIAL STATE
    // =========================================================

    private void SetupInitialState()
    {
        if (topDownCamera != null)
            topDownCamera.Priority = topDownPriority;

        if (wideCamera != null)
            wideCamera.Priority = 0;

        DisableAllMinigameCameras();

        DisableAllMinigameTables();

        // IMPORTANT:
        // Keep the WarioWare panel GameObject ACTIVE so that
        // we can move it on/off screen.
        if (minigamePanel != null)
        {
            minigamePanel.gameObject.SetActive(true);
            minigamePanel.anchoredPosition = panelHiddenPosition;
        }

        if (instructionText != null)
            instructionText.gameObject.SetActive(false);

        if (countdownText != null)
            countdownText.gameObject.SetActive(false);
    }

    // =========================================================
    // START CHALLENGE
    // =========================================================

    public void StartChallenge(
        CardColor color,
        string instruction)
    {
        if (transitionRunning)
            return;

        StartCoroutine(
            ChallengeSequence(
                color,
                instruction
            )
        );
    }

    // =========================================================
    // CHALLENGE SEQUENCE
    // =========================================================

    private IEnumerator ChallengeSequence(
        CardColor color,
        string instruction)
    {
        transitionRunning = true;

        // -----------------------------------------------------
        // 1. ACTIVATE CORRECT MINIGAME TABLE
        // -----------------------------------------------------

        ActivateCorrectMinigame(color);

        // -----------------------------------------------------
        // 2. MAIN GAME -> WIDE CAMERA
        // -----------------------------------------------------

        DisableAllMinigameCameras();

        if (wideCamera != null)
        {
            wideCamera.Priority = widePriority;
        }

        yield return new WaitForSeconds(
            wideViewDuration
        );

        // -----------------------------------------------------
        // 3. WIDE CAMERA -> MINIGAME CAMERA
        // -----------------------------------------------------

        activeMinigameCamera =
            GetMinigameCamera(color);

        if (activeMinigameCamera == null)
        {
            Debug.LogError(
                "TransitionManager: No regular Unity Camera " +
                "has been assigned for " +
                color
            );

            ReturnCameraToMainGameImmediate();

            transitionRunning = false;

            yield break;
        }

        activeMinigameCamera.gameObject.SetActive(true);
        activeMinigameCamera.enabled = true;

        if (wideCamera != null)
            wideCamera.Priority = 0;

        if (topDownCamera != null)
            topDownCamera.Priority = 0;

        yield return null;

        // -----------------------------------------------------
        // 4. MAKE WARIOWARE PANEL VISIBLE
        // -----------------------------------------------------

        if (minigamePanel != null)
        {
            minigamePanel.gameObject.SetActive(true);
        }

        // -----------------------------------------------------
        // 5. SET INSTRUCTION TEXT
        // -----------------------------------------------------

        if (instructionText != null)
        {
            instructionText.text = instruction;
            instructionText.gameObject.SetActive(true);
        }

        if (countdownText != null)
        {
            countdownText.text = "";
            countdownText.gameObject.SetActive(false);
        }

        // -----------------------------------------------------
        // 6. SLIDE PANEL IN
        // -----------------------------------------------------

        yield return StartCoroutine(
            SlidePanelIn()
        );

        yield return new WaitForSeconds(
            countdownStartDelay
        );

        // -----------------------------------------------------
        // 7. COUNTDOWN
        // -----------------------------------------------------

        yield return StartCoroutine(
            ShowCountdown("3")
        );

        yield return StartCoroutine(
            ShowCountdown("2")
        );

        yield return StartCoroutine(
            ShowCountdown("1")
        );

        yield return StartCoroutine(
            ShowCountdown("GO!")
        );

        // -----------------------------------------------------
        // 8. HIDE INTRO TEXT
        // -----------------------------------------------------

        if (instructionText != null)
            instructionText.gameObject.SetActive(false);

        if (countdownText != null)
            countdownText.gameObject.SetActive(false);

        // -----------------------------------------------------
        // 9. SLIDE PANEL OUT
        // -----------------------------------------------------

        yield return StartCoroutine(
            SlidePanelOut()
        );

        // -----------------------------------------------------
        // 10. START ACTUAL MINIGAME
        // -----------------------------------------------------

        if (MinigameManager.Instance != null)
        {
            MinigameManager.Instance.StartActualMinigame();
        }
        else
        {
            Debug.LogError(
                "TransitionManager: " +
                "MinigameManager.Instance is missing."
            );
        }

        transitionRunning = false;
    }

    // =========================================================
    // GET MINIGAME CAMERA
    // =========================================================

    private Camera GetMinigameCamera(
        CardColor color)
    {
        switch (color)
        {
            case CardColor.Purple:
                return knowledgeCamera;

            case CardColor.Yellow:
                return speedCamera;

            case CardColor.Red:
                return physicalCamera;

            case CardColor.Green:
                return luckCamera;

            case CardColor.Wild:
                return knowledgeCamera;

            default:
                return knowledgeCamera;
        }
    }

    // =========================================================
    // ACTIVATE CORRECT MINIGAME TABLE
    // =========================================================

    private void ActivateCorrectMinigame(
        CardColor color)
    {
        DisableAllMinigameTables();

        switch (color)
        {
            case CardColor.Purple:

                if (knowledgeTable != null)
                    knowledgeTable.SetActive(true);

                break;

            case CardColor.Yellow:

                if (speedTable != null)
                    speedTable.SetActive(true);

                break;

            case CardColor.Red:

                if (physicalTable != null)
                    physicalTable.SetActive(true);

                break;

            case CardColor.Green:

                if (luckTable != null)
                    luckTable.SetActive(true);

                break;

            case CardColor.Wild:

                if (knowledgeTable != null)
                    knowledgeTable.SetActive(true);

                break;
        }
    }

    // =========================================================
    // DISABLE ALL MINIGAME CAMERAS
    // =========================================================

    private void DisableAllMinigameCameras()
    {
        if (knowledgeCamera != null)
            knowledgeCamera.gameObject.SetActive(false);

        if (speedCamera != null)
            speedCamera.gameObject.SetActive(false);

        if (physicalCamera != null)
            physicalCamera.gameObject.SetActive(false);

        if (luckCamera != null)
            luckCamera.gameObject.SetActive(false);

        activeMinigameCamera = null;
    }

    // =========================================================
    // DISABLE ALL MINIGAME TABLES
    // =========================================================

    private void DisableAllMinigameTables()
    {
        if (knowledgeTable != null)
            knowledgeTable.SetActive(false);

        if (speedTable != null)
            speedTable.SetActive(false);

        if (physicalTable != null)
            physicalTable.SetActive(false);

        if (luckTable != null)
            luckTable.SetActive(false);
    }

    // =========================================================
    // COUNTDOWN
    // =========================================================

    private IEnumerator ShowCountdown(
        string text)
    {
        if (countdownText == null)
            yield break;

        countdownText.gameObject.SetActive(true);

        countdownText.text = text;

        countdownText.transform.localScale =
            Vector3.one * 0.6f;

        float elapsed = 0f;

        while (elapsed < 0.15f)
        {
            elapsed += Time.deltaTime;

            float t =
                Mathf.Clamp01(
                    elapsed / 0.15f
                );

            t =
                Mathf.SmoothStep(
                    0f,
                    1f,
                    t
                );

            countdownText.transform.localScale =
                Vector3.Lerp(
                    Vector3.one * 0.6f,
                    Vector3.one,
                    t
                );

            yield return null;
        }

        countdownText.transform.localScale =
            Vector3.one;

        yield return new WaitForSeconds(
            countdownStepDuration
        );

        if (text == "GO!")
        {
            countdownText.gameObject.SetActive(false);

            yield return new WaitForSeconds(
                goDuration
            );
        }
    }

    // =========================================================
    // SLIDE PANEL IN
    // =========================================================

    private IEnumerator SlidePanelIn()
    {
        if (minigamePanel == null)
            yield break;

        // Make absolutely sure it is active.
        minigamePanel.gameObject.SetActive(true);

        Vector2 start =
            panelHiddenPosition;

        Vector2 end =
            panelShownPosition;

        float elapsed = 0f;

        minigamePanel.anchoredPosition =
            start;

        while (elapsed < panelSlideDuration)
        {
            elapsed += Time.deltaTime;

            float t =
                Mathf.Clamp01(
                    elapsed /
                    panelSlideDuration
                );

            t =
                Mathf.SmoothStep(
                    0f,
                    1f,
                    t
                );

            minigamePanel.anchoredPosition =
                Vector2.Lerp(
                    start,
                    end,
                    t
                );

            yield return null;
        }

        minigamePanel.anchoredPosition =
            end;
    }

    // =========================================================
    // SLIDE PANEL OUT
    // =========================================================

    private IEnumerator SlidePanelOut()
    {
        if (minigamePanel == null)
            yield break;

        Vector2 start =
            panelShownPosition;

        Vector2 end =
            panelHiddenPosition;

        float elapsed = 0f;

        while (elapsed < panelSlideDuration)
        {
            elapsed += Time.deltaTime;

            float t =
                Mathf.Clamp01(
                    elapsed /
                    panelSlideDuration
                );

            t =
                Mathf.SmoothStep(
                    0f,
                    1f,
                    t
                );

            minigamePanel.anchoredPosition =
                Vector2.Lerp(
                    start,
                    end,
                    t
                );

            yield return null;
        }

        minigamePanel.anchoredPosition =
            end;
    }

    // =========================================================
    // END CHALLENGE
    // =========================================================

    public void EndChallenge()
    {
        if (transitionRunning)
            return;

        StartCoroutine(
            ReturnToMainGame()
        );
    }

    // =========================================================
    // RETURN TO MAIN GAME
    // =========================================================

    private IEnumerator ReturnToMainGame()
    {
        transitionRunning = true;

        // -----------------------------------------------------
        // 1. MINIGAME -> WIDE CAMERA
        // -----------------------------------------------------

        DisableAllMinigameCameras();

        if (wideCamera != null)
        {
            wideCamera.Priority =
                widePriority;
        }

        yield return new WaitForSeconds(
            returnDelay
        );

        // -----------------------------------------------------
        // 2. DISABLE MINIGAME TABLES
        // -----------------------------------------------------

        DisableAllMinigameTables();

        // -----------------------------------------------------
        // 3. WIDE -> MAIN TABLE
        // -----------------------------------------------------

        if (topDownCamera != null)
        {
            topDownCamera.Priority =
                topDownPriority;
        }

        yield return new WaitForSeconds(
            returnDelay
        );

        // -----------------------------------------------------
        // 4. TURN OFF WIDE
        // -----------------------------------------------------

        if (wideCamera != null)
            wideCamera.Priority = 0;

        // -----------------------------------------------------
        // 5. HIDE WARIOWARE PANEL
        // -----------------------------------------------------

        if (minigamePanel != null)
        {
            minigamePanel.anchoredPosition =
                panelHiddenPosition;

            // Keep it ACTIVE so it can be reused next time.
            minigamePanel.gameObject.SetActive(true);
        }

        if (instructionText != null)
            instructionText.gameObject.SetActive(false);

        if (countdownText != null)
            countdownText.gameObject.SetActive(false);

        transitionRunning = false;
    }

    // =========================================================
    // IMMEDIATE MAIN GAME RETURN
    // =========================================================

    private void ReturnCameraToMainGameImmediate()
    {
        DisableAllMinigameCameras();

        if (wideCamera != null)
            wideCamera.Priority = 0;

        if (topDownCamera != null)
            topDownCamera.Priority =
                topDownPriority;

        DisableAllMinigameTables();

        if (minigamePanel != null)
        {
            minigamePanel.gameObject.SetActive(true);
            minigamePanel.anchoredPosition =
                panelHiddenPosition;
        }

        if (instructionText != null)
            instructionText.gameObject.SetActive(false);

        if (countdownText != null)
            countdownText.gameObject.SetActive(false);
    }

    // =========================================================
    // FORCE RETURN
    // =========================================================

    public void ForceReturnToMainGame()
    {
        StopAllCoroutines();

        transitionRunning = false;

        DisableAllMinigameCameras();

        DisableAllMinigameTables();

        if (topDownCamera != null)
            topDownCamera.Priority =
                topDownPriority;

        if (wideCamera != null)
            wideCamera.Priority = 0;

        if (minigamePanel != null)
        {
            minigamePanel.gameObject.SetActive(true);
            minigamePanel.anchoredPosition =
                panelHiddenPosition;
        }

        if (instructionText != null)
            instructionText.gameObject.SetActive(false);

        if (countdownText != null)
            countdownText.gameObject.SetActive(false);
    }
}