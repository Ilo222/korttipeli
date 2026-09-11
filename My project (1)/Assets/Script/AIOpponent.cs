using System.Collections.Generic;
using UnityEngine;

public class AIOpponent : MonoBehaviour
{
    [Header("AI Identity")]
    public string opponentName = "AI";

    [Header("AI Difficulty")]
    [Range(0.1f, 1f)]
    public float challengeChance = 0.75f;

    public CardData ChooseCard(
        List<CardData> hand,
        CardData topCard)
    {
        List<CardData> playable =
            new List<CardData>();

        foreach (CardData card in hand)
        {
            if (card.CanPlay(topCard))
                playable.Add(card);
        }

        if (playable.Count == 0)
            return null;

        // Prefer normal number cards.
        foreach (CardData card in playable)
        {
            if (card.type == CardType.Number)
                return card;
        }

        // Otherwise play the first special.
        return playable[0];
    }

    public bool ShouldChallenge(int difficulty)
    {
        float chance =
            Mathf.Clamp(
                challengeChance -
                difficulty * 0.15f,
                0.10f,
                0.75f);

        return Random.value < chance;
    }

    public bool SimulateMinigame(int difficulty)
    {
        float winChance =
            Mathf.Clamp(
                0.90f -
                difficulty * 0.18f,
                0.05f,
                0.90f);

        bool result =
            Random.value < winChance;

        Debug.Log(
            $"{opponentName} challenge: " +
            $"{(result ? "WIN" : "LOSE")} " +
            $"at difficulty {difficulty}");

        return result;
    }
}