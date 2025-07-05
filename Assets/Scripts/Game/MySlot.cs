using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using System.Linq;
using TMPro;

namespace BalatroOnline.Game
{
    public class MySlot : MonoBehaviour
    {
        public List<Card> handCards = new List<Card>();
        public Transform[] handPositions; // 8개 슬롯
        public Transform[] handPlayPositions; // 5개 슬롯
        public CardDealer cardDealer;
        [SerializeField] private TextMeshProUGUI rankText;
        [SerializeField] private TextMeshProUGUI scoreText;
        public TextMeshProUGUI nicknameText; // 인스펙터에서 연결

        public enum SortType { Rank, Suit }
        public SortType userSortType = SortType.Rank; // TODO: UserSettings에서 불러오도록

        private int cardsArrived = 0;

        // 드래그 중인 카드를 추적하기 위한 필드
        private Card currentDraggingCard = null;

        private string currentUserId;

        void Awake()
        {

        }

        public void SetUser(string userId, string nickname)
        {
            currentUserId = userId;
            if (nicknameText != null)
            {
                nicknameText.text = string.IsNullOrEmpty(nickname) ? userId : nickname;
                nicknameText.gameObject.SetActive(true);
            }
        }

        public void ReceiveInitialCards(List<CardData> cardDatas)
        {
            StartCoroutine(ReceiveInitialCardsRoutine(cardDatas));
        }

        private IEnumerator ReceiveInitialCardsRoutine(List<CardData> cardDatas)
        {
            handCards.Clear();
            cardsArrived = 0;
            for (int i = 0; i < cardDatas.Count; i++)
            {
                var cardData = cardDatas[i];
                if (cardDealer != null)
                {
                    cardData.sprite = cardDealer.FindSprite(cardData.suit, cardData.rank);
                }
                Card card = cardDealer.DealCard(cardData.sprite, handPositions[i], cardData);
                card.myPlayer = this;
                card.OnMoveComplete = OnCardArrived;
                handCards.Add(card);
                card.MoveToPosition(handPositions[i].position, i);
                yield return new WaitForSeconds(0.1f);
            }
            while (cardsArrived < handCards.Count)
                yield return null;
            Debug.Log("[MySlot] 정렬 전 handCards 순서:");
            LogHandCards();
            if (userSortType == SortType.Rank)
                SortHandByRank();
            else
                SortHandBySuit();
            Debug.Log("[MySlot] 정렬 후 handCards 순서:");
            LogHandCards();
            UpdateHandCardPositions();
        }

        private void OnCardArrived(Card card)
        {
            cardsArrived++;
            //Debug.Log($"[MySlot] OnCardArrived 호출: cardsArrived={cardsArrived}/{handCards.Count}, card={card?.gameObject?.name}");
            // 카드 도착 후 오픈
            var holder = card.GetComponent<CardDataHolder>();
            if (holder != null && cardDealer != null)
            {
                var sprite = cardDealer.FindSprite(holder.suit, holder.rank);
                card.SetCard(sprite);
            }
        }

        private void LogHandCards()
        {
            for (int i = 0; i < handCards.Count; i++)
            {
                var holder = handCards[i].GetComponent<CardDataHolder>();
                if (holder != null)
                    Debug.Log($"[{i}] {holder.rank} {holder.suit}");
                else
                    Debug.Log($"[{i}] CardDataHolder 없음");
            }
        }

        // 카드 위치를 handCards 순서에 맞게 handPositions로 이동
        public void UpdateHandCardPositions()
        {
            for (int i = 0; i < handCards.Count && i < handPositions.Length; i++)
            {
                if (handCards[i] != null && handCards[i] != currentDraggingCard)
                {
                    var card = handCards[i];
                    var rectTransform = card.GetComponent<RectTransform>();
                    if (rectTransform != null)
                    {
                        Vector3 targetPos = handPositions[i].position;
                        // 선택된 카드는 y 위치를 유지하고 x만 정렬
                        if (card.IsSelected())
                        {
                            targetPos.y = rectTransform.position.y;
                        }
                        card.MoveToPosition(targetPos, i);
                    }
                }
            }
        }

        // 카드 드래그 중 마우스 X좌표에 따라 handCards 인덱스 실시간 변경
        public void OnCardDrag(Card draggingCard, float mouseX)
        {
            currentDraggingCard = draggingCard;
            int oldIndex = handCards.IndexOf(draggingCard);
            int newIndex = oldIndex;
            float minDist = float.MaxValue;
            for (int i = 0; i < handPositions.Length; i++)
            {
                float dist = Mathf.Abs(mouseX - handPositions[i].position.x);
                if (dist < minDist)
                {
                    minDist = dist;
                    newIndex = i;
                }
            }
            if (newIndex != oldIndex)
            {
                handCards.RemoveAt(oldIndex);
                handCards.Insert(newIndex, draggingCard);
                UpdateHandCardPositions();
                // 드래그 중에는 정렬 함수 호출하지 않음
            }
        }

        // 드래그 종료 시 호출(카드가 놓였을 때)
        public void OnCardDragEnd(Card draggingCard)
        {
            currentDraggingCard = null;
            UpdateHandCardPositions(); // 모든 카드 위치를 handCards 순서에 맞게 재배치
        }

        // Rank 기준 정렬: 숫자 내림차순, suit 내림차순
        public void SortHandByRank()
        {
            Debug.Log("[MySlot] SortHandByRank 호출");
            handCards.Sort((a, b) =>
            {
                var aData = a.GetComponent<CardDataHolder>();
                var bData = b.GetComponent<CardDataHolder>();
                if (aData == null || bData == null) return 0;
                int cmp = bData.rank.CompareTo(aData.rank); // 내림차순
                if (cmp == 0)
                    cmp = string.Compare(bData.suit, aData.suit); // 내림차순(문자열)
                return cmp;
            });
        }
        // Suit 기준 정렬: suit 오름차순, 숫자 내림차순
        public void SortHandBySuit()
        {
            Debug.Log("[MySlot] SortHandBySuit 호출");
            handCards.Sort((a, b) =>
            {
                var aData = a.GetComponent<CardDataHolder>();
                var bData = b.GetComponent<CardDataHolder>();
                if (aData == null || bData == null) return 0;
                int cmp = string.Compare(aData.suit, bData.suit); // 오름차순(문자열)
                if (cmp == 0)
                    cmp = bData.rank.CompareTo(aData.rank); // 내림차순
                return cmp;
            });
        }

        // 선택된 카드의 suit/rank 반환
        public List<Dictionary<string, object>> GetSelectedCardInfos()
        {
            var infos = new List<Dictionary<string, object>>();
            foreach (var card in handCards)
            {
                if (card != null && card.IsSelected())
                {
                    var holder = card.GetComponent<CardDataHolder>();
                    if (holder != null)
                        infos.Add(new Dictionary<string, object> { { "suit", holder.suit }, { "rank", holder.rank } });
                }
            }
            return infos;
        }

        // 선택된 카드 개수 반환
        public int GetSelectedCount()
        {
            int cnt = 0;
            foreach (var card in handCards)
                if (card != null && card.IsSelected()) cnt++;
            return cnt;
        }

        // 카드 선택 시 최대 5장 제한
        public bool CanSelectMore()
        {
            return GetSelectedCount() < 5;
        }

        // 버리기 버튼 클릭 시 호출
        public void DiscardSelectedCards(string roomId)
        {
            var cards = GetSelectedCardInfos();
            if (cards.Count == 0) return;
            // 네트워크 직접 호출 제거, InGameSceneManager로 위임
            if (BalatroOnline.InGame.InGameSceneManager.Instance != null)
            {
                BalatroOnline.InGame.InGameSceneManager.Instance.OnDiscard(cards, roomId);
            }
            else
            {
                Debug.LogWarning("[MySlot] InGameSceneManager.Instance가 null");
            }
        }

        // discardResult 수신 시 핸드 갱신
        public void OnDiscardResult(List<BalatroOnline.Game.CardData> newHand)
        {
            StartCoroutine(OnDiscardResultRoutine(newHand));
        }

        private IEnumerator OnDiscardResultRoutine(List<BalatroOnline.Game.CardData> newHand)
        {
            // 1. 기존 handCards에서 newHand에 없는 카드(더 이상 안쓰는 카드) 먼저 삭제(fadeout 파괴 및 리스트에서 제거)
            var toRemove = new List<Card>();
            foreach (var card in handCards)
            {
                var holder = card.GetComponent<CardDataHolder>();
                bool existsInNewHand = newHand.Exists(c => c.suit == holder.suit && c.rank == holder.rank);
                if (!existsInNewHand)
                {
                    card.FadeOutAndDestroy(0.5f);
                    toRemove.Add(card);
                }
            }
            foreach (var card in toRemove)
                handCards.Remove(card);
            yield return new WaitForSeconds(0.55f); // fadeout 끝까지 대기

            // 2. 서버에서 받은 newHand 전체를 정렬
            var sortedHand = new List<BalatroOnline.Game.CardData>(newHand);
            if (userSortType == SortType.Rank)
                sortedHand.Sort((a, b) =>
                {
                    int cmp = b.rank.CompareTo(a.rank);
                    if (cmp == 0) cmp = string.Compare(b.suit, a.suit);
                    return cmp;
                });
            else
                sortedHand.Sort((a, b) =>
                {
                    int cmp = string.Compare(a.suit, b.suit);
                    if (cmp == 0) cmp = b.rank.CompareTo(a.rank);
                    return cmp;
                });

            // 3. 정렬된 순서대로 handCards를 재구성 (기존 카드 재사용, 없는 곳은 null)
            var newHandCards = new List<Card>();
            foreach (var cardData in sortedHand)
            {
                var existCard = handCards.Find(card =>
                {
                    var holder = card.GetComponent<CardDataHolder>();
                    return holder.suit == cardData.suit && holder.rank == cardData.rank;
                });
                if (existCard != null)
                {
                    newHandCards.Add(existCard);
                    handCards.Remove(existCard); // 중복 방지
                }
                else
                {
                    newHandCards.Add(null); // 나중에 새 카드 딜링
                }
            }
            handCards = newHandCards;

            // 4. 기존 카드들 먼저 정렬된 위치로 이동
            UpdateHandCardPositions();
            yield return new WaitForSeconds(0.15f); // 카드 이동 애니메이션 약간 대기(필요시)

            // 5. 빈자리에만 새 카드 딜링
            for (int i = 0; i < handCards.Count; i++)
            {
                if (handCards[i] == null)
                {
                    var cardData = sortedHand[i];
                    if (cardDealer != null)
                        cardData.sprite = cardDealer.FindSprite(cardData.suit, cardData.rank);
                    Card card = cardDealer.DealCard(cardData.sprite, handPositions[i], cardData);
                    card.OnMoveComplete = OnCardArrived;
                    card.myPlayer = this;
                    handCards[i] = card;
                    card.MoveToPosition(handPositions[i].position, i);
                    yield return new WaitForSeconds(0.1f);
                }
            }
            // (딜링 후에는 UpdateHandCardPositions() 호출하지 않음)

            // 카드 버리기 후에는 선택된 카드가 없으므로 족보/점수 UI도 클리어
            if (rankText != null) rankText.text = "";
            if (scoreText != null) scoreText.text = "";
        }

        // 카드 클릭 시 호출(이제 선택/해제는 Card.OnPointerClick에서만 처리)
        public void OnCardClicked(Card card)
        {
            var selectedCards = handCards.Where(c => c.IsSelected()).Select(c => c.GetComponent<CardDataHolder>()).Select(h => new CardData(h.suit, h.rank)).ToArray();
            if (selectedCards.Length >= 1 && selectedCards.Length <= 5)
            {
                var result = HandEvaluator.Evaluate(selectedCards);
                Debug.Log($"[Poker] 족보: {result.HandName}, 점수: {result.Score}, 배수: {result.Multiplier}");
                if (rankText != null) rankText.text = result.HandName;
                if (scoreText != null) scoreText.text = $"{result.Score} x {result.Multiplier}";
            }
            else
            {
                if (rankText != null) rankText.text = "";
                if (scoreText != null) scoreText.text = "";
            }
        }

        // handPlayResult(최종 핸드) 연출: 결과 카드들을 handPlayPositions로 이동
        public void MoveHandPlayCardsToCenter(List<CardData> handPlay)
        {
            Debug.Log("MoveHandPlayCardsToCenter 호출 됨");
            if (handPlay == null || handPlay.Count == 0) return;
            // 1. 카드 오브젝트 찾기 (handCards에서 suit/rank로 매칭)
            var moveCards = new List<Card>();
            foreach (var cardData in handPlay)
            {
                var card = handCards.Find(c =>
                {
                    var holder = c.GetComponent<CardDataHolder>();
                    return holder.suit == cardData.suit && holder.rank == cardData.rank;
                });
                if (card != null) moveCards.Add(card);
            }
            // 2. 이동할 handPlayPositions 인덱스 계산
            int n = moveCards.Count;
            int startIdx = 0;
            if (n == 1) startIdx = 2; // 가운데
            else if (n == 2) startIdx = 1; // 1,2번 슬롯
            else if (n == 3) startIdx = 0; // 0,1,2번 슬롯
            else if (n == 4) startIdx = 0; // 0~3번 슬롯
            else if (n == 5) startIdx = 0; // 0~4번 슬롯
            // 3. 카드 이동
            for (int i = 0; i < moveCards.Count; i++)
            {
                int posIdx = startIdx + i;
                if (posIdx >= 0 && posIdx < handPlayPositions.Length)
                {
                    moveCards[i].MoveToPosition(handPlayPositions[posIdx].position, posIdx);
                }
            }
            // 4. 족보/점수 계산 및 UI 표시
            var evalCards = handPlay.Select(cd => new CardData(cd.suit, cd.rank)).ToArray();
            if (evalCards.Length >= 1 && evalCards.Length <= 5)
            {
                var result = HandEvaluator.Evaluate(evalCards);
                Debug.Log($"[Poker] 최종 족보: {result.HandName}, 점수: {result.Score}, 배수: {result.Multiplier}");
                if (rankText != null) rankText.text = result.HandName;
                if (scoreText != null) scoreText.text = $"{result.Score} x {result.Multiplier}";
            }
            else
            {
                if (rankText != null) rankText.text = "";
                if (scoreText != null) scoreText.text = "";
            }
        }

        // 모든 handCards의 상호작용 비활성화(최종 픽스)
        public void FixHandCards()
        {
            foreach (var card in handCards)
            {
                if (card != null)
                    card.SetInteractable(false);
            }
        }

        public string GetUserId()
        {
            return currentUserId;
        }
    }
}

