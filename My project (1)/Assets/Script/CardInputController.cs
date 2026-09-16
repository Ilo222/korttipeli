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
            gameplayCamera = Camera.main;

        if (gameplayCamera == null)
        {
            Debug.LogError(
                "CardInputController: " +
                "No gameplay camera found."
            );
        }
    }

    private void Update()
    {
        if (GameManager.Instance == null)
            return;

        UpdateHover();

        if (Mouse.current == null)
            return;

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            TryClick();
        }
    }

    private void UpdateHover()
    {
        Card3D newCard =
            FindCardUnderMouse();

        if (newCard == hoveredCard)
            return;

        if (hoveredCard != null)
            hoveredCard.SetHovered(false);

        hoveredCard = newCard;

        if (hoveredCard != null)
            hoveredCard.SetHovered(true);
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

        Ray ray =
            gameplayCamera.ScreenPointToRay(
                Mouse.current.position.ReadValue()
            );

        RaycastHit[] hits =
            Physics.RaycastAll(
                ray,
                rayDistance
            );

        Card3D bestCard = null;
        float bestDistance = float.MaxValue;

        foreach (RaycastHit hit in hits)
        {
            Card3D card =
                hit.collider
                    .GetComponentInParent<Card3D>();

            if (card == null)
                continue;

            if (!GameManager.Instance.IsPlayerCard(card))
                continue;

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

    private void TryClick()
    {
        if (gameplayCamera == null)
            return;

        if (GameManager.Instance.state !=
            GameState.Playing)
        {
            return;
        }

        if (!GameManager.Instance.PlayerTurn)
            return;

        Ray ray =
            gameplayCamera.ScreenPointToRay(
                Mouse.current.position.ReadValue()
            );

        RaycastHit[] hits =
            Physics.RaycastAll(
                ray,
                rayDistance
            );

        Card3D bestCard = null;
        float bestCardDistance = float.MaxValue;

        DrawPile drawPile = null;
        float drawPileDistance = float.MaxValue;

        foreach (RaycastHit hit in hits)
        {
            Card3D card =
                hit.collider
                    .GetComponentInParent<Card3D>();

            if (card != null &&
                GameManager.Instance.IsPlayerCard(card))
            {
                if (hit.distance < bestCardDistance)
                {
                    bestCardDistance =
                        hit.distance;

                    bestCard =
                        card;
                }

                continue;
            }

            DrawPile pile =
                hit.collider
                    .GetComponentInParent<DrawPile>();

            if (pile != null &&
                hit.distance < drawPileDistance)
            {
                drawPileDistance =
                    hit.distance;

                drawPile =
                    pile;
            }
        }

        if (bestCard != null)
        {
            GameManager.Instance.PlayCard(
                bestCard
            );

            return;
        }

        if (drawPile != null)
        {
            GameManager.Instance.PlayerDrawCard();
        }
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