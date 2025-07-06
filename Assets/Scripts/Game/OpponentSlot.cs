using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace BalatroOnline.Game
{
    public class OpponentSlot : MonoBehaviour
    {
        public TextMeshProUGUI nicknameText; // 인스펙터에서 연결
        public GameObject handPlayReadyIndicator; // 인스펙터에서 연결
        public GameObject nextRoundReadyIndicator; // 인스펙터에서 연결
        private string currentUserId;
        public List<Card> handCards = new List<Card>();
        public Transform[] handPlayPositions; // 인스펙터에서 연결(최대 5개)
        public CardDealer cardDealer; // 인스펙터에서 연결

        public void SetUser(string userId, string nickname)
        {
            Debug.Log("SetUser " + userId + " " + nickname);
            currentUserId = userId;
            if (nicknameText != null)
            {
                nicknameText.text = string.IsNullOrEmpty(nickname) ? userId : nickname;
                Debug.Log("SetUser2 " + userId + " " + nickname);
            }
            gameObject.SetActive(true); // 슬롯 전체 활성화

            Debug.Log("SetUser3 " + userId + " " + nickname);
        }

        public void ClearSlot()
        {
            currentUserId = null;
            if (nicknameText != null)
            {
                nicknameText.text = "";
            }
            gameObject.SetActive(false); // 슬롯 전체 비활성화
        }

        public bool IsEmpty()
        {
            return string.IsNullOrEmpty(currentUserId);
        }

        public string GetUserId()
        {
            return currentUserId;
        }

        public void SetReady(bool isReady)
        {
            if (handPlayReadyIndicator != null)
                handPlayReadyIndicator.SetActive(isReady);
        }

        // 상대방의 hand를 중앙에 오픈 연출
        public void OpenHandPlayCardsToCenter(List<CardData> handPlay)
        {
            // 기존 카드 제거
            foreach (var card in handCards)
            {
                if (card != null)
                    Destroy(card.gameObject);
            }
            handCards.Clear();
            if (handPlay == null || handPlay.Count == 0) return;
            // 중앙에 배치할 위치 계산 (MySlot과 동일)
            int n = handPlay.Count;
            int startIdx = 0;
            if (n == 1) startIdx = 2;
            else if (n == 2) startIdx = 1;
            else if (n == 3) startIdx = 0;
            else if (n == 4) startIdx = 0;
            else if (n == 5) startIdx = 0;
            for (int i = 0; i < handPlay.Count; i++)
            {
                int posIdx = startIdx + i;
                if (posIdx >= 0 && posIdx < handPlayPositions.Length)
                {
                    var cardData = handPlay[i];
                    if (cardDealer != null)
                        cardData.sprite = cardDealer.FindSprite(cardData.suit, cardData.rank);
                    Card card = cardDealer.DealCard(cardData.sprite, handPlayPositions[posIdx], cardData);
                    handCards.Add(card);
                    card.MoveToPosition(handPlayPositions[posIdx].position, posIdx);
                    card.SetInteractable(false); // 상대방 카드는 상호작용 불가
                }
            }
        }
    }
}