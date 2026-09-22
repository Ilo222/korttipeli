using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class SpeedDiceMinigame : MonoBehaviour
{
    [Header("3D Dice")]
    [SerializeField] private SpeedDie dicePrefab;
    [SerializeField] private Transform diceSpawnArea;
    [SerializeField]
    private Vector3 spawnAreaSize =
        new Vector3(2.8f, 0.4f, 1.8f);

    [SerializeField] private float spawnHeight = 0.8f;
    [SerializeField] private float horizontalSpacing = 0.7f;
    [SerializeField] private float rollForce = 5.5f;
    [SerializeField] private float rollTorque = 7f;

    [Header("Invisible Safety Walls")]
    [SerializeField] private bool createSafetyWalls = true;
    [SerializeField] private float wallHeight = 1.5f;
    [SerializeField] private float wallThickness = 0.15f;
    [SerializeField] private float wallPadding = 0.10f;

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

    private GameObject safetyWallsRoot;

    private int difficulty;
    private bool isAI;
    private bool running;
    private bool acceptingAnswer;

    private int correctAnswer;
    private float answerTimeRemaining;

    public void StartGame(
        int difficultyLevel,
        bool aiControlled)
    {
        StopCurrentGame();

        difficulty =
            Mathf.Max(
                1,
                difficultyLevel
            );

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

            submitButton.onClick
                .RemoveAllListeners();

            submitButton.onClick
                .AddListener(
                    SubmitPlayerAnswer
                );
        }

        running = true;
        acceptingAnswer = false;

        gameRoutine =
            StartCoroutine(
                RunGame()
            );
    }

    private void Update()
    {
        if (
            !running ||
            !acceptingAnswer ||
            isAI)
        {
            return;
        }

        HandleDirectKeyboardInput();
        HandleDirectMouseInput();
    }

    private IEnumerator RunGame()
    {
        ClearDice();
        ClearSafetyWalls();

        int diceCount =
            Mathf.Clamp(
                minimumDice +
                Mathf.Max(
                    0,
                    (difficulty - 1) / 2
                ),
                minimumDice,
                maximumDice
            );

        if (createSafetyWalls)
            CreateSafetyWalls();

        SpawnDice(diceCount);

        yield return StartCoroutine(
            RollDiceAndWaitForStop()
        );

        correctAnswer = 0;

        foreach (
            SpeedDie die
            in spawnedDice)
        {
            if (
                die != null &&
                die.TryGetTopValue(
                    out int value))
            {
                correctAnswer += value;
            }
        }

        answerTimeRemaining =
            Mathf.Max(
                minimumAnswerTime,
                baseAnswerTime -
                (
                    (difficulty - 1) *
                    answerTimeReductionPerLevel
                )
            );

        acceptingAnswer = true;

        if (answerInput != null)
            answerInput.interactable = false;

        if (submitButton != null)
            submitButton.interactable = true;

        if (isAI)
        {
            yield return StartCoroutine(
                RunAIAttempt()
            );

            yield break;
        }

        if (answerInput != null)
            answerInput.text = "";

        while (
            running &&
            acceptingAnswer &&
            answerTimeRemaining > 0f)
        {
            UpdateTimerText(
                answerTimeRemaining
            );

            answerTimeRemaining -=
                Time.deltaTime;

            yield return null;
        }

        if (
            running &&
            acceptingAnswer)
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

        while (
            elapsed <
            maximumRollDuration)
        {
            bool allSettled =
                spawnedDice.Count > 0;

            foreach (
                SpeedDie die
                in spawnedDice)
            {
                if (
                    die == null ||
                    !die.IsSettled(
                        linearSpeedThreshold,
                        angularSpeedThreshold,
                        uprightThreshold))
                {
                    allSettled = false;
                    break;
                }
            }

            if (
                elapsed >=
                minimumRollTime &&
                allSettled)
            {
                calmTime +=
                    Time.deltaTime;

                if (
                    calmTime >=
                    settleCheckDuration)
                {
                    yield break;
                }
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
        float thinkingTime =
            Mathf.Clamp(
                1.15f -
                ((difficulty - 1) * 0.05f),
                0.45f,
                1.15f
            );

        float timer =
            thinkingTime;

        while (timer > 0f)
        {
            UpdateTimerText(
                Mathf.Max(
                    0f,
                    timer
                )
            );

            timer -=
                Time.deltaTime;

            yield return null;
        }

        if (answerInput != null)
        {
            answerInput.text =
                correctAnswer.ToString();
        }

        yield return new WaitForSeconds(
            0.25f
        );

        float chance =
            Mathf.Clamp(
                0.96f -
                ((difficulty - 1) * 0.11f),
                0.12f,
                0.96f
            );

        Finish(
            UnityEngine.Random.value <
            chance
        );
    }

    private void HandleDirectKeyboardInput()
    {
        Keyboard keyboard =
            Keyboard.current;

        if (keyboard == null)
            return;

        if (
            keyboard.enterKey
                .wasPressedThisFrame)
        {
            SubmitPlayerAnswer();
            return;
        }

        if (
            keyboard.backspaceKey
                .wasPressedThisFrame)
        {
            if (answerInput == null)
                return;

            string current =
                answerInput.text;

            if (
                !string.IsNullOrEmpty(
                    current))
            {
                answerInput.text =
                    current.Substring(
                        0,
                        current.Length - 1
                    );
            }

            return;
        }

        if (answerInput == null)
            return;

        if (answerInput.text.Length >= 2)
            return;

        int digit = -1;

        if (
            keyboard.digit0Key
                .wasPressedThisFrame)
        {
            digit = 0;
        }
        else if (
            keyboard.digit1Key
                .wasPressedThisFrame)
        {
            digit = 1;
        }
        else if (
            keyboard.digit2Key
                .wasPressedThisFrame)
        {
            digit = 2;
        }
        else if (
            keyboard.digit3Key
                .wasPressedThisFrame)
        {
            digit = 3;
        }
        else if (
            keyboard.digit4Key
                .wasPressedThisFrame)
        {
            digit = 4;
        }
        else if (
            keyboard.digit5Key
                .wasPressedThisFrame)
        {
            digit = 5;
        }
        else if (
            keyboard.digit6Key
                .wasPressedThisFrame)
        {
            digit = 6;
        }
        else if (
            keyboard.digit7Key
                .wasPressedThisFrame)
        {
            digit = 7;
        }
        else if (
            keyboard.digit8Key
                .wasPressedThisFrame)
        {
            digit = 8;
        }
        else if (
            keyboard.digit9Key
                .wasPressedThisFrame)
        {
            digit = 9;
        }

        if (digit >= 0)
        {
            answerInput.text +=
                digit.ToString();
        }
    }

    private void HandleDirectMouseInput()
    {
        Mouse mouse =
            Mouse.current;

        if (mouse == null)
            return;

        if (
            !mouse.leftButton
                .wasPressedThisFrame)
        {
            return;
        }

        if (submitButton == null)
            return;

        RectTransform rect =
            submitButton.transform
                as RectTransform;

        if (rect == null)
            return;

        Canvas canvas =
            submitButton
                .GetComponentInParent<Canvas>();

        Camera uiCamera = null;

        if (
            canvas != null &&
            canvas.renderMode !=
            RenderMode.ScreenSpaceOverlay)
        {
            uiCamera =
                canvas.worldCamera;
        }

        bool inside =
            RectTransformUtility
                .RectangleContainsScreenPoint(
                    rect,
                    mouse.position.ReadValue(),
                    uiCamera
                );

        if (inside)
        {
            SubmitPlayerAnswer();
        }
    }

    private void SubmitPlayerAnswer()
    {
        if (
            !running ||
            !acceptingAnswer ||
            isAI)
        {
            return;
        }

        acceptingAnswer = false;

        bool parsed =
            int.TryParse(
                answerInput != null
                    ? answerInput.text.Trim()
                    : "",
                out int playerAnswer
            );

        Finish(
            parsed &&
            playerAnswer ==
            correctAnswer
        );
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

        MinigameManager.Instance
            ?.OnMinigameCompleted(won);
    }

    private void SpawnDice(int count)
    {
        if (dicePrefab == null)
        {
            Debug.LogError(
                "SpeedDiceMinigame: Dice Prefab is missing."
            );

            Finish(false);
            return;
        }

        Transform center =
            diceSpawnArea != null
                ? diceSpawnArea
                : transform;

        float usableWidth =
            Mathf.Max(
                1f,
                spawnAreaSize.x
            );

        float spacing =
            count <= 1
                ? 0f
                : Mathf.Min(
                    horizontalSpacing,
                    usableWidth /
                    (count + 0.5f)
                );

        for (
            int i = 0;
            i < count;
            i++)
        {
            float x =
                (
                    i -
                    (count - 1) * 0.5f
                ) * spacing;

            float z =
                Random.Range(
                    -0.20f,
                    0.20f
                );

            Vector3 local =
                new Vector3(
                    x,
                    spawnHeight +
                    (i * 0.05f),
                    z
                );

            SpeedDie die =
                Instantiate(
                    dicePrefab,
                    center.TransformPoint(
                        local
                    ),
                    UnityEngine.Random.rotation
                );

            spawnedDice.Add(die);

            die.PrepareForRoll();

            Rigidbody body =
                die.Rigidbody;

            if (body == null)
            {
                Debug.LogError(
                    "SpeedDiceMinigame: Dice prefab needs a Rigidbody."
                );

                continue;
            }

            float directionX =
                x >= 0f
                    ? -1f
                    : 1f;

            if (Mathf.Abs(x) < 0.05f)
            {
                directionX =
                    (i % 2 == 0)
                        ? -1f
                        : 1f;
            }

            Vector3 direction =
                new Vector3(
                    directionX *
                    Random.Range(
                        0.25f,
                        0.7f
                    ),
                    Random.Range(
                        0.25f,
                        0.55f
                    ),
                    Random.Range(
                        -0.25f,
                        0.25f
                    )
                ).normalized;

            body.AddForce(
                direction *
                rollForce,
                ForceMode.Impulse
            );

            body.AddTorque(
                UnityEngine.Random
                    .insideUnitSphere *
                rollTorque,
                ForceMode.Impulse
            );
        }
    }

    private void CreateSafetyWalls()
    {
        Transform center =
            diceSpawnArea != null
                ? diceSpawnArea
                : transform;

        safetyWallsRoot =
            new GameObject(
                "SpeedDiceSafetyWalls"
            );

        safetyWallsRoot.transform
            .SetParent(
                center,
                false
            );

        float halfX =
            spawnAreaSize.x * 0.5f +
            wallPadding;

        float halfZ =
            spawnAreaSize.z * 0.5f +
            wallPadding;

        CreateWall(
            "LeftWall",
            new Vector3(
                -halfX,
                wallHeight * 0.5f,
                0f
            ),
            new Vector3(
                wallThickness,
                wallHeight,
                spawnAreaSize.z +
                wallThickness
            )
        );

        CreateWall(
            "RightWall",
            new Vector3(
                halfX,
                wallHeight * 0.5f,
                0f
            ),
            new Vector3(
                wallThickness,
                wallHeight,
                spawnAreaSize.z +
                wallThickness
            )
        );

        CreateWall(
            "FrontWall",
            new Vector3(
                0f,
                wallHeight * 0.5f,
                halfZ
            ),
            new Vector3(
                spawnAreaSize.x +
                wallThickness,
                wallHeight,
                wallThickness
            )
        );

        CreateWall(
            "BackWall",
            new Vector3(
                0f,
                wallHeight * 0.5f,
                -halfZ
            ),
            new Vector3(
                spawnAreaSize.x +
                wallThickness,
                wallHeight,
                wallThickness
            )
        );
    }

    private void CreateWall(
        string wallName,
        Vector3 localPosition,
        Vector3 localSize)
    {
        if (safetyWallsRoot == null)
            return;

        GameObject wall =
            new GameObject(
                wallName
            );

        wall.transform.SetParent(
            safetyWallsRoot.transform,
            false
        );

        wall.transform.localPosition =
            localPosition;

        BoxCollider collider =
            wall.AddComponent<BoxCollider>();

        collider.size =
            localSize;

        collider.isTrigger = false;
    }

    private void UpdateTimerText(
        float time)
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
    }

    private void ClearDice()
    {
        for (
            int i =
                spawnedDice.Count - 1;
            i >= 0;
            i--)
        {
            if (
                spawnedDice[i] != null)
            {
                Destroy(
                    spawnedDice[i].gameObject
                );
            }
        }

        spawnedDice.Clear();
    }

    private void ClearSafetyWalls()
    {
        if (safetyWallsRoot != null)
        {
            Destroy(
                safetyWallsRoot
            );

            safetyWallsRoot = null;
        }
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
        acceptingAnswer = false;

        ClearDice();
        ClearSafetyWalls();
    }

    private void OnDisable()
    {
        StopCurrentGame();
    }
}