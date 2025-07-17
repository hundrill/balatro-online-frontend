using System.Collections.Generic;

namespace BalatroOnline.Game
{
    public enum PokerHand
    {
        None,
        HighCard,
        OnePair,
        TwoPair,
        ThreeOfAKind,
        Straight,
        Flush,
        FullHouse,
        FourOfAKind,
        StraightFlush,
    }

    public class Paytable
    {
        private readonly Dictionary<PokerHand, int> baseChips = new()
        {
            { PokerHand.HighCard, 5 },
            { PokerHand.OnePair, 10 },
            { PokerHand.TwoPair, 20 },
            { PokerHand.ThreeOfAKind, 30 },
            { PokerHand.Straight, 30 },
            { PokerHand.Flush, 35 },
            { PokerHand.FullHouse, 50 },
            { PokerHand.FourOfAKind, 60 },
            { PokerHand.StraightFlush, 100 },
        };
        private readonly Dictionary<PokerHand, int> multipliers = new()
        {
            { PokerHand.HighCard, 1 },
            { PokerHand.OnePair, 2 },
            { PokerHand.TwoPair, 2 },
            { PokerHand.ThreeOfAKind, 3 },
            { PokerHand.Straight, 4 },
            { PokerHand.Flush, 4 },
            { PokerHand.FullHouse, 5 },
            { PokerHand.FourOfAKind, 7 },
            { PokerHand.StraightFlush, 8 },
        };
        private readonly Dictionary<PokerHand, int> levels = new();
        private readonly Dictionary<PokerHand, int> counts = new();
        private PokerHand? currentWin = null;

        public Paytable()
        {
            foreach (PokerHand hand in System.Enum.GetValues(typeof(PokerHand)))
            {
                levels[hand] = 1; // 레벨 1부터 시작
                counts[hand] = 0;
            }
        }

        public int GetLevel(PokerHand hand) => levels.TryGetValue(hand, out var v) ? v : 0;
        public int GetCount(PokerHand hand) => counts.TryGetValue(hand, out var v) ? v : 0;
        public int GetChips(PokerHand hand) => baseChips.TryGetValue(hand, out var v) ? v : 0;
        public int GetMultiplier(PokerHand hand) => multipliers.TryGetValue(hand, out var v) ? v : 0;

        public void EnhanceLevel(PokerHand hand) { if (levels.ContainsKey(hand)) levels[hand]++; }
        public void EnhanceCount(PokerHand hand) { if (counts.ContainsKey(hand)) counts[hand]++; }
        public void EnhanceMultiplier(PokerHand hand, int plus) { if (multipliers.ContainsKey(hand)) multipliers[hand] += plus; }
        public void EnhanceChips(PokerHand hand, int plus) { if (baseChips.ContainsKey(hand)) baseChips[hand] += plus; }

        public void SetCurrentWin(PokerHand hand) { currentWin = hand; }
        public PokerHand? GetCurrentWin() => currentWin;
        public int GetCurrentWinMultiplier() => currentWin.HasValue ? GetMultiplier(currentWin.Value) : 0;
        public int GetCurrentChips() => currentWin.HasValue ? GetChips(currentWin.Value) : 0;

        public void ResetWins() { currentWin = null; }
    }
}