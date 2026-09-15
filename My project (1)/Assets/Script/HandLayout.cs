using System.Collections.Generic;
using UnityEngine;

public class HandLayout : MonoBehaviour
{
    [Header("Card Size")]
    [SerializeField] private float cardWidth = 1.2f;

    [Header("Fan Layout")]
    [Range(0f, 0.8f)]
    [SerializeField] private float overlap = 0.48f;

    [SerializeField] private float fanAngle = 28f;

    [SerializeField] private float fanDepth = 0.12f;

    [Header("Hand Size")]
    [SerializeField] private float maximumHandWidth = 5.5f;

    [Header("Card Height")]
    [SerializeField] private float cardHeight = 0.08f;

    [Header("Center Lift")]
    [SerializeField] private float centerLift = 0.05f;

    public void UpdateHandLayout(List<Card3D> cards)
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

        float totalWidth =
            spacing * (count - 1);

        for (int i = 0; i < count; i++)
        {
            Card3D card3D = cards[i];

            if (card3D == null)
                continue;

            // Clear old hover state before laying the hand out.
            card3D.SetHovered(false);

            float normalized;

            if (count == 1)
            {
                normalized = 0f;
            }
            else
            {
                normalized =
                    (float)i /
                    (count - 1);
            }

            // -1 = left
            //  0 = center
            // +1 = right
            float centered =
                normalized * 2f - 1f;

            float x =
                centered *
                totalWidth *
                0.5f;

            float middleFactor =
                1f - Mathf.Abs(centered);

            float y =
                cardHeight +
                middleFactor * centerLift;

            float z =
                -middleFactor * fanDepth;

            float rotationY =
                -centered * fanAngle;

            Transform card =
                card3D.transform;

            card.SetParent(transform);

            card.localPosition =
                new Vector3(
                    x,
                    y,
                    z
                );

            card.localRotation =
                Quaternion.Euler(
                    0f,
                    rotationY,
                    0f
                );

            card3D.CaptureCurrentTransformAsNormal();
        }
    }
}