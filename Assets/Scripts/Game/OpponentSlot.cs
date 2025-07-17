using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Collections; // Added for IEnumerator
using System.Linq; // Added for Select
using System;
using BalatroOnline.Common;

namespace BalatroOnline.Game
{
    public class OpponentSlot : MonoBehaviour
    {
        public TextMeshProUGUI nicknameText; // 인스펙터에서 연결
        public GameObject handPlayReadyIndicator; // 인스펙터에서 연결
        public GameObject nextRoundReadyIndicator; // 인스펙터에서 연결
        public GameObject winIndicator; // 인스펙터에서 연결
        public Transform avatarPosition; // 아바타 위치 (인스펙터에서 연결)
        private string currentUserId;
        private Dictionary<string, object> currentUserData; // 서버에서 받은 모든 사용자 정보 저장
        public List<Card> handCards = new List<Card>();
        // HandPlay UI 오브젝트 연결용 필드
        public GameObject handPlayRoot; // HandPlay 전체 오브젝트 (활성/비활성용)
        public Transform[] handPlayPositions; // handplaypos0~5 (핸드플레이 카드용)
        public Transform[] jokerPlayPositions; // jokerpos0~5 (조커 카드용)
        public TMPro.TextMeshProUGUI handPlayRankText; // 족보 표시용
        public TMPro.TextMeshProUGUI handPlayScoreText; // 점수 표시용
        public CardDealer cardDealer; // 인스펙터에서 연결

        // === 조커 카드 관리 ===
        public GameObject cardJokerPrefab; // CardJoker 프리팹 (인스펙터에서 연결)
        public Transform[] ownedJokerPositions; // 구매한 조커 카드 위치들 (JokerPos0~4)
        private List<CardJoker> ownedJokers = new List<CardJoker>();
        private List<ServerCardData> ownedJokerData = new List<ServerCardData>();

        // 발표 전 상태 저장용 필드
        private Dictionary<Card, CardOriginalState> lastCardOriginalStates = new Dictionary<Card, CardOriginalState>();
        private Dictionary<CardJoker, CardOriginalState> lastJokerOriginalStates = new Dictionary<CardJoker, CardOriginalState>();
        private List<Card> lastInactiveHandCards = new List<Card>();

        // 칩 표시용 TMP (인스펙터에서 연결)
        public TextMeshProUGUI chipText;
        public TextMeshProUGUI fundsText; // 인스펙터에서 연결

        public void SetUser(Dictionary<string, object> userData)
        {
            if (userData == null)
            {
                currentUserId = null;
                currentUserData = null;
                if (nicknameText != null)
                {
                    nicknameText.text = "";
                }
                // 칩 표시도 초기화
                if (chipText != null)
                {
                    chipText.text = "";
                    chipText.gameObject.SetActive(false);
                }
                gameObject.SetActive(false); // 슬롯 전체 비활성화
                return;
            }

            // userData에서 userId와 nickname 추출
            string userId = userData.TryGetValue("userId", out var uidObj) ? uidObj.ToString() : "";
            string nickname = userData.TryGetValue("nickname", out var nickObj) ? nickObj.ToString() : "";

            // 칩 정보 로그
            int silverChip = userData.TryGetValue("silverChip", out var silverObj) ? Convert.ToInt32(silverObj) : 0;
            int goldChip = userData.TryGetValue("goldChip", out var goldObj) ? Convert.ToInt32(goldObj) : 0;
            if (chipText != null)
            {
                chipText.text = BalatroOnline.Common.ChipDisplayUtil.FormatChipAmount(silverChip);
                chipText.gameObject.SetActive(true);
            }
            Debug.Log($"[OpponentSlot.SetUser] 서버에서 받은 칩 정보: 실버칩={silverChip}, 골드칩={goldChip}");

            Debug.Log("SetUser " + userId + " " + nickname);
            currentUserId = userId;
            currentUserData = userData;

            if (nicknameText != null)
            {
                nicknameText.text = string.IsNullOrEmpty(nickname) ? userId : nickname;
                Debug.Log("SetUser2 " + userId + " " + nickname);
            }
            gameObject.SetActive(true); // 슬롯 전체 활성화

            Debug.Log("SetUser3 " + userId + " " + nickname);
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

        public void ClearSlot()
        {
            currentUserId = null;
            currentUserData = null;
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

        // handCards와 ownedCards(조커 카드 정보)를 함께 받아서 처리 (코루틴 버전)
        public IEnumerator OpenHandPlayCardsCoroutine(List<CardData> handPlay, List<ServerCardData> ownedJokers = null)
        {
            // 기존 카드 파괴/생성 없이, 앞에서부터 받은 카드 데이터만큼 오픈
            if (handPlay != null && handPlay.Count != 0)
            {
                for (int i = 0; i < handPlay.Count && i < handCards.Count; i++)
                {
                    var card = handCards[i];
                    var cardData = handPlay[i];
                    var holder = card.GetComponent<CardDataHolder>();
                    if (holder != null)
                    {
                        holder.suit = cardData.suit;
                        holder.rank = cardData.rank;
                        if (cardDealer != null)
                        {
                            var sprite = cardDealer.FindSprite(cardData.suit, cardData.rank);
                            card.SetCard(sprite);
                        }
                    }
                    card.SetInteractable(false); // 상호작용 비활성화
                }
                // === 결과 발표 애니메이션(점수 표시) ===
                yield return StartCoroutine(ShowHandPlayResultAnimation(handPlay));
            }
            // 조커 카드 정보가 있으면 오픈
            if (ownedJokers != null)
            {
                OpenOwnedJokers(ownedJokers);
            }
        }

        // 카드/조커 원래 상태 저장용 구조체
        private class CardOriginalState
        {
            public Transform parent;
            public Vector3 position;
            public Vector3 localScale;
            public int siblingIndex;
            public Vector2? sizeDelta; // RectTransform 크기 복구용
        }

        // 결과 발표 애니메이션(카드별 점수, 누적 점수 표시)
        private IEnumerator ShowHandPlayResultAnimation(List<CardData> handPlay)
        {
            // 1. 카드/조커 오브젝트 찾기 (handCards, ownedJokers에서 suit/rank로 매칭)
            var moveCards = new List<Card>();
            var usedCardSet = new HashSet<string>(handPlay.Select(cd => cd.suit + "," + cd.rank));
            lastInactiveHandCards.Clear();
            foreach (var card in handCards)
            {
                var holder = card.GetComponent<CardDataHolder>();
                if (holder != null)
                {
                    string key = holder.suit + "," + holder.rank;
                    if (!usedCardSet.Contains(key))
                    {
                        card.gameObject.SetActive(false); // 사용하지 않는 핸드카드 비활성화
                        lastInactiveHandCards.Add(card); // 복구용 저장
                    }
                }
            }
            foreach (var cardData in handPlay)
            {
                var card = handCards.Find(c =>
                {
                    var holder = c.GetComponent<CardDataHolder>();
                    return holder.suit == cardData.suit && holder.rank == cardData.rank;
                });
                if (card != null) moveCards.Add(card);
            }
            // 조커는 ownedJokers 전체를 대상으로 함 (필요시 handPlay에 조커 정보 추가 가능)
            var moveJokers = new List<CardJoker>(ownedJokers);

            // 2. 원래 상태 저장 (복구용 필드에 저장)
            lastCardOriginalStates.Clear();
            lastJokerOriginalStates.Clear();
            for (int i = 0; i < moveCards.Count && i < handPlayPositions.Length; i++)
            {
                var card = moveCards[i];
                if (card != null)
                {
                    var rect = card.GetComponent<RectTransform>();
                    lastCardOriginalStates[card] = new CardOriginalState
                    {
                        parent = card.transform.parent,
                        position = card.transform.position,
                        localScale = card.transform.localScale,
                        siblingIndex = card.transform.GetSiblingIndex(),
                        sizeDelta = rect != null ? (Vector2?)rect.sizeDelta : null
                    };
                }
            }
            for (int i = 0; i < moveJokers.Count && i < jokerPlayPositions.Length; i++)
            {
                var joker = moveJokers[i];
                if (joker != null)
                {
                    var rect = joker.GetComponent<RectTransform>();
                    lastJokerOriginalStates[joker] = new CardOriginalState
                    {
                        parent = joker.transform.parent,
                        position = joker.transform.position,
                        localScale = joker.transform.localScale,
                        siblingIndex = joker.transform.GetSiblingIndex(),
                        sizeDelta = rect != null ? (Vector2?)rect.sizeDelta : null
                    };
                }
            }

            // 3. HandPlayRoot 활성화
            if (handPlayRoot != null) handPlayRoot.SetActive(true);

            // 4. 카드/조커 handPlay UI 위치로 이동, 크기/레이어 조정
            for (int i = 0; i < moveCards.Count && i < handPlayPositions.Length; i++)
            {
                var card = moveCards[i];
                var pos = handPlayPositions[i];
                if (card != null && pos != null)
                {
                    card.transform.SetParent(handPlayRoot.transform, true);
                    card.transform.position = pos.position;
                    card.transform.localScale = pos.localScale;
                    // === 크기 맞추기 ===
                    var cardRect = card.GetComponent<RectTransform>();
                    var posRect = pos.GetComponent<RectTransform>();
                    if (cardRect != null && posRect != null)
                    {
                        cardRect.sizeDelta = posRect.sizeDelta;
                    }
                    card.transform.SetAsLastSibling(); // 최상위 레이어
                }
            }
            for (int i = 0; i < moveJokers.Count && i < jokerPlayPositions.Length; i++)
            {
                var joker = moveJokers[i];
                var pos = jokerPlayPositions[i];
                if (joker != null && pos != null)
                {
                    joker.transform.SetParent(handPlayRoot.transform, true);
                    joker.transform.position = pos.position;
                    joker.transform.localScale = pos.localScale;
                    // === 크기 맞추기 ===
                    var jokerRect = joker.GetComponent<RectTransform>();
                    var posRect = pos.GetComponent<RectTransform>();
                    if (jokerRect != null && posRect != null)
                    {
                        jokerRect.sizeDelta = posRect.sizeDelta;
                    }
                    joker.transform.SetAsLastSibling();
                }
            }

            // 5. 족보/점수 계산 및 UI 표시
            var evalCards = handPlay.Select(cd => new CardData(cd.suit, cd.rank)).ToArray();
            PokerHandResult result = null;
            if (evalCards.Length >= 1 && evalCards.Length <= 5)
            {
                result = HandEvaluator.Evaluate(evalCards);
                if (handPlayRankText != null) handPlayRankText.text = $"{result.PokerHand} Lv.{result.Level}";
                if (handPlayScoreText != null) handPlayScoreText.text = $"{result.Score} x {result.Multiplier} = {result.Score * result.Multiplier}";
            }
            else
            {
                if (handPlayRankText != null) handPlayRankText.text = "";
                if (handPlayScoreText != null) handPlayScoreText.text = "";
            }

            // 6. 카드별 AddCardScore 애니메이션
            yield return StartCoroutine(ShowAddCardScoresSequentially(moveCards, result));

            // 7. 발표 끝나면 원래 상태로 복귀하지 않고, handPlayRoot도 그대로 활성화 상태로 둔다.
            // (아래 복귀 코드 전체 주석처리)
            /*
            for (int i = 0; i < moveCards.Count && i < handPlayPositions.Length; i++)
            {
                var card = moveCards[i];
                if (card != null && cardOriginalStates.ContainsKey(card))
                {
                    var state = cardOriginalStates[card];
                    card.transform.SetParent(state.parent, true);
                    card.transform.position = state.position;
                    card.transform.localScale = state.localScale;
                    card.transform.SetSiblingIndex(state.siblingIndex);
                }
            }
            for (int i = 0; i < moveJokers.Count && i < jokerPlayPositions.Length; i++)
            {
                var joker = moveJokers[i];
                if (joker != null && jokerOriginalStates.ContainsKey(joker))
                {
                    var state = jokerOriginalStates[joker];
                    joker.transform.SetParent(state.parent, true);
                    joker.transform.position = state.position;
                    joker.transform.localScale = state.localScale;
                    joker.transform.SetSiblingIndex(state.siblingIndex);
                }
            }
            if (handPlayRoot != null) handPlayRoot.SetActive(false);
            */
        }

        private IEnumerator ShowAddCardScoresSequentially(List<Card> cards, PokerHandResult handResult)
        {
            // UsedCards만 점수/애니메이션에 사용
            var usedCards = handResult?.UsedCards ?? new List<CardData>();
            var usedCardSet = new HashSet<string>(usedCards.Select(c => c.suit + "," + c.rank));
            int multiplier = handResult?.Multiplier ?? 1;
            int baseScore = handResult?.Score ?? 0; // 기본 족보 점수
            int totalScore = baseScore;
            if (handPlayScoreText != null && handResult != null)
                handPlayScoreText.text = $"{baseScore} x {multiplier} = {baseScore * multiplier}";
            for (int i = 0; i < cards.Count; i++)
            {
                var card = cards[i];
                if (card != null && card.AddCardScore != null)
                {
                    var holder = card.GetComponent<CardDataHolder>();
                    int value = 0;
                    bool isUsed = holder != null && usedCardSet.Contains(holder.suit + "," + holder.rank);
                    card.AddCardScore.gameObject.SetActive(true); // 무조건 비지블
                    if (isUsed)
                    {
                        if (holder.rank >= 2 && holder.rank <= 10)
                            value = holder.rank;
                        else if (holder.rank == 1) // Ace
                            value = 11;
                        else // J, Q, K
                            value = 10;
                        card.AddCardScore.text = $"+{value}";
                        totalScore += value;
                        if (handPlayScoreText != null && handResult != null)
                            handPlayScoreText.text = $"{totalScore} x {multiplier} = {totalScore * multiplier}";

                        yield return new WaitForSeconds(0.5f); // 사용된 카드만 대기
                    }
                    else
                    {
                        card.AddCardScore.text = "";
                        // 사용되지 않은 카드는 대기 없이 바로 넘어감
                    }
                }
            }
            yield break;
        }

        // 새로운 판이 시작될 때 기존 handCards를 모두 제거
        public void ClearHandCards()
        {
            foreach (var card in handCards)
            {
                if (card != null)
                    Destroy(card.gameObject);
            }
            handCards.Clear();
        }

        // 상대방이 조커 카드를 구매했을 때 카드 뒷면으로만 표시 (기존)
        public void AddOwnedJokerBack()
        {
            ownedJokerData.Add(new ServerCardData
            {
                id = "unknown",
                type = "joker",
                price = 0,
                sprite = -1, // 뒷면 스프라이트를 따로 지정
                name = "",
                description = ""
            });
            ShowOwnedJokers();
        }
        // 카드 정보가 들어온 경우 해당 정보로 표시
        public void AddOwnedJokerBack(ServerCardData cardData)
        {
            ownedJokerData.Add(cardData);
            ShowOwnedJokers();
        }

        public void ShowOwnedJokers()
        {
            // 기존 카드 오브젝트 제거
            foreach (var joker in ownedJokers)
            {
                if (joker != null)
                    Destroy(joker.gameObject);
            }
            ownedJokers.Clear();

            // 구매한 조커 카드 수만큼 뒷면 카드 생성
            for (int i = 0; i < ownedJokerData.Count && i < ownedJokerPositions.Length; i++)
            {
                var position = ownedJokerPositions[i];
                if (position != null)
                {
                    GameObject jokerObj = Instantiate(cardJokerPrefab, position);
                    var cardJoker = jokerObj.GetComponent<CardJoker>();
                    // ownedJokerData[i] 값으로 셋팅
                    var cardData = ownedJokerData[i];
                    Sprite sprite = null;
                    if (cardData.sprite >= 0 && SpriteManager.Instance.jokerSprites != null && cardData.sprite < SpriteManager.Instance.jokerSprites.Count)
                    {
                        sprite = SpriteManager.Instance.jokerSprites[cardData.sprite];
                    }
                    cardJoker.SetData(cardData.id, sprite);
                    if (cardJoker.priceText != null)
                        cardJoker.priceText.gameObject.SetActive(false);
                    // === 크기/스케일 맞추기 ===
                    var cardRect = jokerObj.GetComponent<RectTransform>();
                    var posRect = position.GetComponent<RectTransform>();
                    if (cardRect != null && posRect != null)
                    {
                        cardRect.sizeDelta = posRect.sizeDelta;
                        cardRect.localScale = posRect.localScale;
                    }
                    ownedJokers.Add(cardJoker);
                }
            }
        }

        // 서버에서 받은 조커 카드 정보로 ownedJokers를 오픈
        public void OpenOwnedJokers(List<ServerCardData> jokerList)
        {
            Debug.Log("OpenOwnedJokers 11111111 " + jokerList.Count);

            for (int i = 0; i < ownedJokers.Count && i < jokerList.Count; i++)
            {
                var cardJoker = ownedJokers[i];
                var cardData = jokerList[i];
                Sprite sprite = null;
                if (cardData.sprite >= 0 && SpriteManager.Instance.jokerSprites != null && cardData.sprite < SpriteManager.Instance.jokerSprites.Count)
                {
                    sprite = SpriteManager.Instance.jokerSprites[cardData.sprite];
                }

                Debug.Log("OpenHandPlayCards 2222" + sprite + " " + cardData.sprite);

                cardJoker.SetData(cardData.id, sprite);
                if (cardJoker.priceText != null)
                    cardJoker.priceText.gameObject.SetActive(false);
            }
            // 남은 ownedJokers는 뒷면 등으로 유지
        }

        public void ClearOwnedJokers()
        {
            // 기존 조커 카드들 파괴
            foreach (var joker in ownedJokers)
            {
                if (joker != null)
                {
                    Destroy(joker.gameObject);
                }
            }
            ownedJokers.Clear();
            ownedJokerData.Clear();
        }

        /// <summary>
        /// 특정 조커 카드를 제거합니다.
        /// </summary>
        /// <param name="cardId">제거할 조커 카드의 ID</param>
        public void RemoveOwnedJoker(string cardId)
        {
            Debug.Log($"[OpponentSlot] RemoveOwnedJoker 호출: cardId={cardId}, currentUserId={currentUserId}");

            // ownedJokerData에서 해당 카드 찾기
            int dataIndex = -1;
            for (int i = 0; i < ownedJokerData.Count; i++)
            {
                if (ownedJokerData[i].id == cardId)
                {
                    dataIndex = i;
                    break;
                }
            }

            if (dataIndex != -1)
            {
                // 데이터에서 제거
                ownedJokerData.RemoveAt(dataIndex);

                // 해당 인덱스의 조커 오브젝트 파괴
                if (dataIndex < ownedJokers.Count && ownedJokers[dataIndex] != null)
                {
                    Destroy(ownedJokers[dataIndex].gameObject);
                    ownedJokers.RemoveAt(dataIndex);
                }

                Debug.Log($"[OpponentSlot] 조커 카드 제거 완료: cardId={cardId}");

                // 남은 조커들을 다시 배치
                RearrangeOwnedJokers();
            }
            else
            {
                Debug.LogWarning($"[OpponentSlot] 제거할 조커 카드를 찾을 수 없음: cardId={cardId}");
            }
        }

        /// <summary>
        /// 조커 카드들을 다시 배치합니다.
        /// </summary>
        private void RearrangeOwnedJokers()
        {
            // 기존 조커 오브젝트들 파괴
            foreach (var joker in ownedJokers)
            {
                if (joker != null)
                {
                    Destroy(joker.gameObject);
                }
            }
            ownedJokers.Clear();

            // 데이터를 기반으로 조커 오브젝트들 다시 생성
            for (int i = 0; i < ownedJokerData.Count && i < ownedJokerPositions.Length; i++)
            {
                var cardData = ownedJokerData[i];
                var position = ownedJokerPositions[i];

                if (cardJokerPrefab != null && position != null)
                {
                    var jokerObj = Instantiate(cardJokerPrefab, position);
                    var joker = jokerObj.GetComponent<CardJoker>();
                    if (joker != null)
                    {
                        Sprite sprite = null;
                        if (cardData.sprite >= 0 && SpriteManager.Instance.jokerSprites != null && cardData.sprite < SpriteManager.Instance.jokerSprites.Count)
                        {
                            sprite = SpriteManager.Instance.jokerSprites[cardData.sprite];
                        }
                        joker.SetData(cardData.id, sprite);
                        if (joker.priceText != null)
                        {
                            joker.priceText.gameObject.SetActive(false);
                        }
                        ownedJokers.Add(joker);
                    }
                }
            }
        }

        // 발표 끝나고 원래 상태로 복구하는 함수
        public void RestoreHandPlayState()
        {
            // 발표에 사용된 카드/조커를 원래 상태로 복귀
            foreach (var kv in lastCardOriginalStates)
            {
                var card = kv.Key;
                var state = kv.Value;
                if (card != null)
                {
                    card.transform.SetParent(state.parent, true);
                    card.transform.position = state.position;
                    card.transform.localScale = state.localScale;
                    card.transform.SetSiblingIndex(state.siblingIndex);
                    var rect = card.GetComponent<RectTransform>();
                    if (rect != null && state.sizeDelta.HasValue)
                        rect.sizeDelta = state.sizeDelta.Value;
                    // 카드점수 표시 숨김
                    if (card.AddCardScore != null)
                    {
                        card.AddCardScore.text = "";
                        card.AddCardScore.gameObject.SetActive(false);
                    }
                }
            }
            foreach (var kv in lastJokerOriginalStates)
            {
                var joker = kv.Key;
                var state = kv.Value;
                if (joker != null)
                {
                    joker.transform.SetParent(state.parent, true);
                    joker.transform.position = state.position;
                    joker.transform.localScale = state.localScale;
                    joker.transform.SetSiblingIndex(state.siblingIndex);
                    var rect = joker.GetComponent<RectTransform>();
                    if (rect != null && state.sizeDelta.HasValue)
                        rect.sizeDelta = state.sizeDelta.Value;
                }
            }
            // 비활성화했던 handCards 복구
            foreach (var card in lastInactiveHandCards)
            {
                if (card != null) card.gameObject.SetActive(true);
            }
            // 발표 UI 비활성화
            if (handPlayRoot != null) handPlayRoot.SetActive(false);
            // 복구 정보 초기화
            lastCardOriginalStates.Clear();
            lastJokerOriginalStates.Clear();
            lastInactiveHandCards.Clear();
        }

        // 라운드 승패 결과 업데이트 메서드 (서버로부터 받은 결과값으로 유저 슬롯 업데이트)
        public IEnumerator UpdateRoundResult(int silverChipsGain, int goldChipsGain, int totalSilverChips, int totalGoldChips, int totalMoney)
        {
            // 칩 정보를 UI에 표시하는 로직
            // 예: TextMeshPro 컴포넌트에 표시하거나 다른 UI 요소에 반영
            Debug.Log($"[OpponentSlot] 라운드 결과 업데이트: 실버칩={silverChipsGain}, 골드칩={goldChipsGain}, 총 실버칩={totalSilverChips}, 총 골드칩={totalGoldChips}, 총 자금={totalMoney}");

            chipText.text = ChipDisplayUtil.FormatChipAmount(totalSilverChips);

            // funds 텍스트 업데이트
            if (fundsText != null)
            {
                fundsText.text = $"${totalMoney}";
            }

            // if (silverChipsGain > 0 || goldChipsGain > 0)
            // {
            //     winIndicator.SetActive(true);

            //     if (ChipAnimationManager.Instance != null)
            //     {
            //         ChipAnimationManager.Instance.MoveChipsFromStackToSlot(currentUserId, 0, () =>
            //         {
            //             Debug.Log("모든 칩 이동 완료!");
            //         });
            //     }
            //     else
            //     {
            //         Debug.LogWarning("[MySlot] ChipAnimationManager.Instance가 null입니다.");
            //     }
            // }

            yield break;

        }
    }
}