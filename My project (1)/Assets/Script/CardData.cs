public enum CardColor { Red, Yellow, Green, Blue, Wild }
public enum CardType { Number, Draw2, Draw4, Reverse, Skip, ColorChange }

[System.Serializable]
public class CardData
{
    public CardColor color;
    public CardType type;
    public int number;

    public CardData(CardColor c, CardType t, int n = -1) { color = c; type = t; number = n; }

    public bool CanPlay(CardData top)
    {
        if (type != CardType.Number) return true; // Specials act as Jokers
        return color == top.color || number == top.number;
    }
}