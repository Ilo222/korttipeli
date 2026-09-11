using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class ChanceMinigame : MonoBehaviour
{
    public float startWinChance = 75f;      // starting win chance, in percent
    public float winChanceDecrease = 7f;    // how much the win chance drops after each win
    public float minWinChance = 1f;         // floor so it can't go negative
    public TMP_Text text;

    private float currentWinChance;

    void Start()
    {
        currentWinChance = startWinChance;
        text.text = "Press Space to try your luck!";
    }

    void Update()
    {
        bool pressed = Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame;

        if (pressed)
        {
            Roll();
        }
    }

    void Roll()
    {
        float roll = Random.Range(0f, 100f); // 0 to 100, exclusive-ish on the high end

        if (roll < currentWinChance)
        {
            text.text = $"You win! (was {currentWinChance:F1}% chance)";
            currentWinChance = Mathf.Max(minWinChance, currentWinChance - winChanceDecrease);
        }
        else
        {
            text.text = $"You lose! (was {currentWinChance:F1}% chance)";
        }
    }

    public void ResetChance()
    {
        currentWinChance = startWinChance;
        text.text = "Press Space to try your luck!";
    }
}