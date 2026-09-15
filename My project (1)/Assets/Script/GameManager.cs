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

    [Header("Deck")]
    [SerializeField] private int startingCards = 7;

    [Header("Special Card Counts")]
    [SerializeField] private int draw2Copies = 1;
    [SerializeField] private int draw4Copies = 1;
    [SerializeField] private int reverseCopies = 1;
    [SerializeField] private int skipCopies = 1;
    [SerializeField] private int colorChangeCopies = 1;

    [Header("Draw Pile Visual")]
    [SerializeField] private int drawPileVisualCards = 8;
    [SerializeField] private float drawPileCardOffset = 0.015f;

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

    [Header("Deck State")]
    public List<CardData> deck =
        new List<CardData>();

    public CardData currentTopCard;

    [Header("Turn")]
    public int currentPlayerIndex = 0;

    // 0 = Player
    // 1 = AI1
    // 2 = AI2
    // 3 = AI3

    public bool PlayerTurn =>
        currentPlayerIndex == 0;

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
        state = GameState.Playing;

        StopAllCoroutines();

        BuildDeck();
        ShuffleDeck();

        DealInitialHands();

        PlaceStartingCard();

        SpawnDrawPileVisual();

        currentPlayerIndex = 0;

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
    // DEALING
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
            DrawToPlayer();
            DrawToAI1();
            DrawToAI2();
            DrawToAI3();
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
                "Deck is empty."
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

    private void DrawToPlayer()
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

    private void DrawToAI1()
    {
        CardData card =
            DrawFromDeck();

        if (card == null)
            return;

        ai1Hand.Add(card);

        Card3D visual =
            SpawnHandCard(
                card,
                ai1HandCenter
            );

        if (visual != null)
        {
            ai1VisualCards.Add(
                visual
            );
        }
    }

    private void DrawToAI2()
    {
        CardData card =
            DrawFromDeck();

        if (card == null)
            return;

        ai2Hand.Add(card);

        Card3D visual =
            SpawnHandCard(
                card,
                ai2HandCenter
            );

        if (visual != null)
        {
            ai2VisualCards.Add(
                visual
            );
        }
    }

    private void DrawToAI3()
    {
        CardData card =
            DrawFromDeck();

        if (card == null)
            return;

        ai3Hand.Add(card);

        Card3D visual =
            SpawnHandCard(
                card,
                ai3HandCenter
            );

        if (visual != null)
        {
            ai3VisualCards.Add(
                visual
            );
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
                "Could not find starting number card."
            );

            return;
        }

        currentTopCard =
            startingCard;

        Debug.Log(
            $"Starting discard: " +
            $"{startingCard.color} " +
            $"{startingCard.number}"
        );

        SpawnDiscardCard(
            startingCard
        );
    }

    // ============================================================
    // HAND CARD SPAWN
    // ============================================================

    private Card3D SpawnHandCard(
        CardData data,
        Transform parent)
    {
        if (cardPrefab == null)
        {
            Debug.LogError(
                "GAME MANAGER: Card Prefab is not assigned."
            );

            return null;
        }

        if (parent == null)
        {
            Debug.LogError(
                "GAME MANAGER: Hand position is not assigned."
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
    // DISCARD PILE
    // ============================================================

    private void SpawnDiscardCard(
        CardData data)
    {
        if (discardPilePosition == null)
        {
            Debug.LogError(
                "DISCARD PILE POSITION IS NOT ASSIGNED."
            );

            return;
        }

        if (cardPrefab == null)
        {
            Debug.LogError(
                "CARD PREFAB IS NOT ASSIGNED."
            );

            return;
        }

        if (currentDiscardVisual != null)
        {
            Destroy(
                currentDiscardVisual.gameObject
            );

            currentDiscardVisual = null;
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
            Debug.LogError(
                "CardPrefab is missing Card3D."
            );

            Destroy(obj);

            return;
        }

        card.SetupCard(data);

        card.SetCardBackVisible(false);

        // THIS IS IMPORTANT:
        // Card3D needs to forget the prefab's
        // old transform and use this pile position
        // as its new normal position.
        card.CaptureCurrentTransformAsNormal();

        Collider[] colliders =
            obj.GetComponentsInChildren<Collider>();

        foreach (Collider col in colliders)
        {
            col.enabled = false;
        }

        currentDiscardVisual =
            card;

        Debug.Log(
            "Discard card spawned at: " +
            obj.transform.position
        );
    }

    // ============================================================
    // DRAW PILE
    // ============================================================

    private void SpawnDrawPileVisual()
    {
        if (drawPilePosition == null)
        {
            Debug.LogError(
                "DRAW PILE POSITION IS NOT ASSIGNED."
            );

            return;
        }

        if (cardPrefab == null)
        {
            Debug.LogError(
                "CARD PREFAB IS NOT ASSIGNED."
            );

            return;
        }

        // Remove old draw-pile cards.
        for (int i =
             drawPilePosition.childCount - 1;
             i >= 0;
             i--)
        {
            Destroy(
                drawPilePosition.GetChild(i).gameObject
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
                $"DrawPileCard_{i}";

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

                // THIS IS THE IMPORTANT FIX.
                // Tell Card3D that this newly
                // positioned transform is its
                // actual normal position.
                card.CaptureCurrentTransformAsNormal();
            }

            // These are visual-only cards.
            // DrawPile.cs handles the click.
            Collider[] colliders =
                obj.GetComponentsInChildren<Collider>();

            foreach (Collider col in colliders)
            {
                col.enabled = false;
            }
        }

        Debug.Log(
            "Draw pile spawned at: " +
            drawPilePosition.position
        );
    }

    // ============================================================
    // PLAYER PLAYS CARD
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

        CardData playedCard =
            clickedCard.data;

        playerHand.Remove(
            playedCard
        );

        playerVisualCards.Remove(
            clickedCard
        );

        Destroy(
            clickedCard.gameObject
        );

        currentTopCard =
            playedCard;

        SpawnDiscardCard(
            playedCard
        );

        RefreshAllHands();

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

        CardData drawnCard =
            DrawFromDeck();

        if (drawnCard == null)
            return;

        playerHand.Add(
            drawnCard
        );

        Card3D visual =
            SpawnHandCard(
                drawnCard,
                playerHandCenter
            );

        if (visual != null)
        {
            playerVisualCards.Add(
                visual
            );
        }

        RefreshAllHands();

        Debug.Log(
            $"You drew " +
            $"{drawnCard.color} " +
            $"{drawnCard.number}"
        );

        // Temporary simple rule:
        // drawing ends the turn.
        GoToNextPlayer();
    }

    // ============================================================
    // CARD RESOLUTION
    // ============================================================

    private void ResolvePlayedCard(
        CardData card,
        int playedByPlayerIndex)
    {
        // Basic card-game loop first.
        // Minigames will be connected after
        // the core game is stable.

        GoToNextPlayer();
    }

    // ============================================================
    // TURN SYSTEM
    // ============================================================

    private void GoToNextPlayer()
    {
        state =
            GameState.Playing;

        currentPlayerIndex =
            GetNextPlayerIndex();

        UpdateTurnUI();

        if (currentPlayerIndex != 0)
        {
            StartCoroutine(
                RunAITurn()
            );
        }
    }

    public int GetNextPlayerIndex()
    {
        return
            (currentPlayerIndex + 1) % 4;
    }

    // ============================================================
    // AI TURN
    // ============================================================

    private IEnumerator RunAITurn()
    {
        yield return new WaitForSeconds(
            1f
        );

        if (state != GameState.Playing)
            yield break;

        AIOpponent opponent =
            GetAIForPlayerIndex(
                currentPlayerIndex
            );

        List<CardData> hand =
            GetAIHand(
                currentPlayerIndex
            );

        if (opponent == null ||
            hand == null)
        {
            Debug.LogError(
                "AI setup is incomplete."
            );

            GoToNextPlayer();

            yield break;
        }

        CardData chosen =
            opponent.ChooseCard(
                hand,
                currentTopCard
            );

        if (chosen == null)
        {
            CardData drawn =
                DrawFromDeck();

            if (drawn != null)
            {
                hand.Add(
                    drawn
                );

                List<Card3D> visuals =
                    GetAIVisuals(
                        currentPlayerIndex
                    );

                Transform anchor =
                    GetAIHandCenter(
                        currentPlayerIndex
                    );

                Card3D visual =
                    SpawnHandCard(
                        drawn,
                        anchor
                    );

                if (visual != null)
                {
                    visuals.Add(
                        visual
                    );
                }

                RefreshAllHands();
            }

            yield return new WaitForSeconds(
                0.5f
            );

            GoToNextPlayer();

            yield break;
        }

        PlayAICard(
            currentPlayerIndex,
            chosen
        );
    }

    private AIOpponent GetAIForPlayerIndex(
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

    private void PlayAICard(
        int playerIndex,
        CardData card)
    {
        List<CardData> hand =
            GetAIHand(
                playerIndex
            );

        if (hand == null)
            return;

        hand.Remove(
            card
        );

        RemoveAIVisual(
            playerIndex,
            card
        );

        currentTopCard =
            card;

        SpawnDiscardCard(
            card
        );

        RefreshAllHands();

        ResolvePlayedCard(
            card,
            playerIndex
        );
    }

    private void RemoveAIVisual(
        int playerIndex,
        CardData card)
    {
        List<Card3D> visuals =
            GetAIVisuals(
                playerIndex
            );

        if (visuals == null)
            return;

        for (int i = 0;
             i < visuals.Count;
             i++)
        {
            Card3D visual =
                visuals[i];

            if (visual == null)
                continue;

            if (visual.data == card)
            {
                Destroy(
                    visual.gameObject
                );

                visuals.RemoveAt(i);

                return;
            }
        }
    }

    // ============================================================
    // VISUAL HELPERS
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

        switch (currentPlayerIndex)
        {
            case 0:
                turnText.text =
                    "YOUR TURN";
                break;

            case 1:
                turnText.text =
                    "AI 1'S TURN";
                break;

            case 2:
                turnText.text =
                    "AI 2'S TURN";
                break;

            case 3:
                turnText.text =
                    "AI 3'S TURN";
                break;
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
            Debug.LogError(
                "MinigameManager is missing."
            );

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

        if (!challengerWon)
        {
            ApplyChallengePenalty();
        }

        if (TransitionManager.Instance != null)
        {
            TransitionManager.Instance
                .EndChallenge();
        }

        StartCoroutine(
            FinishChallenge()
        );
    }

    private void ApplyChallengePenalty()
    {
        if (currentTopCard == null)
            return;

        switch (currentTopCard.type)
        {
            case CardType.Draw2:

                DrawPenalty(
                    GetNextPlayerIndex(),
                    4
                );

                break;

            case CardType.Draw4:

                DrawPenalty(
                    GetNextPlayerIndex(),
                    8
                );

                break;
        }
    }

    private void DrawPenalty(
        int playerIndex,
        int amount)
    {
        for (int i = 0;
             i < amount;
             i++)
        {
            AddCardToPlayer(
                playerIndex
            );
        }

        RefreshAllHands();
    }

    private void AddCardToPlayer(
        int playerIndex)
    {
        CardData card =
            DrawFromDeck();

        if (card == null)
            return;

        List<Card3D> visuals = null;
        Transform anchor = null;

        switch (playerIndex)
        {
            case 0:

                playerHand.Add(card);

                visuals =
                    playerVisualCards;

                anchor =
                    playerHandCenter;

                break;

            case 1:

                ai1Hand.Add(card);

                visuals =
                    ai1VisualCards;

                anchor =
                    ai1HandCenter;

                break;

            case 2:

                ai2Hand.Add(card);

                visuals =
                    ai2VisualCards;

                anchor =
                    ai2HandCenter;

                break;

            case 3:

                ai3Hand.Add(card);

                visuals =
                    ai3VisualCards;

                anchor =
                    ai3HandCenter;

                break;
        }

        if (visuals == null)
            return;

        Card3D visual =
            SpawnHandCard(
                card,
                anchor
            );

        if (visual != null)
        {
            visuals.Add(
                visual
            );
        }
    }

    private IEnumerator FinishChallenge()
    {
        yield return new WaitForSeconds(
            2f
        );

        GoToNextPlayer();
    }
}