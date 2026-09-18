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

    // Used by ColorChange cards.
    // Remains Wild until a player/AI chooses a color.
    public CardColor chosenColor = CardColor.Wild;

    public CardData(
        CardColor color,
        CardType type,
        int number = -1)
    {
        this.color = color;
        this.type = type;
        this.number = number;

        if (type != CardType.ColorChange)
            chosenColor = CardColor.Wild;
    }

    public bool CanPlay(CardData topCard)
    {
        if (topCard == null)
            return true;

        // All special cards are jokers for
        // normal placement.
        if (type != CardType.Number)
            return true;

        return color == topCard.color ||
               number == topCard.number;
    }

    public bool IsSpecial()
    {
        return type != CardType.Number;
    }

    public bool IsDrawCard()
    {
        return type == CardType.Draw2 ||
               type == CardType.Draw4;
    }

    public int GetDrawValue()
    {
        if (type == CardType.Draw2)
            return 2;

        if (type == CardType.Draw4)
            return 4;

        return 0;
    }

    public CardColor GetEffectiveColor()
    {
        if (type == CardType.ColorChange)
            return chosenColor;

        return color;
    }

    public void SetChosenColor(CardColor newColor)
    {
        if (type != CardType.ColorChange)
            return;

        if (newColor == CardColor.Wild)
            return;

        chosenColor = newColor;
    }
}