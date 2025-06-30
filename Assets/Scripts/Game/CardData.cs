using UnityEngine;

namespace BalatroOnline.Game
{
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
    }
} 