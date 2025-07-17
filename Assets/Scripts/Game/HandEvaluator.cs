using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BalatroOnline.Game
{
    public class PokerHandResult
    {
        public PokerHand PokerHand;
        public int Score;
        public int Multiplier;
        public int Level; // 추가: 족보 레벨
        public List<int> UsedCardIndices; // 족보에 사용된 카드 인덱스
        public List<CardData> UsedCards;  // 족보에 사용된 카드 객체
        public List<CardData> UnUsedCards; // 사용되지 않은 카드들
        public PokerHand UnUsedPokerHand; // 사용되지 않은 카드들로 만들 수 있는 최고 족보
    }

    public static class HandEvaluator
    {
        // Paytable 인스턴스 (싱글턴 또는 DI로 교체 가능)
        public static Paytable paytable = new Paytable();

        // 카드 인덱스 매핑을 위한 헬퍼 메서드
        private static void AddCardsByValue(CardData[] cards, CardValue targetValue, List<int> usedIndices, List<CardData> usedCards)
        {
            for (int i = 0; i < cards.Length; i++)
            {
                if (cards[i].ValueEnum == targetValue)
                {
                    usedIndices.Add(i);
                    usedCards.Add(cards[i]);
                }
            }
        }

        private static void AddCardsByValues(CardData[] cards, CardValue value1, CardValue value2, List<int> usedIndices, List<CardData> usedCards)
        {
            for (int i = 0; i < cards.Length; i++)
            {
                if (cards[i].ValueEnum == value1 || cards[i].ValueEnum == value2)
                {
                    usedIndices.Add(i);
                    usedCards.Add(cards[i]);
                }
            }
        }

        private static void AddAllCards(CardData[] cards, List<int> usedIndices, List<CardData> usedCards)
        {
            for (int i = 0; i < cards.Length; i++)
            {
                usedIndices.Add(i);
                usedCards.Add(cards[i]);
            }
        }

        public static PokerHandResult Evaluate(CardData[] cards)
        {
            if (cards == null || cards.Length < 1) 
            {
                return new PokerHandResult 
                { 
                    PokerHand = PokerHand.None, 
                    Score = 0, 
                    Multiplier = 0, 
                    UsedCardIndices = new List<int>(), 
                    UsedCards = new List<CardData>(), 
                    UnUsedCards = new List<CardData>(), 
                    UnUsedPokerHand = PokerHand.None 
                };
            }

            // 한 번만 계산하여 재사용
            var values = cards.Select(c => c.ValueEnum).OrderBy(v => v).ToList();
            var suits = cards.Select(c => c.TypeEnum).ToList();
            var groups = values.GroupBy(v => v).OrderByDescending(g => g.Count()).ToList();
            
            bool isFlush = suits.Distinct().Count() == 1 && cards.Length == 5;
            bool isStraight = IsStraight(values);

            // 카드 인덱스 매핑용
            List<int> usedIndices = new List<int>();
            List<CardData> usedCards = new List<CardData>();

            // Straight Flush
            if (isFlush && isStraight)
            {
                AddAllCards(cards, usedIndices, usedCards);
                return MakeResult(PokerHand.StraightFlush, usedIndices, usedCards, cards);
            }
            
            // Four of a Kind
            if (groups[0].Count() == 4)
            {
                AddCardsByValue(cards, groups[0].Key, usedIndices, usedCards);
                return MakeResult(PokerHand.FourOfAKind, usedIndices, usedCards, cards);
            }
            
            // Full House
            if (groups[0].Count() == 3 && groups.Count > 1 && groups[1].Count() == 2)
            {
                AddCardsByValues(cards, groups[0].Key, groups[1].Key, usedIndices, usedCards);
                return MakeResult(PokerHand.FullHouse, usedIndices, usedCards, cards);
            }
            
            // Flush
            if (isFlush)
            {
                AddAllCards(cards, usedIndices, usedCards);
                return MakeResult(PokerHand.Flush, usedIndices, usedCards, cards);
            }
            
            // Straight
            if (isStraight)
            {
                AddAllCards(cards, usedIndices, usedCards);
                return MakeResult(PokerHand.Straight, usedIndices, usedCards, cards);
            }
            
            // Three of a Kind
            if (groups[0].Count() == 3)
            {
                AddCardsByValue(cards, groups[0].Key, usedIndices, usedCards);
                return MakeResult(PokerHand.ThreeOfAKind, usedIndices, usedCards, cards);
            }
            
            // Two Pair
            if (groups.Count > 1 && groups[0].Count() == 2 && groups[1].Count() == 2)
            {
                AddCardsByValues(cards, groups[0].Key, groups[1].Key, usedIndices, usedCards);
                return MakeResult(PokerHand.TwoPair, usedIndices, usedCards, cards);
            }
            
            // One Pair
            if (groups[0].Count() == 2)
            {
                AddCardsByValue(cards, groups[0].Key, usedIndices, usedCards);
                return MakeResult(PokerHand.OnePair, usedIndices, usedCards, cards);
            }
            
            // High Card - 가장 높은 카드 1장만 사용
            usedIndices.Add(0);
            usedCards.Add(cards[0]);
            return MakeResult(PokerHand.HighCard, usedIndices, usedCards, cards);
        }

        private static bool IsStraight(List<CardValue> values)
        {
            if (values.Count != 5) return false;
            
            // 정렬된 값들을 배열로 변환하여 더 빠른 접근
            var ordered = values.OrderBy(v => (int)v).ToArray();

            // A,2,3,4,5 스트레이트 처리 (Ace를 1로 사용)
            if (ordered[0] == CardValue.Ace && ordered[1] == CardValue.Two && 
                ordered[2] == CardValue.Three && ordered[3] == CardValue.Four && ordered[4] == CardValue.Five)
                return true;

            // A,K,Q,J,10 스트레이트 처리 (Ace를 14로 사용)
            if (ordered[0] == CardValue.Ten && ordered[1] == CardValue.Jack && 
                ordered[2] == CardValue.Queen && ordered[3] == CardValue.King && ordered[4] == CardValue.Ace)
                return true;

            // 일반적인 스트레이트 처리
            for (int i = 1; i < 5; i++)
            {
                if ((int)ordered[i] != (int)ordered[i - 1] + 1)
                    return false;
            }
            return true;
        }

        private static PokerHandResult MakeResult(PokerHand hand, List<int> usedIndices, List<CardData> usedCards, CardData[] allCards)
        {
            // HashSet을 사용하여 더 빠른 검색
            var usedIndicesSet = new HashSet<int>(usedIndices);
            
            // 사용되지 않은 카드들 계산
            List<CardData> unusedCards = new List<CardData>();
            for (int i = 0; i < allCards.Length; i++)
            {
                if (!usedIndicesSet.Contains(i))
                {
                    unusedCards.Add(allCards[i]);
                }
            }

            // 사용되지 않은 카드들로 만들 수 있는 최고 족보 계산
            PokerHand unusedPokerHand = unusedCards.Count > 0 ? EvaluateUnusedCards(unusedCards.ToArray()) : PokerHand.None;

            return new PokerHandResult
            {
                PokerHand = hand,
                Score = paytable.GetChips(hand),
                Multiplier = paytable.GetMultiplier(hand),
                Level = paytable.GetLevel(hand),
                UsedCardIndices = usedIndices,
                UsedCards = usedCards,
                UnUsedCards = unusedCards,
                UnUsedPokerHand = unusedPokerHand
            };
        }

        // 사용되지 않은 카드들로 만들 수 있는 최고 족보를 계산하는 메서드
        private static PokerHand EvaluateUnusedCards(CardData[] cards)
        {
            if (cards == null || cards.Length < 1) return PokerHand.None;
            
            var values = cards.Select(c => c.ValueEnum).OrderBy(v => v).ToList();
            var suits = cards.Select(c => c.TypeEnum).ToList();
            var groups = values.GroupBy(v => v).OrderByDescending(g => g.Count()).ToList();
            
            bool isFlush = suits.Distinct().Count() == 1 && cards.Length == 5;
            bool isStraight = IsStraight(values);

            // Straight Flush
            if (isFlush && isStraight) return PokerHand.StraightFlush;
            // Four of a Kind
            if (groups[0].Count() == 4) return PokerHand.FourOfAKind;
            // Full House
            if (groups[0].Count() == 3 && groups.Count > 1 && groups[1].Count() == 2) return PokerHand.FullHouse;
            // Flush
            if (isFlush) return PokerHand.Flush;
            // Straight
            if (isStraight) return PokerHand.Straight;
            // Three of a Kind
            if (groups[0].Count() == 3) return PokerHand.ThreeOfAKind;
            // Two Pair
            if (groups.Count > 1 && groups[0].Count() == 2 && groups[1].Count() == 2) return PokerHand.TwoPair;
            // One Pair
            if (groups[0].Count() == 2) return PokerHand.OnePair;
            // High Card
            return PokerHand.HighCard;
        }

        // enum을 snake_case로 변환하는 함수 (사용되지 않는다면 제거 가능)
        private static string ToSnakeCase(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;
            
            var result = new System.Text.StringBuilder();
            for (int i = 0; i < input.Length; i++)
            {
                if (char.IsUpper(input[i]))
                {
                    if (i > 0) result.Append('_');
                    result.Append(char.ToLowerInvariant(input[i]));
                }
                else
                {
                    result.Append(input[i]);
                }
            }
            return result.ToString();
        }
    }
}