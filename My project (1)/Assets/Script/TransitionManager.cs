using System.Collections;
using UnityEngine;
using TMPro;
using Unity.Cinemachine;

public class TransitionManager : MonoBehaviour
{
    public static TransitionManager Instance;

    [Header("Cameras")]
    [SerializeField] private CinemachineCamera topDownCamera;
    [SerializeField] private CinemachineCamera wideCamera;
    [SerializeField] private CinemachineCamera minigameCamera;

    [Header("Intro UI")]
    [SerializeField] private RectTransform minigamePanel;
    [SerializeField] private TextMeshProUGUI countdownText;

    [Header("Timing")]
    [SerializeField] private float wideHoldTime = 1.2f;
    [SerializeField] private float minigameHoldTime = 0.8f;
    [SerializeField] private float panelSlideTime = 0.3f;

    private readonly Vector2 offscreen =
        new Vector2(-2500f, 0f);

    private readonly Vector2 onscreen =
        Vector2.zero;

    private void Awake()
    {
        Instance = this;

        if (minigamePanel != null)
        {
            minigamePanel.anchoredPosition =
                offscreen;
        }

        SetNormalCamera();
    }

    private void SetNormalCamera()
    {
        if (topDownCamera != null)
            topDownCamera.Priority = 100;

        if (wideCamera != null)
            wideCamera.Priority = 10;

        if (minigameCamera != null)
            minigameCamera.Priority = 5;
    }

    public void StartChallenge(
        string instruction)
    {
        StopAllCoroutines();

        StartCoroutine(
            ChallengeSequence(
                instruction
            )
        );
    }

    private IEnumerator ChallengeSequence(
        string instruction)
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.state =
                GameState.CameraTransition;
        }

        if (wideCamera != null)
            wideCamera.Priority = 110;

        yield return new WaitForSeconds(
            wideHoldTime
        );

        if (minigameCamera != null)
            minigameCamera.Priority = 120;

        yield return new WaitForSeconds(
            minigameHoldTime
        );

        if (minigamePanel != null)
        {
            float timer = 0f;

            while (timer < panelSlideTime)
            {
                float t =
                    timer /
                    panelSlideTime;

                minigamePanel.anchoredPosition =
                    Vector2.Lerp(
                        offscreen,
                        onscreen,
                        t
                    );

                timer += Time.deltaTime;

                yield return null;
            }

            minigamePanel.anchoredPosition =
                onscreen;
        }

        if (countdownText != null)
        {
            countdownText.text =
                instruction;

            yield return new WaitForSeconds(
                0.8f
            );

            countdownText.text = "3";

            yield return new WaitForSeconds(
                0.55f
            );

            countdownText.text = "2";

            yield return new WaitForSeconds(
                0.55f
            );

            countdownText.text = "1";

            yield return new WaitForSeconds(
                0.55f
            );

            countdownText.text = "GO!";

            yield return new WaitForSeconds(
                0.35f
            );

            countdownText.text = "";
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.state =
                GameState.Minigame;
        }

        if (MinigameManager.Instance != null)
        {
            MinigameManager.Instance
                .StartActualMinigame();
        }
    }

    public void EndChallenge()
    {
        StopAllCoroutines();

        StartCoroutine(
            ReturnToTable()
        );
    }

    private IEnumerator ReturnToTable()
    {
        if (minigamePanel != null)
        {
            float timer = 0f;

            Vector2 current =
                minigamePanel.anchoredPosition;

            while (timer < panelSlideTime)
            {
                float t =
                    timer /
                    panelSlideTime;

                minigamePanel.anchoredPosition =
                    Vector2.Lerp(
                        current,
                        offscreen,
                        t
                    );

                timer += Time.deltaTime;

                yield return null;
            }

            minigamePanel.anchoredPosition =
                offscreen;
        }

        if (wideCamera != null)
            wideCamera.Priority = 110;

        if (minigameCamera != null)
            minigameCamera.Priority = 10;

        yield return new WaitForSeconds(1f);

        if (topDownCamera != null)
            topDownCamera.Priority = 120;

        if (wideCamera != null)
            wideCamera.Priority = 10;

        yield return new WaitForSeconds(1f);

        SetNormalCamera();
    }
}