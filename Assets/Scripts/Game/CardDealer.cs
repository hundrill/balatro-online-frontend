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

        private List<Card> dealtCards = new List<Card>();

        public void StartDeal()
        {
            Debug.Log("[CardDealer] StartDeal 호출됨");
            StartCoroutine(DealRoutine());
        }

        private IEnumerator DealRoutine()
        {
            dealtCards.Clear();
            for (int i = 0; i < 8; i++)
            {
                Card card = Instantiate(cardPrefab, deckPos.position, Quaternion.identity, transform);
                card.SetBack(backSprite); // 뒷면으로 세팅
                StartCoroutine(MoveCard(card, handPositions[i].position, 0.3f, cardSprites != null && cardSprites.Length > 0 ? cardSprites[i % cardSprites.Length] : null));
                yield return new WaitForSeconds(0.1f);
            }
        }

        private IEnumerator MoveCard(Card card, Vector3 target, float duration, Sprite frontSprite)
        {
            Transform cardTransform = card.transform;
            Vector3 start = cardTransform.position;
            float t = 0;
            while (t < 1)
            {
                t += Time.deltaTime / duration;
                cardTransform.position = Vector3.Lerp(start, target, t);
                yield return null;
            }
            cardTransform.position = target;
            card.SetCard(frontSprite); // 도착하면 바로 오픈
        }

        // MyPlayer에서 호출: 카드 한 장을 지정 위치로 딜링
        public Card DealCard(Sprite cardSprite, Transform targetPos)
        {
            Card card = Instantiate(cardPrefab, deckPos.position, Quaternion.identity, transform);
            card.SetBack(backSprite);
            StartCoroutine(MoveCard(card, targetPos.position, 0.3f, cardSprite));
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