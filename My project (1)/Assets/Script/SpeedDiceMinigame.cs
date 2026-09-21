using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SpeedDiceMinigame : MonoBehaviour
{
    [Header("3D Dice")]
    [SerializeField] private SpeedDie dicePrefab;
    [SerializeField] private Transform diceSpawnArea;
    [SerializeField] private Vector3 spawnAreaSize = new Vector3(2.8f, 0.4f, 1.8f);
    [SerializeField] private float spawnHeight = 0.8f;
    [SerializeField] private float horizontalSpacing = 0.7f;
    [SerializeField] private float rollForce = 5.5f;
    [SerializeField] private float rollTorque = 7f;

    [Header("Difficulty")]
    [SerializeField] private int minimumDice = 2;
    [SerializeField] private int maximumDice = 6;
    [SerializeField] private float baseAnswerTime = 5f;
    [SerializeField] private float minimumAnswerTime = 2f;
    [SerializeField] private float answerTimeReductionPerLevel = 0.5f;

    [Header("Settling")]
    [SerializeField] private float minimumRollTime = 1f;
    [SerializeField] private float settleCheckDuration = 0.35f;
    [SerializeField] private float linearSpeedThreshold = 0.08f;
    [SerializeField] private float angularSpeedThreshold = 0.12f;
    [SerializeField] private float uprightThreshold = 0.90f;
    [SerializeField] private float maximumRollDuration = 8f;

    [Header("HUD")]
    [SerializeField] private GameObject speedHUD;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TMP_InputField answerInput;
    [SerializeField] private Button submitButton;

    private readonly List<SpeedDie> spawnedDice = new();
    private Coroutine gameRoutine;
    private int difficulty;
    private bool isAI;
    private bool running;
    private bool acceptingAnswer;
    private int correctAnswer;
    private float answerTimeRemaining;

    public void StartGame(int difficultyLevel, bool aiControlled)
    {
        StopCurrentGame();

        difficulty = Mathf.Max(1, difficultyLevel);
        isAI = aiControlled;

        if (speedHUD != null)
            speedHUD.SetActive(true);

        if (answerInput != null)
        {
            answerInput.text = "";
            answerInput.interactable = false;
        }

        if (submitButton != null)
        {
            submitButton.interactable = false;
            submitButton.onClick.RemoveAllListeners();
            submitButton.onClick.AddListener(SubmitPlayerAnswer);
        }

        running = true;
        acceptingAnswer = false;
        gameRoutine = StartCoroutine(RunGame());
    }

    private IEnumerator RunGame()
    {
        ClearDice();

        int diceCount = Mathf.Clamp(
            minimumDice + Mathf.Max(0, (difficulty - 1) / 2),
            minimumDice,
            maximumDice
        );

        SpawnDice(diceCount);

        yield return StartCoroutine(RollDiceAndWaitForStop());

        correctAnswer = 0;

        foreach (SpeedDie die in spawnedDice)
        {
            if (die != null && die.TryGetTopValue(out int value))
                correctAnswer += value;
        }

        answerTimeRemaining = Mathf.Max(
            minimumAnswerTime,
            baseAnswerTime - ((difficulty - 1) * answerTimeReductionPerLevel)
        );

        acceptingAnswer = true;

        if (answerInput != null)
            answerInput.interactable = !isAI;

        if (submitButton != null)
            submitButton.interactable = !isAI;

        if (isAI)
        {
            yield return StartCoroutine(RunAIAttempt());
            yield break;
        }

        while (running && acceptingAnswer && answerTimeRemaining > 0f)
        {
            UpdateTimerText(answerTimeRemaining);
            answerTimeRemaining -= Time.deltaTime;
            yield return null;
        }

        if (running && acceptingAnswer)
        {
            acceptingAnswer = false;
            UpdateTimerText(0f);
            Finish(false);
        }
    }

    private IEnumerator RollDiceAndWaitForStop()
    {
        float elapsed = 0f;
        float calmTime = 0f;

        while (elapsed < maximumRollDuration)
        {
            bool allSettled = spawnedDice.Count > 0;

            foreach (SpeedDie die in spawnedDice)
            {
                if (die == null ||
                    !die.IsSettled(
                        linearSpeedThreshold,
                        angularSpeedThreshold,
                        uprightThreshold))
                {
                    allSettled = false;
                    break;
                }
            }

            if (elapsed >= minimumRollTime && allSettled)
            {
                calmTime += Time.deltaTime;
                if (calmTime >= settleCheckDuration)
                    yield break;
            }
            else
            {
                calmTime = 0f;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    private IEnumerator RunAIAttempt()
    {
        float thinkingTime = Mathf.Clamp(
            1.15f - ((difficulty - 1) * 0.05f),
            0.45f,
            1.15f
        );

        float timer = thinkingTime;

        while (timer > 0f)
        {
            UpdateTimerText(Mathf.Max(0f, timer));
            timer -= Time.deltaTime;
            yield return null;
        }

        if (answerInput != null)
            answerInput.text = correctAnswer.ToString();

        yield return new WaitForSeconds(0.25f);

        float chance = Mathf.Clamp(
            0.96f - ((difficulty - 1) * 0.11f),
            0.12f,
            0.96f
        );

        Finish(Random.value < chance);
    }

    private void SubmitPlayerAnswer()
    {
        if (!running || !acceptingAnswer || isAI)
            return;

        acceptingAnswer = false;

        bool parsed = int.TryParse(
            answerInput != null ? answerInput.text.Trim() : "",
            out int playerAnswer
        );

        Finish(parsed && playerAnswer == correctAnswer);
    }

    private void Finish(bool won)
    {
        if (!running)
            return;

        running = false;
        acceptingAnswer = false;

        if (answerInput != null)
            answerInput.interactable = false;

        if (submitButton != null)
            submitButton.interactable = false;

        MinigameManager.Instance?.OnMinigameCompleted(won);
    }

    private void SpawnDice(int count)
    {
        if (dicePrefab == null)
        {
            Debug.LogError("SpeedDiceMinigame: Dice Prefab is missing.");
            Finish(false);
            return;
        }

        Transform center = diceSpawnArea != null ? diceSpawnArea : transform;

        for (int i = 0; i < count; i++)
        {
            Vector3 local = new Vector3(
                Mathf.Clamp(
                    (i - (count - 1) * 0.5f) * horizontalSpacing,
                    -spawnAreaSize.x * 0.5f,
                    spawnAreaSize.x * 0.5f),
                spawnHeight,
                Random.Range(-spawnAreaSize.z * 0.5f, spawnAreaSize.z * 0.5f)
            );

            SpeedDie die = Instantiate(
                dicePrefab,
                center.TransformPoint(local),
                Random.rotation
            );

            spawnedDice.Add(die);
            die.PrepareForRoll();

            Rigidbody body = die.Rigidbody;
            if (body == null)
            {
                Debug.LogError("SpeedDiceMinigame: Dice prefab needs a Rigidbody.");
                continue;
            }

            Vector3 direction = new Vector3(
                Random.Range(-0.8f, 0.8f),
                Random.Range(0.2f, 0.6f),
                Random.Range(-0.8f, 0.8f)
            ).normalized;

            body.AddForce(direction * rollForce, ForceMode.Impulse);
            body.AddTorque(Random.insideUnitSphere * rollTorque, ForceMode.Impulse);
        }
    }

    private void UpdateTimerText(float time)
    {
        if (timerText != null)
            timerText.text = Mathf.CeilToInt(Mathf.Max(0f, time)).ToString();
    }

    private void ClearDice()
    {
        for (int i = spawnedDice.Count - 1; i >= 0; i--)
        {
            if (spawnedDice[i] != null)
                Destroy(spawnedDice[i].gameObject);
        }
        spawnedDice.Clear();
    }

    private void StopCurrentGame()
    {
        if (gameRoutine != null)
        {
            StopCoroutine(gameRoutine);
            gameRoutine = null;
        }

        running = false;
        acceptingAnswer = false;
        ClearDice();
    }

    private void OnDisable()
    {
        StopCurrentGame();
    }
}
