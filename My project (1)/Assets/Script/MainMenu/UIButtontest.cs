using UnityEngine;
using UnityEngine.EventSystems;

public class UIButtonTest : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("NAPPia KLIKATTIIN!");
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("HIIRI ON NAPIN PÄÄLLÄ!");
    }
}