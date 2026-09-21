using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChallengePanel : MonoBehaviour
{
    public static ChallengePanel Instance;

    [Header("Main Panel")]
    [SerializeField] private GameObject challengeRoot;

    [Header("Challenge Text")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;

    [Header("Challenge Buttons")]
    [SerializeField] private Button challengeButton;
    [SerializeField] private Button continueButton;

    [Header("Wild Color Panel")]
    [SerializeField] private GameObject colorSelectionRoot;
    [SerializeField] private TextMeshProUGUI colorSelectionTitle;

    [Header("Wild Color Buttons")]
    [SerializeField] private Button purpleButton;
    [SerializeField] private Button greenButton;
    [SerializeField] private Button yellowButton;
    [SerializeField] private Button redButton;

    private void Awake()
    {
        Instance = this;

        HideAll();
    }

    // =========================================================
    // SHOW CHALLENGE
    // =========================================================

    public void ShowChallenge(
        CardData card,
        int pendingDrawAmount)
    {
        if (challengeRoot != null)
            challengeRoot.SetActive(true);

        if (colorSelectionRoot != null)
            colorSelectionRoot.SetActive(false);

        if (titleText != null)
            titleText.text = "WANT TO CHALLENGE?";

        if (descriptionText != null)
        {
            if (card == null)
            {
                descriptionText.text =
                    "Challenge this card?";
            }
            else
            {
                switch (card.type)
                {
                    case CardType.Draw2:
                        descriptionText.text =
                            "+" + pendingDrawAmount +
                            " INCOMING!";
                        break;

                    case CardType.Draw4:
                        descriptionText.text =
                            "+" + pendingDrawAmount +
                            " INCOMING!";
                        break;

                    case CardType.Skip:
                        descriptionText.text =
                            "CHALLENGE THE SKIP!";
                        break;

                    case CardType.Reverse:
                        descriptionText.text =
                            "CHALLENGE THE REVERSE!";
                        break;

                    case CardType.ColorChange:
                        descriptionText.text =
                            "CHALLENGE THE COLOR CHANGE!";
                        break;

                    default:
                        descriptionText.text =
                            "CHALLENGE THIS CARD?";
                        break;
                }
            }
        }
    }

    // =========================================================
    // CHALLENGE BUTTON
    // =========================================================

    public void OnChallengePressed()
    {
        if (GameManager.Instance == null)
            return;

        GameManager.Instance.AcceptPlayerChallenge();

        HideAll();
    }

    // =========================================================
    // CONTINUE BUTTON
    // =========================================================

    public void OnContinuePressed()
    {
        if (GameManager.Instance == null)
            return;

        GameManager.Instance.DeclinePlayerChallenge();

        // GameManager decides whether the player should
        // remain in control because of +2/+4 stacking.
        if (challengeRoot != null)
            challengeRoot.SetActive(false);
    }

    // =========================================================
    // WILD COLOR SELECTION
    // =========================================================

    public void ShowWildColorSelection()
    {
        if (challengeRoot != null)
            challengeRoot.SetActive(false);

        if (colorSelectionRoot != null)
            colorSelectionRoot.SetActive(true);

        if (colorSelectionTitle != null)
            colorSelectionTitle.text =
                "CHOOSE A COLOR";
    }

    // =========================================================
    // PURPLE
    // =========================================================

    public void SelectPurple()
    {
        SelectColor(CardColor.Purple);
    }

    // =========================================================
    // GREEN
    // =========================================================

    public void SelectGreen()
    {
        SelectColor(CardColor.Green);
    }

    // =========================================================
    // YELLOW
    // =========================================================

    public void SelectYellow()
    {
        SelectColor(CardColor.Yellow);
    }

    // =========================================================
    // RED
    // =========================================================

    public void SelectRed()
    {
        SelectColor(CardColor.Red);
    }

    // =========================================================
    // COLOR
    // =========================================================

    private void SelectColor(CardColor color)
    {
        if (GameManager.Instance == null)
            return;

        GameManager.Instance.PlayerSelectedWildColor(
            color
        );

        HideAll();
    }

    // =========================================================
    // HIDE
    // =========================================================

    public void HideAll()
    {
        if (challengeRoot != null)
            challengeRoot.SetActive(false);

        if (colorSelectionRoot != null)
            colorSelectionRoot.SetActive(false);
    }
}