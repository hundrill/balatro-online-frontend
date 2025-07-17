using System.Collections.Generic;
using BalatroOnline.Game;

// public enum Suit
// {
//     Spade,
//     Diamond,
//     Heart,
//     Club
// }

// public class CardInfo
// {
//     public Suit Suit;
//     public int Rank; // 1~13 (A~K)
// }

public class HandContext
{
    // 카드 관련 데이터
    public List<CardData> PlayedCards { get; } = new List<CardData>();
    public List<CardData> UnUsedCards { get; } = new List<CardData>();
    public CardDataHolder currentCardData;
    public Card currentCard;

    // 족보 관련 데이터
    public PokerHand PokerHand { get; set; } = PokerHand.None;
    public PokerHand UnUsedPokerHand { get; set; } = PokerHand.None;

    /// <summary>
    /// 남은 버리기 횟수
    /// </summary>
    public int remainingDiscards = 0;
    /// <summary>
    /// 남은 덱 카드 수
    /// </summary>
    public int remainingDeck = 0;
    /// <summary>
    /// 남은 7 카드 수
    /// </summary>
    public int remainingSevens = 0;

    /// <summary>
    /// 왼쪽 조커 카드 (조커 AO에서 참조)
    /// </summary>
    public JokerCard leftJokerCard = null;

    // 점수 관련 데이터
    public float Multiplier { get; set; } = 1f;
    public float Chips { get; set; } = 0;

    /// <summary>
    /// 사용하지 않은 카드 중 특정 무늬(CardType)의 개수를 반환
    /// </summary>
    public int CountUnUsedCardsOfSuit(CardType suit)
    {
        if (UnUsedCards == null) return 0;
        int count = 0;
        foreach (var card in UnUsedCards)
        {
            if (card.suit == suit)
                count++;
        }
        return count;
    }

    /// <summary>
    /// 족보에 사용한 카드(UsedCards) 중 Ace가 몇 개인지 반환
    /// </summary>
    public int CountAcesInUsedCards()
    {
        if (PlayedCards == null) return 0;
        int count = 0;
        foreach (var card in PlayedCards)
        {
            if (card.rank == 1) // Ace는 1로 정의됨
                count++;
        }
        return count;
    }

    /// <summary>
    /// 족보에 사용한 카드가 count장이고, 모든 카드의 무늬가 인자와 같으면 true 반환
    /// </summary>
    public bool IsUsedCardsOfSuitCount(CardType suit, int count)
    {
        if (PlayedCards == null || PlayedCards.Count != count)
            return false;
        foreach (var card in PlayedCards)
        {
            if (card.suit != suit)
                return false;
        }
        return true;
    }

    /// <summary>
    /// currentCardData의 rank가 짝수면 true 반환
    /// </summary>
    public bool IsCurrentCardDataEvenRank()
    {
        return currentCardData != null && currentCardData.rank % 2 == 0;
    }

    // 유틸리티 메서드
    public void Clear()
    {
        PlayedCards.Clear();
        UnUsedCards.Clear();
        currentCardData = null;
        currentCard = null;
        PokerHand = PokerHand.None;
        UnUsedPokerHand = PokerHand.None;
        Multiplier = 1f;
        Chips = 0;
        remainingDiscards = 0;
        remainingDeck = 0;
        remainingSevens = 0;
        leftJokerCard = null;
    }

    public bool HasPlayedCards => PlayedCards.Count > 0;
    public bool HasUnUsedCards => UnUsedCards.Count > 0;
    public int TotalCardCount => PlayedCards.Count + UnUsedCards.Count;

    // Pair 관련 체크 함수들
    public bool HasPairInPlayedCards()
    {
        return IsPairHand(PokerHand);
    }

    public bool HasPairInUnUsedCards()
    {
        return IsPairHand(UnUsedPokerHand);
    }

    // Triple 관련 체크 함수들
    public bool HasTripleInPlayedCards()
    {
        return IsTripleHand(PokerHand);
    }

    public bool HasTripleInUnUsedCards()
    {
        return IsTripleHand(UnUsedPokerHand);
    }

    // 주어진 족보가 pair를 포함하는지 확인하는 헬퍼 메서드
    private bool IsPairHand(PokerHand hand)
    {
        switch (hand)
        {
            case PokerHand.OnePair:
            case PokerHand.TwoPair:
            case PokerHand.ThreeOfAKind:
            case PokerHand.FullHouse:
            case PokerHand.FourOfAKind:
                return true;
            default:
                return false;
        }
    }

    // 주어진 족보가 triple을 포함하는지 확인하는 헬퍼 메서드
    private bool IsTripleHand(PokerHand hand)
    {
        switch (hand)
        {
            case PokerHand.ThreeOfAKind:
            case PokerHand.FullHouse:
            case PokerHand.FourOfAKind:
                return true;
            default:
                return false;
        }
    }
}