using UnityEngine;

namespace BalatroOnline.Game
{
    // Card 오브젝트에 부착되어 카드의 rank/suit 정보를 보관
    public class CardDataHolder : MonoBehaviour
    {
        public int rank; // 1~13 (A=1, J=11, Q=12, K=13)
        public string suit; // "Clubs", "Diamonds", "Hearts", "Spades"

        // CardData로부터 값 세팅
        public void SetData(CardData data)
        {
            this.rank = data.rank;
            this.suit = data.suit;
        }
    }
} 