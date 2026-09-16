using System.Collections.Generic;
using UnityEngine;

public class HandLayout : MonoBehaviour
{
    [Header("Card Size")]
    [SerializeField] private float cardWidth = 1.2f;

    [Header("Overlap")]
    [Range(0f, 0.8f)]
    [SerializeField] private float overlap = 0.48f;

    [Header("Fan")]
    [SerializeField] private float fanAngle = 28f;
    [SerializeField] private float fanDepth = 0.12f;

    [Header("Hand Width")]
    [SerializeField] private float maximumHandWidth = 5.5f;

    [Header("Height")]
    [SerializeField] private float cardHeight = 0.08f;
    [SerializeField] private float centerLift = 0.05f;

    [Header("Animation")]
    [SerializeField] private float movementSpeed = 12f;

    public void UpdateHandLayout(
        List<Card3D> cards)
    {
        if (cards == null ||
            cards.Count == 0)
        {
            return;
        }

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
                    maxSpacing);
        }

        float totalWidth =
            spacing * (count - 1);

        for (int i = 0;
             i < count;
             i++)
        {
            Card3D card3D = cards[i];

            if (card3D == null)
                continue;

            card3D.SetHovered(false);

            float normalized =
                count == 1
                    ? 0f
                    : (float)i / (count - 1);

            float centered =
                normalized * 2f - 1f;

            float x =
                centered *
                totalWidth *
                0.5f;

            float middleFactor =
                1f -
                Mathf.Abs(centered);

            float y =
                cardHeight +
                middleFactor * centerLift;

            float z =
                -middleFactor * fanDepth;

            float rotationY =
                -centered * fanAngle;

            Transform card =
                card3D.transform;

            card.SetParent(
                transform
            );

            Vector3 targetPosition =
                new Vector3(
                    x,
                    y,
                    z
                );

            Quaternion targetRotation =
                Quaternion.Euler(
                    0f,
                    rotationY,
                    0f
                );

            // Store the target as the card's normal
            // transform, but DON'T instantly move there.
            StartCoroutine(
                MoveCardToHandPosition(
                    card3D,
                    targetPosition,
                    targetRotation
                )
            );
        }
    }

    private System.Collections.IEnumerator
        MoveCardToHandPosition(
            Card3D card,
            Vector3 targetPosition,
            Quaternion targetRotation)
    {
        card.CaptureCurrentTransformAsNormal();

        float timer = 0f;
        float duration =
            1f / Mathf.Max(movementSpeed, 0.01f);

        Vector3 startPosition =
            card.transform.localPosition;

        Quaternion startRotation =
            card.transform.localRotation;

        while (timer < duration)
        {
            if (card == null)
                yield break;

            float t =
                timer / duration;

            t = Mathf.SmoothStep(
                0f,
                1f,
                t
            );

            card.transform.localPosition =
                Vector3.Lerp(
                    startPosition,
                    targetPosition,
                    t
                );

            card.transform.localRotation =
                Quaternion.Slerp(
                    startRotation,
                    targetRotation,
                    t
                );

            timer += Time.deltaTime;

            yield return null;
        }

        card.transform.localPosition =
            targetPosition;

        card.transform.localRotation =
            targetRotation;

        card.CaptureCurrentTransformAsNormal();
    }
}