using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LuckMinigame : MonoBehaviour
{
    [Header("Hats")]
    [SerializeField] private LuckHat[] hats;
    [SerializeField] private Transform[] hatPositions;
    [SerializeField] private Transform prizeObject;
    [SerializeField] private float prizeOffset = -0.15f;
    [SerializeField] private float shuffleDuration = 1.2f;
    [SerializeField] private int baseHatCount = 2;

    [Header("Difficulty")]
    [SerializeField] private int maximumHatCount = 10;
    [SerializeField] private float baseTime = 6f;
    [SerializeField] private float minimumTime = 3f;
    [SerializeField] private float timeReductionPerLevel = 0.3f;

    [Header("HUD")]
    [SerializeField] private GameObject luckHUD;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI instructionText;

    private readonly List<int> permutation = new();
    private Coroutine gameRoutine;
    private bool running;
    private bool acceptingPick;
    private bool aiControlled;
    private int difficulty;
    private int activeHatCount;
    private int winningVisibleIndex;
    private int winningSourceHat;
    private float timeRemaining;

    public void StartGame(int difficultyLevel, bool ai)
    {
        StopCurrentGame();

        difficulty = Mathf.Max(1, difficultyLevel);
        aiControlled = ai;

        if (luckHUD != null)
            luckHUD.SetActive(true);

        int availableHats = hats != null ? hats.Length : 0;
        int availablePositions = hatPositions != null ? hatPositions.Length : 0;
        int maximumAvailable = Mathf.Min(availableHats, availablePositions);

        activeHatCount = Mathf.Clamp(
            baseHatCount + (((difficulty - 1) / 2) * 2),
            baseHatCount,
            Mathf.Min(maximumHatCount, maximumAvailable)
        );

        if (activeHatCount < baseHatCount)
        {
            Debug.LogError("LuckMinigame: Assign enough hats and hat positions.");
            Finish(false);
            return;
        }

        ConfigureHats();

        timeRemaining = Mathf.Max(
            minimumTime,
            baseTime - ((difficulty - 1) * timeReductionPerLevel)
        );

        running = true;
        acceptingPick = false;
        gameRoutine = StartCoroutine(RunGame());
    }

    private IEnumerator RunGame()
    {
        CreatePermutation();
        PlaceHatsInPositions();

        winningVisibleIndex = Random.Range(0, activeHatCount);
        winningSourceHat = permutation[winningVisibleIndex];

        PlacePrizeUnderWinningHat();

        ShowInstruction("WATCH THE HATS!");
        yield return StartCoroutine(ShuffleHats());

        acceptingPick = true;
        ShowInstruction(aiControlled ? "AI IS PICKING..." : "PICK A HAT!");

        if (aiControlled)
        {
            yield return new WaitForSeconds(0.9f);

            int aiPick = ChooseAIHat();
            RevealChoice(aiPick);

            yield return new WaitForSeconds(0.7f);
            Finish(aiPick == winningVisibleIndex);
            yield break;
        }

        while (running && acceptingPick && timeRemaining > 0f)
        {
            UpdateTimerText(timeRemaining);
            timeRemaining -= Time.deltaTime;
            yield return null;
        }

        if (running && acceptingPick)
        {
            acceptingPick = false;
            Finish(false);
        }
    }

    public void TryPickHat(int visibleHatIndex)
    {
        if (!running || !acceptingPick || aiControlled)
            return;

        if (visibleHatIndex < 0 || visibleHatIndex >= activeHatCount)
            return;

        acceptingPick = false;
        RevealChoice(visibleHatIndex);

        StartCoroutine(FinishAfterReveal(
            visibleHatIndex == winningVisibleIndex
        ));
    }

    private IEnumerator FinishAfterReveal(bool won)
    {
        yield return new WaitForSeconds(0.8f);
        Finish(won);
    }

    private void ConfigureHats()
    {
        for (int i = 0; i < hats.Length; i++)
        {
            if (hats[i] == null)
                continue;

            hats[i].gameObject.SetActive(i < activeHatCount);
            hats[i].Configure(this, i);
        }
    }

    private void CreatePermutation()
    {
        permutation.Clear();

        for (int i = 0; i < activeHatCount; i++)
            permutation.Add(i);
    }

    private void PlaceHatsInPositions()
    {
        for (int visibleIndex = 0; visibleIndex < activeHatCount; visibleIndex++)
        {
            int sourceHat = permutation[visibleIndex];
            hats[sourceHat].transform.position = hatPositions[visibleIndex].position;
            hats[sourceHat].transform.rotation = hatPositions[visibleIndex].rotation;
        }

        HidePrize();
    }

    private void PlacePrizeUnderWinningHat()
    {
        if (prizeObject == null)
            return;

        Transform winningHat = hats[winningSourceHat].transform;

        prizeObject.SetParent(
            winningHat,
            false
        );

        prizeObject.localPosition =
            Vector3.up * prizeOffset;

        prizeObject.localRotation =
            Quaternion.identity;

        prizeObject.gameObject.SetActive(false);
    }

    private IEnumerator ShuffleHats()
    {
        int swaps = Mathf.Clamp(3 + difficulty, 3, 12);
        float swapDuration = shuffleDuration / swaps;

        for (int swap = 0; swap < swaps; swap++)
        {
            int a = Random.Range(0, activeHatCount);
            int b = Random.Range(0, activeHatCount);

            if (a == b)
            {
                swap--;
                continue;
            }

            Transform transformA = hats[permutation[a]].transform;
            Transform transformB = hats[permutation[b]].transform;

            Vector3 posA = transformA.position;
            Quaternion rotA = transformA.rotation;
            Vector3 posB = transformB.position;
            Quaternion rotB = transformB.rotation;

            float elapsed = 0f;

            while (elapsed < swapDuration)
            {
                float t = elapsed / swapDuration;

                transformA.position = Vector3.Lerp(posA, posB, t);
                transformA.rotation = Quaternion.Slerp(rotA, rotB, t);
                transformB.position = Vector3.Lerp(posB, posA, t);
                transformB.rotation = Quaternion.Slerp(rotB, rotA, t);

                elapsed += Time.deltaTime;
                yield return null;
            }

            transformA.position = posB;
            transformA.rotation = rotB;
            transformB.position = posA;
            transformB.rotation = rotA;

            int temp = permutation[a];
            permutation[a] = permutation[b];
            permutation[b] = temp;
        }
    }

    private int ChooseAIHat()
    {
        float smartness = Mathf.Clamp01(
            0.88f - ((difficulty - 1) * 0.09f)
        );

        if (Random.value < smartness)
            return winningVisibleIndex;

        return Random.Range(0, activeHatCount);
    }

    private void RevealChoice(int visibleHatIndex)
    {
        if (visibleHatIndex < 0 || visibleHatIndex >= activeHatCount)
            return;

        int chosenSourceHat = permutation[visibleHatIndex];
        bool correct = chosenSourceHat == winningSourceHat;

        ShowInstruction(correct ? "CORRECT!" : "WRONG!");

        if (correct)
        {
            prizeObject?.gameObject.SetActive(true);
            RaiseHat(hats[chosenSourceHat].transform);
        }
        else
        {
            RaiseHat(hats[chosenSourceHat].transform);
            prizeObject?.gameObject.SetActive(false);
        }
    }

    private void RaiseHat(Transform hat)
    {
        if (hat == null)
            return;

        hat.position += Vector3.up * 0.65f;
    }

    private void HidePrize()
    {
        if (prizeObject != null)
            prizeObject.gameObject.SetActive(false);
    }

    private void UpdateTimerText(float time)
    {
        if (timerText != null)
            timerText.text = Mathf.CeilToInt(Mathf.Max(0f, time)).ToString();
    }

    private void ShowInstruction(string text)
    {
        if (instructionText != null)
            instructionText.text = text;
    }

    private void Finish(bool won)
    {
        if (!running)
            return;

        running = false;
        acceptingPick = false;
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
        acceptingPick = false;
        HidePrize();
    }

    private void OnDisable()
    {
        StopCurrentGame();
    }
}
