using System.Collections.Generic;
using UnityEngine;

public class AIHandLayout : MonoBehaviour
{
    [Header("Layout")]
    [SerializeField] private float cardWidth = 1.2f;

    [Range(0f, 0.8f)]
    [SerializeField] private float overlap = 0.50f;

    [SerializeField] private float maximumHandWidth = 4.5f;

    [Header("Height")]
    [SerializeField] private float cardHeight = 0.04f;

    public void UpdateAIHand(List<Card3D> cards)
    {
        if (cards == null ||
            cards.Count == 0)
            return;

        int count = cards.Count;

        float spacing =
            cardWidth * (1f - overlap);

        if (count > 1)
        {
            float maxSpacing =
                maximumHandWidth /
                (count - 1);

            spacing =
                Mathf.Min(
                    spacing,
                    maxSpacing
                );
        }

        for (int i = 0; i < count; i++)
        {
            Card3D card3D = cards[i];

            if (card3D == null)
                continue;

            card3D.SetHovered(false);

            float offset =
                i - (count - 1) / 2f;

            float x =
                offset * spacing;

            Transform card =
                card3D.transform;

            card.SetParent(transform);

            card.localPosition =
                new Vector3(
                    x,
                    cardHeight,
                    0f
                );

            card.localRotation =
                Quaternion.identity;

            card3D.SetCardBackVisible(true);

            card3D.CaptureCurrentTransformAsNormal();
        }
    }
}