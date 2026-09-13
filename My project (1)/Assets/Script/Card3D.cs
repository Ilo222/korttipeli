using UnityEngine;

public class Card3D : MonoBehaviour
{
    [Header("Card Data")]
    public CardData data;

    [Header("Artwork Renderer")]
    [SerializeField] private MeshRenderer artworkRenderer;

    // =========================================================
    // NUMBER CARDS
    // =========================================================

    [Header("Purple Number Cards")]
    [SerializeField] private Texture2D purple0;
    [SerializeField] private Texture2D purple1;
    [SerializeField] private Texture2D purple2;
    [SerializeField] private Texture2D purple3;
    [SerializeField] private Texture2D purple4;

    [Header("Green Number Cards")]
    [SerializeField] private Texture2D green0;
    [SerializeField] private Texture2D green1;
    [SerializeField] private Texture2D green2;
    [SerializeField] private Texture2D green3;
    [SerializeField] private Texture2D green4;

    [Header("Yellow Number Cards")]
    [SerializeField] private Texture2D yellow0;
    [SerializeField] private Texture2D yellow1;
    [SerializeField] private Texture2D yellow2;
    [SerializeField] private Texture2D yellow3;
    [SerializeField] private Texture2D yellow4;

    [Header("Red Number Cards")]
    [SerializeField] private Texture2D red0;
    [SerializeField] private Texture2D red1;
    [SerializeField] private Texture2D red2;
    [SerializeField] private Texture2D red3;
    [SerializeField] private Texture2D red4;

    // =========================================================
    // SPECIAL CARDS
    // =========================================================

    [Header("Special Cards")]
    [SerializeField] private Texture2D draw2Texture;
    [SerializeField] private Texture2D draw4Texture;
    [SerializeField] private Texture2D reverseTexture;
    [SerializeField] private Texture2D skipTexture;
    [SerializeField] private Texture2D colorChangeTexture;

    // =========================================================
    // CARD BACK
    // =========================================================

    [Header("Card Back")]
    [SerializeField] private Texture2D cardBackTexture;

    // =========================================================
    // HOVER
    // =========================================================

    [Header("Hover")]
    [SerializeField] private float hoverHeight = 0.12f;

    private Vector3 startPosition;
    private bool hovering;

    // =========================================================
    // SETUP
    // =========================================================

    public void SetupCard(CardData newData)
    {
        data = newData;

        ApplyFrontTexture();

        startPosition = transform.localPosition;
    }

    // =========================================================
    // FRONT / BACK
    // =========================================================

    public void SetCardBackVisible(bool showBack)
    {
        if (artworkRenderer == null)
        {
            Debug.LogError(
                "Card3D: Artwork Renderer is not assigned on " +
                gameObject.name
            );

            return;
        }

        if (showBack)
        {
            ApplyTexture(cardBackTexture);
        }
        else
        {
            ApplyFrontTexture();
        }
    }

    private void ApplyFrontTexture()
    {
        if (data == null)
        {
            Debug.LogWarning(
                "Card3D: No CardData assigned."
            );

            return;
        }

        Texture2D texture = null;

        if (data.type == CardType.Number)
        {
            texture = GetNumberTexture(
                data.color,
                data.number
            );
        }
        else
        {
            texture = GetSpecialTexture(
                data.type
            );
        }

        if (texture == null)
        {
            Debug.LogWarning(
                $"Card3D: No texture assigned for " +
                $"{data.color} / {data.type} / {data.number}"
            );

            return;
        }

        ApplyTexture(texture);
    }

    // =========================================================
    // APPLY TEXTURE
    // =========================================================

    private void ApplyTexture(Texture2D texture)
    {
        if (artworkRenderer == null)
        {
            Debug.LogError(
                "Card3D: Artwork Renderer is missing."
            );

            return;
        }

        if (texture == null)
        {
            Debug.LogWarning(
                "Card3D: Texture is null."
            );

            return;
        }

        // Get the material used by this specific card.
        Material material =
            artworkRenderer.material;

        // URP uses _BaseMap.
        if (material.HasProperty("_BaseMap"))
        {
            material.SetTexture(
                "_BaseMap",
                texture
            );
        }
        else
        {
            material.mainTexture = texture;
        }
    }

    // =========================================================
    // NUMBER TEXTURES
    // =========================================================

    private Texture2D GetNumberTexture(
        CardColor color,
        int number)
    {
        switch (color)
        {
            case CardColor.Purple:
                return GetPurpleTexture(number);

            case CardColor.Green:
                return GetGreenTexture(number);

            case CardColor.Yellow:
                return GetYellowTexture(number);

            case CardColor.Red:
                return GetRedTexture(number);

            default:
                return null;
        }
    }

    private Texture2D GetPurpleTexture(int number)
    {
        switch (number)
        {
            case 0: return purple0;
            case 1: return purple1;
            case 2: return purple2;
            case 3: return purple3;
            case 4: return purple4;
            default: return null;
        }
    }

    private Texture2D GetGreenTexture(int number)
    {
        switch (number)
        {
            case 0: return green0;
            case 1: return green1;
            case 2: return green2;
            case 3: return green3;
            case 4: return green4;
            default: return null;
        }
    }

    private Texture2D GetYellowTexture(int number)
    {
        switch (number)
        {
            case 0: return yellow0;
            case 1: return yellow1;
            case 2: return yellow2;
            case 3: return yellow3;
            case 4: return yellow4;
            default: return null;
        }
    }

    private Texture2D GetRedTexture(int number)
    {
        switch (number)
        {
            case 0: return red0;
            case 1: return red1;
            case 2: return red2;
            case 3: return red3;
            case 4: return red4;
            default: return null;
        }
    }

    // =========================================================
    // SPECIAL TEXTURES
    // =========================================================

    private Texture2D GetSpecialTexture(
        CardType type)
    {
        switch (type)
        {
            case CardType.Draw2:
                return draw2Texture;

            case CardType.Draw4:
                return draw4Texture;

            case CardType.Reverse:
                return reverseTexture;

            case CardType.Skip:
                return skipTexture;

            case CardType.ColorChange:
                return colorChangeTexture;

            default:
                return null;
        }
    }

    // =========================================================
    // MOUSE HOVER
    // =========================================================

    private void OnMouseEnter()
    {
        if (hovering)
            return;

        startPosition =
            transform.localPosition;

        transform.localPosition =
            startPosition +
            Vector3.up * hoverHeight;

        hovering = true;
    }

    private void OnMouseExit()
    {
        if (!hovering)
            return;

        transform.localPosition =
            startPosition;

        hovering = false;
    }

    // =========================================================
    // CLICK
    // =========================================================

    private void OnMouseDown()
    {
        if (GameManager.Instance == null)
        {
            Debug.Log(
                "Card clicked, but GameManager.Instance is null."
            );

            return;
        }

        GameManager.Instance.PlayCard(this);
    }
}