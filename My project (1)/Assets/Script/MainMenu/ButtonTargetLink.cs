using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonTargetLink : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public MenuHand menuHand;
    public Transform targetTransform;
    public EyeColorChanger[] eyeColorChangers; // Voit vet‰‰ t‰h‰n useamman silm‰n

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (menuHand != null && targetTransform != null)
        {
            menuHand.SetTarget(targetTransform);
        }

        if (eyeColorChangers != null)
        {
            foreach (var eye in eyeColorChangers)
            {
                if (eye != null) eye.MakeRed();
            }
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (menuHand != null)
        {
            menuHand.ResetTarget();
        }

        if (eyeColorChangers != null)
        {
            foreach (var eye in eyeColorChangers)
            {
                if (eye != null) eye.ResetColor();
            }
        }
    }
}