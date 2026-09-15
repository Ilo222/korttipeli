using UnityEngine;

public class Card3D : MonoBehaviour
{
    [Header("Data")]
    public CardData data;

    [Header("Artwork")]
    [SerializeField] private MeshRenderer artworkRenderer;

    [Header("Purple")]
    [SerializeField] private Texture2D purple0;
    [SerializeField] private Texture2D purple1;
    [SerializeField] private Texture2D purple2;
    [SerializeField] private Texture2D purple3;
    [SerializeField] private Texture2D purple4;

    [Header("Green")]
    [SerializeField] private Texture2D green0;
    [SerializeField] private Texture2D green1;
    [SerializeField] private Texture2D green2;
    [SerializeField] private Texture2D green3;
    [SerializeField] private Texture2D green4;

    [Header("Yellow")]
    [SerializeField] private Texture2D yellow0;
    [SerializeField] private Texture2D yellow1;
    [SerializeField] private Texture2D yellow2;
    [SerializeField] private Texture2D yellow3;
    [SerializeField] private Texture2D yellow4;

    [Header("Red")]
    [SerializeField] private Texture2D red0;
    [SerializeField] private Texture2D red1;
    [SerializeField] private Texture2D red2;
    [SerializeField] private Texture2D red3;
    [SerializeField] private Texture2D red4;

    [Header("Special Cards")]
    [SerializeField] private Texture2D draw2Texture;
    [SerializeField] private Texture2D draw4Texture;
    [SerializeField] private Texture2D reverseTexture;
    [SerializeField] private Texture2D skipTexture;
    [SerializeField] private Texture2D colorChangeTexture;

    [Header("Card Back")]
    [SerializeField] private Texture2D cardBackTexture;

    [Header("Hover")]
    [SerializeField] private float hoverHeight = 0.45f;
    [SerializeField] private float hoverScale = 1.08f;
    [SerializeField] private float hoverSmooth = 14f;

    private Vector3 normalLocalPosition;
    private Quaternion normalLocalRotation;
    private Vector3 normalLocalScale;

    private Vector3 targetLocalPosition;
    private Quaternion targetLocalRotation;
    private Vector3 targetLocalScale;

    private bool hovered;

    public bool IsHovered => hovered;

    private void Awake()
    {
        CaptureCurrentTransformAsNormal();
        ResetHoverTarget();
    }

    private void Update()
    {
        transform.localPosition =
            Vector3.Lerp(
                transform.localPosition,
                targetLocalPosition,
                Time.deltaTime * hoverSmooth
            );

        transform.localRotation =
            Quaternion.Slerp(
                transform.localRotation,
                targetLocalRotation,
                Time.deltaTime * hoverSmooth
            );

        transform.localScale =
            Vector3.Lerp(
                transform.localScale,
                targetLocalScale,
                Time.deltaTime * hoverSmooth
            );
    }

    public void CaptureCurrentTransformAsNormal()
    {
        normalLocalPosition = transform.localPosition;
        normalLocalRotation = transform.localRotation;
        normalLocalScale = transform.localScale;

        ResetHoverTarget();
    }

    private void ResetHoverTarget()
    {
        targetLocalPosition = normalLocalPosition;
        targetLocalRotation = normalLocalRotation;
        targetLocalScale = normalLocalScale;
    }

    public void SetHovered(bool value)
    {
        if (value == hovered)
            return;

        hovered = value;

        if (hovered)
        {
            targetLocalPosition =
                normalLocalPosition +
                Vector3.up * hoverHeight;

            targetLocalRotation =
                normalLocalRotation;

            targetLocalScale =
                normalLocalScale * hoverScale;
        }
        else
        {
            ResetHoverTarget();
        }
    }

    public void SetupCard(CardData newData)
    {
        data = newData;

        CaptureCurrentTransformAsNormal();

        UpdateArtwork();
    }

    public void SetCardBackVisible(bool visible)
    {
        if (artworkRenderer == null)
            return;

        if (visible)
        {
            if (cardBackTexture != null)
            {
                artworkRenderer.material.mainTexture =
                    cardBackTexture;
            }
        }
        else
        {
            UpdateArtwork();
        }
    }

    private void UpdateArtwork()
    {
        if (artworkRenderer == null ||
            data == null)
            return;

        Texture2D texture = null;

        if (data.type == CardType.Number)
        {
            texture =
                GetNumberTexture(
                    data.color,
                    data.number
                );
        }
        else
        {
            texture =
                GetSpecialTexture(
                    data.type
                );
        }

        if (texture != null)
        {
            artworkRenderer.material.mainTexture =
                texture;
        }
    }

    private Texture2D GetNumberTexture(
        CardColor color,
        int number)
    {
        switch (color)
        {
            case CardColor.Purple:
                return GetPurple(number);

            case CardColor.Green:
                return GetGreen(number);

            case CardColor.Yellow:
                return GetYellow(number);

            case CardColor.Red:
                return GetRed(number);

            default:
                return null;
        }
    }

    private Texture2D GetPurple(int number)
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

    private Texture2D GetGreen(int number)
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

    private Texture2D GetYellow(int number)
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

    private Texture2D GetRed(int number)
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
}