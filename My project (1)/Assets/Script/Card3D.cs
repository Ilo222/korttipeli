using UnityEngine;

public class Card3D : MonoBehaviour
{
    public CardData data;
    private void OnMouseDown() => GameManager.Instance.PlayCard(this);
}