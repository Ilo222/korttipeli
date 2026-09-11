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
    public GameObject cardPrefab;

    [Header("Table Positions")]
    public Transform drawPilePosition;
    public Transform discardPilePosition;

    [Header("Player Hand")]
    public Transform playerHandCenter;
    public HandLayout playerHandLayout;

    [Header("AI 1")]
    public Transform ai1HandCenter;
    public AIHandLayout ai1HandLayout;
    public AIOpponent ai1;

    [Header("AI 2")]
    public Transform ai2HandCenter;
    public AIHandLayout ai2HandLayout;
    public AIOpponent ai2;

    [Header("AI 3")]
    public Transform ai3HandCenter;
    public AIHandLayout ai3HandLayout;
    public AIOpponent ai3;

    [Header("UI")]
    public TextMeshProUGUI turnText;

    [Header("Deck")]
    [SerializeField] private int startingCards = 7;

    [Header("Special Card Counts")]
    [SerializeField] private int draw2Copies = 1;
    [SerializeField] private int draw4Copies = 1;
    [SerializeField] private int reverseCopies = 1;
    [SerializeField] private int skipCopies = 1;
    [SerializeField] private int colorChangeCopies = 1;

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

    // 0 = player
    // 1 = AI1
    // 2 = AI2
    // 3 = AI3

    public bool PlayerTurn =>
        currentPlayerIndex == 0;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        StartGame();
    }

    public void StartGame()
    {
        state = GameState.Playing;

        BuildDeck();
        ShuffleDeck();

        DealInitialHands();

        PlaceStartingCard();

        currentPlayerIndex = 0;

        RefreshAllHands();
        UpdateTurnUI();
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

        // 0-4 for each color.
        foreach (CardColor color in colors)
        {
            for (int number = 0; number <= 4; number++)
            {
                deck.Add(
                    new CardData(
                        color,
                        CardType.Number,
                        number));
            }
        }

        AddSpecialCards(CardType.Draw2, draw2Copies);
        AddSpecialCards(CardType.Draw4, draw4Copies);
        AddSpecialCards(CardType.Reverse, reverseCopies);
        AddSpecialCards(CardType.Skip, skipCopies);
        AddSpecialCards(CardType.ColorChange, colorChangeCopies);
    }

    private void AddSpecialCards(
        CardType type,
        int copyCount)
    {
        CardColor[] colors =
        {
            CardColor.Purple,
            CardColor.Green,
            CardColor.Yellow,
            CardColor.Red
        };

        for (int copy = 0; copy < copyCount; copy++)
        {
            foreach (CardColor color in colors)
            {
                deck.Add(
                    new CardData(
                        color,
                        type,
                        -1));
            }
        }
    }

    private void ShuffleDeck()
    {
        for (int i = deck.Count - 1; i > 0; i--)
        {
            int random =
                Random.Range(0, i + 1);

            CardData temp = deck[i];

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
    // DRAWING
    // ============================================================

    private CardData DrawFromDeck()
    {
        if (deck.Count == 0)
        {
            Debug.LogWarning(
                "Deck is empty.");
            return null;
        }

        int last =
            deck.Count - 1;

        CardData card =
            deck[last];

        deck.RemoveAt(last);

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
                playerHandCenter);

        playerVisualCards.Add(visual);
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
                ai1HandCenter);

        ai1VisualCards.Add(visual);
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
                ai2HandCenter);

        ai2VisualCards.Add(visual);
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
                ai3HandCenter);

        ai3VisualCards.Add(visual);
    }

    // ============================================================
    // STARTING CARD
    // ============================================================

    private void PlaceStartingCard()
    {
        currentTopCard =
            DrawFromDeck();

        if (currentTopCard == null)
        {
            Debug.LogError(
                "Could not draw starting card.");

            return;
        }

        SpawnDiscardCard(
            currentTopCard);
    }

    // ============================================================
    // PLAYER CARD
    // ============================================================

    public void PlayCard(Card3D clickedCard)
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
                "That card cannot be played.");

            return;
        }

        CardData playedCard =
            clickedCard.data;

        playerHand.Remove(
            playedCard);

        playerVisualCards.Remove(
            clickedCard);

        Destroy(
            clickedCard.gameObject);

        currentTopCard =
            playedCard;

        SpawnDiscardCard(
            playedCard);

        RefreshAllHands();

        ResolvePlayedCard(
            playedCard,
            0);
    }

    // ============================================================
    // PLAY SPECIAL
    // ============================================================

    private void ResolvePlayedCard(
        CardData card,
        int playedByPlayerIndex)
    {
        if (!card.IsSpecial())
        {
            GoToNextPlayer();
            return;
        }

        state =
            GameState.WaitingForChallenge;

        // If player played it, the next AI gets
        // the challenge decision.
        if (playedByPlayerIndex == 0)
        {
            StartCoroutine(
                NextAIChallengeDecision());
        }
        else
        {
            ShowPlayerChallengePrompt();
        }
    }

    // ============================================================
    // AI CHALLENGE
    // ============================================================

    private IEnumerator NextAIChallengeDecision()
    {
        yield return new WaitForSeconds(0.75f);

        AIOpponent nextAI =
            GetAIForPlayerIndex(
                GetNextPlayerIndex());

        bool challenge =
            nextAI != null &&
            nextAI.ShouldChallenge(
                MinigameManager.Instance.CurrentDifficulty);

        if (challenge)
        {
            int challenger =
                GetNextPlayerIndex();

            BeginChallenge(
                challenger == 0);

            yield break;
        }

        GoToNextPlayer();
    }

    // ============================================================
    // PLAYER CHALLENGE UI
    // ============================================================

    private void ShowPlayerChallengePrompt()
    {
        Debug.Log(
            "Show Challenge / Decline UI.");

        // Connect your Canvas buttons to:
        //
        // AcceptPlayerChallenge()
        // DeclinePlayerChallenge()
    }

    public void AcceptPlayerChallenge()
    {
        if (state != GameState.WaitingForChallenge)
            return;

        BeginChallenge(
            true);
    }

    public void DeclinePlayerChallenge()
    {
        if (state != GameState.WaitingForChallenge)
            return;

        GoToNextPlayer();
    }

    // ============================================================
    // CHALLENGE
    // ============================================================

    private void BeginChallenge(
        bool playerIsChallenger)
    {
        state =
            GameState.CameraTransition;

        MinigameManager.Instance
            .StartChallenge(
                currentTopCard.color,
                playerIsChallenger);
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

        TransitionManager.Instance
            .EndChallenge();

        StartCoroutine(
            FinishChallenge());
    }

    private void ApplyChallengePenalty()
    {
        CardType type =
            currentTopCard.type;

        // This is the generic current foundation.
        // The detailed stacking/reverse/skip rules
        // can be added without changing the card system.

        switch (type)
        {
            case CardType.Draw2:
                DrawPenalty(
                    GetNextPlayerIndex(),
                    4);
                break;

            case CardType.Draw4:
                DrawPenalty(
                    GetNextPlayerIndex(),
                    8);
                break;

            default:
                break;
        }
    }

    private void DrawPenalty(
        int playerIndex,
        int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            AddCardToPlayer(
                playerIndex);
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

        switch (playerIndex)
        {
            case 0:
                playerHand.Add(card);

                playerVisualCards.Add(
                    SpawnHandCard(
                        card,
                        playerHandCenter));
                break;

            case 1:
                ai1Hand.Add(card);

                ai1VisualCards.Add(
                    SpawnHandCard(
                        card,
                        ai1HandCenter));
                break;

            case 2:
                ai2Hand.Add(card);

                ai2VisualCards.Add(
                    SpawnHandCard(
                        card,
                        ai2HandCenter));
                break;

            case 3:
                ai3Hand.Add(card);

                ai3VisualCards.Add(
                    SpawnHandCard(
                        card,
                        ai3HandCenter));
                break;
        }
    }

    private IEnumerator FinishChallenge()
    {
        yield return new WaitForSeconds(
            2f);

        GoToNextPlayer();
    }

    // ============================================================
    // TURNS
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
                RunAITurn());
        }
    }

    public int GetNextPlayerIndex()
    {
        return
            (currentPlayerIndex + 1) % 4;
    }

    private AIOpponent GetAIForPlayerIndex(
        int index)
    {
        switch (index)
        {
            case 1: return ai1;
            case 2: return ai2;
            case 3: return ai3;
            default: return null;
        }
    }

    // ============================================================
    // AI TURN
    // ============================================================

    private IEnumerator RunAITurn()
    {
        yield return new WaitForSeconds(1f);

        AIOpponent opponent =
            GetAIForPlayerIndex(
                currentPlayerIndex);

        List<CardData> hand =
            GetAIHand(
                currentPlayerIndex);

        CardData chosen =
            opponent.ChooseCard(
                hand,
                currentTopCard);

        if (chosen == null)
        {
            AddCardToPlayer(
                currentPlayerIndex);

            RefreshAllHands();

            yield return new WaitForSeconds(
                0.5f);

            GoToNextPlayer();

            yield break;
        }

        PlayAICard(
            currentPlayerIndex,
            chosen);
    }

    private List<CardData> GetAIHand(
        int index)
    {
        switch (index)
        {
            case 1: return ai1Hand;
            case 2: return ai2Hand;
            case 3: return ai3Hand;
            default: return null;
        }
    }

    private void PlayAICard(
        int playerIndex,
        CardData chosen)
    {
        List<CardData> hand =
            GetAIHand(playerIndex);

        hand.Remove(chosen);

        RemoveAIVisual(
            playerIndex,
            chosen);

        currentTopCard =
            chosen;

        SpawnDiscardCard(
            chosen);

        RefreshAllHands();

        ResolvePlayedCard(
            chosen,
            playerIndex);
    }

    private void RemoveAIVisual(
        int playerIndex,
        CardData card)
    {
        List<Card3D> visuals =
            GetAIVisuals(playerIndex);

        for (int i = 0;
             i < visuals.Count;
             i++)
        {
            if (visuals[i].data == card)
            {
                Destroy(
                    visuals[i].gameObject);

                visuals.RemoveAt(i);

                return;
            }
        }
    }

    // ============================================================
    // VISUALS
    // ============================================================

    private Card3D SpawnHandCard(
        CardData data,
        Transform parent)
    {
        GameObject obj =
            Instantiate(
                cardPrefab,
                parent);

        Card3D card =
            obj.GetComponent<Card3D>();

        if (card == null)
        {
            Debug.LogError(
                "CardPrefab is missing Card3D.");

            Destroy(obj);

            return null;
        }

        card.SetupCard(data);

        return card;
    }

    private void SpawnDiscardCard(
        CardData data)
    {
        GameObject obj =
            Instantiate(
                cardPrefab,
                discardPilePosition);

        Card3D card =
            obj.GetComponent<Card3D>();

        card.SetupCard(data);

        card.SetCardBackVisible(false);

        obj.transform.localPosition =
            Vector3.zero;

        obj.transform.localRotation =
            Quaternion.identity;
    }

    private List<Card3D> GetAIVisuals(
        int index)
    {
        switch (index)
        {
            case 1: return ai1VisualCards;
            case 2: return ai2VisualCards;
            case 3: return ai3VisualCards;
            default: return null;
        }
    }

    public void RefreshAllHands()
    {
        if (playerHandLayout != null)
            playerHandLayout.UpdateHandLayout(
                playerVisualCards);

        if (ai1HandLayout != null)
            ai1HandLayout.UpdateAIHand(
                ai1VisualCards);

        if (ai2HandLayout != null)
            ai2HandLayout.UpdateAIHand(
                ai2VisualCards);

        if (ai3HandLayout != null)
            ai3HandLayout.UpdateAIHand(
                ai3VisualCards);
    }

    private void DestroyAllHandVisuals()
    {
        DestroyVisualList(playerVisualCards);
        DestroyVisualList(ai1VisualCards);
        DestroyVisualList(ai2VisualCards);
        DestroyVisualList(ai3VisualCards);

        playerVisualCards.Clear();
        ai1VisualCards.Clear();
        ai2VisualCards.Clear();
        ai3VisualCards.Clear();
    }

    private void DestroyVisualList(
        List<Card3D> cards)
    {
        foreach (Card3D card in cards)
        {
            if (card != null)
                Destroy(card.gameObject);
        }
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
                turnText.text = "YOUR TURN";
                break;

            case 1:
                turnText.text = "AI 1'S TURN";
                break;

            case 2:
                turnText.text = "AI 2'S TURN";
                break;

            case 3:
                turnText.text = "AI 3'S TURN";
                break;
        }
    }
}