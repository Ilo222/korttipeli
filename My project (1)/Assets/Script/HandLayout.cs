using System.Collections.Generic;
using UnityEngine;

public class HandLayout : MonoBehaviour
{
    [Header("Spacing")]
    [SerializeField] private float cardSpacing = 0.55f;

    [Header("Curve")]
    [SerializeField] private float cardArc = 0.10f;

    [Header("Height")]
    [SerializeField] private float cardHeight = 0.04f;

    public void UpdateHandLayout(List<Card3D> cards)
    {
        int count = cards.Count;

        for (int i = 0; i < count; i++)
        {
            float offset =
                i - (count - 1) / 2f;

            float x =
                offset * cardSpacing;

            float z =
                -Mathf.Abs(offset) * cardArc;

            Transform card =
                cards[i].transform;

            card.SetParent(transform);

            card.localPosition =
                new Vector3(
                    x,
                    cardHeight,
                    z);

            // The anchor controls the direction
            // of this player's hand.
            card.localRotation =
                Quaternion.identity;

            cards[i].SetCardBackVisible(false);
        }
    }
}