using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public class MinigameManager : MonoBehaviour
{
    public static MinigameManager Instance;

    public const int PlayerCount = 4;

    [Header("Difficulty")]
    [SerializeField] private int baseDifficulty = 1;
    [SerializeField] private int minimumDifficulty = 1;
    [SerializeField] private int maximumDifficulty = 99;

    private readonly int[] challengeCounts = new int[PlayerCount];
    private readonly int[] presentationDifficultyBoosts = new int[PlayerCount];

    [Header("Current Challenge")]
    [SerializeField] private CardColor challengeColor = CardColor.Purple;
    [SerializeField] private int challengerPlayerIndex = 0;
    [SerializeField] private bool challengeIsPresentationOnly;

    private bool waitingForActualMinigame;

    [Header("Minigame HUDs")]
    [SerializeField] private GameObject speedHUD;
    [SerializeField] private GameObject physicalHUD;
    [SerializeField] private GameObject luckHUD;
    [SerializeField] private GameObject knowledgeHUD;

    [Header("Minigame Controllers")]
    [SerializeField] private SpeedDiceMinigame speedMinigame;
    [SerializeField] private PhysicalBalanceMinigame physicalMinigame;
    [SerializeField] private LuckMinigame luckMinigame;
    [SerializeField] private KnowledgeMinigame knowledgeMinigame;

    [Header("Result UI")]
    [SerializeField] private GameObject challengeResultPanel;
    [SerializeField] private TMPro.TextMeshProUGUI resultTitle;
    [SerializeField] private TMPro.TextMeshProUGUI resultText;
    [SerializeField] private float resultDisplayDuration = 1.25f;

    private bool completingResult;

    public CardColor CurrentChallengeColor => challengeColor;
    public int CurrentChallengerPlayerIndex => challengerPlayerIndex;
    public bool CurrentChallengeIsPresentationOnly => challengeIsPresentationOnly;

    // ============================================================
    // UNITY
    // ============================================================

    private void Awake()
    {
        Instance = this;

        ResetAllPlayerDifficulty();

        HideAllMinigameHUDs();

        HideResult();

        // IMPORTANT:
        // Force Unity UI to use the NEW Input System.
        EnsureInputSystemUI();
    }

    // ============================================================
    // INPUT SYSTEM UI
    // ============================================================

    private void EnsureInputSystemUI()
    {
        EventSystem eventSystem = EventSystem.current;

        // Try to find an ACTIVE EventSystem first.
        if (eventSystem == null)
        {
            EventSystem[] allEventSystems =
                FindObjectsByType<EventSystem>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None
                );

            foreach (EventSystem candidate in allEventSystems)
            {
                if (candidate == null)
                    continue;

                if (candidate.gameObject.activeInHierarchy)
                {
                    eventSystem = candidate;
                    break;
                }
            }
        }

        // If there is still no usable EventSystem,
        // create one automatically.
        if (eventSystem == null)
        {
            GameObject eventSystemObject =
                new GameObject("EventSystem_Minipgames");

            eventSystem =
                eventSystemObject.AddComponent<EventSystem>();

            Debug.Log(
                "MinigameManager: Created missing EventSystem."
            );
        }

        // Make sure the EventSystem itself is enabled.
        if (!eventSystem.gameObject.activeSelf)
            eventSystem.gameObject.SetActive(true);

        eventSystem.enabled = true;

        // --------------------------------------------------------
        // REMOVE OLD INPUT MODULE
        // --------------------------------------------------------

        StandaloneInputModule oldModule =
            eventSystem.GetComponent<StandaloneInputModule>();

        if (oldModule != null)
        {
            Destroy(oldModule);

            Debug.Log(
                "MinigameManager: Removed old StandaloneInputModule."
            );
        }

        // --------------------------------------------------------
        // ADD / ENABLE NEW INPUT SYSTEM UI MODULE
        // --------------------------------------------------------

        InputSystemUIInputModule inputModule =
            eventSystem.GetComponent<InputSystemUIInputModule>();

        if (inputModule == null)
        {
            inputModule =
                eventSystem.gameObject
                    .AddComponent<InputSystemUIInputModule>();

            Debug.Log(
                "MinigameManager: Added InputSystemUIInputModule."
            );
        }

        inputModule.enabled = true;

        // Make absolutely sure the module has usable default actions.
        inputModule.AssignDefaultActions();

        // --------------------------------------------------------
        // MAKE SURE UI CANVASES HAVE GRAPHIC RAYCASTERS
        // --------------------------------------------------------

        Canvas[] canvases =
            FindObjectsByType<Canvas>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            );

        foreach (Canvas canvas in canvases)
        {
            if (canvas == null)
                continue;

            GraphicRaycaster raycaster =
                canvas.GetComponent<GraphicRaycaster>();

            if (raycaster == null)
            {
                canvas.gameObject.AddComponent<GraphicRaycaster>();

                Debug.Log(
                    "MinigameManager: Added GraphicRaycaster to " +
                    canvas.name
                );
            }

            raycaster.enabled = true;
        }
    }

    // ============================================================
    // DIFFICULTY
    // ============================================================

    public void ResetAllPlayerDifficulty()
    {
        for (int i = 0; i < PlayerCount; i++)
        {
            challengeCounts[i] = 0;
            presentationDifficultyBoosts[i] = 0;
        }
    }

    public int GetChallengeCount(int playerIndex)
    {
        if (!IsValidPlayerIndex(playerIndex))
            return 0;

        return challengeCounts[playerIndex];
    }

    public int GetPlayerDifficulty(int playerIndex)
    {
        if (!IsValidPlayerIndex(playerIndex))
        {
            return Mathf.Max(
                minimumDifficulty,
                baseDifficulty
            );
        }

        int difficulty =
            Mathf.Max(
                baseDifficulty,
                challengeCounts[playerIndex]
            );

        difficulty +=
            presentationDifficultyBoosts[playerIndex];

        return Mathf.Clamp(
            difficulty,
            minimumDifficulty,
            maximumDifficulty
        );
    }

    // Kept for compatibility with existing GameManager code.
    public int GetCurrentDifficulty()
    {
        int playerIndex = 0;

        if (GameManager.Instance != null)
            playerIndex =
                GameManager.Instance.currentPlayerIndex;

        return GetPlayerDifficulty(playerIndex);
    }

    public int GetCurrentDifficulty(int playerIndex)
    {
        return GetPlayerDifficulty(playerIndex);
    }

    public void RegisterChallenge(int playerIndex)
    {
        if (!IsValidPlayerIndex(playerIndex))
            return;

        challengeCounts[playerIndex] =
            Mathf.Clamp(
                challengeCounts[playerIndex] + 1,
                0,
                maximumDifficulty
            );

        Debug.Log(
            $"Challenge started by Player " +
            $"{GetDisplayPlayerName(playerIndex)}. " +
            $"Challenge count: {challengeCounts[playerIndex]}, " +
            $"difficulty: {GetPlayerDifficulty(playerIndex)}"
        );
    }

    public void AddPresentationDifficulty(
        int playerIndex,
        int amount = 1)
    {
        if (!IsValidPlayerIndex(playerIndex))
            return;

        if (amount <= 0)
            return;

        presentationDifficultyBoosts[playerIndex] =
            Mathf.Clamp(
                presentationDifficultyBoosts[playerIndex] + amount,
                0,
                maximumDifficulty
            );

        Debug.Log(
            $"PRESENTATION DEBUG: " +
            $"{GetDisplayPlayerName(playerIndex)} difficulty increased. " +
            $"Difficulty is now {GetPlayerDifficulty(playerIndex)}"
        );
    }

    public void RemovePresentationDifficulty(
        int playerIndex,
        int amount = 1)
    {
        if (!IsValidPlayerIndex(playerIndex))
            return;

        if (amount <= 0)
            return;

        presentationDifficultyBoosts[playerIndex] =
            Mathf.Max(
                0,
                presentationDifficultyBoosts[playerIndex] - amount
            );
    }

    // ============================================================
    // REAL CHALLENGE
    // ============================================================

    public void StartChallenge(
        CardColor color,
        bool playerIsChallenger)
    {
        int playerIndex =
            GameManager.Instance != null
                ? GameManager.Instance.currentPlayerIndex
                : 0;

        StartChallengeInternal(
            color,
            playerIndex,
            false
        );
    }

    private void StartChallengeInternal(
        CardColor color,
        int playerIndex,
        bool presentationOnly)
    {
        if (!IsValidPlayerIndex(playerIndex))
            playerIndex = 0;

        challengeColor = color;
        challengerPlayerIndex = playerIndex;
        challengeIsPresentationOnly = presentationOnly;

        waitingForActualMinigame = true;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.state =
                GameState.CameraTransition;
        }

        string instruction =
            GetInstruction(color);

        if (TransitionManager.Instance == null)
        {
            Debug.LogError(
                "MinigameManager: TransitionManager.Instance " +
                "is missing. " +
                "Assign TransitionManager to the scene before " +
                "starting a minigame."
            );

            waitingForActualMinigame = false;

            if (GameManager.Instance != null)
            {
                GameManager.Instance.state =
                    GameState.Playing;
            }

            return;
        }

        TransitionManager.Instance.StartChallenge(
            color,
            instruction
        );
    }

    // ============================================================
    // PRESENTATION / F9 MINIGAME
    // ============================================================

    public void StartPresentationMinigame(
        CardColor color,
        int targetPlayerIndex)
    {
        if (!IsValidPlayerIndex(targetPlayerIndex))
            targetPlayerIndex = 0;

        if (waitingForActualMinigame)
        {
            Debug.LogWarning(
                "MinigameManager: A minigame is already running. " +
                "The presentation key was ignored."
            );

            return;
        }

        Debug.Log(
            $"PRESENTATION DEBUG: Starting {color} minigame for " +
            $"{GetDisplayPlayerName(targetPlayerIndex)} at difficulty " +
            $"{GetPlayerDifficulty(targetPlayerIndex)}."
        );

        StartChallengeInternal(
            color,
            targetPlayerIndex,
            true
        );
    }

    // ============================================================
    // START ACTUAL MINIGAME
    // ============================================================

    public void StartActualMinigame()
    {
        if (!waitingForActualMinigame)
            return;

        // Re-run this right before the minigame starts.
        // This is important because the minigame UI may have
        // been inactive during the normal game.
        EnsureInputSystemUI();

        waitingForActualMinigame = false;
        completingResult = false;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.state =
                GameState.Minigame;
        }

        int difficulty =
            GetPlayerDifficulty(
                challengerPlayerIndex
            );

        bool aiControlled =
            challengerPlayerIndex != 0;

        Debug.Log(
            $"MINIGAME STARTED: {challengeColor} | " +
            $"Challenger: {GetDisplayPlayerName(challengerPlayerIndex)} | " +
            $"Difficulty: {difficulty} | " +
            $"Presentation Only: {challengeIsPresentationOnly}"
        );

        HideAllMinigameHUDs();
        HideResult();

        switch (challengeColor)
        {
            case CardColor.Yellow:

                if (speedMinigame == null)
                {
                    Debug.LogError(
                        "MinigameManager: SpeedDiceMinigame " +
                        "reference is missing."
                    );

                    OnMinigameCompleted(false);
                    return;
                }

                speedHUD?.SetActive(true);

                speedMinigame.StartGame(
                    difficulty,
                    aiControlled
                );

                break;

            case CardColor.Red:

                if (physicalMinigame == null)
                {
                    Debug.LogError(
                        "MinigameManager: PhysicalBalanceMinigame " +
                        "reference is missing."
                    );

                    OnMinigameCompleted(false);
                    return;
                }

                physicalHUD?.SetActive(true);

                physicalMinigame.StartGame(
                    difficulty,
                    aiControlled
                );

                break;

            case CardColor.Green:

                if (luckMinigame == null)
                {
                    Debug.LogError(
                        "MinigameManager: LuckMinigame reference " +
                        "is missing."
                    );

                    OnMinigameCompleted(false);
                    return;
                }

                luckHUD?.SetActive(true);

                luckMinigame.StartGame(
                    difficulty,
                    aiControlled
                );

                break;

            case CardColor.Purple:
            default:

                if (knowledgeMinigame == null)
                {
                    Debug.LogError(
                        "MinigameManager: KnowledgeMinigame " +
                        "reference is missing."
                    );

                    OnMinigameCompleted(false);
                    return;
                }

                knowledgeHUD?.SetActive(true);

                knowledgeMinigame.StartGame(
                    difficulty,
                    aiControlled
                );

                break;
        }
    }

    // ============================================================
    // MINIGAME RESULTS
    // ============================================================

    public void OnMinigameCompleted(
        bool challengerWon)
    {
        if (completingResult)
            return;

        completingResult = true;

        StartCoroutine(
            FinishMinigameResultRoutine(
                challengerWon
            )
        );
    }

    private IEnumerator FinishMinigameResultRoutine(
        bool challengerWon)
    {
        HideAllMinigameHUDs();

        ShowResult(
            challengerWon
        );

        yield return new WaitForSeconds(
            Mathf.Max(
                0f,
                resultDisplayDuration
            )
        );

        HideResult();

        completingResult = false;

        if (challengeIsPresentationOnly)
        {
            FinishPresentationMinigame(
                challengerWon
            );

            yield break;
        }

        challengeIsPresentationOnly = false;

        if (TransitionManager.Instance != null)
        {
            TransitionManager.Instance.EndChallenge();
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.ChallengeFinished(
                challengerWon
            );
        }
    }

    public void CompleteAIChallenge(
        bool challengerWon)
    {
        challengeIsPresentationOnly = false;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.ChallengeFinished(
                challengerWon
            );
        }
    }

    private void FinishPresentationMinigame(
        bool challengerWon)
    {
        Debug.Log(
            $"PRESENTATION DEBUG: Minigame finished. " +
            $"{GetDisplayPlayerName(challengerPlayerIndex)} " +
            (challengerWon ? "WON." : "LOST.")
        );

        challengeIsPresentationOnly = false;
        waitingForActualMinigame = false;

        if (TransitionManager.Instance != null)
        {
            TransitionManager.Instance.EndChallenge();
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.state =
                GameState.Playing;
        }
    }

    // ============================================================
    // HUD
    // ============================================================

    private void HideAllMinigameHUDs()
    {
        speedHUD?.SetActive(false);
        physicalHUD?.SetActive(false);
        luckHUD?.SetActive(false);
        knowledgeHUD?.SetActive(false);
    }

    // ============================================================
    // RESULT UI
    // ============================================================

    private void ShowResult(bool won)
    {
        if (challengeResultPanel == null)
            return;

        challengeResultPanel.SetActive(true);

        if (resultTitle != null)
        {
            resultTitle.text =
                won
                    ? "CHALLENGE WON!"
                    : "CHALLENGE LOST!";
        }

        if (resultText != null)
        {
            resultText.text =
                won
                    ? $"{GetDisplayPlayerName(challengerPlayerIndex)} PASSED THE MINIGAME!"
                    : $"{GetDisplayPlayerName(challengerPlayerIndex)} FAILED THE MINIGAME!";
        }
    }

    private void HideResult()
    {
        challengeResultPanel?.SetActive(false);
    }

    // ============================================================
    // INSTRUCTIONS
    // ============================================================

    public string GetInstruction(CardColor color)
    {
        switch (color)
        {
            case CardColor.Yellow:
                return "ADD THE DICE!";

            case CardColor.Red:
                return "KEEP THE PACE!";

            case CardColor.Green:
                return "PICK THE HAT!";

            case CardColor.Purple:
                return "ANSWER!";

            default:
                return "GET READY!";
        }
    }

    // ============================================================
    // PLAYER NAME
    // ============================================================

    public string GetDisplayPlayerName(int playerIndex)
    {
        switch (playerIndex)
        {
            case 0:
                return "PLAYER";

            case 1:
                return "AI 1";

            case 2:
                return "AI 2";

            case 3:
                return "AI 3";

            default:
                return "PLAYER";
        }
    }

    // ============================================================
    // VALID PLAYER
    // ============================================================

    private bool IsValidPlayerIndex(
        int playerIndex)
    {
        return playerIndex >= 0 &&
               playerIndex < PlayerCount;
    }
}