using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class PhysicalBalanceMinigame : MonoBehaviour
{
    [Header("Pace Settings")]
    [SerializeField] private float baseDuration = 8f;

    [Tooltip("Starting target presses per second at difficulty 1.")]
    [SerializeField] private float baseTargetPace = 3f;

    [Tooltip("How much the starting target increases per difficulty level.")]
    [SerializeField] private float targetPacePerDifficulty = 0.5f;

    [Tooltip("How quickly the target pace rises during the minigame.")]
    [SerializeField] private float baseRampPerSecond = 0.15f;

    [Tooltip("Extra ramp speed added by difficulty.")]
    [SerializeField] private float rampDifficultyBonus = 0.015f;

    [Tooltip("How far from the target pace the player may be.")]
    [SerializeField] private float baseTolerance = 1.4f;

    [Tooltip("Tolerance reduction per difficulty level.")]
    [SerializeField] private float toleranceReductionPerDifficulty = 0.08f;

    [Tooltip("Minimum tolerance at very high difficulty.")]
    [SerializeField] private float minimumTolerance = 0.45f;

    [Tooltip("How long the player can remain outside the target pace.")]
    [SerializeField] private float baseMistakeGrace = 1.5f;

    [Tooltip("Mistake grace reduction per difficulty level.")]
    [SerializeField] private float mistakeGraceReductionPerDifficulty = 0.08f;

    [Tooltip("Minimum mistake grace at high difficulty.")]
    [SerializeField] private float minimumMistakeGrace = 0.4f;

    [Tooltip("Initial warmup period where the player is not penalized for being slow.")]
    [SerializeField] private float warmupTime = 0.75f;

    [Header("Input")]
    [SerializeField] private bool allowMouseClick = true;

    [Header("HUD")]
    [SerializeField] private GameObject physicalHUD;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI balanceText;

    // Kept so your existing Inspector does not lose the old reference.
    // The new minigame does not use the Rigidbody anymore.
    [Header("Legacy")]
    [SerializeField] private Rigidbody balancePlatform;

    private readonly List<float> pressTimes = new();

    private Coroutine gameRoutine;

    private bool running;
    private bool aiControlled;

    private int difficulty;

    private float timeRemaining;
    private float elapsedTime;

    private float currentTargetPace;
    private float currentTolerance;
    private float currentMistakeGrace;

    private float badPaceTime;

    private int totalPresses;

    public void StartGame(
        int difficultyLevel,
        bool ai)
    {
        StopCurrentGame();

        difficulty =
            Mathf.Max(
                1,
                difficultyLevel
            );

        aiControlled = ai;

        if (physicalHUD != null)
            physicalHUD.SetActive(true);

        pressTimes.Clear();

        totalPresses = 0;

        timeRemaining =
            Mathf.Max(
                0.1f,
                baseDuration
            );

        elapsedTime = 0f;
        badPaceTime = 0f;

        currentTargetPace =
            GetTargetPace(0f);

        currentTolerance =
            GetTolerance();

        currentMistakeGrace =
            GetMistakeGrace();

        running = true;

        UpdateHUD(
            timeRemaining,
            0f,
            currentTargetPace
        );

        gameRoutine =
            StartCoroutine(
                RunGame()
            );
    }

    private void Update()
    {
        if (!running)
            return;

        if (aiControlled)
            return;

        HandlePlayerInput();
    }

    private IEnumerator RunGame()
    {
        if (aiControlled)
        {
            yield return StartCoroutine(
                RunAIAttempt()
            );

            yield break;
        }

        while (
            running &&
            timeRemaining > 0f)
        {
            elapsedTime =
                baseDuration -
                timeRemaining;

            currentTargetPace =
                GetTargetPace(
                    elapsedTime
                );

            currentTolerance =
                GetTolerance();

            currentMistakeGrace =
                GetMistakeGrace();

            CleanOldPresses();

            float currentPace =
                GetCurrentPace();

            bool paceIsGood =
                elapsedTime < warmupTime ||
                Mathf.Abs(
                    currentPace -
                    currentTargetPace
                ) <= currentTolerance;

            if (paceIsGood)
            {
                badPaceTime =
                    Mathf.Max(
                        0f,
                        badPaceTime -
                        Time.deltaTime * 0.5f
                    );
            }
            else
            {
                badPaceTime +=
                    Time.deltaTime;
            }

            UpdateHUD(
                timeRemaining,
                currentPace,
                currentTargetPace
            );

            if (
                elapsedTime >= warmupTime &&
                badPaceTime >= currentMistakeGrace)
            {
                Finish(false);
                yield break;
            }

            timeRemaining -=
                Time.deltaTime;

            yield return null;
        }

        if (running)
        {
            float finalPace =
                GetCurrentPace();

            bool finalGood =
                Mathf.Abs(
                    finalPace -
                    currentTargetPace
                ) <= currentTolerance;

            Finish(finalGood);
        }
    }

    private IEnumerator RunAIAttempt()
    {
        float aiTime =
            Mathf.Max(
                1f,
                baseDuration
            );

        float aiTimer = 0f;

        float aiErrorChance =
            Mathf.Clamp(
                0.05f +
                ((difficulty - 1) * 0.08f),
                0.05f,
                0.85f
            );

        while (
            aiTimer < aiTime &&
            running)
        {
            elapsedTime =
                aiTimer;

            currentTargetPace =
                GetTargetPace(
                    elapsedTime
                );

            currentTolerance =
                GetTolerance();

            float simulatedPace =
                currentTargetPace;

            if (
                Random.value <
                aiErrorChance *
                Time.deltaTime)
            {
                float error =
                    Random.Range(
                        -1.5f,
                        1.5f
                    );

                simulatedPace =
                    Mathf.Max(
                        0f,
                        currentTargetPace +
                        error
                    );
            }

            UpdateHUD(
                baseDuration -
                aiTimer,
                simulatedPace,
                currentTargetPace
            );

            aiTimer +=
                Time.deltaTime;

            yield return null;
        }

        float aiSuccessChance =
            Mathf.Clamp(
                0.95f -
                ((difficulty - 1) * 0.10f),
                0.10f,
                0.95f
            );

        yield return new WaitForSeconds(
            0.35f
        );

        Finish(
            Random.value <
            aiSuccessChance
        );
    }

    private void HandlePlayerInput()
    {
        Keyboard keyboard =
            Keyboard.current;

        if (keyboard != null)
        {
            if (
                keyboard.spaceKey
                    .wasPressedThisFrame)
            {
                RegisterPress();
            }
        }

        if (!allowMouseClick)
            return;

        Mouse mouse =
            Mouse.current;

        if (mouse != null)
        {
            if (
                mouse.leftButton
                    .wasPressedThisFrame)
            {
                RegisterPress();
            }
        }
    }

    private void RegisterPress()
    {
        if (!running)
            return;

        float currentTime =
            Time.time;

        pressTimes.Add(
            currentTime
        );

        totalPresses++;
    }

    private void CleanOldPresses()
    {
        float cutoff =
            Time.time - 1f;

        for (
            int i =
                pressTimes.Count - 1;
            i >= 0;
            i--)
        {
            if (
                pressTimes[i] <
                cutoff)
            {
                pressTimes.RemoveAt(i);
            }
        }
    }

    private float GetCurrentPace()
    {
        CleanOldPresses();

        return pressTimes.Count;
    }

    private float GetTargetPace(
        float elapsed)
    {
        float startingPace =
            baseTargetPace +
            (
                (difficulty - 1) *
                targetPacePerDifficulty
            );

        float rampSpeed =
            baseRampPerSecond +
            (
                (difficulty - 1) *
                rampDifficultyBonus
            );

        return startingPace +
               (elapsed * rampSpeed);
    }

    private float GetTolerance()
    {
        float tolerance =
            baseTolerance -
            (
                (difficulty - 1) *
                toleranceReductionPerDifficulty
            );

        return Mathf.Max(
            minimumTolerance,
            tolerance
        );
    }

    private float GetMistakeGrace()
    {
        float grace =
            baseMistakeGrace -
            (
                (difficulty - 1) *
                mistakeGraceReductionPerDifficulty
            );

        return Mathf.Max(
            minimumMistakeGrace,
            grace
        );
    }

    private void UpdateHUD(
        float time,
        float currentPace,
        float targetPace)
    {
        if (timerText != null)
        {
            timerText.text =
                Mathf.CeilToInt(
                    Mathf.Max(
                        0f,
                        time
                    )
                ).ToString();
        }

        if (balanceText == null)
            return;

        string status;

        float difference =
            currentPace -
            targetPace;

        if (elapsedTime < warmupTime)
        {
            status =
                "GET READY!";
        }
        else if (
            Mathf.Abs(difference) <=
            currentTolerance)
        {
            status =
                "PERFECT!";
        }
        else if (difference < 0f)
        {
            status =
                "TOO SLOW!";
        }
        else
        {
            status =
                "TOO FAST!";
        }

        balanceText.text =
            "TARGET: " +
            targetPace.ToString("0.0") +
            " / SEC\n" +

            "YOUR PACE: " +
            currentPace.ToString("0") +
            " / SEC\n\n" +

            status;
    }

    private void Finish(bool won)
    {
        if (!running)
            return;

        running = false;

        MinigameManager.Instance
            ?.OnMinigameCompleted(won);
    }

    private void StopCurrentGame()
    {
        if (gameRoutine != null)
        {
            StopCoroutine(
                gameRoutine
            );

            gameRoutine = null;
        }

        running = false;

        pressTimes.Clear();

        totalPresses = 0;
    }

    private void OnDisable()
    {
        StopCurrentGame();
    }
}