using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class KnowledgeMinigame : MonoBehaviour
{
    [Serializable]
    private class Question
    {
        public int minimumDifficulty;
        public string question;
        public string[] answers;
        public int correctAnswerIndex;

        public Question(int difficulty, string text, string[] options, int correct)
        {
            minimumDifficulty = difficulty;
            question = text;
            answers = options;
            correctAnswerIndex = correct;
        }
    }

    [Header("Difficulty")]
    [SerializeField] private float baseTime = 8f;
    [SerializeField] private float minimumTime = 3f;
    [SerializeField] private float timeReductionPerLevel = 0.4f;

    [Header("HUD")]
    [SerializeField] private GameObject knowledgeHUD;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI questionText;
    [SerializeField] private Button answerA;
    [SerializeField] private Button answerB;
    [SerializeField] private Button answerC;
    [SerializeField] private Button answerD;

    [Header("Question Bank")]
    [SerializeField] private List<Question> questions = new();

    private Question currentQuestion;
    private Button[] answerButtons;
    private Coroutine gameRoutine;
    private bool running;
    private bool acceptingAnswer;
    private bool aiControlled;
    private int difficulty;
    private float timeRemaining;

    public void StartGame(int difficultyLevel, bool ai)
    {
        StopCurrentGame();

        difficulty = Mathf.Max(1, difficultyLevel);
        aiControlled = ai;

        if (knowledgeHUD != null)
            knowledgeHUD.SetActive(true);

        BuildDefaultQuestionsIfNeeded();

        answerButtons = new[]
        {
            answerA,
            answerB,
            answerC,
            answerD
        };

        SelectQuestion();

        if (currentQuestion == null)
        {
            Debug.LogError(
                "KnowledgeMinigame: No valid questions are available."
            );

            Finish(false);
            return;
        }

        ShowQuestion();

        timeRemaining = Mathf.Max(
            minimumTime,
            baseTime -
            ((difficulty - 1) * timeReductionPerLevel)
        );

        running = true;
        acceptingAnswer = true;

        gameRoutine = StartCoroutine(RunGame());
    }

    private void Update()
    {
        if (!running ||
            !acceptingAnswer ||
            aiControlled)
        {
            return;
        }

        HandleDirectMouseInput();
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
            acceptingAnswer &&
            timeRemaining > 0f)
        {
            UpdateTimerText(
                timeRemaining
            );

            timeRemaining -=
                Time.deltaTime;

            yield return null;
        }

        if (
            running &&
            acceptingAnswer)
        {
            acceptingAnswer = false;

            Finish(false);
        }
    }

    private IEnumerator RunAIAttempt()
    {
        float thinkingTime = Mathf.Clamp(
            1.25f -
            ((difficulty - 1) * 0.05f),
            0.55f,
            1.25f
        );

        float elapsed = 0f;

        while (elapsed < thinkingTime)
        {
            UpdateTimerText(
                Mathf.Max(
                    0f,
                    timeRemaining - elapsed
                )
            );

            elapsed += Time.deltaTime;

            yield return null;
        }

        float successChance = Mathf.Clamp(
            0.96f -
            ((difficulty - 1) * 0.12f),
            0.10f,
            0.96f
        );

        bool knowsAnswer =
            UnityEngine.Random.value <
            successChance;

        int selectedAnswer = knowsAnswer
            ? currentQuestion.correctAnswerIndex
            : GetWrongAnswerIndex();

        HighlightSelectedButton(
            selectedAnswer
        );

        yield return new WaitForSeconds(
            0.35f
        );

        Finish(
            selectedAnswer ==
            currentQuestion.correctAnswerIndex
        );
    }

    private void ShowQuestion()
    {
        if (questionText != null)
            questionText.text =
                currentQuestion.question;

        for (
            int i = 0;
            i < answerButtons.Length;
            i++)
        {
            Button button =
                answerButtons[i];

            if (button == null)
                continue;

            // Keep the existing UI setup.
            // We no longer rely on onClick for the player.
            button.onClick.RemoveAllListeners();

            button.interactable =
                !aiControlled;

            int capturedIndex = i;

            // Keep this listener as a fallback.
            button.onClick.AddListener(
                () =>
                    OnPlayerAnswer(
                        capturedIndex
                    )
            );

            TextMeshProUGUI text =
                button.GetComponentInChildren<
                    TextMeshProUGUI
                >();

            if (text != null)
            {
                text.text =
                    currentQuestion.answers[i];
            }
        }
    }

    private void HandleDirectMouseInput()
    {
        Mouse mouse =
            Mouse.current;

        if (mouse == null)
            return;

        if (!mouse.leftButton.wasPressedThisFrame)
            return;

        Vector2 mousePosition =
            mouse.position.ReadValue();

        for (
            int i = 0;
            i < answerButtons.Length;
            i++)
        {
            Button button =
                answerButtons[i];

            if (button == null)
                continue;

            if (!button.gameObject.activeInHierarchy)
                continue;

            RectTransform rect =
                button.transform as RectTransform;

            if (rect == null)
                continue;

            Canvas canvas =
                button.GetComponentInParent<Canvas>();

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
                        mousePosition,
                        uiCamera
                    );

            if (!inside)
                continue;

            OnPlayerAnswer(i);

            return;
        }
    }

    private void OnPlayerAnswer(
        int answerIndex)
    {
        if (
            !running ||
            !acceptingAnswer ||
            aiControlled)
        {
            return;
        }

        if (
            answerIndex < 0 ||
            answerIndex >= answerButtons.Length)
        {
            return;
        }

        acceptingAnswer = false;

        HighlightSelectedButton(
            answerIndex
        );

        StartCoroutine(
            FinishAfterAnswer(
                answerIndex ==
                currentQuestion.correctAnswerIndex
            )
        );
    }

    private IEnumerator FinishAfterAnswer(
        bool won)
    {
        yield return new WaitForSeconds(
            0.35f
        );

        Finish(won);
    }

    private void HighlightSelectedButton(
        int selectedIndex)
    {
        for (
            int i = 0;
            i < answerButtons.Length;
            i++)
        {
            if (answerButtons[i] != null)
            {
                answerButtons[i]
                    .interactable = false;
            }
        }
    }

    private int GetWrongAnswerIndex()
    {
        List<int> wrong = new();

        for (int i = 0; i < 4; i++)
        {
            if (
                i !=
                currentQuestion.correctAnswerIndex)
            {
                wrong.Add(i);
            }
        }

        return wrong[
            UnityEngine.Random.Range(
                0,
                wrong.Count
            )
        ];
    }

    private void SelectQuestion()
    {
        List<Question> candidates =
            new();

        foreach (
            Question question
            in questions)
        {
            if (
                question == null ||
                question.answers == null ||
                question.answers.Length != 4)
            {
                continue;
            }

            if (
                question.minimumDifficulty <=
                difficulty)
            {
                candidates.Add(question);
            }
        }

        if (candidates.Count == 0)
            candidates = questions;

        if (candidates.Count == 0)
        {
            currentQuestion = null;
            return;
        }

        currentQuestion =
            candidates[
                UnityEngine.Random.Range(
                    0,
                    candidates.Count
                )
            ];
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

    private void Finish(bool won)
    {
        if (!running)
            return;

        running = false;
        acceptingAnswer = false;

        foreach (
            Button button
            in answerButtons)
        {
            if (button != null)
            {
                button.interactable = false;
            }
        }

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
        acceptingAnswer = false;
    }

    private void BuildDefaultQuestionsIfNeeded()
    {
        if (
            questions != null &&
            questions.Count > 0)
        {
            return;
        }

        questions = new List<Question>
        {
            new Question(
                1,
                "Which planet is known as the Red Planet?",
                new[]
                {
                    "Earth",
                    "Mars",
                    "Venus",
                    "Jupiter"
                },
                1
            ),

            new Question(
                1,
                "How many sides does a triangle have?",
                new[]
                {
                    "2",
                    "3",
                    "4",
                    "5"
                },
                1
            ),

            new Question(
                1,
                "What is the largest ocean on Earth?",
                new[]
                {
                    "Atlantic",
                    "Indian",
                    "Pacific",
                    "Arctic"
                },
                2
            ),

            new Question(
                3,
                "What is the chemical symbol for gold?",
                new[]
                {
                    "Ag",
                    "Au",
                    "Fe",
                    "Gd"
                },
                1
            ),

            new Question(
                3,
                "Which country gifted the Statue of Liberty to the United States?",
                new[]
                {
                    "France",
                    "Spain",
                    "Italy",
                    "Canada"
                },
                0
            ),

            new Question(
                3,
                "What is the capital of Australia?",
                new[]
                {
                    "Sydney",
                    "Melbourne",
                    "Perth",
                    "Canberra"
                },
                3
            ),

            new Question(
                5,
                "Which element has the atomic number 6?",
                new[]
                {
                    "Carbon",
                    "Oxygen",
                    "Nitrogen",
                    "Neon"
                },
                0
            ),

            new Question(
                5,
                "Who wrote The Odyssey?",
                new[]
                {
                    "Plato",
                    "Homer",
                    "Virgil",
                    "Socrates"
                },
                1
            ),

            new Question(
                5,
                "What is the square root of 144?",
                new[]
                {
                    "10",
                    "11",
                    "12",
                    "14"
                },
                2
            ),

            new Question(
                8,
                "Which moon is the largest moon of Saturn?",
                new[]
                {
                    "Europa",
                    "Titan",
                    "Triton",
                    "Ganymede"
                },
                1
            ),

            new Question(
                8,
                "Which ancient civilization built Machu Picchu?",
                new[]
                {
                    "Roman",
                    "Mayan",
                    "Inca",
                    "Egyptian"
                },
                2
            ),

            new Question(
                8,
                "What is the process by which plants convert light into chemical energy?",
                new[]
                {
                    "Respiration",
                    "Photosynthesis",
                    "Fermentation",
                    "Osmosis"
                },
                1
            ),

            new Question(
                12,
                "Which scientist formulated the three laws of motion?",
                new[]
                {
                    "Newton",
                    "Einstein",
                    "Kepler",
                    "Galileo"
                },
                0
            ),

            new Question(
                12,
                "Which empire used the road network known as the Qhapaq Nan?",
                new[]
                {
                    "Roman",
                    "Inca",
                    "Mongol",
                    "Ottoman"
                },
                1
            ),

            new Question(
                12,
                "What is the deepest known point in Earth's oceans called?",
                new[]
                {
                    "Puerto Rico Trench",
                    "Challenger Deep",
                    "Mariana Ridge",
                    "Java Trench"
                },
                1
            )
        };
    }

    private void OnDisable()
    {
        StopCurrentGame();
    }
}