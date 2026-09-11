using UnityEngine;

public class MinigameManager : MonoBehaviour
{
    public static MinigameManager Instance;

    [Header("Difficulty")]
    [SerializeField]
    private int currentDifficultyLevel = 1;

    public int CurrentDifficulty =>
        currentDifficultyLevel;

    [Header("References")]
    public TransitionManager transitionManager;

    private CardColor challengeColor;
    private bool challengerIsPlayer;

    private void Awake()
    {
        Instance = this;
    }

    public void StartChallenge(
        CardColor color,
        bool playerIsChallenger)
    {
        challengeColor = color;
        challengerIsPlayer = playerIsChallenger;

        string instruction =
            GetInstruction(color);

        transitionManager.StartChallenge(
            instruction);
    }

    public void StartActualMinigame()
    {
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
        }
    }

    private string GetInstruction(
        CardColor color)
    {
        switch (color)
        {
            case CardColor.Yellow:
                return "ADD THE DICE!";

            case CardColor.Red:
                return "KEEP BALANCE!";

            case CardColor.Green:
                return "PICK THE RIGHT HAT!";

            case CardColor.Purple:
                return "ANSWER!";

            default:
                return "CHALLENGE!";
        }
    }

    private void StartSpeedGame()
    {
        Debug.Log(
            $"Starting SPEED game at " +
            $"difficulty {currentDifficultyLevel}");

        // Yellow:
        // dice addition.
    }

    private void StartPhysicalGame()
    {
        Debug.Log(
            $"Starting PHYSICAL game at " +
            $"difficulty {currentDifficultyLevel}");

        // Red:
        // balance.
    }

    private void StartLuckGame()
    {
        Debug.Log(
            $"Starting LUCK game at " +
            $"difficulty {currentDifficultyLevel}");

        // Green:
        // hats / chance.
    }

    private void StartKnowledgeGame()
    {
        Debug.Log(
            $"Starting KNOWLEDGE game at " +
            $"difficulty {currentDifficultyLevel}");

        // Purple:
        // knowledge.
    }

    public void OnMinigameCompleted(
        bool challengerWon)
    {
        currentDifficultyLevel++;

        GameManager.Instance
            .ChallengeFinished(
                challengerWon);
    }

    public bool SimulateAIResult()
    {
        float winChance =
            Mathf.Clamp(
                0.90f -
                currentDifficultyLevel * 0.18f,
                0.05f,
                0.90f);

        return Random.value < winChance;
    }
}