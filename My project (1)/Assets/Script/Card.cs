using UnityEngine;

public enum CardColor { Red, Yellow, Green, Blue, Wild }
public enum CardType { Number, Draw2, Draw4, Reverse, Skip, ColorChange }

[System.Serializable]
public class Card
{
    public CardColor color;
    public CardType type;
    public int number; // 0 to 4. Use -1 for Special Cards.

    public Card(CardColor color, CardType type, int number = -1)
    {
        this.color = color;
        this.type = type;
        this.number = number;
    }

    public bool CanBePlayedOn(Card topCard)
    {
        // Custom Rule: Special cards act as Jokers and can be played anytime
        if (type != CardType.Number) return true;

        return color == topCard.color || number == topCard.number;
    }
}