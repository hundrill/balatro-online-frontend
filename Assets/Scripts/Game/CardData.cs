using UnityEngine;

namespace BalatroOnline.Game
{
    public enum CardType { Clubs, Diamonds, Hearts, Spades }
    public enum CardValue { Ace = 1, Two, Three, Four, Five, Six, Seven, Eight, Nine, Ten, Jack, Queen, King }

    public class CardData
    {
        public string suit; // "Clubs", "Diamonds", "Hearts", "Spades"
        public int rank;    // 1~13 (A~K)
        public Sprite sprite;

        public CardData(string suit, int rank)
        {
            this.suit = suit;
            this.rank = rank;
            this.sprite = null;
        }

        // Poker용 enum 변환 프로퍼티
        public CardType TypeEnum => (CardType)System.Enum.Parse(typeof(CardType), suit);
        public CardValue ValueEnum => (CardValue)rank;
    }
} 