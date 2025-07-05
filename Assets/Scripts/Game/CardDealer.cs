using System.Collections;
using UnityEngine;
using System.Collections.Generic;

namespace BalatroOnline.Game
{
    public class CardDealer : MonoBehaviour
    {
        public Card cardPrefab;
        public Transform deckPos;
        public Transform[] handPositions; // 8개 핸드 위치
        public Sprite[] cardSprites;  // 실제 카드 스프라이트(52장)
        public Sprite backSprite; // 카드 뒷면 스프라이트
        public Transform mySlotTransform; // 카드의 부모로 사용할 MySlot Transform

        private List<Card> dealtCards = new List<Card>();

        // MySlot에서 호출: 카드 한 장을 지정 위치로 딜링
        public Card DealCard(Sprite cardSprite, Transform targetPos, CardData cardData = null)
        {
            Card card = Instantiate(cardPrefab, deckPos.position, Quaternion.identity, mySlotTransform);
            card.SetBack(backSprite);
            // CardDataHolder 부착 및 데이터 세팅
            var holder = card.GetComponent<CardDataHolder>();
            if (holder == null)
                holder = card.gameObject.AddComponent<CardDataHolder>();
            if (cardData != null)
                holder.SetData(cardData);
            // 카드 이동/애니메이션은 MySlot 등 핸드 관리자가 직접 수행
            return card;
        }

        // suit/rank로부터 알맞은 sprite를 찾아 반환 (최적화)
        private static readonly string[] suitOrder = { "Clubs", "Diamonds", "Hearts", "Spades" };
        public Sprite FindSprite(string suit, int rank)
        {
            // 입력 방어
            if (string.IsNullOrEmpty(suit) || rank < 1 || rank > 13 || cardSprites == null || cardSprites.Length < 52)
                return null;
            int suitIdx = System.Array.IndexOf(suitOrder, suit);
            if (suitIdx < 0) return null;
            int idx = suitIdx * 13 + (rank - 1);
            return (idx >= 0 && idx < cardSprites.Length) ? cardSprites[idx] : null;
        }
    }
} 