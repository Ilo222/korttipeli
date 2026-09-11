using UnityEngine;

public class MinigameManager : MonoBehaviour
{
    public static MinigameManager Instance;

    [Header("Difficulty")]
    [SerializeField] private int currentDifficulty = 1;

    public int CurrentDifficulty => currentDifficulty;

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

        TransitionManager.Instance.StartChallenge(
            GetInstruction(color));
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

    private string GetInstruction(CardColor color)
    {
        switch (color)
        {
            case CardColor.Yellow:
                return "ADD THE DICE!";

            case CardColor.Red:
                return "KEEP BALANCE!";

            case CardColor.Green:
                return "PICK THE HAT!";

            case CardColor.Purple:
                return "ANSWER!";

            default:
                return "CHALLENGE!";
        }
    }

    private void StartSpeedGame()
    {
        Debug.Log(
            $"Starting SPEED challenge " +
            $"at level {currentDifficulty}");

        // Later:
        // YellowSpeedMinigame.Instance.StartGame(...)
    }

    private void StartPhysicalGame()
    {
        Debug.Log(
            $"Starting PHYSICAL challenge " +
            $"at level {currentDifficulty}");
    }

    private void StartLuckGame()
    {
        Debug.Log(
            $"Starting LUCK challenge " +
            $"at level {currentDifficulty}");
    }

    private void StartKnowledgeGame()
    {
        Debug.Log(
            $"Starting KNOWLEDGE challenge " +
            $"at level {currentDifficulty}");
    }

    public bool SimulateAIResult()
    {
        float winChance =
            Mathf.Clamp(
                0.90f -
                currentDifficulty * 0.18f,
                0.05f,
                0.90f);

        return Random.value < winChance;
    }

    public void OnMinigameCompleted(
        bool challengerWon)
    {
        // Increase difficulty for NEXT challenge.
        currentDifficulty++;

        GameManager.Instance
            .ChallengeFinished(
                challengerWon);
    }

    public CardColor GetChallengeColor()
    {
        return challengeColor;
    }

    public bool IsPlayerChallenger()
    {
        return challengerIsPlayer;
    }
}