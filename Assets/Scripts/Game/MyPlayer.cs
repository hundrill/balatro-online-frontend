using System.Collections.Generic;
using UnityEngine;
using System.Collections;

namespace BalatroOnline.Game
{
    public class MyPlayer : MonoBehaviour
    {
        public List<Card> handCards = new List<Card>();
        public Transform[] handPositions; // 8개 슬롯
        public CardDealer cardDealer;

        public void ReceiveInitialCards(List<CardData> cardDatas)
        {
            StartCoroutine(ReceiveInitialCardsRoutine(cardDatas));
        }

        private IEnumerator ReceiveInitialCardsRoutine(List<CardData> cardDatas)
        {
            handCards.Clear();
            for (int i = 0; i < cardDatas.Count; i++)
            {
                // suit/rank로부터 sprite를 찾아서 CardData.sprite에 할당
                var cardData = cardDatas[i];
                if (cardDealer != null)
                {
                    cardData.sprite = cardDealer.FindSprite(cardData.suit, cardData.rank);
                }
                Card card = cardDealer.DealCard(cardData.sprite, handPositions[i]);
                handCards.Add(card);
                yield return new WaitForSeconds(0.1f);
            }
        }
    }
} 

