using UnityEngine;

public class EyeColorChanger : MonoBehaviour
{
    public Color hoverColor = Color.red;
    private Renderer eyeRenderer;
    private Color originalColor;

    void Start()
    {
        eyeRenderer = GetComponent<Renderer>();
        if (eyeRenderer != null && eyeRenderer.material != null)
        {
            originalColor = eyeRenderer.material.color;
        }
    }

    public void MakeRed()
    {
        if (eyeRenderer != null && eyeRenderer.material != null)
        {
            eyeRenderer.material.color = hoverColor;
        }
    }

    public void ResetColor()
    {
        if (eyeRenderer != null && eyeRenderer.material != null)
        {
            eyeRenderer.material.color = originalColor;
        }
    }
}