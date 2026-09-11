using UnityEngine;

public class MenuHand : MonoBehaviour
{
    public Transform hand;
    public Transform normalTransform; // Tyhj‰ objekti k‰den perusasennolle
    public float speed = 10f;

    private Transform currentTarget;

    void Start()
    {
        if (normalTransform != null)
        {
            currentTarget = normalTransform;
            hand.position = normalTransform.position;
            hand.rotation = normalTransform.rotation;
        }
    }

    void Update()
    {
        if (currentTarget != null)
        {
            // Liikutetaan ja k‰‰nnet‰‰n k‰tt‰ sulavasti kohteeseen maailmankoordinaateissa
            hand.position = Vector3.Lerp(hand.position, currentTarget.position, Time.deltaTime * speed);
            hand.rotation = Quaternion.Lerp(hand.rotation, currentTarget.rotation, Time.deltaTime * speed);
        }
    }

    public void SetTarget(Transform newTarget)
    {
        currentTarget = newTarget;
    }

    public void ResetTarget()
    {
        if (normalTransform != null)
        {
            currentTarget = normalTransform;
        }
    }
}