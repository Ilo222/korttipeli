using UnityEngine;

public class CardVisualTest : MonoBehaviour
{
    [SerializeField] private Card3D card;

    private void Start()
    {
        CardData testCard =
            new CardData(
                CardColor.Red,
                CardType.Number,
                0
            );

        card.SetupCard(testCard);

        card.SetCardBackVisible(true);

        Debug.Log(
            "Card back test."
        );
    }
}