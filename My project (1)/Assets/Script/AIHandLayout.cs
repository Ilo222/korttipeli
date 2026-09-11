using System.Collections.Generic;
using UnityEngine;

public class AIHandLayout : MonoBehaviour
{
    [SerializeField] private float cardSpacing = 0.55f;
    [SerializeField] private float cardHeight = 0.04f;

    public void UpdateAIHand(List<Card3D> cards)
    {
        int count = cards.Count;

        for (int i = 0; i < count; i++)
        {
            float offset =
                i - (count - 1) / 2f;

            float x =
                offset * cardSpacing;

            Card3D card3D =
                cards[i];

            Transform card =
                card3D.transform;

            card.SetParent(transform);

            card.localPosition =
                new Vector3(
                    x,
                    cardHeight,
                    0f);

            card.localRotation =
                Quaternion.identity;

            // Hide information from the player.
            card3D.SetCardBackVisible(true);
        }
    }
}