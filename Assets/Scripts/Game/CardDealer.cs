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

        // [수정] 상대방용 핸드 위치와 슬롯 (최대 3명, 1명당 8장)
        public Transform[] opponentHandPositions; // 24개 (3명×8장, 0~7:1번, 8~15:2번, 16~23:3번)
        public Transform[] opponentSlotTransforms; // 3개 (상대방별 카드 부모)

        private List<Card> dealtCards = new List<Card>();

        // MySlot에서 호출: 카드 한 장을 지정 위치로 딜링
        public Card DealCard(Sprite cardSprite, Transform targetPos, CardData cardData = null)
        {
            Card card = Instantiate(cardPrefab, deckPos.position, Quaternion.identity, mySlotTransform);
            card.SetupAsMyCard(cardData, cardSprite, backSprite);
            // 카드 이동/애니메이션은 MySlot 등 핸드 관리자가 직접 수행
            return card;
        }

        // [수정] 상대방 카드 딜링 (상대방 인덱스 지정)
        public Card DealOpponentCard(int opponentIndex, int cardIndex)
        {
            int posIdx = opponentIndex * 8 + cardIndex;
            if (opponentHandPositions == null || opponentHandPositions.Length < 24 || opponentSlotTransforms == null || opponentSlotTransforms.Length < 3)
                return null;
            var targetPos = opponentHandPositions[posIdx];
            var parent = opponentSlotTransforms[opponentIndex];
            Card card = Instantiate(cardPrefab, deckPos.position, Quaternion.identity, parent);
            card.SetupAsOpponentCard(backSprite);
            card.transform.position = targetPos.position;
            // 크기/스케일 복사
            var cardRect = card.GetComponent<RectTransform>();
            var targetRect = targetPos.GetComponent<RectTransform>();
            if (cardRect != null && targetRect != null)
            {
                cardRect.sizeDelta = targetRect.sizeDelta;
                cardRect.localScale = targetRect.localScale;
            }
            else
            {
                card.transform.localScale = targetPos.localScale;
            }
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