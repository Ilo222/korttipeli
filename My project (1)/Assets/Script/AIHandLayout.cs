using System.Collections.Generic;
using UnityEngine;

public class AIHandLayout : MonoBehaviour
{
    public float cardSpacing = 0.65f;
    public float cardHeight = 0.03f;

    public void UpdateAIHand(
        List<Card3D> cards)
    {
        int count = cards.Count;

        for (int i = 0; i < count; i++)
        {
            float offset =
                i - (count - 1) / 2f;

            float x =
                offset * cardSpacing;

            Transform card =
                cards[i].transform;

            card.SetParent(transform);

            card.localPosition =
                new Vector3(
                    x,
                    cardHeight,
                    0f);

            card.localRotation =
                Quaternion.Euler(
                    0f,
                    180f,
                    0f);

            card.SetCardBackVisible(true);
        }
    }
}