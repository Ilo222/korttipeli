using System.Collections.Generic;
using UnityEngine;

public class AIOpponent : MonoBehaviour
{
    [Header("Identity")]
    public string opponentName = "AI";

    [Header("Challenge Behavior")]
    [Range(0.1f, 1f)]
    public float challengeChance = 0.75f;

    // =========================================================
    // NORMAL CARD CHOICE
    // =========================================================

    public CardData ChooseCard(
        List<CardData> hand,
        CardData topCard,
        CardColor activeColor,
        int pendingDrawAmount = 0,
        CardType pendingDrawType = CardType.Number)
    {
        if (hand == null ||
            hand.Count == 0)
        {
            return null;
        }

        List<CardData> playable =
            new List<CardData>();

        foreach (CardData card in hand)
        {
            if (card == null)
                continue;

            if (CanPlayCard(
                    card,
                    topCard,
                    activeColor,
                    pendingDrawAmount,
                    pendingDrawType))
            {
                playable.Add(card);
            }
        }

        if (playable.Count == 0)
            return null;

        // -----------------------------------------------------
        // If a draw penalty is incoming, prioritize a
        // legal +2/+4 response.
        // -----------------------------------------------------

        if (pendingDrawAmount > 0)
        {
            foreach (CardData card in playable)
            {
                if (card.type == CardType.Draw4)
                    return card;
            }

            foreach (CardData card in playable)
            {
                if (card.type == CardType.Draw2)
                    return card;
            }

            return null;
        }

        // -----------------------------------------------------
        // Prefer normal number cards.
        // -----------------------------------------------------

        foreach (CardData card in playable)
        {
            if (card.type == CardType.Number)
                return card;
        }

        // -----------------------------------------------------
        // Otherwise use a special.
        // -----------------------------------------------------

        return playable[0];
    }

    // =========================================================
    // LEGAL PLAY CHECK
    // =========================================================

    public bool CanPlayCard(
        CardData card,
        CardData topCard,
        CardColor activeColor,
        int pendingDrawAmount,
        CardType pendingDrawType)
    {
        if (card == null)
            return false;

        // -----------------------------------------------------
        // DRAW STACK
        // -----------------------------------------------------

        if (pendingDrawAmount > 0)
        {
            if (pendingDrawType == CardType.Draw2)
            {
                return card.type == CardType.Draw2 ||
                       card.type == CardType.Draw4;
            }

            if (pendingDrawType == CardType.Draw4)
            {
                return card.type == CardType.Draw4;
            }

            return false;
        }

        // -----------------------------------------------------
        // SPECIALS ARE JOKERS
        // -----------------------------------------------------

        if (card.type != CardType.Number)
            return true;

        if (topCard == null)
            return true;

        // Match active color.
        if (activeColor != CardColor.Wild &&
            card.color == activeColor)
        {
            return true;
        }

        // Match number.
        if (topCard.type == CardType.Number &&
            card.number == topCard.number)
        {
            return true;
        }

        return false;
    }

    // =========================================================
    // CHALLENGE DECISION
    // =========================================================

    public bool ShouldChallenge(
        int difficulty)
    {
        float chance =
            Mathf.Clamp(
                challengeChance -
                difficulty * 0.15f,
                0.10f,
                0.75f
            );

        return Random.value < chance;
    }

    // =========================================================
    // MINIGAME SIMULATION
    // =========================================================

    public bool SimulateMinigame(
        int difficulty)
    {
        float winChance =
            Mathf.Clamp(
                0.90f -
                difficulty * 0.18f,
                0.05f,
                0.90f
            );

        return Random.value < winChance;
    }
}