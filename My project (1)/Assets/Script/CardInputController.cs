using UnityEngine;
using UnityEngine.InputSystem;

public class CardInputController : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField] private Camera gameplayCamera;

    [Header("Raycast")]
    [SerializeField] private float rayDistance = 100f;

    private Card3D hoveredCard;

    private void Start()
    {
        if (gameplayCamera == null)
        {
            gameplayCamera = Camera.main;
        }

        if (gameplayCamera == null)
        {
            Debug.LogError(
                "CardInputController: No gameplay camera found."
            );
        }
    }

    private void Update()
    {
        if (GameManager.Instance == null)
            return;

        UpdateHover();

        if (Mouse.current != null &&
            Mouse.current.leftButton.wasPressedThisFrame)
        {
            TryClickCard();
        }
    }

    private void UpdateHover()
    {
        Card3D newCard =
            FindCardUnderMouse();

        if (newCard == hoveredCard)
            return;

        if (hoveredCard != null)
        {
            hoveredCard.SetHovered(false);
        }

        hoveredCard = newCard;

        if (hoveredCard != null)
        {
            hoveredCard.SetHovered(true);
        }
    }

    private Card3D FindCardUnderMouse()
    {
        if (gameplayCamera == null)
            return null;

        if (GameManager.Instance.state !=
            GameState.Playing)
        {
            return null;
        }

        if (!GameManager.Instance.PlayerTurn)
            return null;

        if (Mouse.current == null)
            return null;

        Vector2 mousePosition =
            Mouse.current.position.ReadValue();

        Ray ray =
            gameplayCamera.ScreenPointToRay(
                mousePosition
            );

        RaycastHit[] hits =
            Physics.RaycastAll(
                ray,
                rayDistance
            );

        if (hits == null ||
            hits.Length == 0)
        {
            return null;
        }

        Card3D bestCard = null;

        float bestDistance =
            float.MaxValue;

        foreach (RaycastHit hit in hits)
        {
            Card3D card =
                hit.collider.GetComponentInParent<Card3D>();

            if (card == null)
                continue;

            if (!GameManager.Instance
                .IsPlayerCard(card))
            {
                continue;
            }

            if (hit.distance < bestDistance)
            {
                bestDistance =
                    hit.distance;

                bestCard =
                    card;
            }
        }

        return bestCard;
    }

    private void TryClickCard()
    {
        Card3D card =
            FindCardUnderMouse();

        if (card == null)
            return;

        GameManager.Instance.PlayCard(card);
    }

    private void OnDisable()
    {
        if (hoveredCard != null)
        {
            hoveredCard.SetHovered(false);
            hoveredCard = null;
        }
    }
}