using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BalatroOnline.Game
{
    public class PokerHandResult
    {
        public string HandName;
        public int Score;
        public int Multiplier;
    }

    public static class HandEvaluator
    {
        // 족보별 점수/배수 테이블(임시)
        private static readonly Dictionary<string, (int score, int multiplier)> payTable = new()
        {
            { "Royal Flush", (800, 800) },
            { "Straight Flush", (50, 50) },
            { "Four of a Kind", (25, 25) },
            { "Full House", (9, 9) },
            { "Flush", (6, 6) },
            { "Straight", (4, 4) },
            { "Three of a Kind", (3, 3) },
            { "Two Pair", (2, 2) },
            { "One Pair", (1, 1) },
            { "High Card", (0, 0) },
        };

        public static PokerHandResult Evaluate(CardData[] cards)
        {
            if (cards == null || cards.Length < 1) return new PokerHandResult { HandName = "", Score = 0, Multiplier = 0 };
            var values = cards.Select(c => c.ValueEnum).OrderBy(v => v).ToList();
            var suits = cards.Select(c => c.TypeEnum).ToList();
            bool isFlush = suits.Distinct().Count() == 1 && cards.Length == 5;
            bool isStraight = IsStraight(values);
            var groups = values.GroupBy(v => v).OrderByDescending(g => g.Count()).ToList();

            // Royal Flush
            if (isFlush && isStraight && values.Min() == CardValue.Ten && values.Max() == CardValue.Ace)
                return MakeResult("Royal Flush");
            // Straight Flush
            if (isFlush && isStraight)
                return MakeResult("Straight Flush");
            // Four of a Kind
            if (groups[0].Count() == 4)
                return MakeResult("Four of a Kind");
            // Full House
            if (groups[0].Count() == 3 && groups.Count > 1 && groups[1].Count() == 2)
                return MakeResult("Full House");
            // Flush
            if (isFlush)
                return MakeResult("Flush");
            // Straight
            if (isStraight)
                return MakeResult("Straight");
            // Three of a Kind
            if (groups[0].Count() == 3)
                return MakeResult("Three of a Kind");
            // Two Pair
            if (groups.Count > 1 && groups[0].Count() == 2 && groups[1].Count() == 2)
                return MakeResult("Two Pair");
            // One Pair
            if (groups[0].Count() == 2)
                return MakeResult("One Pair");
            // High Card
            return MakeResult("High Card");
        }

        private static bool IsStraight(List<CardValue> values)
        {
            if (values.Count != 5) return false;
            var ordered = values.OrderBy(v => (int)v).ToList();
            // A,2,3,4,5 스트레이트 처리
            if (ordered[0] == CardValue.Ace && ordered[1] == CardValue.Two && ordered[2] == CardValue.Three && ordered[3] == CardValue.Four && ordered[4] == CardValue.Five)
                return true;
            for (int i = 1; i < 5; i++)
                if ((int)ordered[i] != (int)ordered[i - 1] + 1)
                    return false;
            return true;
        }

        private static PokerHandResult MakeResult(string handName)
        {
            var (score, multiplier) = payTable[handName];
            return new PokerHandResult { HandName = handName, Score = score, Multiplier = multiplier };
        }
    }
} 