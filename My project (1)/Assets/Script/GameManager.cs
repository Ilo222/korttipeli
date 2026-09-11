using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Prefab")]
    public GameObject cardPrefab;

    [Header("Positions")]
    public Transform drawPilePosition;
    public Transform discardPilePosition;
    public Transform playerHandCenter;
    public Transform aiHandCenter;

    [Header("Layouts")]
    public HandLayout playerHandLayout;
    public AIHandLayout aiHandLayout;

    [Header("UI")]
    public TextMeshProUGUI turnText;

    [Header("AI")]
    public AIOpponent aiOpponent;

    [Header("Game")]
    public GameState state = GameState.Playing;

    public List<CardData> deck =
        new List<CardData>();

    public List<CardData> playerHand =
        new List<CardData>();

    public List<CardData> aiHand =
        new List<CardData>();

    public List<Card3D> playerVisualCards =
        new List<Card3D>();

    public List<Card3D> aiVisualCards =
        new List<Card3D>();

    public CardData currentTopCard;

    public bool playerTurn = true;

    [Header("Deck Settings")]
    [SerializeField] private int startingHandSize = 7;

    // These counts are intentionally configurable because
    // your source specifies the card types but does not
    // give us a definitive copy count for each special.
    [SerializeField] private int draw2Copies = 1;
    [SerializeField] private int draw4Copies = 1;
    [SerializeField] private int reverseCopies = 1;
    [SerializeField] private int skipCopies = 1;
    [SerializeField] private int colorChangeCopies = 1;

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

        UpdateTurnUI();
    }

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
                        number));
            }
        }

        // Specials.
        AddSpecialCards(
            CardType.Draw2,
            draw2Copies);

        AddSpecialCards(
            CardType.Draw4,
            draw4Copies);

        AddSpecialCards(
            CardType.Reverse,
            reverseCopies);

        AddSpecialCards(
            CardType.Skip,
            skipCopies);

        AddSpecialCards(
            CardType.ColorChange,
            colorChangeCopies);
    }

    private void AddSpecialCards(
        CardType type,
        int amount)
    {
        CardColor[] colors =
        {
            CardColor.Purple,
            CardColor.Green,
            CardColor.Yellow,
            CardColor.Red
        };

        for (int i = 0; i < amount; i++)
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
            deck[i] = deck[random];
            deck[random] = temp;
        }
    }

    private void DealInitialHands()
    {
        playerHand.Clear();
        aiHand.Clear();

        ClearVisualHands();

        for (int i = 0; i < startingHandSize; i++)
        {
            DrawToPlayer();
            DrawToAI();
        }

        RefreshHands();
    }

    private void ClearVisualHands()
    {
        foreach (Card3D card in playerVisualCards)
        {
            if (card != null)
                Destroy(card.gameObject);
        }

        foreach (Card3D card in aiVisualCards)
        {
            if (card != null)
                Destroy(card.gameObject);
        }

        playerVisualCards.Clear();
        aiVisualCards.Clear();
    }

    private void PlaceStartingCard()
    {
        currentTopCard = DrawCardFromDeck();

        if (currentTopCard == null)
        {
            Debug.LogError("Deck is empty.");
            return;
        }

        SpawnCardOnDiscardPile(currentTopCard);
    }

    public void PlayCard(Card3D clickedCard)
    {
        if (state != GameState.Playing)
            return;

        if (!playerTurn)
            return;

        if (clickedCard == null ||
            clickedCard.data == null)
            return;

        if (!clickedCard.data.CanPlay(
                currentTopCard))
        {
            Debug.Log("That card cannot be played.");
            return;
        }

        CardData playedCard =
            clickedCard.data;

        playerHand.Remove(playedCard);
        playerVisualCards.Remove(clickedCard);

        Destroy(clickedCard.gameObject);

        currentTopCard = playedCard;

        SpawnCardOnDiscardPile(playedCard);

        RefreshHands();

        HandleCardEffect(
            playedCard,
            true);
    }

    private void HandleCardEffect(
        CardData playedCard,
        bool playedByPlayer)
    {
        if (!playedCard.IsSpecial())
        {
            EndTurn();
            return;
        }

        state = GameState.WaitingForChallenge;

        // This is where we later show the Challenge/Decline UI.
        if (playedByPlayer)
        {
            ShowChallengeForAI();
        }
        else
        {
            ShowChallengeForPlayer();
        }
    }

    private void ShowChallengeForAI()
    {
        StartCoroutine(
            AIChallengeDecision());
    }

    private IEnumerator AIChallengeDecision()
    {
        yield return new WaitForSeconds(0.75f);

        bool challenge =
            aiOpponent != null &&
            aiOpponent.ShouldChallenge(
                MinigameManager.Instance.CurrentDifficulty);

        if (!challenge)
        {
            ResolveNoChallenge();
            yield break;
        }

        BeginChallenge(
            challengerIsPlayer: false);
    }

    private void ShowChallengeForPlayer()
    {
        Debug.Log(
            "Show the Challenge / Decline button.");

        // We will connect this to the Canvas.
    }

    public void PlayerAcceptChallenge()
    {
        if (state != GameState.WaitingForChallenge)
            return;

        BeginChallenge(
            challengerIsPlayer: true);
    }

    public void PlayerDeclineChallenge()
    {
        if (state != GameState.WaitingForChallenge)
            return;

        ResolveNoChallenge();
    }

    private void BeginChallenge(
        bool challengerIsPlayer)
    {
        state = GameState.CameraTransition;

        MinigameManager.Instance.StartChallenge(
            currentTopCard.color,
            challengerIsPlayer);
    }

    public void ChallengeFinished(
        bool challengerWon)
    {
        state = GameState.ResolvingChallenge;

        ResolveChallenge(
            currentTopCard.type,
            challengerWon);
    }

    private void ResolveChallenge(
        CardType specialType,
        bool challengerWon)
    {
        if (!challengerWon)
        {
            ApplyFailedChallengePenalty(
                specialType);
        }

        // Challenge has been resolved.
        StartCoroutine(
            ReturnToNormalGame());
    }

    private void ResolveNoChallenge()
    {
        StartCoroutine(
            ReturnToNormalGame());
    }

    private void ApplyFailedChallengePenalty(
        CardType specialType)
    {
        // The source specifically defines +2
        // challenge failure as a doubled penalty.
        //
        // For the other special cards, their
        // individual effects will be handled as
        // we add the full challenge rules.

        int penalty = 0;

        switch (specialType)
        {
            case CardType.Draw2:
                penalty = 4;
                break;

            case CardType.Draw4:
                penalty = 8;
                break;
        }

        if (penalty > 0)
        {
            if (playerTurn)
            {
                DrawMultiple(
                    playerHand,
                    playerVisualCards,
                    penalty,
                    true);
            }
            else
            {
                DrawMultiple(
                    aiHand,
                    aiVisualCards,
                    penalty,
                    false);
            }
        }
    }

    private IEnumerator ReturnToNormalGame()
    {
        yield return new WaitForSeconds(0.25f);

        TransitionManager.Instance.EndChallenge();

        yield return new WaitForSeconds(1.5f);

        EndTurn();
    }

    public void EndTurn()
    {
        if (playerHand.Count == 0)
        {
            EndGame(true);
            return;
        }

        if (aiHand.Count == 0)
        {
            EndGame(false);
            return;
        }

        playerTurn = !playerTurn;

        state = GameState.Playing;

        UpdateTurnUI();

        if (!playerTurn)
        {
            StartCoroutine(
                RunAITurn());
        }
    }

    private IEnumerator RunAITurn()
    {
        yield return new WaitForSeconds(1f);

        CardData chosen =
            aiOpponent.ChooseCard(
                aiHand,
                currentTopCard);

        if (chosen == null)
        {
            DrawToAI();

            RefreshHands();

            yield return new WaitForSeconds(0.5f);

            EndTurn();

            yield break;
        }

        PlayAICard(chosen);
    }

    private void PlayAICard(CardData chosen)
    {
        aiHand.Remove(chosen);

        RemoveAIVisualForCard(chosen);

        currentTopCard = chosen;

        SpawnCardOnDiscardPile(chosen);

        RefreshHands();

        HandleCardEffect(
            chosen,
            false);
    }

    private void RemoveAIVisualForCard(
        CardData chosen)
    {
        for (int i = 0;
             i < aiVisualCards.Count;
             i++)
        {
            if (aiVisualCards[i].data == chosen)
            {
                Destroy(
                    aiVisualCards[i].gameObject);

                aiVisualCards.RemoveAt(i);
                return;
            }
        }
    }

    private void DrawToPlayer()
    {
        CardData card =
            DrawCardFromDeck();

        if (card == null)
            return;

        playerHand.Add(card);

        Card3D visual =
            SpawnHandCard(
                card,
                playerHandCenter);

        playerVisualCards.Add(visual);
    }

    private void DrawToAI()
    {
        CardData card =
            DrawCardFromDeck();

        if (card == null)
            return;

        aiHand.Add(card);

        Card3D visual =
            SpawnHandCard(
                card,
                aiHandCenter);

        aiVisualCards.Add(visual);
    }

    private void DrawMultiple(
        List<CardData> hand,
        List<Card3D> visuals,
        int amount,
        bool human)
    {
        for (int i = 0; i < amount; i++)
        {
            CardData card =
                DrawCardFromDeck();

            if (card == null)
                break;

            hand.Add(card);

            Card3D visual =
                SpawnHandCard(
                    card,
                    human
                        ? playerHandCenter
                        : aiHandCenter);

            visuals.Add(visual);
        }

        RefreshHands();
    }

    public CardData DrawCardFromDeck()
    {
        if (deck.Count == 0)
        {
            RebuildDeckFromDiscard();
        }

        if (deck.Count == 0)
            return null;

        CardData card =
            deck[deck.Count - 1];

        deck.RemoveAt(deck.Count - 1);

        return card;
    }

    private void RebuildDeckFromDiscard()
    {
        Debug.Log(
            "Deck needs recycling. " +
            "Add discard-pile recycling here.");

        // We deliberately leave the recycle
        // algorithm simple until the discard
        // history is implemented.
    }

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

        card.SetupCard(data);

        return card;
    }

    private void SpawnCardOnDiscardPile(
        CardData data)
    {
        GameObject obj =
            Instantiate(
                cardPrefab,
                discardPilePosition);

        Card3D card =
            obj.GetComponent<Card3D>();

        card.SetupCard(data);

        card.transform.localPosition =
            Vector3.zero;

        card.transform.localRotation =
            Quaternion.identity;
    }

    private void RefreshHands()
    {
        if (playerHandLayout != null)
        {
            playerHandLayout.UpdateHandLayout(
                playerVisualCards);
        }

        if (aiHandLayout != null)
        {
            aiHandLayout.UpdateAIHand(
                aiVisualCards);
        }
    }

    private void UpdateTurnUI()
    {
        if (turnText == null)
            return;

        turnText.text =
            playerTurn
                ? "YOUR TURN"
                : "AI TURN";
    }

    private void EndGame(bool playerWon)
    {
        state = GameState.GameOver;

        if (turnText != null)
        {
            turnText.text =
                playerWon
                    ? "YOU WIN!"
                    : "AI WINS!";
        }

        Debug.Log(
            playerWon
                ? "PLAYER WON"
                : "AI WON");
    }
}