using UnityEngine;

public class SpeedDie : MonoBehaviour
{
    [Header("Physics")]
    [SerializeField] private Rigidbody dieRigidbody;

    [Header("Face Markers")]
    [Tooltip("Each marker's +Y axis must point OUT of that die face.")]
    [SerializeField] private Transform face1;
    [SerializeField] private Transform face2;
    [SerializeField] private Transform face3;
    [SerializeField] private Transform face4;
    [SerializeField] private Transform face5;
    [SerializeField] private Transform face6;

    public Rigidbody Rigidbody => dieRigidbody;

    private void Awake()
    {
        if (dieRigidbody == null)
            dieRigidbody = GetComponent<Rigidbody>();
    }

    public void PrepareForRoll()
    {
        if (dieRigidbody == null)
            return;

        dieRigidbody.isKinematic = false;
        dieRigidbody.linearVelocity = Vector3.zero;
        dieRigidbody.angularVelocity = Vector3.zero;
        dieRigidbody.WakeUp();
    }

    public bool TryGetTopValue(out int value)
    {
        value = 0;

        Transform[] faces =
        {
            face1, face2, face3,
            face4, face5, face6
        };

        float bestDot = -2f;
        int bestValue = 0;

        for (int i = 0; i < faces.Length; i++)
        {
            Transform face = faces[i];
            if (face == null)
                continue;

            float dot = Vector3.Dot(
                face.up.normalized,
                Vector3.up
            );

            if (dot > bestDot)
            {
                bestDot = dot;
                bestValue = i + 1;
            }
        }

        if (bestValue <= 0)
            return false;

        value = bestValue;
        return true;
    }

    public bool IsSettled(
        float linearSpeedThreshold,
        float angularSpeedThreshold,
        float uprightThreshold)
    {
        if (dieRigidbody == null)
            return false;

        if (dieRigidbody.linearVelocity.magnitude > linearSpeedThreshold)
            return false;

        if (dieRigidbody.angularVelocity.magnitude > angularSpeedThreshold)
            return false;

        if (!TryGetTopValue(out _))
            return false;

        Transform[] faces =
        {
            face1, face2, face3,
            face4, face5, face6
        };

        float bestDot = -2f;

        foreach (Transform face in faces)
        {
            if (face == null)
                continue;

            float dot = Vector3.Dot(
                face.up.normalized,
                Vector3.up
            );

            if (dot > bestDot)
                bestDot = dot;
        }

        return bestDot >= uprightThreshold;
    }
}
