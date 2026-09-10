using System.Collections.Generic;
using UnityEngine;

public class HandLayout : MonoBehaviour
{
    public float cardSpacing = 0.8f;
    public float arcCurve = 0.1f;
    public float rotationFan = 5.0f;

    public void UpdateHandLayout(List<Card3D> handCards)
    {
        int count = handCards.Count;
        for (int i = 0; i < count; i++)
        {
            // Center the fan around 0
            float offset = (i - (count - 1) / 2f);

            // Calculate positions
            float xPos = offset * cardSpacing;
            // Negative parabola for the arc: middle cards are higher
            float yPos = -Mathf.Abs(offset) * arcCurve;
            // Layering so cards overlap correctly
            float zPos = -i * 0.01f;

            // Calculate rotation
            float zRot = -offset * rotationFan;

            // Apply to card
            Transform cardTransform = handCards[i].transform;
            cardTransform.SetParent(this.transform);

            // Move smoothly to the new position in hand
            cardTransform.localPosition = new Vector3(xPos, yPos, zPos);
            cardTransform.localRotation = Quaternion.Euler(0, 0, zRot);
        }
    }
}