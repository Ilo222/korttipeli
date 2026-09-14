using UnityEngine;
using UnityEngine.InputSystem;

public class SimpleScroll : MonoBehaviour
{
    public float scrollSpeed = 25f;
    public float minY = -150f;
    public float maxY = 150f;

    void Update()
    {
        if (Mouse.current != null)
        {
            float scrollInput = Mouse.current.scroll.y.ReadValue();
            if (scrollInput != 0f)
            {
                Vector3 pos = transform.localPosition;
                pos.y += (scrollInput > 0 ? -scrollSpeed : scrollSpeed);
                pos.y = Mathf.Clamp(pos.y, minY, maxY);
                transform.localPosition = pos;
            }
        }
    }
}