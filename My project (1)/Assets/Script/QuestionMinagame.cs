using UnityEngine;
using TMPro;

public class QuestionMinigame : MonoBehaviour
{
    [System.Serializable]
    public class Question
    {
        public string questionText;
        public string answer;
    }

    public Question[] questions;
    public TMP_Text questionDisplay;
    public TMP_InputField answerInput;
    public TMP_Text timerDisplay; // optional, shows time remaining
    public float timeLimit = 10f; // seconds allowed per question

    private int currentIndex;
    private bool locked;
    private float timeRemaining;

    void Start()
    {
        currentIndex = 0;
        ShowQuestion();

        answerInput.onSubmit.AddListener(OnSubmit);
    }

    void Update()
    {
        if (locked || currentIndex >= questions.Length) return;

        timeRemaining -= Time.deltaTime;

        if (timerDisplay != null)
        {
            timerDisplay.text = Mathf.Ceil(Mathf.Max(0f, timeRemaining)) + "s";
        }

        if (timeRemaining <= 0f)
        {
            TimeUp();
        }
    }

    void ShowQuestion()
    {
        locked = false;
        timeRemaining = timeLimit;

        if (currentIndex < questions.Length)
        {
            questionDisplay.text = questions[currentIndex].questionText;
            answerInput.text = "";
            answerInput.gameObject.SetActive(true);
            answerInput.ActivateInputField();
        }
        else
        {
            questionDisplay.text = "You finished all the questions!";
            answerInput.gameObject.SetActive(false);
            if (timerDisplay != null) timerDisplay.text = "";
        }
    }

    void OnSubmit(string typedAnswer)
    {
        CheckAnswer(typedAnswer);
    }

    public void CheckAnswer(string typedAnswer)
    {
        if (locked || currentIndex >= questions.Length) return;

        locked = true;
        answerInput.gameObject.SetActive(false);

        string correct = questions[currentIndex].answer.Trim().ToLower();
        string given = typedAnswer.Trim().ToLower();

        if (given == correct)
        {
            currentIndex++;
            ShowQuestion();
        }
        else
        {
            questionDisplay.text = "Wrong! The answer was: " + questions[currentIndex].answer;
        }
    }

    void TimeUp()
    {
        locked = true;
        answerInput.gameObject.SetActive(false);
        questionDisplay.text = "Time's up! The answer was: " + questions[currentIndex].answer;
    }
}