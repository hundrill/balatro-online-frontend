using UnityEngine;
using System;

namespace BalatroOnline.Game
{
    public enum CardType { Clubs, Diamonds, Hearts, Spades }
    public enum CardValue { Ace = 1, Two, Three, Four, Five, Six, Seven, Eight, Nine, Ten, Jack, Queen, King }

    public class CardData
    {
        public CardType suit; // "Clubs", "Diamonds", "Hearts", "Spades"
        public int rank;    // 1~13 (A~K)
        public Sprite sprite;

        public CardData(CardType suit, int rank)
        {
            this.suit = suit;
            this.rank = rank;
            this.sprite = null;
        }

        // Poker용 enum 변환 프로퍼티
        public CardType TypeEnum => suit;
        public CardValue ValueEnum => (CardValue)rank;

        // CardType <-> string 변환 유틸리티
        public static CardType StringToCardType(string suit)
        {
            if (Enum.TryParse<CardType>(suit, ignoreCase: true, out var result))
                return result;
            throw new ArgumentException($"Invalid suit string: {suit}");
        }

        public static string CardTypeToString(CardType suit)
        {
            return suit.ToString();
        }
    }
}