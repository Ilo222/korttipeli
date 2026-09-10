using System.Transactions;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public GameObject cardPrefab;
    public CardData currentTopCard;

    private void Awake() => Instance = this;

    private void Start()
    {
        // Placeholder for initial deck setup
        currentTopCard = new CardData(CardColor.Red, CardType.Number, 3);
    }

    public void PlayCard(Card3D clickedCard)
    {
        if (clickedCard.data.CanPlay(currentTopCard))
        {
            currentTopCard = clickedCard.data;
            clickedCard.transform.position = Vector3.zero; // Move to center table

            if (currentTopCard.type != CardType.Number)
            {
                // Trigger WarioWare Challenge
                TransitionManager.Instance.StartChallenge("CHALLENGE!");
            }
        }
    }
}