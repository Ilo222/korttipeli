using UnityEngine;

public class MinigameManager : MonoBehaviour
{
    public static MinigameManager Instance;

    [Header("Challenge Settings")]
    public int currentDifficulty = 1;

    private CardColor challengeColor;
    private bool challengerIsPlayer;

    private void Awake()
    {
        Instance = this;
    }

    // =========================================================
    // START CHALLENGE
    // =========================================================

    public void StartChallenge(
        CardColor color,
        bool playerIsChallenger)
    {
        challengeColor = color;
        challengerIsPlayer =
            playerIsChallenger;

        Debug.Log(
            "MinigameManager: Challenge started. " +
            "Color = " + color +
            ", Player Challenger = " +
            playerIsChallenger +
            ", Difficulty = " +
            currentDifficulty
        );

        if (TransitionManager.Instance == null)
        {
            Debug.LogError(
                "MinigameManager: " +
                "TransitionManager.Instance is missing."
            );

            return;
        }

        string instruction =
            GetInstruction(color);

        TransitionManager.Instance.StartChallenge(
            color,
            instruction
        );
    }

    // =========================================================
    // START ACTUAL MINIGAME
    // =========================================================

    public void StartActualMinigame()
    {
        Debug.Log(
            "MinigameManager: Starting actual minigame. " +
            challengeColor +
            " | Difficulty " +
            currentDifficulty
        );

        switch (challengeColor)
        {
            case CardColor.Yellow:
                StartSpeedGame();
                break;

            case CardColor.Red:
                StartPhysicalGame();
                break;

            case CardColor.Green:
                StartLuckGame();
                break;

            case CardColor.Purple:
                StartKnowledgeGame();
                break;

            case CardColor.Wild:
                StartKnowledgeGame();
                break;

            default:
                Debug.LogWarning(
                    "Unknown challenge color."
                );
                break;
        }
    }

    // =========================================================
    // INSTRUCTIONS
    // =========================================================

    private string GetInstruction(
        CardColor color)
    {
        switch (color)
        {
            case CardColor.Yellow:
                return
                    "ADD THE DICE AS FAST AS YOU CAN!";

            case CardColor.Red:
                return
                    "KEEP YOUR BALANCE!";

            case CardColor.Green:
                return
                    "TEST YOUR LUCK!";

            case CardColor.Purple:
                return
                    "ANSWER THE QUESTION!";

            default:
                return
                    "GET READY!";
        }
    }

    // =========================================================
    // MINIGAMES
    // =========================================================

    private void StartSpeedGame()
    {
        Debug.Log(
            "SPEED MINIGAME STARTED"
        );

        // Actual Speed minigame later.
        //
        // Yellow = Speed
        // Dice addition.
    }

    private void StartPhysicalGame()
    {
        Debug.Log(
            "PHYSICAL MINIGAME STARTED"
        );

        // Actual Physical minigame later.
        //
        // Red = Physical
        // Balance challenge.
    }

    private void StartLuckGame()
    {
        Debug.Log(
            "LUCK MINIGAME STARTED"
        );

        // Actual Luck minigame later.
        //
        // Green = Luck.
    }

    private void StartKnowledgeGame()
    {
        Debug.Log(
            "KNOWLEDGE MINIGAME STARTED"
        );

        // Actual Knowledge minigame later.
        //
        // Purple = Knowledge.
    }

    // =========================================================
    // PLAYER/ACTUAL MINIGAME FINISHED
    // =========================================================

    public void OnMinigameCompleted(
        bool challengerWon)
    {
        Debug.Log(
            "MinigameManager: " +
            "Minigame completed. Challenger won = " +
            challengerWon
        );

        currentDifficulty++;

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

    // =========================================================
    // COMPATIBILITY
    // =========================================================

    public void OnMinigameCompleted()
    {
        OnMinigameCompleted(true);
    }

    // =========================================================
    // AI CHALLENGE FINISHED
    // =========================================================

    public void CompleteAIChallenge(
        bool challengerWon)
    {
        Debug.Log(
            "MinigameManager: " +
            "AI challenge simulated. Won = " +
            challengerWon
        );

        currentDifficulty++;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.ChallengeFinished(
                challengerWon
            );
        }
    }

    // =========================================================
    // GETTERS
    // =========================================================

    public CardColor GetChallengeColor()
    {
        return challengeColor;
    }

    public bool IsPlayerChallenger()
    {
        return challengerIsPlayer;
    }

    public int GetCurrentDifficulty()
    {
        return currentDifficulty;
    }
}