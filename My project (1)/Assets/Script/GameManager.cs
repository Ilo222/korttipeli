using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    // ============================================================
    // GAME STATE
    // ============================================================

    [Header("Game State")]
    public GameState state = GameState.Playing;

    // ============================================================
    // CARD PREFAB
    // ============================================================

    [Header("Card Prefab")]
    [SerializeField] private GameObject cardPrefab;

    // ============================================================
    // TABLE POSITIONS
    // ============================================================

    [Header("Table Positions")]
    [SerializeField] private Transform drawPilePosition;
    [SerializeField] private Transform discardPilePosition;

    // ============================================================
    // PLAYER HAND
    // ============================================================

    [Header("Player Hand")]
    [SerializeField] private Transform playerHandCenter;
    [SerializeField] private HandLayout playerHandLayout;

    // ============================================================
    // AI 1
    // ============================================================

    [Header("AI 1")]
    [SerializeField] private Transform ai1HandCenter;
    [SerializeField] private AIHandLayout ai1HandLayout;
    [SerializeField] private AIOpponent ai1;

    // ============================================================
    // AI 2
    // ============================================================

    [Header("AI 2")]
    [SerializeField] private Transform ai2HandCenter;
    [SerializeField] private AIHandLayout ai2HandLayout;
    [SerializeField] private AIOpponent ai2;

    // ============================================================
    // AI 3
    // ============================================================

    [Header("AI 3")]
    [SerializeField] private Transform ai3HandCenter;
    [SerializeField] private AIHandLayout ai3HandLayout;
    [SerializeField] private AIOpponent ai3;

    // ============================================================
    // UI
    // ============================================================

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI turnText;
    [SerializeField] private ChallengePanel challengePanel;

    // ============================================================
    // STARTING HAND
    // ============================================================

    [Header("Starting Hand")]
    [SerializeField] private int startingCards = 7;

    // ============================================================
    // SPECIAL CARD COUNTS
    // ============================================================

    [Header("Special Card Counts")]
    [SerializeField] private int draw2Copies = 1;
    [SerializeField] private int draw4Copies = 1;
    [SerializeField] private int reverseCopies = 1;
    [SerializeField] private int skipCopies = 1;
    [SerializeField] private int colorChangeCopies = 1;

    // ============================================================
    // DRAW PILE VISUAL
    // ============================================================

    [Header("Draw Pile Visual")]
    [SerializeField] private int drawPileVisualCards = 10;
    [SerializeField] private float drawPileCardOffset = 0.015f;

    // ============================================================
    // DRAW RULES
    // ============================================================

    [Header("Draw Rules")]
    [SerializeField] private int maximumPlayerDraws = 3;

    // ============================================================
    // CARD ANIMATIONS
    // ============================================================

    [Header("Card Animations")]
    [SerializeField] private float drawAnimationTime = 0.22f;
    [SerializeField] private float drawArcHeight = 0.60f;

    [SerializeField] private float playAnimationTime = 0.25f;
    [SerializeField] private float playArcHeight = 0.45f;
    [SerializeField] private float playSpinDegrees = 90f;

    // ============================================================
    // AI
    // ============================================================

    [Header("AI")]
    [SerializeField] private float aiThinkingTime = 0.35f;

    // ============================================================
    // HANDS
    // ============================================================

    [Header("Hands")]
    public List<CardData> playerHand =
        new List<CardData>();

    public List<CardData> ai1Hand =
        new List<CardData>();

    public List<CardData> ai2Hand =
        new List<CardData>();

    public List<CardData> ai3Hand =
        new List<CardData>();

    // ============================================================
    // VISUAL CARDS
    // ============================================================

    [Header("Visual Cards")]
    public List<Card3D> playerVisualCards =
        new List<Card3D>();

    public List<Card3D> ai1VisualCards =
        new List<Card3D>();

    public List<Card3D> ai2VisualCards =
        new List<Card3D>();

    public List<Card3D> ai3VisualCards =
        new List<Card3D>();

    // ============================================================
    // DECK
    // ============================================================

    [Header("Deck")]
    public List<CardData> deck =
        new List<CardData>();

    public CardData currentTopCard;

    // ============================================================
    // ACTIVE COLOR
    // ============================================================

    [Header("Active Color")]
    public CardColor activeColor = CardColor.Wild;

    // ============================================================
    // TURN
    // ============================================================

    [Header("Turn")]
    public int currentPlayerIndex = 0;

    // 0 = Player
    // 1 = AI 1
    // 2 = AI 2
    // 3 = AI 3

    // 1 = clockwise
    // -1 = reverse
    private int turnDirection = 1;

    public bool PlayerTurn =>
        currentPlayerIndex == 0;

    // ============================================================
    // PLAYER DRAW COUNT
    // ============================================================

    private int playerDrawCount;

    private bool playerDrewPlayableCardThisTurn;

    // ============================================================
    // STACKING
    // ============================================================

    [Header("Draw Stack")]
    [SerializeField] private int pendingDrawAmount = 0;

    [SerializeField]
    private CardType pendingDrawType =
        CardType.Number;

    // ============================================================
    // CHALLENGE
    // ============================================================

    private CardData pendingSpecialCard;

    private int pendingSpecialPlayerIndex = -1;

    private bool waitingForWildColor;

    // ============================================================
    // DISCARD VISUAL
    // ============================================================

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

        if (MinigameManager.Instance != null)
        {
            MinigameManager.Instance.ResetAllPlayerDifficulty();
        }

        currentPlayerIndex = 0;

        turnDirection = 1;

        playerDrawCount = 0;

        playerDrewPlayableCardThisTurn = false;

        pendingDrawAmount = 0;

        pendingDrawType = CardType.Number;

        pendingSpecialCard = null;

        pendingSpecialPlayerIndex = -1;

        waitingForWildColor = false;

        activeColor = CardColor.Wild;

        BuildDeck();
        ShuffleDeck();

        DealInitialHands();

        PlaceStartingCard();

        SpawnDrawPileVisual();

        RefreshAllHands();

        UpdateTurnUI();

        HideChallengePanel();

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

        // --------------------------------------------------------
        // Number cards 0-4
        // --------------------------------------------------------

        foreach (CardColor color in colors)
        {
            for (int number = 0;
                 number <= 4;
                 number++)
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

        // --------------------------------------------------------
        // Draw 2
        // --------------------------------------------------------

        AddColoredSpecialCards(
            CardType.Draw2,
            draw2Copies
        );

        // --------------------------------------------------------
        // Draw 4
        // --------------------------------------------------------

        AddColoredSpecialCards(
            CardType.Draw4,
            draw4Copies
        );

        // --------------------------------------------------------
        // Reverse
        // --------------------------------------------------------

        AddColoredSpecialCards(
            CardType.Reverse,
            reverseCopies
        );

        // --------------------------------------------------------
        // Skip
        // --------------------------------------------------------

        AddColoredSpecialCards(
            CardType.Skip,
            skipCopies
        );

        // --------------------------------------------------------
        // Color Change / Wild
        // --------------------------------------------------------

        for (int i = 0;
             i < colorChangeCopies * 4;
             i++)
        {
            deck.Add(
                new CardData(
                    CardColor.Wild,
                    CardType.ColorChange,
                    -1
                )
            );
        }

        Debug.Log(
            $"Deck built: {deck.Count} cards"
        );
    }

    // ============================================================
    // ADD COLORED SPECIALS
    // ============================================================

    private void AddColoredSpecialCards(
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

        for (int i = 0;
             i < copies;
             i++)
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

    // ============================================================
    // SHUFFLE
    // ============================================================

    private void ShuffleDeck()
    {
        for (int i = deck.Count - 1;
             i > 0;
             i--)
        {
            int random =
                Random.Range(
                    0,
                    i + 1
                );

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

        for (int i = 0;
             i < startingCards;
             i++)
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
    // INITIAL PLAYER DRAW
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
            playerVisualCards.Add(
                visual
            );
        }
    }

    // ============================================================
    // INITIAL AI DRAW
    // ============================================================

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
            visual.SetCardBackVisible(true);

            visuals.Add(visual);
        }
    }

    // ============================================================
    // STARTING DISCARD
    // ============================================================

    private void PlaceStartingCard()
    {
        CardData startingCard = null;

        for (int i = deck.Count - 1;
             i >= 0;
             i--)
        {
            if (deck[i].type ==
                CardType.Number)
            {
                startingCard =
                    deck[i];

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

        activeColor =
            startingCard.color;

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

        if (!CanPlayCardNow(
                clickedCard.data))
        {
            Debug.Log(
                "That card cannot be played."
            );

            return;
        }

        List<CardData> cardsToPlay =
            BuildPlayerPlayGroup(
                clickedCard.data
            );

        StartCoroutine(
            PlayPlayerCardGroupRoutine(
                cardsToPlay
            )
        );
    }

    // ============================================================
    // PLAYER CARD GROUP
    // ============================================================

    private List<CardData> BuildPlayerPlayGroup(
        CardData clicked)
    {
        List<CardData> group =
            new List<CardData>();

        if (clicked == null)
            return group;

        // Specials are always played alone.
        if (clicked.type != CardType.Number)
        {
            group.Add(clicked);

            return group;
        }

        // Same-number cards are played together.
        foreach (CardData card in playerHand)
        {
            if (card == null)
                continue;

            if (card.type != CardType.Number)
                continue;

            if (card.number == clicked.number)
                group.Add(card);
        }

        // Safety.
        if (group.Count == 0)
            group.Add(clicked);

        return group;
    }

    // ============================================================
    // PLAYER CARD GROUP ROUTINE
    // ============================================================

    private IEnumerator PlayPlayerCardGroupRoutine(
        List<CardData> cards)
    {
        if (cards == null ||
            cards.Count == 0)
        {
            state =
                GameState.Playing;

            yield break;
        }

        state =
            GameState.ResolvingChallenge;

        // --------------------------------------------------------
        // Find visual cards first.
        // --------------------------------------------------------

        List<Card3D> visuals =
            new List<Card3D>();

        foreach (CardData card in cards)
        {
            Card3D visual =
                FindVisualForCard(
                    playerVisualCards,
                    card
                );

            if (visual != null)
                visuals.Add(visual);
        }

        // --------------------------------------------------------
        // Remove logical cards and visuals.
        // --------------------------------------------------------

        foreach (CardData card in cards)
            playerHand.Remove(card);

        foreach (Card3D visual in visuals)
            playerVisualCards.Remove(visual);

        // --------------------------------------------------------
        // Lock visuals.
        // --------------------------------------------------------

        foreach (Card3D visual in visuals)
        {
            visual.SetHovered(false);

            visual.SetAnimationLocked(true);
        }

        // --------------------------------------------------------
        // Refresh remaining hand.
        // --------------------------------------------------------

        RefreshAllHands();

        // --------------------------------------------------------
        // Clear old discard.
        // --------------------------------------------------------

        ClearDiscardVisuals();

        // --------------------------------------------------------
        // Animate every card in the group.
        // --------------------------------------------------------

        for (int i = 0;
             i < visuals.Count;
             i++)
        {
            Card3D visual =
                visuals[i];

            if (visual == null)
                continue;

            Vector3 startPosition =
                visual.transform.position;

            Quaternion startRotation =
                visual.transform.rotation;

            visual.transform.SetParent(
                null,
                true
            );

            Vector3 targetPosition =
                discardPilePosition.position;

            targetPosition +=
                Vector3.up *
                (i * 0.012f);

            Quaternion targetRotation =
                discardPilePosition.rotation;

            yield return AnimatePlayCard(
                visual,
                startPosition,
                startRotation,
                targetPosition,
                targetRotation
            );

            visual.transform.SetParent(
                discardPilePosition,
                true
            );

            visual.transform.position =
                targetPosition;

            visual.transform.rotation =
                targetRotation;

            visual.transform.localScale =
                Vector3.one;

            visual.SetAnimationLocked(false);

            visual.CaptureCurrentTransformAsNormal();

            DisableCardColliders(
                visual.gameObject
            );

            currentDiscardVisual =
                visual;
        }

        if (cards.Count == 0)
        {
            state =
                GameState.Playing;

            yield break;
        }

        CardData finalCard =
            cards[cards.Count - 1];

        currentTopCard =
            finalCard;

        playerDrawCount = 0;

        playerDrewPlayableCardThisTurn =
            false;

        // --------------------------------------------------------
        // Resolve.
        // --------------------------------------------------------

        yield return StartCoroutine(
            ResolvePlayedCardAfterAnimation(
                finalCard,
                0
            )
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

        // --------------------------------------------------------
        // If a draw penalty is active, accept it immediately.
        // --------------------------------------------------------

        if (pendingDrawAmount > 0)
        {
            StartCoroutine(
                PlayerTakePenaltyRoutine()
            );

            return;
        }

        // --------------------------------------------------------
        // Normal draw limit.
        // --------------------------------------------------------

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

    // ============================================================
    // NORMAL PLAYER DRAW
    // ============================================================

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

        bool drawnCardPlayable =
            CanPlayCardNow(
                drawnCard
            );

        if (drawnCardPlayable)
        {
            playerDrewPlayableCardThisTurn =
                true;
        }

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

        RefreshAllHands();

        Vector3 finalWorldPosition =
            visual.transform.position;

        Quaternion finalWorldRotation =
            visual.transform.rotation;

        visual.transform.SetParent(
            null,
            true
        );

        visual.SetAnimationLocked(true);

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

        visual.SetCardBackVisible(true);

        yield return AnimateDrawCard(
            visual,
            startPosition,
            startRotation,
            finalWorldPosition,
            finalWorldRotation
        );

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

        // --------------------------------------------------------
        // Three unplayable drawn cards = skip turn.
        // --------------------------------------------------------

        if (playerDrawCount >= maximumPlayerDraws &&
            !playerDrewPlayableCardThisTurn)
        {
            Debug.Log(
                "Drew 3 cards without finding " +
                "a playable drawn card. Turn skipped."
            );

            playerDrawCount = 0;

            playerDrewPlayableCardThisTurn =
                false;

            GoToNextPlayer();

            yield break;
        }

        state =
            GameState.Playing;

        UpdateTurnUI();
    }

    // ============================================================
    // PLAYER TAKES DRAW PENALTY
    // ============================================================

    private IEnumerator PlayerTakePenaltyRoutine()
    {
        state =
            GameState.ResolvingChallenge;

        int amount =
            pendingDrawAmount;

        pendingDrawAmount = 0;

        pendingDrawType =
            CardType.Number;

        for (int i = 0;
             i < amount;
             i++)
        {
            CardData card =
                DrawFromDeck();

            if (card == null)
                break;

            Card3D visual =
                SpawnHandCard(
                    card,
                    playerHandCenter
                );

            if (visual == null)
                break;

            playerHand.Add(card);

            playerVisualCards.Add(
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

            startPosition.y += 0.15f;

            visual.transform.position =
                startPosition;

            visual.transform.rotation =
                drawPilePosition.rotation;

            visual.SetCardBackVisible(true);

            yield return AnimateDrawCard(
                visual,
                startPosition,
                drawPilePosition.rotation,
                finalPosition,
                finalRotation
            );

            visual.transform.SetParent(
                playerHandCenter,
                true
            );

            visual.transform.position =
                finalPosition;

            visual.transform.rotation =
                finalRotation;

            visual.SetCardBackVisible(false);

            visual.SetAnimationLocked(false);

            visual.CaptureCurrentTransformAsNormal();

            yield return new WaitForSeconds(
                0.03f
            );
        }

        playerDrawCount = 0;

        playerDrewPlayableCardThisTurn =
            false;

        RefreshAllHands();

        GoToNextPlayer();
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

            if (raw >= 0.5f)
            {
                card.SetCardBackVisible(false);
            }

            timer +=
                Time.deltaTime;

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

            rotation *=
                Quaternion.Euler(
                    0f,
                    playSpinDegrees * t,
                    0f
                );

            card.transform.rotation =
                rotation;

            timer +=
                Time.deltaTime;

            yield return null;
        }

        card.transform.position =
            targetPosition;

        card.transform.rotation =
            targetRotation;
    }

    // ============================================================
    // CAN PLAY CARD NOW
    // ============================================================

    public bool CanPlayCardNow(
        CardData card)
    {
        if (card == null)
            return false;

        // --------------------------------------------------------
        // Draw stacking
        // --------------------------------------------------------

        if (pendingDrawAmount > 0)
        {
            if (pendingDrawType ==
                CardType.Draw2)
            {
                return card.type ==
                           CardType.Draw2 ||
                       card.type ==
                           CardType.Draw4;
            }

            if (pendingDrawType ==
                CardType.Draw4)
            {
                return card.type ==
                       CardType.Draw4;
            }

            return false;
        }

        // --------------------------------------------------------
        // All specials are jokers.
        // --------------------------------------------------------

        if (card.type != CardType.Number)
            return true;

        if (currentTopCard == null)
            return true;

        // --------------------------------------------------------
        // Color match
        // --------------------------------------------------------

        if (activeColor != CardColor.Wild &&
            card.color == activeColor)
        {
            return true;
        }

        // --------------------------------------------------------
        // Number match
        // --------------------------------------------------------

        if (currentTopCard.type ==
                CardType.Number &&
            card.number ==
                currentTopCard.number)
        {
            return true;
        }

        return false;
    }

    // ============================================================
    // PLAYABLE CARD CHECK
    // ============================================================

    private bool HasPlayableCard(
        List<CardData> hand)
    {
        if (hand == null)
            return false;

        foreach (CardData card in hand)
        {
            if (CanPlayCardNow(card))
                return true;
        }

        return false;
    }

    // ============================================================
    // RESOLVE PLAYED CARD
    // ============================================================

    private IEnumerator ResolvePlayedCardAfterAnimation(
        CardData card,
        int playedByPlayerIndex)
    {
        if (card == null)
        {
            state =
                GameState.Playing;

            GoToNextPlayerFrom(
                playedByPlayerIndex
            );

            yield break;
        }

        // --------------------------------------------------------
        // NORMAL NUMBER CARD
        // --------------------------------------------------------

        if (card.type ==
            CardType.Number)
        {
            activeColor =
                card.color;

            pendingSpecialCard =
                null;

            pendingSpecialPlayerIndex =
                -1;

            yield return null;

            GoToNextPlayerFrom(
                playedByPlayerIndex
            );

            yield break;
        }

        // --------------------------------------------------------
        // WILD COLOR SELECTION
        // --------------------------------------------------------

        if (card.type ==
            CardType.ColorChange)
        {
            if (playedByPlayerIndex == 0)
            {
                waitingForWildColor = true;

                state =
                    GameState.WaitingForWildColor;

                if (ChallengePanel.Instance != null)
                {
                    ChallengePanel.Instance
                        .ShowWildColorSelection();
                }
                else
                {
                    card.SetChosenColor(
                        CardColor.Purple
                    );

                    waitingForWildColor = false;

                    PrepareSpecialChallenge(
                        card,
                        playedByPlayerIndex
                    );
                }

                yield break;
            }

            // AI chooses a color.
            List<CardData> aiHand =
                GetAIHand(
                    playedByPlayerIndex
                );

            CardColor aiColor =
                ChooseAIWildColor(
                    aiHand
                );

            card.SetChosenColor(
                aiColor
            );
        }

        // --------------------------------------------------------
        // Every special creates a challenge opportunity.
        // --------------------------------------------------------

        PrepareSpecialChallenge(
            card,
            playedByPlayerIndex
        );

        yield break;
    }

    // ============================================================
    // PLAYER SELECTED WILD COLOR
    // ============================================================

    public void PlayerSelectedWildColor(
        CardColor color)
    {
        if (state !=
            GameState.WaitingForWildColor)
        {
            return;
        }

        if (!waitingForWildColor)
            return;

        if (pendingSpecialCard != null)
            return;

        // Find the ColorChange card from the discard.
        CardData wildCard =
            currentTopCard;

        if (wildCard == null ||
            wildCard.type !=
                CardType.ColorChange)
        {
            return;
        }

        wildCard.SetChosenColor(
            color
        );

        waitingForWildColor =
            false;

        PrepareSpecialChallenge(
            wildCard,
            0
        );
    }

    // ============================================================
    // PREPARE SPECIAL CHALLENGE
    // ============================================================

    private void PrepareSpecialChallenge(
        CardData card,
        int playedByPlayerIndex)
    {
        pendingSpecialCard =
            card;

        pendingSpecialPlayerIndex =
            playedByPlayerIndex;

        // --------------------------------------------------------
        // Add to draw stack.
        // --------------------------------------------------------

        if (card.type ==
            CardType.Draw2)
        {
            pendingDrawAmount += 2;

            pendingDrawType =
                CardType.Draw2;
        }
        else if (card.type ==
                 CardType.Draw4)
        {
            pendingDrawAmount += 4;

            pendingDrawType =
                CardType.Draw4;
        }

        // --------------------------------------------------------
        // The player after the person who played the card
        // becomes the challenger.
        // --------------------------------------------------------

        currentPlayerIndex =
            GetNextPlayerIndex(
                playedByPlayerIndex
            );

        state =
            GameState.WaitingForChallenge;

        UpdateTurnUI();

        OpenChallengeForCurrentPlayer();
    }

    // ============================================================
    // OPEN CHALLENGE
    // ============================================================

    private void OpenChallengeForCurrentPlayer()
    {
        if (currentPlayerIndex == 0)
        {
            // ----------------------------------------------------
            // HUMAN
            // ----------------------------------------------------

            if (ChallengePanel.Instance != null)
            {
                ChallengePanel.Instance
                    .ShowChallenge(
                        pendingSpecialCard,
                        pendingDrawAmount
                    );
            }
            else
            {
                Debug.LogWarning(
                    "ChallengePanel is missing. " +
                    "Automatically continuing."
                );

                DeclinePlayerChallenge();
            }

            return;
        }

        // --------------------------------------------------------
        // AI
        // --------------------------------------------------------

        StartCoroutine(
            HandleAIChallengeDecision()
        );
    }

    // ============================================================
    // PLAYER ACCEPTS CHALLENGE
    // ============================================================

    public void AcceptPlayerChallenge()
    {
        if (state !=
            GameState.WaitingForChallenge)
        {
            return;
        }

        if (!PlayerTurn)
            return;

        BeginChallenge(
            true
        );
    }

    // ============================================================
    // PLAYER DECLINES CHALLENGE
    // ============================================================

    public void DeclinePlayerChallenge()
    {
        if (state !=
            GameState.WaitingForChallenge)
        {
            return;
        }

        if (!PlayerTurn)
            return;

        HideChallengePanel();

        // --------------------------------------------------------
        // Draw card stack:
        //
        // Player now gets the choice:
        // stack another +2/+4 OR draw penalty.
        // --------------------------------------------------------

        if (pendingDrawAmount > 0)
        {
            state =
                GameState.Playing;

            UpdateTurnUI();

            return;
        }

        // --------------------------------------------------------
        // Other specials immediately resolve.
        // --------------------------------------------------------

        ResolveAcceptedSpecial();
    }

    // ============================================================
    // AI CHALLENGE DECISION
    // ============================================================

    private IEnumerator HandleAIChallengeDecision()
    {
        yield return new WaitForSeconds(
            aiThinkingTime
        );

        if (state !=
            GameState.WaitingForChallenge)
        {
            yield break;
        }

        AIOpponent opponent =
            GetAI(
                currentPlayerIndex
            );

        if (opponent == null)
        {
            ResolveAcceptedSpecial();

            yield break;
        }

        int difficulty = 1;

        if (MinigameManager.Instance != null)
        {
            difficulty =
                MinigameManager.Instance
                    .GetCurrentDifficulty(currentPlayerIndex);
        }

        bool challenge =
            opponent.ShouldChallenge(
                difficulty
            );

        // --------------------------------------------------------
        // AI CHALLENGES
        // --------------------------------------------------------

        if (challenge)
        {
            if (MinigameManager.Instance != null)
            {
                MinigameManager.Instance.RegisterChallenge(
                    currentPlayerIndex
                );

                difficulty =
                    MinigameManager.Instance
                        .GetCurrentDifficulty(currentPlayerIndex);
            }

            bool challengerWon =
                opponent.SimulateMinigame(
                    difficulty
                );

            BeginChallenge(
                false,
                challengerWon
            );

            yield break;
        }

        // --------------------------------------------------------
        // AI DOES NOT CHALLENGE
        // --------------------------------------------------------

        if (pendingDrawAmount > 0)
        {
            List<CardData> hand =
                GetAIHand(
                    currentPlayerIndex
                );

            CardData response =
                opponent.ChooseCard(
                    hand,
                    currentTopCard,
                    activeColor,
                    pendingDrawAmount,
                    pendingDrawType
                );

            if (response != null)
            {
                state =
                    GameState.Playing;

                yield return new WaitForSeconds(
                    0.08f
                );

                yield return
                    PlayAICardGroupRoutine(
                        currentPlayerIndex,
                        response
                    );

                yield break;
            }

            // No stack card.
            yield return
                AITakePenaltyRoutine();

            yield break;
        }

        // Skip / Reverse / Wild.
        ResolveAcceptedSpecial();
    }

    // ============================================================
    // BEGIN CHALLENGE
    // ============================================================

    private void BeginChallenge(
        bool playerIsChallenger)
    {
        int difficulty = 1;

        if (MinigameManager.Instance != null)
        {
            difficulty =
                MinigameManager.Instance
                    .GetCurrentDifficulty(currentPlayerIndex);
        }

        if (!playerIsChallenger)
        {
            AIOpponent opponent =
                GetAI(
                    currentPlayerIndex
                );

            if (opponent == null)
            {
                ChallengeFinished(true);

                return;
            }

            bool result =
                opponent.SimulateMinigame(
                    difficulty
                );

            BeginChallenge(
                false,
                result
            );

            return;
        }

        if (MinigameManager.Instance == null)
        {
            Debug.LogError(
                "MinigameManager is missing."
            );

            return;
        }

        // The player/AI has actually committed to the challenge here.
        // Increase only that challenger's personal progression.
        MinigameManager.Instance.RegisterChallenge(
            currentPlayerIndex
        );

        difficulty =
            MinigameManager.Instance
                .GetCurrentDifficulty(currentPlayerIndex);

        state =
            GameState.CameraTransition;

        CardColor challengeColor =
            GetChallengeColorForPendingCard();

        MinigameManager.Instance
            .StartChallenge(
                challengeColor,
                true
            );
    }

    // ============================================================
    // AI CHALLENGE RESULT DIRECT
    // ============================================================

    private void BeginChallenge(
        bool playerIsChallenger,
        bool challengerWon)
    {
        if (playerIsChallenger)
        {
            BeginChallenge(
                true
            );

            return;
        }

        if (MinigameManager.Instance != null)
        {
            MinigameManager.Instance
                .CompleteAIChallenge(
                    challengerWon
                );
        }
        else
        {
            ChallengeFinished(
                challengerWon
            );
        }
    }

    // ============================================================
    // GET CHALLENGE COLOR
    // ============================================================

    private CardColor GetChallengeColorForPendingCard()
    {
        if (pendingSpecialCard == null)
            return CardColor.Purple;

        if (pendingSpecialCard.type ==
            CardType.ColorChange)
        {
            if (pendingSpecialCard.chosenColor !=
                CardColor.Wild)
            {
                return pendingSpecialCard
                    .chosenColor;
            }
        }

        if (pendingSpecialCard.color ==
            CardColor.Wild)
        {
            return CardColor.Purple;
        }

        return pendingSpecialCard.color;
    }

    // ============================================================
    // CHALLENGE FINISHED
    // ============================================================

    public void ChallengeFinished(
        bool challengerWon)
    {
        if (pendingSpecialCard == null)
        {
            state =
                GameState.Playing;

            GoToNextPlayer();

            return;
        }

        state =
            GameState.ResolvingChallenge;

        HideChallengePanel();

        StartCoroutine(
            FinishChallengeRoutine(
                challengerWon
            )
        );
    }

    // ============================================================
    // FINISH CHALLENGE
    // ============================================================

    private IEnumerator FinishChallengeRoutine(
        bool challengerWon)
    {
        yield return new WaitForSeconds(
            0.15f
        );

        if (challengerWon)
        {
            ResolveSuccessfulChallenge();
        }
        else
        {
            ResolveFailedChallenge();
        }
    }

    // ============================================================
    // SUCCESSFUL CHALLENGE
    // ============================================================

    private void ResolveSuccessfulChallenge()
    {
        Debug.Log(
            "CHALLENGE WON - SPECIAL EFFECT CANCELLED."
        );

        // Entire draw stack is cancelled.
        pendingDrawAmount = 0;

        pendingDrawType =
            CardType.Number;

        // Color Change is cancelled too.
        // activeColor remains whatever it was before.
        pendingSpecialCard =
            null;

        pendingSpecialPlayerIndex =
            -1;

        waitingForWildColor =
            false;

        // Challenger used their turn challenging,
        // so move to the next player.
        GoToNextPlayerFrom(
            currentPlayerIndex
        );
    }

    // ============================================================
    // FAILED CHALLENGE
    // ============================================================

    private void ResolveFailedChallenge()
    {
        CardData card =
            pendingSpecialCard;

        Debug.Log(
            "CHALLENGE FAILED."
        );

        // --------------------------------------------------------
        // +2 / +4
        //
        // Entire accumulated penalty doubles.
        // --------------------------------------------------------

        if (card.type ==
                CardType.Draw2 ||
            card.type ==
                CardType.Draw4)
        {
            pendingDrawAmount *= 2;

            if (pendingDrawAmount <= 0)
                pendingDrawAmount =
                    card.GetDrawValue();

            Debug.Log(
                "Failed challenge. " +
                "New penalty = +" +
                pendingDrawAmount
            );

            StartCoroutine(
                ApplyFailedDrawPenalty()
            );

            return;
        }

        // --------------------------------------------------------
        // SKIP
        //
        // Normal skip = 1 turn.
        // Failed challenge = 2 turns.
        // --------------------------------------------------------

        if (card.type ==
            CardType.Skip)
        {
            pendingSpecialCard =
                null;

            pendingSpecialPlayerIndex =
                -1;

            AdvanceTurnsFromCurrent(
                2
            );

            return;
        }

        // --------------------------------------------------------
        // REVERSE
        //
        // Reverse still happens.
        // Challenger then loses one turn.
        // --------------------------------------------------------

        if (card.type ==
            CardType.Reverse)
        {
            turnDirection *= -1;

            pendingSpecialCard =
                null;

            pendingSpecialPlayerIndex =
                -1;

            AdvanceTurnsFromCurrent(
                1
            );

            return;
        }

        // --------------------------------------------------------
        // WILD / COLOR CHANGE
        //
        // Selected color applies.
        // Challenger loses one turn.
        // --------------------------------------------------------

        if (card.type ==
            CardType.ColorChange)
        {
            if (card.chosenColor !=
                CardColor.Wild)
            {
                activeColor =
                    card.chosenColor;
            }

            pendingSpecialCard =
                null;

            pendingSpecialPlayerIndex =
                -1;

            AdvanceTurnsFromCurrent(
                1
            );

            return;
        }

        pendingSpecialCard =
            null;

        pendingSpecialPlayerIndex =
            -1;

        GoToNextPlayer();
    }

    // ============================================================
    // FAILED DRAW PENALTY
    // ============================================================

    private IEnumerator ApplyFailedDrawPenalty()
    {
        state =
            GameState.ResolvingChallenge;

        int amount =
            pendingDrawAmount;

        pendingDrawAmount = 0;

        pendingDrawType =
            CardType.Number;

        // --------------------------------------------------------
        // Challenger draws the doubled penalty.
        // --------------------------------------------------------

        List<CardData> hand =
            GetHand(
                currentPlayerIndex
            );

        List<Card3D> visuals =
            GetVisuals(
                currentPlayerIndex
            );

        Transform center =
            GetHandCenter(
                currentPlayerIndex
            );

        bool isPlayer =
            currentPlayerIndex == 0;

        for (int i = 0;
             i < amount;
             i++)
        {
            CardData drawn =
                DrawFromDeck();

            if (drawn == null)
                break;

            if (hand == null)
                break;

            hand.Add(drawn);

            Card3D visual =
                SpawnHandCard(
                    drawn,
                    center
                );

            if (visual == null)
                continue;

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

            startPosition.y += 0.15f;

            visual.transform.position =
                startPosition;

            visual.transform.rotation =
                drawPilePosition.rotation;

            visual.SetCardBackVisible(
                !isPlayer
            );

            yield return AnimateDrawCard(
                visual,
                startPosition,
                drawPilePosition.rotation,
                finalPosition,
                finalRotation
            );

            visual.transform.SetParent(
                center,
                true
            );

            visual.transform.position =
                finalPosition;

            visual.transform.rotation =
                finalRotation;

            visual.SetCardBackVisible(
                !isPlayer
            );

            visual.SetAnimationLocked(false);

            visual.CaptureCurrentTransformAsNormal();

            yield return new WaitForSeconds(
                0.025f
            );
        }

        pendingSpecialCard =
            null;

        pendingSpecialPlayerIndex =
            -1;

        RefreshAllHands();

        // Challenger has consumed their turn.
        AdvanceTurnsFromCurrent(
            1
        );
    }

    // ============================================================
    // ACCEPT SPECIAL WITHOUT CHALLENGING
    // ============================================================

    private void ResolveAcceptedSpecial()
    {
        if (pendingSpecialCard == null)
        {
            GoToNextPlayer();

            return;
        }

        CardData card =
            pendingSpecialCard;

        HideChallengePanel();

        // --------------------------------------------------------
        // DRAW CARDS
        //
        // Do NOT resolve immediately.
        // The current player may stack another +2/+4.
        // --------------------------------------------------------

        if (card.type ==
                CardType.Draw2 ||
            card.type ==
                CardType.Draw4)
        {
            state =
                GameState.Playing;

            UpdateTurnUI();

            return;
        }

        // --------------------------------------------------------
        // SKIP
        // --------------------------------------------------------

        if (card.type ==
            CardType.Skip)
        {
            pendingSpecialCard =
                null;

            pendingSpecialPlayerIndex =
                -1;

            AdvanceTurnsFromCurrent(
                2
            );

            return;
        }

        // --------------------------------------------------------
        // REVERSE
        // --------------------------------------------------------

        if (card.type ==
            CardType.Reverse)
        {
            turnDirection *= -1;

            pendingSpecialCard =
                null;

            pendingSpecialPlayerIndex =
                -1;

            GoToNextPlayer();

            return;
        }

        // --------------------------------------------------------
        // COLOR CHANGE
        // --------------------------------------------------------

        if (card.type ==
            CardType.ColorChange)
        {
            if (card.chosenColor !=
                CardColor.Wild)
            {
                activeColor =
                    card.chosenColor;
            }

            pendingSpecialCard =
                null;

            pendingSpecialPlayerIndex =
                -1;

            GoToNextPlayer();

            return;
        }

        pendingSpecialCard =
            null;

        pendingSpecialPlayerIndex =
            -1;

        GoToNextPlayer();
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

        if (opponent == null ||
            hand == null)
        {
            GoToNextPlayer();

            yield break;
        }

        CardData chosen =
            opponent.ChooseCard(
                hand,
                currentTopCard,
                activeColor,
                pendingDrawAmount,
                pendingDrawType
            );

        if (chosen != null)
        {
            yield return
                PlayAICardGroupRoutine(
                    currentPlayerIndex,
                    chosen
                );

            yield break;
        }

        // --------------------------------------------------------
        // If a draw stack is active, AI must take it.
        // --------------------------------------------------------

        if (pendingDrawAmount > 0)
        {
            yield return
                AITakePenaltyRoutine();

            yield break;
        }

        // --------------------------------------------------------
        // Normal AI draw up to 3 times.
        // --------------------------------------------------------

        bool aiFoundPlayable =
            false;

        for (int draw = 0;
             draw < maximumPlayerDraws;
             draw++)
        {
            CardData drawn =
                DrawFromDeck();

            if (drawn == null)
                break;

            bool playable =
                CanPlayCardNow(
                    drawn
                );

            if (playable)
                aiFoundPlayable = true;

            hand.Add(
                drawn
            );

            List<Card3D> visuals =
                GetAIVisuals(
                    currentPlayerIndex
                );

            Transform center =
                GetAIHandCenter(
                    currentPlayerIndex
                );

            Card3D visual =
                SpawnHandCard(
                    drawn,
                    center
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

                startPosition.y += 0.15f;

                visual.transform.position =
                    startPosition;

                visual.transform.rotation =
                    drawPilePosition.rotation;

                visual.SetCardBackVisible(
                    true
                );

                yield return AnimateDrawCard(
                    visual,
                    startPosition,
                    drawPilePosition.rotation,
                    finalPosition,
                    finalRotation
                );

                visual.transform.SetParent(
                    center,
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

            CardData afterDraw =
                opponent.ChooseCard(
                    hand,
                    currentTopCard,
                    activeColor,
                    pendingDrawAmount,
                    pendingDrawType
                );

            if (afterDraw != null)
            {
                yield return
                    PlayAICardGroupRoutine(
                        currentPlayerIndex,
                        afterDraw
                    );

                yield break;
            }

            if (aiFoundPlayable)
            {
                // Keep drawing until AI has used
                // its normal turn decisions.
                continue;
            }
        }

        GoToNextPlayer();
    }

    // ============================================================
    // AI TAKES PENALTY
    // ============================================================

    private IEnumerator AITakePenaltyRoutine()
    {
        state =
            GameState.ResolvingChallenge;

        int amount =
            pendingDrawAmount;

        pendingDrawAmount = 0;

        pendingDrawType =
            CardType.Number;

        List<CardData> hand =
            GetAIHand(
                currentPlayerIndex
            );

        List<Card3D> visuals =
            GetAIVisuals(
                currentPlayerIndex
            );

        Transform center =
            GetAIHandCenter(
                currentPlayerIndex
            );

        for (int i = 0;
             i < amount;
             i++)
        {
            CardData drawn =
                DrawFromDeck();

            if (drawn == null)
                break;

            hand.Add(
                drawn
            );

            Card3D visual =
                SpawnHandCard(
                    drawn,
                    center
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

                visual.SetAnimationLocked(
                    true
                );

                Vector3 startPosition =
                    drawPilePosition.position;

                startPosition.y +=
                    0.15f;

                visual.transform.position =
                    startPosition;

                visual.transform.rotation =
                    drawPilePosition.rotation;

                visual.SetCardBackVisible(
                    true
                );

                yield return AnimateDrawCard(
                    visual,
                    startPosition,
                    drawPilePosition.rotation,
                    finalPosition,
                    finalRotation
                );

                visual.transform.SetParent(
                    center,
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
            }
        }

        RefreshAllHands();

        pendingSpecialCard =
            null;

        pendingSpecialPlayerIndex =
            -1;

        GoToNextPlayer();
    }

    // ============================================================
    // AI PLAY GROUP
    // ============================================================

    private IEnumerator PlayAICardGroupRoutine(
        int playerIndex,
        CardData chosen)
    {
        List<CardData> hand =
            GetAIHand(
                playerIndex
            );

        List<Card3D> visuals =
            GetAIVisuals(
                playerIndex
            );

        if (hand == null ||
            visuals == null ||
            chosen == null)
        {
            GoToNextPlayer();

            yield break;
        }

        List<CardData> cards =
            BuildAIPlayGroup(
                hand,
                chosen
            );

        state =
            GameState.ResolvingChallenge;

        List<Card3D> cardsVisuals =
            new List<Card3D>();

        foreach (CardData card in cards)
        {
            Card3D visual =
                FindVisualForCard(
                    visuals,
                    card
                );

            if (visual != null)
                cardsVisuals.Add(
                    visual
                );
        }

        foreach (CardData card in cards)
            hand.Remove(card);

        foreach (Card3D visual in cardsVisuals)
            visuals.Remove(visual);

        // AI wild selection.
        if (chosen.type ==
            CardType.ColorChange)
        {
            chosen.SetChosenColor(
                ChooseAIWildColor(hand)
            );
        }

        RefreshAllHands();

        ClearDiscardVisuals();

        for (int i = 0;
             i < cardsVisuals.Count;
             i++)
        {
            Card3D visual =
                cardsVisuals[i];

            if (visual == null)
                continue;

            visual.SetHovered(false);

            visual.SetAnimationLocked(
                true
            );

            visual.SetCardBackVisible(
                false
            );

            Vector3 startPosition =
                visual.transform.position;

            Quaternion startRotation =
                visual.transform.rotation;

            visual.transform.SetParent(
                null,
                true
            );

            Vector3 targetPosition =
                discardPilePosition.position;

            targetPosition +=
                Vector3.up *
                (i * 0.012f);

            yield return
                AnimatePlayCard(
                    visual,
                    startPosition,
                    startRotation,
                    targetPosition,
                    discardPilePosition.rotation
                );

            visual.transform.SetParent(
                discardPilePosition,
                true
            );

            visual.transform.position =
                targetPosition;

            visual.transform.rotation =
                discardPilePosition.rotation;

            visual.transform.localScale =
                Vector3.one;

            visual.SetAnimationLocked(
                false
            );

            visual.CaptureCurrentTransformAsNormal();

            DisableCardColliders(
                visual.gameObject
            );

            currentDiscardVisual =
                visual;
        }

        currentTopCard =
            cards[cards.Count - 1];

        yield return StartCoroutine(
            ResolvePlayedCardAfterAnimation(
                currentTopCard,
                playerIndex
            )
        );
    }

    // ============================================================
    // AI GROUP
    // ============================================================

    private List<CardData> BuildAIPlayGroup(
        List<CardData> hand,
        CardData chosen)
    {
        List<CardData> group =
            new List<CardData>();

        if (chosen == null)
            return group;

        if (chosen.type !=
            CardType.Number)
        {
            group.Add(chosen);

            return group;
        }

        foreach (CardData card in hand)
        {
            if (card == null)
                continue;

            if (card.type !=
                CardType.Number)
                continue;

            if (card.number ==
                chosen.number)
            {
                group.Add(card);
            }
        }

        if (group.Count == 0)
            group.Add(chosen);

        return group;
    }

    // ============================================================
    // AI WILD COLOR
    // ============================================================

    private CardColor ChooseAIWildColor(
        List<CardData> hand)
    {
        int purple = 0;
        int green = 0;
        int yellow = 0;
        int red = 0;

        if (hand != null)
        {
            foreach (CardData card in hand)
            {
                if (card == null)
                    continue;

                if (card.type !=
                    CardType.Number)
                    continue;

                switch (card.color)
                {
                    case CardColor.Purple:
                        purple++;
                        break;

                    case CardColor.Green:
                        green++;
                        break;

                    case CardColor.Yellow:
                        yellow++;
                        break;

                    case CardColor.Red:
                        red++;
                        break;
                }
            }
        }

        int highest =
            Mathf.Max(
                purple,
                green,
                yellow,
                red
            );

        if (highest == purple)
            return CardColor.Purple;

        if (highest == green)
            return CardColor.Green;

        if (highest == yellow)
            return CardColor.Yellow;

        return CardColor.Red;
    }

    // ============================================================
    // FIND VISUAL
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

        card.SetupCard(
            data
        );

        return card;
    }

    // ============================================================
    // SPAWN DISCARD
    // ============================================================

    private void SpawnDiscardCard(
        CardData data)
    {
        if (discardPilePosition == null ||
            cardPrefab == null)
        {
            return;
        }

        ClearDiscardVisuals();

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

        card.SetupCard(
            data
        );

        card.SetCardBackVisible(
            false
        );

        card.CaptureCurrentTransformAsNormal();

        DisableCardColliders(
            obj
        );

        currentDiscardVisual =
            card;
    }

    // ============================================================
    // CLEAR DISCARD
    // ============================================================

    private void ClearDiscardVisuals()
    {
        if (discardPilePosition == null)
            return;

        for (int i =
             discardPilePosition.childCount - 1;
             i >= 0;
             i--)
        {
            Destroy(
                discardPilePosition
                    .GetChild(i)
                    .gameObject
            );
        }

        currentDiscardVisual =
            null;
    }

    // ============================================================
    // DRAW PILE VISUAL
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
                card.SetCardBackVisible(
                    true
                );

                card.SetAnimationLocked(
                    true
                );

                card.CaptureCurrentTransformAsNormal();
            }

            DisableCardColliders(
                obj
            );
        }
    }

    // ============================================================
    // TURN SYSTEM
    // ============================================================

    private int GetNextPlayerIndex(
        int fromIndex)
    {
        int next =
            fromIndex +
            turnDirection;

        if (next < 0)
            next = 3;

        if (next > 3)
            next = 0;

        return next;
    }

    private void GoToNextPlayer()
    {
        GoToNextPlayerFrom(
            currentPlayerIndex
        );
    }

    private void GoToNextPlayerFrom(
        int fromIndex)
    {
        currentPlayerIndex =
            GetNextPlayerIndex(
                fromIndex
            );

        playerDrawCount = 0;

        playerDrewPlayableCardThisTurn =
            false;

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
    // ADVANCE MULTIPLE TURNS
    // ============================================================

    private void AdvanceTurnsFromCurrent(
        int numberOfTurns)
    {
        int startingIndex =
            currentPlayerIndex;

        int resultIndex =
            startingIndex;

        for (int i = 0;
             i < numberOfTurns;
             i++)
        {
            resultIndex =
                GetNextPlayerIndex(
                    resultIndex
                );
        }

        currentPlayerIndex =
            resultIndex;

        playerDrawCount = 0;

        playerDrewPlayableCardThisTurn =
            false;

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
    // DISABLE CARD COLLIDERS
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

    // ============================================================
    // GET HAND
    // ============================================================

    private List<CardData> GetHand(
        int index)
    {
        if (index == 0)
            return playerHand;

        return GetAIHand(index);
    }

    // ============================================================
    // GET VISUALS
    // ============================================================

    private List<Card3D> GetVisuals(
        int index)
    {
        if (index == 0)
            return playerVisualCards;

        return GetAIVisuals(index);
    }

    // ============================================================
    // GET HAND CENTER
    // ============================================================

    private Transform GetHandCenter(
        int index)
    {
        if (index == 0)
            return playerHandCenter;

        return GetAIHandCenter(index);
    }

    // ============================================================
    // GET AI HAND
    // ============================================================

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

    // ============================================================
    // GET AI VISUALS
    // ============================================================

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

    // ============================================================
    // GET AI HAND CENTER
    // ============================================================

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

    // ============================================================
    // GET AI
    // ============================================================

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
    // REFRESH HANDS
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

    // ============================================================
    // DESTROY HAND VISUALS
    // ============================================================

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
            playerVisualCards.Contains(
                card
            );
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
            if (state ==
                GameState.WaitingForChallenge)
            {
                turnText.text =
                    "YOUR TURN - CHALLENGE?";
            }
            else if (pendingDrawAmount > 0)
            {
                turnText.text =
                    "YOUR TURN - +" +
                    pendingDrawAmount;
            }
            else
            {
                turnText.text =
                    "YOUR TURN  " +
                    "DRAW " +
                    playerDrawCount +
                    "/" +
                    maximumPlayerDraws;
            }

            return;
        }

        if (currentPlayerIndex == 1)
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
    // CHALLENGE PANEL
    // ============================================================

    private void HideChallengePanel()
    {
        if (ChallengePanel.Instance != null)
        {
            ChallengePanel.Instance.HideAll();
        }
    }

    // ============================================================
    // PUBLIC GETTERS
    // ============================================================

    public int GetPendingDrawAmount()
    {
        return pendingDrawAmount;
    }

    public CardType GetPendingDrawType()
    {
        return pendingDrawType;
    }

    public CardColor GetActiveColor()
    {
        return activeColor;
    }

    public CardData GetPendingSpecialCard()
    {
        return pendingSpecialCard;
    }
}