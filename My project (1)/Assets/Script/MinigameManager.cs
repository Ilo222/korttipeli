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
        challengerIsPlayer = playerIsChallenger;

        Debug.Log(
            "MinigameManager: Challenge started. " +
            "Color = " + color +
            ", Player Challenger = " + playerIsChallenger +
            ", Difficulty = " + currentDifficulty
        );

        if (TransitionManager.Instance == null)
        {
            Debug.LogError(
                "MinigameManager: TransitionManager.Instance is missing."
            );

            return;
        }

        string instruction = GetInstruction(color);

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
                    "MinigameManager: Unknown challenge color."
                );
                break;
        }
    }

    // =========================================================
    // GET INSTRUCTION
    // =========================================================

    private string GetInstruction(CardColor color)
    {
        switch (color)
        {
            case CardColor.Yellow:
                return GetSpeedInstruction();

            case CardColor.Red:
                return GetPhysicalInstruction();

            case CardColor.Green:
                return GetLuckInstruction();

            case CardColor.Purple:
                return GetKnowledgeInstruction();

            case CardColor.Wild:
                return "GET READY!";

            default:
                return "GET READY!";
        }
    }

    // =========================================================
    // SPEED INSTRUCTION
    // =========================================================

    private string GetSpeedInstruction()
    {
        return "ADD THE DICE AS FAST AS YOU CAN!";
    }

    // =========================================================
    // PHYSICAL INSTRUCTION
    // =========================================================

    private string GetPhysicalInstruction()
    {
        return "KEEP YOUR BALANCE!";
    }

    // =========================================================
    // LUCK INSTRUCTION
    // =========================================================

    private string GetLuckInstruction()
    {
        return "TEST YOUR LUCK!";
    }

    // =========================================================
    // KNOWLEDGE INSTRUCTION
    // =========================================================

    private string GetKnowledgeInstruction()
    {
        return "ANSWER THE QUESTION!";
    }

    // =========================================================
    // SPEED MINIGAME
    // =========================================================

    private void StartSpeedGame()
    {
        Debug.Log(
            "MinigameManager: SPEED minigame started. " +
            "Difficulty = " +
            currentDifficulty
        );

        // Actual Speed minigame will be started here.
        //
        // Yellow = Speed
        // Dice addition challenge.
    }

    // =========================================================
    // PHYSICAL MINIGAME
    // =========================================================

    private void StartPhysicalGame()
    {
        Debug.Log(
            "MinigameManager: PHYSICAL minigame started. " +
            "Difficulty = " +
            currentDifficulty
        );

        // Actual Physical minigame will be started here.
        //
        // Red = Physical
        // Balance challenge.
    }

    // =========================================================
    // LUCK MINIGAME
    // =========================================================

    private void StartLuckGame()
    {
        Debug.Log(
            "MinigameManager: LUCK minigame started. " +
            "Difficulty = " +
            currentDifficulty
        );

        // Actual Luck minigame will be started here.
        //
        // Green = Luck
        // Chance / hats / coin-style challenge.
    }

    // =========================================================
    // KNOWLEDGE MINIGAME
    // =========================================================

    private void StartKnowledgeGame()
    {
        Debug.Log(
            "MinigameManager: KNOWLEDGE minigame started. " +
            "Difficulty = " +
            currentDifficulty
        );

        // Actual Knowledge minigame will be started here.
        //
        // Purple = Knowledge
        // Trivia / question challenge.
    }

    // =========================================================
    // MINIGAME COMPLETED
    // =========================================================

    public void OnMinigameCompleted()
    {
        Debug.Log(
            "MinigameManager: Minigame completed."
        );

        // Increase difficulty for the next challenge.
        currentDifficulty++;

        // Return from the minigame camera/table
        // back to the main game.
        if (TransitionManager.Instance != null)
        {
            TransitionManager.Instance.EndChallenge();
        }
        else
        {
            Debug.LogWarning(
                "MinigameManager: TransitionManager.Instance " +
                "was not found while ending minigame."
            );
        }

        /*
         * IMPORTANT:
         *
         * We do NOT call:
         *
         * GameManager.Instance.OnMinigameFinished();
         *
         * because your current GameManager does not contain
         * that method.
         *
         * We will connect the minigame result back into the
         * normal turn system when we update GameManager.
         */
    }

    // =========================================================
    // GET CURRENT CHALLENGE COLOR
    // =========================================================

    public CardColor GetChallengeColor()
    {
        return challengeColor;
    }

    // =========================================================
    // IS PLAYER THE CHALLENGER?
    // =========================================================

    public bool IsPlayerChallenger()
    {
        return challengerIsPlayer;
    }

    // =========================================================
    // GET DIFFICULTY
    // =========================================================

    public int GetCurrentDifficulty()
    {
        return currentDifficulty;
    }
}