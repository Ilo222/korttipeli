using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class PhysicalBalanceMinigame : MonoBehaviour
{
    [Header("3D Setup")]
    [SerializeField] private Rigidbody balancePlatform;

    [Header("Controls")]
    [SerializeField] private float playerTorque = 7f;
    [SerializeField] private float aiControlStrength = 4f;
    [SerializeField] private float difficultyDrift = 0.6f;

    [Header("Difficulty")]
    [SerializeField] private float baseDuration = 7f;
    [SerializeField] private float durationReductionPerLevel = 0.25f;
    [SerializeField] private float minimumDuration = 4f;
    [SerializeField] private float baseFailAngle = 18f;
    [SerializeField] private float failAngleReductionPerLevel = 0.55f;
    [SerializeField] private float minimumFailAngle = 9f;

    [Header("HUD")]
    [SerializeField] private GameObject physicalHUD;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI balanceText;

    private bool running;
    private bool aiControlled;
    private int difficulty;
    private float timeRemaining;
    private Coroutine gameRoutine;
    private Quaternion startingRotation;

    public void StartGame(int difficultyLevel, bool ai)
    {
        StopCurrentGame();

        difficulty = Mathf.Max(1, difficultyLevel);
        aiControlled = ai;

        if (physicalHUD != null)
            physicalHUD.SetActive(true);

        if (balancePlatform == null)
        {
            Debug.LogError("PhysicalBalanceMinigame: Balance Platform Rigidbody is missing.");
            Finish(false);
            return;
        }

        balancePlatform.linearVelocity = Vector3.zero;
        balancePlatform.angularVelocity = Vector3.zero;
        startingRotation = balancePlatform.transform.rotation;

        timeRemaining = Mathf.Max(
            minimumDuration,
            baseDuration - ((difficulty - 1) * durationReductionPerLevel)
        );

        running = true;
        gameRoutine = StartCoroutine(RunGame());
    }

    private IEnumerator RunGame()
    {
        while (running && timeRemaining > 0f)
        {
            float currentAngle = GetSignedBalanceAngle();
            float failAngle = Mathf.Max(
                minimumFailAngle,
                baseFailAngle - ((difficulty - 1) * failAngleReductionPerLevel)
            );

            UpdateHUD(timeRemaining, currentAngle, failAngle);

            if (Mathf.Abs(currentAngle) >= failAngle)
            {
                Finish(false);
                yield break;
            }

            if (aiControlled)
                RunAIController(currentAngle);
            else
                RunPlayerController();

            ApplyDifficultyDrift();

            timeRemaining -= Time.deltaTime;
            yield return null;
        }

        if (running)
            Finish(true);
    }

    private void RunPlayerController()
    {
        if (Keyboard.current == null)
            return;

        float input = 0f;

        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
            input -= 1f;

        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
            input += 1f;

        if (Mathf.Abs(input) > 0.01f)
        {
            balancePlatform.AddTorque(
                Vector3.forward * input * playerTorque,
                ForceMode.Acceleration
            );
        }
    }

    private void RunAIController(float currentAngle)
    {
        float error = -currentAngle;
        float correction = Mathf.Clamp(error / 10f, -1f, 1f);
        float noise = Random.Range(-0.18f, 0.18f) * Mathf.Clamp01((difficulty - 1) * 0.08f);

        correction += noise;

        balancePlatform.AddTorque(
            Vector3.forward * correction * aiControlStrength,
            ForceMode.Acceleration
        );
    }

    private void ApplyDifficultyDrift()
    {
        float levelFactor = Mathf.Max(0f, difficulty - 1);
        float direction = Mathf.Sin(Time.time * (0.75f + levelFactor * 0.08f));

        balancePlatform.AddTorque(
            Vector3.forward * direction * difficultyDrift * (1f + levelFactor * 0.08f),
            ForceMode.Acceleration
        );
    }

    private float GetSignedBalanceAngle()
    {
        return Vector3.SignedAngle(
            Vector3.up,
            balancePlatform.transform.up,
            Vector3.forward
        );
    }

    private void UpdateHUD(float time, float angle, float failAngle)
    {
        if (timerText != null)
        {
            timerText.text = Mathf.CeilToInt(Mathf.Max(0f, time)).ToString();
        }

        if (balanceText != null)
        {
            float normalized = 1f - Mathf.Clamp01(
                Mathf.Abs(angle) / Mathf.Max(0.01f, failAngle)
            );

            balanceText.text = $"BALANCE {Mathf.RoundToInt(normalized * 100f)}%";
        }
    }

    private void Finish(bool won)
    {
        if (!running)
            return;

        running = false;

        if (balancePlatform != null)
        {
            balancePlatform.linearVelocity = Vector3.zero;
            balancePlatform.angularVelocity = Vector3.zero;
            balancePlatform.transform.rotation = startingRotation;
        }

        MinigameManager.Instance?.OnMinigameCompleted(won);
    }

    private void StopCurrentGame()
    {
        if (gameRoutine != null)
        {
            StopCoroutine(gameRoutine);
            gameRoutine = null;
        }

        running = false;

        if (balancePlatform != null)
        {
            balancePlatform.linearVelocity = Vector3.zero;
            balancePlatform.angularVelocity = Vector3.zero;
        }
    }

    private void OnDisable()
    {
        StopCurrentGame();
    }
}
