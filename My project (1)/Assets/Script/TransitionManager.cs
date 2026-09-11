using System.Collections;
using UnityEngine;
using TMPro;
using Unity.Cinemachine;

public class TransitionManager : MonoBehaviour
{
    public static TransitionManager Instance;

    [Header("Cameras")]
    public CinemachineCamera vcamTopDown;
    public CinemachineCamera vcamWide;
    public CinemachineCamera vcamMinigame;

    [Header("UI")]
    public RectTransform minigamePanel;
    public TextMeshProUGUI countdownText;

    [Header("Timing")]
    public float wideDuration = 1.2f;
    public float minigameCameraDuration = 0.8f;
    public float panelSlideDuration = 0.3f;

    private Vector2 offscreen =
        new Vector2(-2500f, 0f);

    private Vector2 onscreen =
        Vector2.zero;

    private void Awake()
    {
        Instance = this;

        minigamePanel
            .anchoredPosition = offscreen;

        SetTopDownCamera();
    }

    private void SetTopDownCamera()
    {
        vcamTopDown.Priority = 50;
        vcamWide.Priority = 10;
        vcamMinigame.Priority = 5;
    }

    public void StartChallenge(
        string instruction)
    {
        StartCoroutine(
            ChallengeSequence(instruction));
    }

    private IEnumerator ChallengeSequence(
        string instruction)
    {
        // ----------------------------------
        // TOP-DOWN -> WIDE
        // ----------------------------------

        vcamWide.Priority = 60;

        yield return new WaitForSeconds(
            wideDuration);

        // ----------------------------------
        // WIDE -> MINIGAME
        // ----------------------------------

        vcamMinigame.Priority = 70;

        yield return new WaitForSeconds(
            minigameCameraDuration);

        // ----------------------------------
        // SLIDE INTRO
        // ----------------------------------

        float timer = 0f;

        while (timer < panelSlideDuration)
        {
            float t =
                timer / panelSlideDuration;

            minigamePanel.anchoredPosition =
                Vector2.Lerp(
                    offscreen,
                    onscreen,
                    t);

            timer += Time.deltaTime;

            yield return null;
        }

        minigamePanel.anchoredPosition =
            onscreen;

        // ----------------------------------
        // COUNTDOWN
        // ----------------------------------

        countdownText.text = instruction;

        yield return new WaitForSeconds(0.8f);

        countdownText.text = "3";

        yield return new WaitForSeconds(0.6f);

        countdownText.text = "2";

        yield return new WaitForSeconds(0.6f);

        countdownText.text = "1";

        yield return new WaitForSeconds(0.6f);

        countdownText.text = "GO!";

        yield return new WaitForSeconds(0.4f);

        countdownText.text = "";

        // ----------------------------------
        // START REAL MINIGAME
        // ----------------------------------

        MinigameManager.Instance
            .StartActualMinigame();

        GameManager.Instance.state =
            GameState.Minigame;
    }

    public void EndChallenge()
    {
        StartCoroutine(
            ReturnSequence());
    }

    private IEnumerator ReturnSequence()
    {
        // Hide panel
        float timer = 0f;

        Vector2 start =
            minigamePanel.anchoredPosition;

        while (timer < panelSlideDuration)
        {
            float t =
                timer / panelSlideDuration;

            minigamePanel.anchoredPosition =
                Vector2.Lerp(
                    start,
                    offscreen,
                    t);

            timer += Time.deltaTime;

            yield return null;
        }

        minigamePanel.anchoredPosition =
            offscreen;

        // Minigame -> Wide
        vcamWide.Priority = 60;
        vcamMinigame.Priority = 10;

        yield return new WaitForSeconds(1f);

        // Wide -> Top Down
        vcamTopDown.Priority = 70;
        vcamWide.Priority = 10;

        yield return new WaitForSeconds(1f);

        SetTopDownCamera();
    }
}