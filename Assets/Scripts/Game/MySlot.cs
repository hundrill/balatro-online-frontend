using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using System.Linq;
using TMPro;
using System;
using Best.HTTP.JSON.LitJson;
using BalatroOnline.Common;

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
        public GameObject handPlayReadyIndicator; // 인스펙터에서 연결
        public GameObject nextRoundReadyIndicator; // 인스펙터에서 연결
        public GameObject winIndicator; // 인스펙터에서 연결
        public Transform avatarPosition; // 아바타 위치 (인스펙터에서 연결)

        // 칩 표시용 TMP (인스펙터에서 연결)
        public TextMeshProUGUI chipText;
        public TextMeshProUGUI fundsText; // 인스펙터에서 연결

        public enum SortType { Rank, Suit }
        public SortType userSortType = SortType.Rank; // TODO: UserSettings에서 불러오도록

        private int cardsArrived = 0;

        // 드래그 중인 카드를 추적하기 위한 필드
        private Card currentDraggingCard = null;
        private CardJoker currentDraggingJoker = null; // 드래그 중인 조커 카드

        private string currentUserId;
        private Dictionary<string, object> currentUserData; // 서버에서 받은 모든 사용자 정보 저장

        // === 조커 카드 관리 ===
        public GameObject cardJokerPrefab; // CardJoker 프리팹 (인스펙터에서 연결)
        public Transform[] ownedJokerPositions; // 구매한 조커 카드 위치들 (JokerPos0~4)
        private List<CardJoker> ownedJokers = new List<CardJoker>();
        private List<ServerCardData> ownedJokerData = new List<ServerCardData>();

        public GameObject ownedJokerInfoPanel;
        public TMP_Text ownedJokerInfoNameText;
        public TMP_Text ownedJokerInfoDescText;

        // 조커 판매 기능을 위한 필드
        private string currentSelectedJokerId = "";
        private int currentSelectedJokerPrice = 0;

        // 서버에서 받은 remaining 값들
        private int remainingDiscards = 0;
        private int remainingDeck = 0;
        private int remainingSevens = 0;

        void Awake()
        {

        }

        /// <summary>
        /// 서버에서 받은 remaining 값들을 설정
        /// </summary>
        public void SetRemainingValues(int discards, int deck, int sevens)
        {
            remainingDiscards = discards;
            remainingDeck = deck;
            remainingSevens = sevens;
            Debug.Log($"[MySlot] SetRemainingValues - 남은버리기: {remainingDiscards}, 남은덱: {remainingDeck}, 남은7: {remainingSevens}");
        }

        /// <summary>
        /// 남은 버리기 횟수 반환
        /// </summary>
        public int GetRemainingDiscards() => remainingDiscards;

        /// <summary>
        /// 남은 덱 카드 수 반환
        /// </summary>
        public int GetRemainingDeck() => remainingDeck;

        /// <summary>
        /// 남은 7 카드 수 반환
        /// </summary>
        public int GetRemainingSevens() => remainingSevens;

        public void SetUser(Dictionary<string, object> userData)
        {
            if (userData == null)
            {
                currentUserId = null;
                currentUserData = null;
                if (nicknameText != null)
                {
                    nicknameText.text = "";
                    nicknameText.gameObject.SetActive(false);
                }
                // 칩 표시도 초기화
                if (chipText != null)
                {
                    chipText.text = "";
                    chipText.gameObject.SetActive(false);
                }
                return;
            }

            // userData에서 userId와 nickname 추출
            string userId = userData.TryGetValue("userId", out var uidObj) ? uidObj.ToString() : "";
            string nickname = userData.TryGetValue("nickname", out var nickObj) ? nickObj.ToString() : "";

            // 칩 정보 로그
            int silverChip = userData.TryGetValue("silverChip", out var silverObj) ? Convert.ToInt32(silverObj) : 0;
            int goldChip = userData.TryGetValue("goldChip", out var goldObj) ? Convert.ToInt32(goldObj) : 0;

            chipText.text = ChipDisplayUtil.FormatChipAmount(silverChip);
            chipText.gameObject.SetActive(true);

            Debug.Log($"[MySlot.SetUser] 서버에서 받은 칩 정보: 실버칩={silverChip}, 골드칩={goldChip}");

            currentUserId = userId;
            currentUserData = userData;

            if (nicknameText != null)
            {
                nicknameText.text = string.IsNullOrEmpty(nickname) ? userId : nickname;
                nicknameText.gameObject.SetActive(true);
            }
        }

        // 서버에서 받은 사용자 정보 반환
        public Dictionary<string, object> GetUserData()
        {
            return currentUserData;
        }

        // 특정 필드 값 반환
        public T GetUserDataField<T>(string fieldName, T defaultValue = default(T))
        {
            if (currentUserData != null && currentUserData.TryGetValue(fieldName, out var value))
            {
                try
                {
                    return (T)Convert.ChangeType(value, typeof(T));
                }
                catch
                {
                    return defaultValue;
                }
            }
            return defaultValue;
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
                // === 크기/스케일 맞추기 ===
                var cardRect = card.GetComponent<RectTransform>();
                var posRect = handPositions[i].GetComponent<RectTransform>();
                if (cardRect != null && posRect != null)
                {
                    cardRect.sizeDelta = posRect.sizeDelta;
                    cardRect.localScale = posRect.localScale;
                }
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

        // === 조커 카드 드래그 기능 ===

        // 조커 카드 드래그 중 마우스 X좌표에 따라 ownedJokers 인덱스 실시간 변경
        public void OnJokerDrag(CardJoker draggingJoker, float mouseX)
        {
            currentDraggingJoker = draggingJoker;
            int oldIndex = ownedJokers.IndexOf(draggingJoker);
            int newIndex = oldIndex;
            float minDist = float.MaxValue;

            // 마우스 X좌표와 가장 가까운 조커 위치 찾기
            for (int i = 0; i < ownedJokerPositions.Length; i++)
            {
                float dist = Mathf.Abs(mouseX - ownedJokerPositions[i].position.x);
                if (dist < minDist)
                {
                    minDist = dist;
                    newIndex = i;
                }
            }

            // 인덱스가 변경되면 리스트에서 재배치
            if (newIndex != oldIndex && newIndex < ownedJokers.Count)
            {
                ownedJokers.RemoveAt(oldIndex);
                ownedJokers.Insert(newIndex, draggingJoker);

                // 데이터도 함께 재배치
                var dataToMove = ownedJokerData[oldIndex];
                ownedJokerData.RemoveAt(oldIndex);
                ownedJokerData.Insert(newIndex, dataToMove);

                UpdateJokerPositions();
            }
        }

        // 드래그 종료 시 호출(조커 카드가 놓였을 때)
        public void OnJokerDragEnd(CardJoker draggingJoker)
        {
            currentDraggingJoker = null;
            UpdateJokerPositions(); // 모든 조커 카드 위치를 ownedJokers 순서에 맞게 재배치
        }

        // 조커 카드 위치를 ownedJokers 순서에 맞게 ownedJokerPositions로 이동
        public void UpdateJokerPositions()
        {
            for (int i = 0; i < ownedJokers.Count && i < ownedJokerPositions.Length; i++)
            {
                if (ownedJokers[i] != null && ownedJokers[i] != currentDraggingJoker)
                {
                    var joker = ownedJokers[i];
                    var rectTransform = joker.GetComponent<RectTransform>();
                    if (rectTransform != null)
                    {
                        Vector3 targetPos = ownedJokerPositions[i].position;
                        joker.MoveToPosition(targetPos);
                    }
                }
            }
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
                int aRank = aData.rank == 1 ? 14 : aData.rank; // Ace를 14로 취급
                int bRank = bData.rank == 1 ? 14 : bData.rank;
                int cmp = bRank.CompareTo(aRank); // 내림차순
                if (cmp == 0)
                    cmp = bData.suit.CompareTo(aData.suit); // CardType 내림차순
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
                int cmp = aData.suit.CompareTo(bData.suit); // CardType 오름차순
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

        // DiscardResultResponse 수신 시 핸드 갱신
        public void OnDiscardResult(List<CardData> newHand)
        {
            StartCoroutine(OnDiscardResultRoutine(newHand));
        }

        private IEnumerator OnDiscardResultRoutine(List<CardData> newHand)
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
            var sortedHand = new List<CardData>(newHand);
            if (userSortType == SortType.Rank)
                sortedHand.Sort((a, b) =>
                {
                    int aRank = a.rank == 1 ? 14 : a.rank;
                    int bRank = b.rank == 1 ? 14 : b.rank;
                    int cmp = bRank.CompareTo(aRank);
                    if (cmp == 0) cmp = b.suit.CompareTo(a.suit);
                    return cmp;
                });
            else
                sortedHand.Sort((a, b) =>
                {
                    int cmp = a.suit.CompareTo(b.suit);
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
                Debug.Log($"[Poker] 족보: {result.PokerHand}, 점수: {result.Score}, 배수: {result.Multiplier} Lv.{result.Level}");
                if (rankText != null) rankText.text = $"{result.PokerHand} Lv.{result.Level}";
                if (scoreText != null) scoreText.text = $"{result.Score} x {result.Multiplier} = {result.Score * result.Multiplier}";
                // 카드 클릭 시에는 AddCardScore를 표시하지 않는다.
                foreach (var c in handCards)
                {
                    if (c.AddCardScore != null)
                    {
                        c.AddCardScore.text = "";
                        c.AddCardScore.gameObject.SetActive(false);
                    }
                }
            }
            else
            {
                if (rankText != null) rankText.text = "";
                if (scoreText != null) scoreText.text = "";
                foreach (var c in handCards)
                {
                    if (c.AddCardScore != null)
                    {
                        c.AddCardScore.text = "";
                        c.AddCardScore.gameObject.SetActive(false);
                    }
                }
            }
        }

        // handPlayResult(최종 핸드) 연출: 결과 카드들을 handPlayPositions로 이동
        public IEnumerator MoveHandPlayCardsToCenter(List<CardData> handPlay)
        {
            Debug.Log("MoveHandPlayCardsToCenter 호출 됨");
            if (handPlay == null || handPlay.Count == 0) yield break;
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
            else if (n == 3) startIdx = 1; // 0,1,2번 슬롯
            else if (n == 4) startIdx = 1; // 0~3번 슬롯
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
            PokerHandResult result = null;
            if (evalCards.Length >= 1 && evalCards.Length <= 5)
            {
                result = HandEvaluator.Evaluate(evalCards);
                Debug.Log($"[Poker] 최종 족보: {result.PokerHand}, 점수: {result.Score}, 배수: {result.Multiplier}");
                if (rankText != null) rankText.text = result.PokerHand.ToString();
                if (scoreText != null) scoreText.text = $"{result.Score} x {result.Multiplier}";
            }
            else
            {
                if (rankText != null) rankText.text = "";
                if (scoreText != null) scoreText.text = "";
            }
            // 5. 카드별 AddCardScore 표시 (코루틴 호출)
            yield return StartCoroutine(ShowAddCardScoresSequentially(moveCards, result));
        }

        private IEnumerator ShowAddCardScoresSequentially(List<Card> cards, PokerHandResult handResult)
        {
            // === 1. HandContext 생성 및 초기화 ===
            var context = CreateHandContext(handResult);

            // === 2. 초기 UI 업데이트 ===
            UpdateScoreUI(context.Chips, context.Multiplier);

            // === 3. OnHandPlay 조커 효과 적용 ===
            if (JokerManager.Instance != null)
            {
                JokerManager.Instance.ApplyJokerEffects(JokerEffectTiming.OnHandPlay, context);
                UpdateScoreUI(context.Chips, context.Multiplier);
            }

            yield return new WaitForSeconds(0.5f);

            // === 4. 카드별 애니메이션 및 점수 계산 ===
            yield return StartCoroutine(ProcessCardAnimations(cards, context, handResult));

            // === 5. OnAfterScoring 조커 효과 적용 ===
            if (JokerManager.Instance != null)
            {
                JokerManager.Instance.ApplyJokerEffects(JokerEffectTiming.OnAfterScoring, context);
                UpdateScoreUI(context.Chips, context.Multiplier);
            }
        }

        // HandContext 생성 헬퍼 메서드
        private HandContext CreateHandContext(PokerHandResult handResult)
        {
            var context = new HandContext();

            if (handResult?.UsedCards != null)
            {
                context.PlayedCards.AddRange(handResult.UsedCards);
            }

            if (handResult?.UnUsedCards != null)
            {
                context.UnUsedCards.AddRange(handResult.UnUsedCards);
            }

            context.Multiplier = handResult?.Multiplier ?? 1f;
            context.Chips = handResult?.Score ?? 0;
            context.PokerHand = handResult?.PokerHand ?? PokerHand.None;
            context.UnUsedPokerHand = handResult?.UnUsedPokerHand ?? PokerHand.None;
            context.remainingDiscards = remainingDiscards;
            context.remainingDeck = remainingDeck;
            context.remainingSevens = remainingSevens;

            // 왼쪽 조커 설정 (첫 번째 조커)
            if (JokerManager.Instance != null && JokerManager.Instance.myJokers.Count > 0)
            {
                context.leftJokerCard = JokerManager.Instance.myJokers[0];
            }

            return context;
        }

        // 점수 UI 업데이트 헬퍼 메서드
        private void UpdateScoreUI(float baseScore, float multiplier)
        {
            if (scoreText != null)
            {
                scoreText.text = $"{baseScore} x {multiplier} = {baseScore * multiplier}";
            }
        }

        // 카드 애니메이션 처리 헬퍼 메서드
        private IEnumerator ProcessCardAnimations(List<Card> cards, HandContext context, PokerHandResult handResult)
        {
            var usedCards = handResult?.UsedCards ?? new List<CardData>();
            var usedCardSet = new HashSet<string>(usedCards.Select(c => $"{c.suit},{c.rank}"));

            for (int i = 0; i < cards.Count; i++)
            {
                var card = cards[i];
                if (card?.AddCardScore == null) continue;

                var holder = card.GetComponent<CardDataHolder>();
                if (holder == null) continue;

                string cardKey = $"{holder.suit},{holder.rank}";
                bool isUsed = usedCardSet.Contains(cardKey);

                if (isUsed)
                {
                    int cardValue = CalculateCardValue(holder.rank);
                    card.ShowAndStoreCardScore(cardValue);

                    context.Chips += cardValue;
                    context.currentCardData = holder;
                    context.currentCard = card;

                    JokerManager.Instance.ApplyJokerEffects(JokerEffectTiming.OnScoring, context);

                    UpdateScoreUI(context.Chips, context.Multiplier);
                    yield return new WaitForSeconds(1f);
                }
                else
                {
                    card.AddCardScore.text = "";
                    card.AddCardScore.gameObject.SetActive(false);
                }
            }
        }

        // 카드 값 계산 헬퍼 메서드
        private int CalculateCardValue(int rank)
        {
            if (rank >= 2 && rank <= 10) return rank;
            if (rank == 1) return 11; // Ace
            return 10; // J, Q, K
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

        public void SetReady(bool isReady)
        {
            if (handPlayReadyIndicator != null)
                handPlayReadyIndicator.SetActive(isReady);
        }

        // 구매한 조커 카드 추가
        public void AddOwnedJoker(ServerCardData cardData)
        {
            Debug.Log($"[MySlot] 구매한 조커 카드 추가: {cardData.name} ({cardData.id})");
            ownedJokerData.Add(cardData);
            ShowOwnedJokers();
            SyncOwnedJokersToJokerManager();
        }

        // 구매한 조커 카드들 UI에 표시
        public void ShowOwnedJokers()
        {
            Debug.Log($"[MySlot] 구매한 조커 카드들 표시 시작. 총 {ownedJokerData.Count}개");
            // 기존 UI 오브젝트들 제거
            foreach (var joker in ownedJokers)
            {
                if (joker != null)
                    Destroy(joker.gameObject);
            }
            ownedJokers.Clear();
            // 구매한 조커 카드들을 UI에 표시
            for (int i = 0; i < ownedJokerData.Count && i < ownedJokerPositions.Length; i++)
            {
                var cardData = ownedJokerData[i];
                var position = ownedJokerPositions[i];
                if (position != null)
                {
                    GameObject jokerObj = Instantiate(cardJokerPrefab, position);
                    var cardJoker = jokerObj.GetComponent<CardJoker>();
                    Sprite sprite = null;
                    if (cardData.sprite >= 0 && cardData.sprite < SpriteManager.Instance.jokerSprites.Count)
                    {
                        sprite = SpriteManager.Instance.jokerSprites[cardData.sprite];
                    }
                    cardJoker.SetData(cardData.id, sprite, null, true);
                    if (cardJoker.priceText != null)
                    {
                        cardJoker.priceText.gameObject.SetActive(false);
                    }
                    cardJoker.SetData(cardData.id, sprite,
                        (clickedCard) =>
                        {
                            ShowJokerInfo(clickedCard.jokerCard.name, clickedCard.jokerCard.GetDescription(), cardData.id, cardData.price);
                        }, true
                    );
                    // MySlot 참조 설정
                    cardJoker.SetMySlot(this);
                    // === 크기/스케일 맞추기 ===
                    var cardRect = jokerObj.GetComponent<RectTransform>();
                    var posRect = position.GetComponent<RectTransform>();
                    if (cardRect != null && posRect != null)
                    {
                        cardRect.sizeDelta = posRect.sizeDelta;
                        cardRect.localScale = posRect.localScale;
                    }
                    ownedJokers.Add(cardJoker);
                    Debug.Log($"[MySlot] 조커 카드 UI 생성: {cardData.name} at position {i}");
                }
            }
            Debug.Log($"[MySlot] 구매한 조커 카드들 표시 완료. 총 {ownedJokers.Count}개");
            SyncOwnedJokersToJokerManager();
        }

        public void ShowJokerInfo(string name, string desc, string id = "", int price = 0)
        {
            Debug.Log($"[MySlot] 조커 정보 표시: 이름={name}, 설명={desc}, 아이디={id}, 가격={price}");

            // 현재 선택된 조커 정보 저장
            currentSelectedJokerId = id;
            currentSelectedJokerPrice = price;

            if (ownedJokerInfoPanel != null) ownedJokerInfoPanel.SetActive(true);
            if (ownedJokerInfoNameText != null) ownedJokerInfoNameText.text = name;
            if (ownedJokerInfoDescText != null) ownedJokerInfoDescText.text = desc;
        }

        // 조커 판매 메서드
        public void SellJoker()
        {
            if (string.IsNullOrEmpty(currentSelectedJokerId))
            {
                Debug.LogWarning("[MySlot] 판매할 조커가 선택되지 않았습니다.");
                return;
            }

            Debug.Log($"[MySlot] 조커 판매 시도: 아이디={currentSelectedJokerId}, 가격={currentSelectedJokerPrice}");

            // 판매 확인 다이얼로그 표시
            BalatroOnline.Common.MessageDialogManager.Instance.Show(
                $"정말로 이 조커를 {currentSelectedJokerPrice} Joker Dollar에 판매하시겠습니까?",
                () =>
                {
                    // 확인 시 서버에 판매 요청
                    var roomId = BalatroOnline.Common.SessionManager.Instance.CurrentRoomId;
                    if (!string.IsNullOrEmpty(roomId))
                    {
                        // var data = new Dictionary<string, object>
                        // {
                        //     { "roomId", roomId },
                        //     { "cardId", currentSelectedJokerId },
                        //     { "cardType", "joker" },
                        //     { "price", currentSelectedJokerPrice }
                        // };
                        // SocketManager.Instance.EmitToServer("sellCard", data);

                        SocketManager.Instance.EmitToServer(new SellCardRequest(roomId, currentSelectedJokerId, "joker", currentSelectedJokerPrice));
                    }
                    else
                    {
                        Debug.LogError("[MySlot] roomId가 없어서 판매 요청을 보낼 수 없습니다.");
                    }
                }
            );
        }

        // MySlot의 ownedJokerData를 JokerManager.Instance.MyJokers와 동기화
        private void SyncOwnedJokersToJokerManager()
        {
            if (JokerManager.Instance == null) return;
            JokerManager.Instance.myJokers.Clear();
            foreach (var data in ownedJokerData)
            {
                JokerManager.Instance.AddJokerById(data.id);
            }
        }

        // 구매한 조커 카드 데이터 반환
        public List<ServerCardData> GetOwnedJokers()
        {
            return new List<ServerCardData>(ownedJokerData);
        }

        // 구매한 조커 카드 개수 반환
        public int GetOwnedJokerCount()
        {
            return ownedJokerData.Count;
        }

        // 조커 카드 제거 (판매 시 사용)
        public void RemoveOwnedJoker(string cardId)
        {
            Debug.Log($"[MySlot] 조커 카드 제거: {cardId}");

            // ownedJokerData에서 해당 카드 찾아서 제거
            var cardToRemove = ownedJokerData.Find(card => card.id == cardId);
            if (cardToRemove != null)
            {
                ownedJokerData.Remove(cardToRemove);
                Debug.Log($"[MySlot] ownedJokerData에서 조커 제거 완료: {cardId}");

                // UI 다시 그리기
                ShowOwnedJokers();
            }
            else
            {
                Debug.LogWarning($"[MySlot] 제거할 조커를 찾을 수 없음: {cardId}");
            }
        }

        public void ClearHandCards()
        {
            foreach (var card in handCards)
            {
                if (card != null)
                    Destroy(card.gameObject);
            }
            handCards.Clear();
            if (rankText != null) rankText.text = "";
            if (scoreText != null) scoreText.text = "";
        }

        public void ClearOwnedJokers()
        {
            foreach (var joker in ownedJokers)
            {
                if (joker != null)
                    Destroy(joker.gameObject);
            }
            ownedJokers.Clear();
            ownedJokerData.Clear();
            if (rankText != null) rankText.text = "";
            if (scoreText != null) scoreText.text = "";
        }

        public bool IsEmpty()
        {
            // 유저 ID가 없거나 닉네임 텍스트가 비활성화면 비어있다고 간주
            return string.IsNullOrEmpty(currentUserId) || (nicknameText != null && !nicknameText.gameObject.activeSelf);
        }

        // 라운드 승패 결과 업데이트 메서드 (서버로부터 받은 결과값으로 유저 슬롯 업데이트)
        public IEnumerator UpdateRoundResult(int silverChipsGain, int goldChipsGain, int totalSilverChips, int totalGoldChips, int totalFunds)
        {
            Debug.Log($"[MySlot] 라운드 결과 업데이트: 실버칩={silverChipsGain}, 골드칩={goldChipsGain}, 총 실버칩={totalSilverChips}, 총 골드칩={totalGoldChips}, 총 자금={totalFunds}");

            chipText.text = ChipDisplayUtil.FormatChipAmount(totalSilverChips);

            // funds 텍스트 업데이트
            if (fundsText != null)
            {
                fundsText.text = $"${totalFunds}";
            }

            if (silverChipsGain > 0 || goldChipsGain > 0)
            {
                winIndicator.SetActive(true);

                if (ChipAnimationManager.Instance != null)
                {
                    ChipAnimationManager.Instance.MoveChipsFromStackToSlot(currentUserId, 0, () =>
                    {
                        Debug.Log("모든 칩 이동 완료!");
                    });
                }
                else
                {
                    Debug.LogWarning("[MySlot] ChipAnimationManager.Instance가 null입니다.");
                }
            }

            yield break;
        }
    }
}

