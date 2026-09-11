using System;

public enum CardColor
{
    Purple,
    Green,
    Yellow,
    Red,
    Wild
}

public enum CardType
{
    Number,
    Draw2,
    Draw4,
    Reverse,
    Skip,
    ColorChange
}

[Serializable]
public class CardData
{
    public CardColor color;
    public CardType type;
    public int number;

    public CardData(
        CardColor color,
        CardType type,
        int number = -1)
    {
        this.color = color;
        this.type = type;
        this.number = number;
    }

    public bool CanPlay(CardData topCard)
    {
        if (topCard == null)
            return true;

        // Your custom rule:
        // every special card acts as a Joker/Wild.
        if (type != CardType.Number)
            return true;

        return color == topCard.color ||
               number == topCard.number;
    }

    public bool IsSpecial()
    {
        return type != CardType.Number;
    }
}