using UnityEngine;
using UnityEngine.InputSystem;

public class PresentationDebugController : MonoBehaviour
{
    public enum DebugTargetPlayer
    {
        Player,
        AI1,
        AI2,
        AI3
    }

    [Header("Secret Presentation Keys")]
    [SerializeField] private Key increaseDifficultyKey = Key.F8;
    [SerializeField] private Key startMinigameKey = Key.F9;

    [Header("Difficulty Demo")]
    [SerializeField]
    private DebugTargetPlayer difficultyTarget =
        DebugTargetPlayer.Player;

    [Header("Minigame Demo")]
    [SerializeField]
    private DebugTargetPlayer minigameTarget =
        DebugTargetPlayer.Player;
    [SerializeField] private CardColor demoMinigame = CardColor.Yellow;

    [Header("Optional Feedback")]
    [SerializeField] private bool logToConsole = true;

    private void Update()
    {
        if (MinigameManager.Instance == null)
            return;

        if (Keyboard.current == null)
            return;

        if (Keyboard.current[increaseDifficultyKey].wasPressedThisFrame)
        {
            IncreaseSelectedDifficulty();
        }

        if (Keyboard.current[startMinigameKey].wasPressedThisFrame)
        {
            StartSelectedMinigame();
        }
    }

    private void IncreaseSelectedDifficulty()
    {
        int playerIndex =
            TargetToPlayerIndex(difficultyTarget);

        MinigameManager.Instance.AddPresentationDifficulty(
            playerIndex,
            1
        );

        if (logToConsole)
        {
            Debug.Log(
                "PRESENTATION: " +
                MinigameManager.Instance.GetDisplayPlayerName(playerIndex) +
                " difficulty = " +
                MinigameManager.Instance.GetPlayerDifficulty(playerIndex)
            );
        }
    }

    private void StartSelectedMinigame()
    {
        int playerIndex =
            TargetToPlayerIndex(minigameTarget);

        MinigameManager.Instance.StartPresentationMinigame(
            demoMinigame,
            playerIndex
        );
    }

    private int TargetToPlayerIndex(
        DebugTargetPlayer target)
    {
        switch (target)
        {
            case DebugTargetPlayer.Player:
                return 0;

            case DebugTargetPlayer.AI1:
                return 1;

            case DebugTargetPlayer.AI2:
                return 2;

            case DebugTargetPlayer.AI3:
                return 3;

            default:
                return 0;
        }
    }
}
