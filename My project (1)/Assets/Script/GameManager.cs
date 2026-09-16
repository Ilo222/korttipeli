using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Game State")]
    public GameState state = GameState.Playing;

    [Header("Card Prefab")]
    [SerializeField] private GameObject cardPrefab;

    [Header("Table Positions")]
    [SerializeField] private Transform drawPilePosition;
    [SerializeField] private Transform discardPilePosition;

    [Header("Player Hand")]
    [SerializeField] private Transform playerHandCenter;
    [SerializeField] private HandLayout playerHandLayout;

    [Header("AI 1")]
    [SerializeField] private Transform ai1HandCenter;
    [SerializeField] private AIHandLayout ai1HandLayout;
    [SerializeField] private AIOpponent ai1;

    [Header("AI 2")]
    [SerializeField] private Transform ai2HandCenter;
    [SerializeField] private AIHandLayout ai2HandLayout;
    [SerializeField] private AIOpponent ai2;

    [Header("AI 3")]
    [SerializeField] private Transform ai3HandCenter;
    [SerializeField] private AIHandLayout ai3HandLayout;
    [SerializeField] private AIOpponent ai3;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI turnText;

    [Header("Starting Hand")]
    [SerializeField] private int startingCards = 7;

    [Header("Special Card Counts")]
    [SerializeField] private int draw2Copies = 1;
    [SerializeField] private int draw4Copies = 1;
    [SerializeField] private int reverseCopies = 1;
    [SerializeField] private int skipCopies = 1;
    [SerializeField] private int colorChangeCopies = 1;

    [Header("Draw Pile Visual")]
    [SerializeField] private int drawPileVisualCards = 10;
    [SerializeField] private float drawPileCardOffset = 0.015f;

    [Header("Draw Rules")]
    [SerializeField] private int maximumPlayerDraws = 3;

    [Header("Card Animations")]
    [SerializeField] private float drawAnimationTime = 0.22f;
    [SerializeField] private float drawArcHeight = 0.60f;

    [SerializeField] private float playAnimationTime = 0.25f;
    [SerializeField] private float playArcHeight = 0.45f;
    [SerializeField] private float playSpinDegrees = 90f;

    [Header("AI")]
    [SerializeField] private float aiThinkingTime = 0.35f;

    [Header("Hands")]
    public List<CardData> playerHand =
        new List<CardData>();

    public List<CardData> ai1Hand =
        new List<CardData>();

    public List<CardData> ai2Hand =
        new List<CardData>();

    public List<CardData> ai3Hand =
        new List<CardData>();

    [Header("Visual Cards")]
    public List<Card3D> playerVisualCards =
        new List<Card3D>();

    public List<Card3D> ai1VisualCards =
        new List<Card3D>();

    public List<Card3D> ai2VisualCards =
        new List<Card3D>();

    public List<Card3D> ai3VisualCards =
        new List<Card3D>();

    [Header("Deck")]
    public List<CardData> deck =
        new List<CardData>();

    public CardData currentTopCard;

    [Header("Turn")]
    public int currentPlayerIndex = 0;

    // 0 = Player
    // 1 = AI 1
    // 2 = AI 2
    // 3 = AI 3

    public bool PlayerTurn =>
        currentPlayerIndex == 0;

    private int playerDrawCount;

    private Card3D currentDiscardVisual;

    // ============================================================
    // UNITY
    // ============================================================

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        StartGame();
    }

    // ============================================================
    // GAME START
    // ============================================================

    public void StartGame()
    {
        StopAllCoroutines();

        state = GameState.Playing;
        currentPlayerIndex = 0;
        playerDrawCount = 0;

        BuildDeck();
        ShuffleDeck();

        DealInitialHands();
        PlaceStartingCard();
        SpawnDrawPileVisual();

        RefreshAllHands();
        UpdateTurnUI();

        Debug.Log(
            $"GAME STARTED. Remaining deck: {deck.Count}"
        );
    }

    // ============================================================
    // DECK
    // ============================================================

    private void BuildDeck()
    {
        deck.Clear();

        CardColor[] colors =
        {
            CardColor.Purple,
            CardColor.Green,
            CardColor.Yellow,
            CardColor.Red
        };

        // Number cards 0-4.
        foreach (CardColor color in colors)
        {
            for (int number = 0; number <= 4; number++)
            {
                deck.Add(
                    new CardData(
                        color,
                        CardType.Number,
                        number
                    )
                );
            }
        }

        AddSpecialCards(
            CardType.Draw2,
            draw2Copies
        );

        AddSpecialCards(
            CardType.Draw4,
            draw4Copies
        );

        AddSpecialCards(
            CardType.Reverse,
            reverseCopies
        );

        AddSpecialCards(
            CardType.Skip,
            skipCopies
        );

        AddSpecialCards(
            CardType.ColorChange,
            colorChangeCopies
        );

        Debug.Log(
            $"Deck built: {deck.Count} cards"
        );
    }

    private void AddSpecialCards(
        CardType type,
        int copies)
    {
        CardColor[] colors =
        {
            CardColor.Purple,
            CardColor.Green,
            CardColor.Yellow,
            CardColor.Red
        };

        for (int i = 0; i < copies; i++)
        {
            foreach (CardColor color in colors)
            {
                deck.Add(
                    new CardData(
                        color,
                        type,
                        -1
                    )
                );
            }
        }
    }

    private void ShuffleDeck()
    {
        for (int i = deck.Count - 1; i > 0; i--)
        {
            int random =
                Random.Range(0, i + 1);

            CardData temp =
                deck[i];

            deck[i] =
                deck[random];

            deck[random] =
                temp;
        }
    }

    // ============================================================
    // INITIAL DEAL
    // ============================================================

    private void DealInitialHands()
    {
        playerHand.Clear();
        ai1Hand.Clear();
        ai2Hand.Clear();
        ai3Hand.Clear();

        DestroyAllHandVisuals();

        for (int i = 0; i < startingCards; i++)
        {
            DrawToPlayerImmediate();
            DrawToAIImmediate(1);
            DrawToAIImmediate(2);
            DrawToAIImmediate(3);
        }
    }

    // ============================================================
    // DRAW FROM DECK
    // ============================================================

    private CardData DrawFromDeck()
    {
        if (deck.Count == 0)
        {
            Debug.LogWarning(
                "Draw pile is empty."
            );

            return null;
        }

        int lastIndex =
            deck.Count - 1;

        CardData card =
            deck[lastIndex];

        deck.RemoveAt(lastIndex);

        return card;
    }

    // ============================================================
    // INITIAL HAND DRAW
    // ============================================================

    private void DrawToPlayerImmediate()
    {
        CardData card =
            DrawFromDeck();

        if (card == null)
            return;

        playerHand.Add(card);

        Card3D visual =
            SpawnHandCard(
                card,
                playerHandCenter
            );

        if (visual != null)
        {
            playerVisualCards.Add(visual);
        }
    }

    private void DrawToAIImmediate(
        int playerIndex)
    {
        CardData card =
            DrawFromDeck();

        if (card == null)
            return;

        List<CardData> hand =
            GetAIHand(playerIndex);

        List<Card3D> visuals =
            GetAIVisuals(playerIndex);

        Transform center =
            GetAIHandCenter(playerIndex);

        if (hand == null ||
            visuals == null ||
            center == null)
        {
            return;
        }

        hand.Add(card);

        Card3D visual =
            SpawnHandCard(
                card,
                center
            );

        if (visual != null)
        {
            visuals.Add(visual);
        }
    }

    // ============================================================
    // STARTING DISCARD
    // ============================================================

    private void PlaceStartingCard()
    {
        CardData startingCard = null;

        // Start with a number card.
        for (int i = deck.Count - 1; i >= 0; i--)
        {
            if (deck[i].type == CardType.Number)
            {
                startingCard = deck[i];
                deck.RemoveAt(i);
                break;
            }
        }

        if (startingCard == null)
        {
            Debug.LogError(
                "Could not find starting card."
            );

            return;
        }

        currentTopCard =
            startingCard;

        SpawnDiscardCard(
            startingCard
        );
    }

    // ============================================================
    // PLAYER PLAY
    // ============================================================

    public void PlayCard(
        Card3D clickedCard)
    {
        if (state != GameState.Playing)
            return;

        if (!PlayerTurn)
            return;

        if (clickedCard == null)
            return;

        if (clickedCard.data == null)
            return;

        if (!clickedCard.data.CanPlay(
                currentTopCard))
        {
            Debug.Log(
                "That card cannot be played."
            );

            return;
        }

        StartCoroutine(
            PlayCardRoutine(
                clickedCard
            )
        );
    }

    private IEnumerator PlayCardRoutine(
        Card3D clickedCard)
    {
        state =
            GameState.ResolvingChallenge;

        CardData playedCard =
            clickedCard.data;

        // Remove from logical hand.
        playerHand.Remove(
            playedCard
        );

        // Remove from visual hand.
        playerVisualCards.Remove(
            clickedCard
        );

        clickedCard.SetHovered(false);
        clickedCard.SetAnimationLocked(true);

        // Remove old discard visual.
        if (currentDiscardVisual != null)
        {
            Destroy(
                currentDiscardVisual.gameObject
            );

            currentDiscardVisual = null;
        }

        // Save where card starts.
        Vector3 startPosition =
            clickedCard.transform.position;

        Quaternion startRotation =
            clickedCard.transform.rotation;

        // Detach from hand.
        clickedCard.transform.SetParent(
            null,
            true
        );

        Vector3 targetPosition =
            discardPilePosition.position;

        Quaternion targetRotation =
            discardPilePosition.rotation;

        // Animate from hand to discard.
        yield return AnimatePlayCard(
            clickedCard,
            startPosition,
            startRotation,
            targetPosition,
            targetRotation
        );

        // Put it under discard pile.
        clickedCard.transform.SetParent(
            discardPilePosition,
            true
        );

        clickedCard.transform.position =
            targetPosition;

        clickedCard.transform.rotation =
            targetRotation;

        clickedCard.transform.localScale =
            Vector3.one;

        clickedCard.SetAnimationLocked(false);

        clickedCard.CaptureCurrentTransformAsNormal();

        DisableCardColliders(
            clickedCard.gameObject
        );

        currentDiscardVisual =
            clickedCard;

        currentTopCard =
            playedCard;

        // The player's draw counter resets
        // after actually playing a card.
        playerDrawCount = 0;

        // Remaining cards slide into their
        // new hand positions.
        RefreshAllHands();

        // For now specials still just advance.
        // We will replace this with the special
        // card system next.
        ResolvePlayedCard(
            playedCard,
            0
        );
    }

    // ============================================================
    // PLAYER DRAW
    // ============================================================

    public void PlayerDrawCard()
    {
        if (state != GameState.Playing)
            return;

        if (!PlayerTurn)
            return;

        if (playerDrawCount >= maximumPlayerDraws)
        {
            Debug.Log(
                "You have already drawn 3 cards."
            );

            return;
        }

        StartCoroutine(
            PlayerDrawCardRoutine()
        );
    }

    private IEnumerator PlayerDrawCardRoutine()
    {
        state =
            GameState.ResolvingChallenge;

        playerDrawCount++;

        CardData drawnCard =
            DrawFromDeck();

        if (drawnCard == null)
        {
            state =
                GameState.Playing;

            yield break;
        }

        // Create visual first.
        Card3D visual =
            SpawnHandCard(
                drawnCard,
                playerHandCenter
            );

        if (visual == null)
        {
            state =
                GameState.Playing;

            yield break;
        }

        playerHand.Add(
            drawnCard
        );

        playerVisualCards.Add(
            visual
        );

        // Temporarily calculate the final hand position.
        RefreshAllHands();

        Vector3 finalWorldPosition =
            visual.transform.position;

        Quaternion finalWorldRotation =
            visual.transform.rotation;

        // Detach before flying.
        visual.transform.SetParent(
            null,
            true
        );

        visual.SetAnimationLocked(true);

        // Start at the draw pile.
        Vector3 startPosition =
            drawPilePosition.position;

        startPosition.y += 0.15f;

        Quaternion startRotation =
            drawPilePosition.rotation;

        visual.transform.position =
            startPosition;

        visual.transform.rotation =
            startRotation;

        visual.transform.localScale =
            Vector3.one;

        // Start hidden.
        visual.SetCardBackVisible(true);

        // Animate.
        yield return AnimateDrawCard(
            visual,
            startPosition,
            startRotation,
            finalWorldPosition,
            finalWorldRotation
        );

        // Reattach to the hand.
        visual.transform.SetParent(
            playerHandCenter,
            true
        );

        visual.transform.position =
            finalWorldPosition;

        visual.transform.rotation =
            finalWorldRotation;

        visual.transform.localScale =
            Vector3.one;

        visual.SetCardBackVisible(false);

        visual.SetAnimationLocked(false);

        visual.CaptureCurrentTransformAsNormal();

        RefreshAllHands();

        bool playable =
            HasPlayableCard(
                playerHand
            );

        // Three draws with no playable card.
        if (playerDrawCount >= maximumPlayerDraws &&
            !playable)
        {
            Debug.Log(
                "Drew 3 cards without finding " +
                "a playable card. Turn skipped."
            );

            playerDrawCount = 0;

            GoToNextPlayer();

            yield break;
        }

        // Player remains in control.
        state =
            GameState.Playing;

        UpdateTurnUI();
    }

    // ============================================================
    // DRAW ANIMATION
    // ============================================================

    private IEnumerator AnimateDrawCard(
        Card3D card,
        Vector3 startPosition,
        Quaternion startRotation,
        Vector3 targetPosition,
        Quaternion targetRotation)
    {
        float timer = 0f;

        while (timer < drawAnimationTime)
        {
            float raw =
                timer /
                drawAnimationTime;

            float t =
                Mathf.SmoothStep(
                    0f,
                    1f,
                    raw
                );

            Vector3 position =
                Vector3.Lerp(
                    startPosition,
                    targetPosition,
                    t
                );

            position +=
                Vector3.up *
                Mathf.Sin(
                    t * Mathf.PI
                ) *
                drawArcHeight;

            card.transform.position =
                position;

            card.transform.rotation =
                Quaternion.Slerp(
                    startRotation,
                    targetRotation,
                    t
                );

            // Reveal the front halfway through.
            if (raw >= 0.5f)
            {
                card.SetCardBackVisible(false);
            }

            timer += Time.deltaTime;

            yield return null;
        }

        card.transform.position =
            targetPosition;

        card.transform.rotation =
            targetRotation;

        card.SetCardBackVisible(false);
    }

    // ============================================================
    // PLAY ANIMATION
    // ============================================================

    private IEnumerator AnimatePlayCard(
        Card3D card,
        Vector3 startPosition,
        Quaternion startRotation,
        Vector3 targetPosition,
        Quaternion targetRotation)
    {
        float timer = 0f;

        while (timer < playAnimationTime)
        {
            float raw =
                timer /
                playAnimationTime;

            float t =
                Mathf.SmoothStep(
                    0f,
                    1f,
                    raw
                );

            Vector3 position =
                Vector3.Lerp(
                    startPosition,
                    targetPosition,
                    t
                );

            position +=
                Vector3.up *
                Mathf.Sin(
                    t * Mathf.PI
                ) *
                playArcHeight;

            card.transform.position =
                position;

            Quaternion rotation =
                Quaternion.Slerp(
                    startRotation,
                    targetRotation,
                    t
                );

            // Small dramatic spin.
            rotation *=
                Quaternion.Euler(
                    0f,
                    playSpinDegrees * t,
                    0f
                );

            card.transform.rotation =
                rotation;

            timer += Time.deltaTime;

            yield return null;
        }

        card.transform.position =
            targetPosition;

        card.transform.rotation =
            targetRotation;
    }

    // ============================================================
    // PLAYABLE CHECK
    // ============================================================

    private bool HasPlayableCard(
        List<CardData> hand)
    {
        if (hand == null)
            return false;

        foreach (CardData card in hand)
        {
            if (card != null &&
                card.CanPlay(currentTopCard))
            {
                return true;
            }
        }

        return false;
    }

    // ============================================================
    // CARD RESOLUTION
    // ============================================================

    private void ResolvePlayedCard(
        CardData card,
        int playedByPlayerIndex)
    {
        // Specials are still using the temporary
        // Joker behavior for now.
        //
        // We will replace this with:
        //
        // +2 animation
        // +4 animation
        // Reverse animation
        // Skip animation
        // Color Change animation
        // Challenge system
        //
        // once the normal card loop is working.

        state =
            GameState.Playing;

        GoToNextPlayer();
    }

    // ============================================================
    // TURN SYSTEM
    // ============================================================

    private void GoToNextPlayer()
    {
        currentPlayerIndex =
            (currentPlayerIndex + 1) % 4;

        if (currentPlayerIndex == 0)
        {
            playerDrawCount = 0;
        }

        state =
            GameState.Playing;

        UpdateTurnUI();

        if (currentPlayerIndex != 0)
        {
            StartCoroutine(
                RunAITurn()
            );
        }
    }

    // ============================================================
    // AI TURN
    // ============================================================

    private IEnumerator RunAITurn()
    {
        yield return new WaitForSeconds(
            aiThinkingTime
        );

        if (state != GameState.Playing)
            yield break;

        AIOpponent opponent =
            GetAI(
                currentPlayerIndex
            );

        List<CardData> hand =
            GetAIHand(
                currentPlayerIndex
            );

        Transform handCenter =
            GetAIHandCenter(
                currentPlayerIndex
            );

        List<Card3D> visuals =
            GetAIVisuals(
                currentPlayerIndex
            );

        if (opponent == null ||
            hand == null ||
            handCenter == null ||
            visuals == null)
        {
            Debug.LogError(
                "AI setup is incomplete."
            );

            GoToNextPlayer();

            yield break;
        }

        // Try an existing playable card.
        CardData chosen =
            opponent.ChooseCard(
                hand,
                currentTopCard
            );

        if (chosen != null)
        {
            yield return
                PlayAICardRoutine(
                    currentPlayerIndex,
                    chosen
                );

            yield break;
        }

        // AI gets up to 3 draws.
        for (int draw = 0;
             draw < maximumPlayerDraws;
             draw++)
        {
            CardData drawn =
                DrawFromDeck();

            if (drawn == null)
                break;

            hand.Add(drawn);

            Card3D visual =
                SpawnHandCard(
                    drawn,
                    handCenter
                );

            if (visual != null)
            {
                visuals.Add(
                    visual
                );

                RefreshAllHands();

                Vector3 finalPosition =
                    visual.transform.position;

                Quaternion finalRotation =
                    visual.transform.rotation;

                visual.transform.SetParent(
                    null,
                    true
                );

                visual.SetAnimationLocked(true);

                Vector3 startPosition =
                    drawPilePosition.position;

                startPosition.y +=
                    0.15f;

                visual.transform.position =
                    startPosition;

                visual.transform.rotation =
                    drawPilePosition.rotation;

                // AI keeps card face down.
                visual.SetCardBackVisible(
                    true
                );

                yield return
                    AnimateDrawCard(
                        visual,
                        startPosition,
                        drawPilePosition.rotation,
                        finalPosition,
                        finalRotation
                    );

                visual.transform.SetParent(
                    handCenter,
                    true
                );

                visual.transform.position =
                    finalPosition;

                visual.transform.rotation =
                    finalRotation;

                visual.SetCardBackVisible(
                    true
                );

                visual.SetAnimationLocked(
                    false
                );

                visual.CaptureCurrentTransformAsNormal();

                RefreshAllHands();
            }

            // See if AI can now play.
            chosen =
                opponent.ChooseCard(
                    hand,
                    currentTopCard
                );

            if (chosen != null)
            {
                yield return
                    new WaitForSeconds(
                        0.08f
                    );

                yield return
                    PlayAICardRoutine(
                        currentPlayerIndex,
                        chosen
                    );

                yield break;
            }
        }

        // No playable card after 3 draws.
        GoToNextPlayer();
    }

    // ============================================================
    // AI PLAY
    // ============================================================

    private IEnumerator PlayAICardRoutine(
        int playerIndex,
        CardData card)
    {
        state =
            GameState.ResolvingChallenge;

        List<CardData> hand =
            GetAIHand(playerIndex);

        List<Card3D> visuals =
            GetAIVisuals(playerIndex);

        if (hand == null ||
            visuals == null)
        {
            GoToNextPlayer();

            yield break;
        }

        hand.Remove(card);

        Card3D visual =
            FindVisualForCard(
                visuals,
                card
            );

        if (visual == null)
        {
            GoToNextPlayer();

            yield break;
        }

        visuals.Remove(
            visual
        );

        visual.SetHovered(false);
        visual.SetAnimationLocked(true);

        // Reveal it only for the play animation.
        visual.SetCardBackVisible(false);

        RefreshAllHands();

        if (currentDiscardVisual != null)
        {
            Destroy(
                currentDiscardVisual.gameObject
            );

            currentDiscardVisual = null;
        }

        Vector3 startPosition =
            visual.transform.position;

        Quaternion startRotation =
            visual.transform.rotation;

        visual.transform.SetParent(
            null,
            true
        );

        yield return
            AnimatePlayCard(
                visual,
                startPosition,
                startRotation,
                discardPilePosition.position,
                discardPilePosition.rotation
            );

        visual.transform.SetParent(
            discardPilePosition,
            true
        );

        visual.transform.position =
            discardPilePosition.position;

        visual.transform.rotation =
            discardPilePosition.rotation;

        visual.transform.localScale =
            Vector3.one;

        visual.SetAnimationLocked(false);
        visual.CaptureCurrentTransformAsNormal();

        DisableCardColliders(
            visual.gameObject
        );

        currentDiscardVisual =
            visual;

        currentTopCard =
            card;

        ResolvePlayedCard(
            card,
            playerIndex
        );
    }

    // ============================================================
    // FIND AI VISUAL
    // ============================================================

    private Card3D FindVisualForCard(
        List<Card3D> visuals,
        CardData data)
    {
        if (visuals == null)
            return null;

        foreach (Card3D visual in visuals)
        {
            if (visual != null &&
                visual.data == data)
            {
                return visual;
            }
        }

        return null;
    }

    // ============================================================
    // SPAWN HAND CARD
    // ============================================================

    private Card3D SpawnHandCard(
        CardData data,
        Transform parent)
    {
        if (cardPrefab == null)
        {
            Debug.LogError(
                "GameManager: " +
                "Card Prefab is missing."
            );

            return null;
        }

        if (parent == null)
        {
            Debug.LogError(
                "GameManager: " +
                "Hand position is missing."
            );

            return null;
        }

        GameObject obj =
            Instantiate(
                cardPrefab,
                parent
            );

        Card3D card =
            obj.GetComponent<Card3D>();

        if (card == null)
        {
            Debug.LogError(
                "CardPrefab is missing Card3D."
            );

            Destroy(obj);

            return null;
        }

        card.SetupCard(data);

        return card;
    }

    // ============================================================
    // DISCARD CARD
    // ============================================================

    private void SpawnDiscardCard(
        CardData data)
    {
        if (discardPilePosition == null ||
            cardPrefab == null)
        {
            return;
        }

        if (currentDiscardVisual != null)
        {
            Destroy(
                currentDiscardVisual.gameObject
            );
        }

        GameObject obj =
            Instantiate(
                cardPrefab
            );

        obj.name =
            "CurrentDiscardCard";

        obj.transform.SetParent(
            discardPilePosition,
            false
        );

        obj.transform.localPosition =
            Vector3.zero;

        obj.transform.localRotation =
            Quaternion.identity;

        obj.transform.localScale =
            Vector3.one;

        Card3D card =
            obj.GetComponent<Card3D>();

        if (card == null)
        {
            Destroy(obj);
            return;
        }

        card.SetupCard(data);
        card.SetCardBackVisible(false);
        card.CaptureCurrentTransformAsNormal();

        DisableCardColliders(
            obj
        );

        currentDiscardVisual =
            card;
    }

    // ============================================================
    // DRAW PILE VISUALS
    // ============================================================

    private void SpawnDrawPileVisual()
    {
        if (drawPilePosition == null ||
            cardPrefab == null)
        {
            return;
        }

        for (int i =
             drawPilePosition.childCount - 1;
             i >= 0;
             i--)
        {
            Destroy(
                drawPilePosition
                    .GetChild(i)
                    .gameObject
            );
        }

        for (int i = 0;
             i < drawPileVisualCards;
             i++)
        {
            GameObject obj =
                Instantiate(
                    cardPrefab
                );

            obj.name =
                $"DrawPileVisual_{i}";

            obj.transform.SetParent(
                drawPilePosition,
                false
            );

            obj.transform.localPosition =
                new Vector3(
                    0f,
                    i * drawPileCardOffset,
                    0f
                );

            obj.transform.localRotation =
                Quaternion.identity;

            obj.transform.localScale =
                Vector3.one;

            Card3D card =
                obj.GetComponent<Card3D>();

            if (card != null)
            {
                card.SetCardBackVisible(true);

                card.SetAnimationLocked(true);

                card.CaptureCurrentTransformAsNormal();
            }

            // Decorative pile cards are not clickable.
            DisableCardColliders(obj);
        }
    }

    // ============================================================
    // HELPERS
    // ============================================================

    private void DisableCardColliders(
        GameObject obj)
    {
        Collider[] colliders =
            obj.GetComponentsInChildren<Collider>();

        foreach (Collider collider in colliders)
        {
            collider.enabled = false;
        }
    }

    private List<CardData> GetAIHand(
        int index)
    {
        switch (index)
        {
            case 1:
                return ai1Hand;

            case 2:
                return ai2Hand;

            case 3:
                return ai3Hand;

            default:
                return null;
        }
    }

    private List<Card3D> GetAIVisuals(
        int index)
    {
        switch (index)
        {
            case 1:
                return ai1VisualCards;

            case 2:
                return ai2VisualCards;

            case 3:
                return ai3VisualCards;

            default:
                return null;
        }
    }

    private Transform GetAIHandCenter(
        int index)
    {
        switch (index)
        {
            case 1:
                return ai1HandCenter;

            case 2:
                return ai2HandCenter;

            case 3:
                return ai3HandCenter;

            default:
                return null;
        }
    }

    private AIOpponent GetAI(
        int index)
    {
        switch (index)
        {
            case 1:
                return ai1;

            case 2:
                return ai2;

            case 3:
                return ai3;

            default:
                return null;
        }
    }

    // ============================================================
    // HAND REFRESH
    // ============================================================

    public void RefreshAllHands()
    {
        if (playerHandLayout != null)
        {
            playerHandLayout.UpdateHandLayout(
                playerVisualCards
            );
        }

        if (ai1HandLayout != null)
        {
            ai1HandLayout.UpdateAIHand(
                ai1VisualCards
            );
        }

        if (ai2HandLayout != null)
        {
            ai2HandLayout.UpdateAIHand(
                ai2VisualCards
            );
        }

        if (ai3HandLayout != null)
        {
            ai3HandLayout.UpdateAIHand(
                ai3VisualCards
            );
        }
    }

    private void DestroyAllHandVisuals()
    {
        DestroyVisualList(
            playerVisualCards
        );

        DestroyVisualList(
            ai1VisualCards
        );

        DestroyVisualList(
            ai2VisualCards
        );

        DestroyVisualList(
            ai3VisualCards
        );

        playerVisualCards.Clear();
        ai1VisualCards.Clear();
        ai2VisualCards.Clear();
        ai3VisualCards.Clear();
    }

    private void DestroyVisualList(
        List<Card3D> cards)
    {
        if (cards == null)
            return;

        foreach (Card3D card in cards)
        {
            if (card != null)
            {
                Destroy(
                    card.gameObject
                );
            }
        }
    }

    // ============================================================
    // PLAYER CARD CHECK
    // ============================================================

    public bool IsPlayerCard(
        Card3D card)
    {
        return
            card != null &&
            playerVisualCards.Contains(card);
    }

    // ============================================================
    // UI
    // ============================================================

    private void UpdateTurnUI()
    {
        if (turnText == null)
            return;

        if (currentPlayerIndex == 0)
        {
            turnText.text =
                $"YOUR TURN  " +
                $"DRAW {playerDrawCount}/" +
                $"{maximumPlayerDraws}";
        }
        else if (currentPlayerIndex == 1)
        {
            turnText.text =
                "AI 1'S TURN";
        }
        else if (currentPlayerIndex == 2)
        {
            turnText.text =
                "AI 2'S TURN";
        }
        else
        {
            turnText.text =
                "AI 3'S TURN";
        }
    }

    // ============================================================
    // CHALLENGE FOUNDATION
    // ============================================================

    public void AcceptPlayerChallenge()
    {
        if (state !=
            GameState.WaitingForChallenge)
        {
            return;
        }

        BeginChallenge(true);
    }

    public void DeclinePlayerChallenge()
    {
        if (state !=
            GameState.WaitingForChallenge)
        {
            return;
        }

        GoToNextPlayer();
    }

    private void BeginChallenge(
        bool playerIsChallenger)
    {
        state =
            GameState.CameraTransition;

        if (MinigameManager.Instance == null)
        {
            state =
                GameState.Playing;

            return;
        }

        MinigameManager.Instance
            .StartChallenge(
                currentTopCard.color,
                playerIsChallenger
            );
    }

    public void ChallengeFinished(
        bool challengerWon)
    {
        state =
            GameState.ResolvingChallenge;

        if (TransitionManager.Instance != null)
        {
            TransitionManager.Instance
                .EndChallenge();
        }

        StartCoroutine(
            FinishChallenge()
        );
    }

    private IEnumerator FinishChallenge()
    {
        yield return new WaitForSeconds(1.5f);

        GoToNextPlayer();
    }
}