using UnityEngine;
using UnityEngine.InputSystem;

public class DrawPile : MonoBehaviour
{
    private void Update()
    {
        if (Mouse.current == null)
            return;

        if (!Mouse.current.leftButton.wasPressedThisFrame)
            return;

        if (GameManager.Instance == null)
            return;

        if (!GameManager.Instance.PlayerTurn)
            return;

        Ray ray =
            Camera.main.ScreenPointToRay(
                Mouse.current.position.ReadValue()
            );

        if (Physics.Raycast(
                ray,
                out RaycastHit hit,
                100f))
        {
            if (hit.collider == GetComponent<Collider>())
            {
                GameManager.Instance.PlayerDrawCard();
            }
        }
    }
}