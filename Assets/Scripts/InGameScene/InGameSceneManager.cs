using UnityEngine;
using TMPro;
using BalatroOnline.Common;
using System.Collections.Generic;
using Best.HTTP.JSON.LitJson;
using System.Linq;
using BalatroOnline.Game;
using System.Collections;

namespace BalatroOnline.InGame
{
    /// <summary>
    /// 인게임 씬의 전체 흐름을 관리하는 매니저
    /// </summary>
    public class InGameSceneManager : MonoBehaviour
    {
        public static InGameSceneManager Instance { get; private set; }

        // 슬롯 참조 (인스펙터에서 할당)
        public MySlot mySlot;
        public OpponentSlot[] opponentSlots;
        public TMP_Text roundText; // 인스펙터에서 연결 필요
        public TextMeshProUGUI discardCountText; // 인스펙터에서 연결
        public string currentPhase = "waiting"; // 서버에서 내려주는 phase를 여기에 저장(이벤트에서 갱신 필요)
        private Coroutine autoReadyCoroutine;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            // 필요시 자동 시작
            StartAutoReady();
        }

        public void StartAutoReady()
        {
            if (autoReadyCoroutine == null)
                autoReadyCoroutine = StartCoroutine(AutoReadyRoutine());
        }

        public void StopAutoReady()
        {
            if (autoReadyCoroutine != null)
            {
                StopCoroutine(autoReadyCoroutine);
                autoReadyCoroutine = null;
            }
        }

        private IEnumerator AutoReadyRoutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(1f);
                int userCount = GetCurrentUserCount();
                if (userCount >= 2 && currentPhase == "waiting")
                {
                    SendReadyToServer();
                }
            }
        }

        private int GetCurrentUserCount()
        {
            int count = 0;
            if (mySlot != null && !mySlot.IsEmpty()) count++;
            if (opponentSlots != null)
            {
                foreach (var slot in opponentSlots)
                    if (slot != null && !slot.IsEmpty()) count++;
            }
            return count;
        }

        private void SendReadyToServer()
        {
            var roomId = SessionManager.Instance.CurrentRoomId;
            if (!string.IsNullOrEmpty(roomId))
            {
                var data = new Dictionary<string, object> { { "roomId", roomId } };
                SocketManager.Instance.EmitToServer(new ReadyRequest(roomId));
                Debug.Log("[InGameSceneManager] 자동 ready 전송");
            }
        }

        public void OnUserLeft(UserLeftResponse response)
        {
            Debug.Log($"[InGameSceneManager] 방 퇴장");
            UnityEngine.SceneManagement.SceneManager.LoadScene("LobbyScene");
        }

        private void OnDestroy()
        {
            // 이벤트 구독 해제
            // SocketManager에서 직접 핸들러를 호출하므로 이벤트 구독 해제 불필요
        }

        public void SendOnHandPlayReady(List<Dictionary<string, object>> selectedCards)
        {
            Debug.Log($"[InGameSceneManager] OnHandPlayReady 호출, 선택된 카드: {selectedCards.Count}장");
            var roomId = SessionManager.Instance.CurrentRoomId;
            if (string.IsNullOrEmpty(roomId))
            {
                Debug.LogWarning("[InGameSceneManager] roomId가 null 또는 빈 문자열");
                return;
            }

            var hand = new List<HandPlayReadyRequest.CardDto>();
            foreach (var card in selectedCards)
            {
                hand.Add(new HandPlayReadyRequest.CardDto(
                    card["suit"].ToString(),
                    card["rank"] is int i ? i : int.Parse(card["rank"].ToString())
                ));
            }
            SocketManager.Instance.EmitToServer(new HandPlayReadyRequest(roomId, hand));
        }

        // HandPlayReady 실제 서버 전송/로직 담당
        public void OnHandPlayReady(HandPlayReadyResponse response)
        {
            string userId = response.userId;
            string myUserId = SessionManager.Instance.UserId;
            if (userId == myUserId)
            {
                if (mySlot != null) mySlot.SetReady(true);
            }
            else
            {
                if (opponentSlots != null)
                {
                    foreach (var slot in opponentSlots)
                    {
                        if (slot != null && slot.GetUserId() == userId)
                        {
                            slot.SetReady(true);
                            break;
                        }
                    }
                }
            }
        }

        // MySlot에서 discard 요청 위임 시 처리
        public void OnDiscard(List<Dictionary<string, object>> cards, string roomId)
        {
            if (cards == null || cards.Count == 0 || string.IsNullOrEmpty(roomId))
            {
                Debug.LogWarning("[InGameSceneManager] OnDiscard: cards 또는 roomId가 유효하지 않음");
                return;
            }
            var cardDtos = new List<DiscardRequest.CardDto>();
            foreach (var card in cards)
            {
                cardDtos.Add(new DiscardRequest.CardDto(
                    card["suit"].ToString(),
                    card["rank"] is int i ? i : int.Parse(card["rank"].ToString())
                ));
            }
            SocketManager.Instance.EmitToServer(new DiscardRequest(roomId, cardDtos));
        }

        // handPlayResult 핸들러 구현
        public void OnHandPlayResult(HandPlayResultResponse response)
        {
            Debug.Log("[InGameSceneManager] handPlayResult 이벤트 수신");

            currentPhase = "shop";

            StartCoroutine("OnHandPlayResultCo", response);
        }
        public IEnumerator OnHandPlayResultCo(HandPlayResultResponse response)
        {
            Debug.Log($"[InGameSceneManager] OnHandPlayResultCo 시작 - round: {response.round}");

            yield return MessageDialogManager.Instance.ShowAndWait("핸드 플레이 결과 발표를 시작 합니다.", null, 2f, 2f);

            // 발표 직전 readyIndicator 모두 숨김
            if (mySlot != null) mySlot.SetReady(false);
            if (opponentSlots != null)
            {
                foreach (var slot in opponentSlots)
                    if (slot != null) slot.SetReady(false);
            }

            if (response.roundResult == null)
            {
                Debug.LogWarning("[InGameSceneManager] handPlayResult roundResult가 null");
                yield break;
            }

            string myUserId = SessionManager.Instance?.UserId;
            Debug.Log($"[InGameSceneManager] myUserId: {myUserId}");

            foreach (var kvp in response.roundResult)
            {
                string userId = kvp.Key;
                var userResult = kvp.Value;

                // 핸드를 CardData 리스트로 변환
                var cardList = new List<CardData>();
                if (userResult.hand != null)
                {
                    foreach (var card in userResult.hand)
                    {
                        cardList.Add(new CardData(card.suit, card.rank));
                    }
                }

                Debug.Log($"[InGameSceneManager] {userId} 결과 - 점수: {userResult.score}, 실버칩: {userResult.silverChipGain}, 골드칩: {userResult.goldChipGain}, 최종실버: {userResult.finalSilverChips}, 최종골드: {userResult.finalGoldChips}, 최종자금: {userResult.finalFunds}");

                // 서버에서 받은 remaining 값들 로그 출력
                Debug.Log($"[InGameSceneManager] {userId} remaining 값들 - 남은버리기: {userResult.remainingDiscards}, 남은덱: {userResult.remainingDeck}, 남은7: {userResult.remainingSevens}");

                if (userId == myUserId)
                {
                    var myPlayer = GameManager.Instance?.myPlayer;
                    if (myPlayer != null)
                    {
                        // 내 플레이어의 remaining 값들을 MySlot에 전달
                        myPlayer.SetRemainingValues(userResult.remainingDiscards, userResult.remainingDeck, userResult.remainingSevens);
                        yield return StartCoroutine(MoveHandPlayAndFix(myPlayer, cardList));
                    }
                }
                else
                {
                    // 상대방 OpponentSlot 찾아서 카드 오픈
                    if (opponentSlots != null)
                    {
                        foreach (var slot in opponentSlots)
                        {
                            if (slot != null && slot.GetUserId() == userId)
                            {
                                // ownedCards에서 해당 userId의 조커 카드 정보 추출
                                List<ServerCardData> ownedJokerList = null;
                                if (response.ownedCards != null && response.ownedCards.TryGetValue(userId, out var cardsObj))
                                {
                                    ownedJokerList = ParseOwnedCardInfoList(cardsObj);
                                }
                                yield return StartCoroutine(slot.OpenHandPlayCardsCoroutine(cardList, ownedJokerList));
                                break;
                            }
                        }
                    }
                }

                yield return new WaitForSeconds(1f);
            }

            yield return new WaitForSeconds(2);

            // 각 유저별로 서버에서 받은 값으로 최종 칩 정보 업데이트
            StartCoroutine(UpdateAllUserChips(response));

            yield return new WaitForSeconds(4);

            // 모든 상대방 handPlayRoot 비활성화 및 발표 전 상태 복구
            if (opponentSlots != null)
            {
                foreach (var slot in opponentSlots)
                {
                    if (slot != null)
                        slot.RestoreHandPlayState();
                }
            }

            // 샵 오픈은 ShopManager에서 관리
            if (response.round >= 5)
            {
                // 5라운드 이상이면 샵 오픈하지 않고 메시지 표시
                yield return MessageDialogManager.Instance.ShowAndWait("모든 라운드가 종료되었습니다.", null, 3f);
                // === 모든 슬롯(내 슬롯, 상대 슬롯) 플레이 정보 초기화 ===
                if (mySlot != null)
                {
                    mySlot.ClearHandCards();
                    mySlot.ClearOwnedJokers();
                    // winIndicator 숨기기
                    if (mySlot.winIndicator != null)
                        mySlot.winIndicator.SetActive(false);
                }
                if (opponentSlots != null)
                {
                    foreach (var slot in opponentSlots)
                        if (slot != null)
                        {
                            slot.ClearHandCards();
                            slot.ClearOwnedJokers();
                            // winIndicator 숨기기
                            if (slot.winIndicator != null)
                                slot.winIndicator.SetActive(false);
                        }
                }
                // 라운드 텍스트 등 UI도 초기화
                if (roundText != null)
                    roundText.text = "시작 대기 중";
                // 샵 UI도 닫기
                if (ShopManager.Instance != null)
                    ShopManager.Instance.CloseShop();

                yield return new WaitForSeconds(1);
                currentPhase = "waiting";
            }
            else if (response.shopCards != null && response.shopCards.Count > 0)
            {
                Debug.Log($"[InGameSceneManager] 이번 라운드 샵 카드: 총 {response.shopCards.Count}장");
                var shopCardDataList = new List<ServerCardData>();

                foreach (var shopCard in response.shopCards)
                {
                    var cardData = new ServerCardData
                    {
                        id = shopCard.id,
                        type = shopCard.type,
                        price = shopCard.price,
                        sprite = shopCard.sprite,
                        name = shopCard.name,
                        description = shopCard.description
                    };
                    shopCardDataList.Add(cardData);
                    Debug.Log($"[InGameSceneManager] 샵 카드: {shopCard.name} (가격: {shopCard.price})");
                }

                // ShopManager에 샵 카드 전달
                if (ShopManager.Instance != null)
                {
                    ShopManager.Instance.ShowShop(shopCardDataList);
                }
            }
        }

        // startGame 핸들러 구현
        public void OnStartGame(StartGameResponse response)
        {
            currentPhase = "playing";
            StartCoroutine("OnStartGameCo", response);
        }

        IEnumerator OnStartGameCo(StartGameResponse response)
        {
            // 받은 데이터 그대로 로그 출력
            Debug.Log($"[InGameSceneManager] startGame 시작 - Round: {response.round}");

            yield return new WaitForSeconds(1);

            // 라운드 시작 시 모든 ready indicator 숨김
            ResetAllReadyIndicators();
            // 라운드 시작 시 모든 opponentSlot의 handCards 제거
            if (opponentSlots != null)
            {
                foreach (var slot in opponentSlots)
                    if (slot != null)
                        slot.ClearHandCards();
            }

            // 라운드 시작 시 UI/카드/버튼 초기화
            var uiMgr = InGameUIManager.Instance;
            if (uiMgr != null) uiMgr.ResetForNewRound();

            // === discardCountText 4로 초기화 ===
            if (discardCountText != null)
                discardCountText.text = "4";

            yield return new WaitForSeconds(1);

            // 시드 칩 정보 사용
            int silverSeedChip = response.silverSeedChip;
            int goldSeedChip = response.goldSeedChip;
            Debug.Log($"[InGameSceneManager] 실버 시드 칩: {silverSeedChip}, 골드 시드 칩: {goldSeedChip}");

            // 구매한 조커 카드들 표시
            if (mySlot != null)
            {
                mySlot.ShowOwnedJokers();
                Debug.Log($"[InGameSceneManager] startGame - 구매한 조커 카드 {mySlot.GetOwnedJokerCount()}개 표시");
            }

            // myCards를 CardData 리스트로 변환
            var myCardsList = new List<CardData>();
            if (response.myCards != null)
            {
                foreach (var card in response.myCards)
                {
                    myCardsList.Add(new CardData(card.suit, card.rank));
                    Debug.Log($"[InGameSceneManager] 카드: {card.suit} {card.rank}");
                }
            }

            // opponents에서 userId 리스트 추출
            var opponentIds = new List<string>();
            if (response.opponents != null)
            {
                foreach (var opponent in response.opponents)
                {
                    opponentIds.Add(opponent.userId);
                    Debug.Log($"[InGameSceneManager] 상대방: {opponent.userId} ({opponent.nickname})");
                }
            }

            // 시드 칩 애니메이션 실행
            ChipAnimationManager.Instance.DeductSeedChip(SessionManager.Instance.UserId, silverSeedChip, goldSeedChip, () =>
            {
                Debug.Log($"[InGameSceneManager] 시드 애니메이션 완료 - 실버: {silverSeedChip}, 골드: {goldSeedChip}");
            });

            // 상대방들도 시드 칩 애니메이션
            foreach (var opponentId in opponentIds)
            {
                ChipAnimationManager.Instance.DeductSeedChip(opponentId, silverSeedChip, goldSeedChip, () =>
                {
                    Debug.Log($"[InGameSceneManager] 상대방 시드 애니메이션 완료: {opponentId}");
                });
            }

            // GameManager에 카드 딜링 정보 전달
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnReceiveCardDeal(myCardsList, opponentIds);
            }

            // round 값 표시
            if (roundText != null)
                roundText.text = $"Round {response.round}/5";

            // userFunds 정보를 각 유저 슬롯에 업데이트
            if (response.userFunds != null)
            {
                Debug.Log($"[InGameSceneManager] userFunds 처리 시작");

                foreach (var kvp in response.userFunds)
                {
                    string userId = kvp.Key;
                    int funds = kvp.Value;

                    Debug.Log($"[InGameSceneManager] 유저 {userId}의 funds: {funds}");

                    // 내 슬롯인지 확인
                    if (userId == SessionManager.Instance.UserId)
                    {
                        if (mySlot != null && mySlot.fundsText != null)
                        {
                            mySlot.fundsText.text = $"${funds}";
                            Debug.Log($"[InGameSceneManager] 내 슬롯 funds 업데이트: ${funds}");
                        }
                    }
                    else
                    {
                        // 상대방 슬롯 찾아서 업데이트
                        if (opponentSlots != null)
                        {
                            foreach (var slot in opponentSlots)
                            {
                                if (slot != null && slot.GetUserId() == userId)
                                {
                                    if (slot.fundsText != null)
                                    {
                                        slot.fundsText.text = $"${funds}";
                                        Debug.Log($"[InGameSceneManager] 상대방 슬롯 funds 업데이트: {userId} -> ${funds}");
                                    }
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                Debug.LogWarning("[InGameSceneManager] userFunds 정보가 없습니다.");
            }
        }

        // DiscardResultResponse 핸들러 구현
        public void OnDiscardResult(DiscardResponse response)
        {
            Debug.Log($"[InGameSceneManager] DiscardResultResponse 이벤트 수신 - success: {response.success}");

            if (response.success)
            {
                // 성공 처리 로직은 기존 코루틴에서 처리
                StartCoroutine("OnDiscardResultCo", response);
            }
            else
            {
                Debug.LogWarning($"[InGameSceneManager] discard 실패: {response.message}");
                MessageDialogManager.Instance.Show($"버리기 실패: {response.message}");
            }
        }

        // DiscardResultResponse 처리 코루틴
        private System.Collections.IEnumerator OnDiscardResultCo(DiscardResponse response)
        {
            Debug.Log($"[InGameSceneManager] OnDiscardResultCo 시작 - newHand: {response.newHand?.Count ?? 0}, discarded: {response.discarded?.Count ?? 0}, remainingDiscards: {response.remainingDiscards}");

            // 1. newHand를 CardData 리스트로 변환
            var newHandCards = new List<CardData>();
            if (response.newHand != null)
            {
                foreach (var card in response.newHand)
                {
                    newHandCards.Add(new CardData(card.suit, card.rank));
                    Debug.Log($"[InGameSceneManager] NewHand Card: {card.suit} {card.rank}");
                }
            }

            // 2. MySlot에 새로운 핸드 전달
            if (mySlot != null)
            {
                mySlot.OnDiscardResult(newHandCards);
            }

            // 3. 남은 discard 횟수 UI 업데이트
            if (discardCountText != null)
            {
                discardCountText.text = $"{response.remainingDiscards}";
            }

            // 4. 버린 카드 정보 로그
            if (response.discarded != null && response.discarded.Count > 0)
            {
                Debug.Log($"[InGameSceneManager] 버린 카드 {response.discarded.Count}장:");
                foreach (var card in response.discarded)
                {
                    Debug.Log($"  - {card.suit} {card.rank}");
                }
            }

            Debug.Log($"[InGameSceneManager] OnDiscardResultCo 완료");
            yield break;
        }

        // BuyCardResponse 핸들러 구현
        public void OnBuyCardResult(BuyCardResponse response)
        {
            Debug.Log($"[InGameSceneManager] BuyCardResponse 이벤트 수신 - success: {response.success}");

            if (response.success)
            {
                Debug.Log($"[InGameSceneManager] 구매 성공: {response.cardId} ({response.cardType}), 가격: {response.price}");

                // 구매한 조커 카드 데이터 생성
                var purchasedCardData = new ServerCardData
                {
                    id = response.cardId,
                    type = response.cardType,
                    price = response.price,
                    sprite = response.cardSprite,
                    name = response.cardName,
                    description = response.cardDescription
                };

                // 구매 성공 메시지 표시
                MessageDialogManager.Instance.Show($"구매 성공!\n{response.message}\n카드: {response.cardName}", () =>
                {
                    // 구매한 조커 카드를 MySlot에 추가
                    if (InGameSceneManager.Instance != null && mySlot != null)
                    {
                        mySlot.AddOwnedJoker(purchasedCardData);
                    }

                    // 샵에서 구매된 카드 제거 (UI 업데이트)
                    if (ShopManager.Instance != null)
                    {
                        ShopManager.Instance.RemovePurchasedCard(response.cardId);
                    }
                });
            }
            else
            {
                Debug.LogWarning($"[InGameSceneManager] 구매 실패: {response.message}");

                // 구매 실패 메시지 표시
                MessageDialogManager.Instance.Show($"구매 실패!\n{response.message}", () =>
                {
                    Debug.Log("[InGameSceneManager] 구매 실패 처리 완료");
                });
            }
        }

        public void OnRoomUsers(RoomUsersResponse response)
        {
            Debug.Log($"[InGameSceneManager] roomUsers 이벤트 수신 - 사용자 수: {response.users?.Count ?? 0}");

            string myUserId = SessionManager.Instance.UserId;

            // 1. 현재 슬롯에 있는 유저 목록 파싱
            HashSet<string> currentUserIds = new HashSet<string>();
            if (mySlot != null && !string.IsNullOrEmpty(mySlot.GetUserId()))
                currentUserIds.Add(mySlot.GetUserId());
            if (opponentSlots != null)
            {
                foreach (var slot in opponentSlots)
                {
                    if (slot != null && !slot.IsEmpty())
                        currentUserIds.Add(slot.GetUserId());
                }
            }

            // 2. roomUsers에 없는 유저만 Clear
            if (mySlot != null && !response.users.Exists(u => u.userId == myUserId))
                mySlot.SetUser(null);
            if (opponentSlots != null)
            {
                foreach (var slot in opponentSlots)
                {
                    if (slot != null && !slot.IsEmpty() && !response.users.Exists(u => u.userId == slot.GetUserId()))
                        slot.ClearSlot();
                }
            }

            // 3. roomUsers 유저를 슬롯에 배치/업데이트 (서버에서 받은 모든 정보 포함)
            foreach (var user in response.users)
            {
                // User 구조체를 Dictionary로 변환 (MySlot.SetUser가 Dictionary를 받기 때문)
                var userData = new Dictionary<string, object>
                {
                    ["userId"] = user.userId,
                    ["nickname"] = user.nickname,
                    ["silverChip"] = user.silverChip,
                    ["goldChip"] = user.goldChip
                };

                if (user.userId == myUserId)
                {
                    if (mySlot != null)
                    {
                        mySlot.SetUser(userData);
                        Debug.Log($"[InGameSceneManager] MySlot에 userId 설정: {user.userId} ({user.nickname})");
                    }
                }
                else
                {
                    // 이미 슬롯에 있으면 업데이트, 없으면 빈 슬롯에 배치
                    OpponentSlot found = null;
                    if (opponentSlots != null)
                    {
                        foreach (var slot in opponentSlots)
                        {
                            if (slot != null && !slot.IsEmpty() && slot.GetUserId() == user.userId)
                            {
                                found = slot;
                                break;
                            }
                        }
                        if (found != null)
                        {
                            found.SetUser(userData); // 모든 정보 갱신
                            Debug.Log($"[InGameSceneManager] 기존 OpponentSlot에 userId 업데이트: {user.userId} ({user.nickname})");
                        }
                        else
                        {
                            foreach (var slot in opponentSlots)
                            {
                                if (slot != null && slot.IsEmpty())
                                {
                                    slot.SetUser(userData);
                                    Debug.Log($"[InGameSceneManager] 새로운 OpponentSlot에 userId 설정: {user.userId} ({user.nickname})");
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }

        public void ResetAllReadyIndicators()
        {
            if (mySlot != null)
            {
                if (mySlot.handPlayReadyIndicator != null)
                    mySlot.handPlayReadyIndicator.SetActive(false);
                if (mySlot.nextRoundReadyIndicator != null)
                    mySlot.nextRoundReadyIndicator.SetActive(false);
                if (mySlot.winIndicator != null)
                    mySlot.winIndicator.SetActive(false);

            }
            if (opponentSlots != null)
            {
                foreach (var slot in opponentSlots)
                {
                    if (slot != null)
                    {
                        if (slot.handPlayReadyIndicator != null)
                            slot.handPlayReadyIndicator.SetActive(false);
                        if (slot.nextRoundReadyIndicator != null)
                            slot.nextRoundReadyIndicator.SetActive(false);
                        if (slot.winIndicator != null)
                            slot.winIndicator.SetActive(false);
                    }
                }
            }
        }

        public void OnNextRoundReady(NextRoundReadyResponse response)
        {
            string userId = response.userId;
            string myUserId = SessionManager.Instance.UserId;
            if (userId == myUserId)
            {
                if (mySlot != null && mySlot.nextRoundReadyIndicator != null)
                    mySlot.nextRoundReadyIndicator.SetActive(true);
            }
            else
            {
                if (opponentSlots != null)
                {
                    foreach (var slot in opponentSlots)
                    {
                        if (slot != null && slot.GetUserId() == userId && slot.nextRoundReadyIndicator != null)
                        {
                            slot.nextRoundReadyIndicator.SetActive(true);
                            break;
                        }
                    }
                }
            }
        }

        // 상대방이 카드 구매 시 호출되는 핸들러
        public void OnCardPurchased(CardPurchasedResponse response)
        {
            Debug.Log($"[InGameSceneManager] CardPurchasedResponse 이벤트 수신 - userId: {response.userId}");

            string userId = response.userId;
            string myUserId = SessionManager.Instance.UserId;
            if (userId == myUserId)
            {
                // 내 구매는 이미 별도 BuyCardResponse로 처리됨
                Debug.Log("[InGameSceneManager] 내 카드 구매 이벤트는 무시");
                return;
            }

            // 카드 정보 파싱
            var cardData = new ServerCardData
            {
                id = response.cardId,
                type = response.cardType,
                price = response.price,
                sprite = response.cardSprite,
                name = response.cardName,
                description = response.cardDescription
            };

            // 상대방 슬롯 찾아서 연출
            if (opponentSlots != null)
            {
                foreach (var slot in opponentSlots)
                {
                    if (slot != null && slot.GetUserId() == userId)
                    {
                        Debug.Log($"[InGameSceneManager] 상대방({userId})가 조커 카드를 구매함");
                        slot.AddOwnedJokerBack(cardData);
                        break;
                    }
                }
            }
        }

        // 조커 판매 결과 핸들러
        public void OnSellCardResult(SellCardResponse response)
        {
            Debug.Log($"[InGameSceneManager] OnSellCardResult 호출 - success: {response.success}");

            if (response.success)
            {
                Debug.Log($"[InGameSceneManager] 조커 판매 성공: {response.soldCardName}");
                MessageDialogManager.Instance.Show($"조커 '{response.soldCardName}' 판매가 완료되었습니다.", null, 2f);

                // 판매된 조커를 MySlot에서 제거
                if (mySlot != null && !string.IsNullOrEmpty(response.soldCardName))
                {
                    // soldCardName으로 조커 ID를 찾아서 제거
                    var ownedJokers = mySlot.GetOwnedJokers();
                    var soldJoker = ownedJokers.FirstOrDefault(j => j.name == response.soldCardName);
                    if (soldJoker != null)
                    {
                        mySlot.RemoveOwnedJoker(soldJoker.id);
                        Debug.Log($"[InGameSceneManager] MySlot에서 조커 제거 완료: {soldJoker.id}");
                    }
                }
            }
            else
            {
                Debug.LogWarning($"[InGameSceneManager] 조커 판매 실패: {response.message}");
                MessageDialogManager.Instance.Show($"판매 실패: {response.message}", null, 3f);
            }
        }

        public void OnReRollShopResult(ReRollShopResponse response)
        {
            Debug.Log($"[InGameSceneManager] OnReRollShopResult 호출 - success: {response.success}");

            if (response.success)
            {
                // 다시뽑기 카드 데이터를 ServerCardData로 변환
                var reRollCards = new List<ServerCardData>();
                if (response.cards != null)
                {
                    foreach (var card in response.cards)
                    {
                        var cardData = new ServerCardData
                        {
                            id = card.id,
                            type = card.type,
                            price = card.price,
                            sprite = card.sprite,
                            name = card.name,
                            description = card.description
                        };
                        reRollCards.Add(cardData);
                    }
                }

                Debug.Log($"[InGameSceneManager] 다시뽑기 카드 {reRollCards.Count}장 수신");

                // ShopManager에 다시뽑기 카드 전달
                if (ShopManager.Instance != null)
                {
                    ShopManager.Instance.ShowReRollCards(reRollCards);
                }
                else
                {
                    Debug.LogWarning("[InGameSceneManager] ShopManager.Instance가 null입니다.");
                }
            }
            else
            {
                Debug.LogWarning($"[InGameSceneManager] 다시뽑기 실패: {response.message}");
                MessageDialogManager.Instance.Show($"다시뽑기 실패: {response.message}", null, 3f);
            }
        }

        // 서버에서 받은 cardsObj(List<object>)를 List<ServerCardData>로 변환
        private List<ServerCardData> ParseOwnedCardInfoList(object cardsObj)
        {
            var result = new List<ServerCardData>();
            List<object> list = null;
            if (cardsObj is List<object> l)
                list = l;
            else if (cardsObj is object[] arr)
                list = new List<object>(arr);

            if (list != null)
            {
                foreach (var item in list)
                {
                    if (item is Dictionary<string, object> dict)
                    {
                        var cardData = new ServerCardData
                        {
                            id = dict.TryGetValue("id", out var idObj) ? idObj.ToString() : "",
                            type = dict.TryGetValue("type", out var typeObj) ? typeObj.ToString() : "",
                            price = dict.TryGetValue("price", out var priceObj) ? int.Parse(priceObj.ToString()) : 0,
                            sprite = dict.TryGetValue("sprite", out var spriteObj) ? int.Parse(spriteObj.ToString()) : -1,
                            name = dict.TryGetValue("name", out var nameObj) ? nameObj.ToString() : "",
                            description = dict.TryGetValue("description", out var descObj) ? descObj.ToString() : ""
                        };
                        result.Add(cardData);
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// 모든 유저의 칩 정보를 서버 데이터로 업데이트합니다.
        /// </summary>
        private System.Collections.IEnumerator UpdateAllUserChips(HandPlayResultResponse response)
        {
            if (response.roundResult != null)
            {
                string myUserId = SessionManager.Instance?.UserId;

                foreach (var kvp in response.roundResult)
                {
                    string userId = kvp.Key;
                    var userResult = kvp.Value;

                    Debug.Log($"[InGameSceneManager] {userId} 칩 정보 업데이트 - 실버획득: {userResult.silverChipGain}, 골드획득: {userResult.goldChipGain}, 최종실버: {userResult.finalSilverChips}, 최종골드: {userResult.finalGoldChips}, 최종자금: {userResult.finalFunds}");

                    // 유저 ID로 슬롯 찾아서 업데이트
                    if (userId == myUserId)
                    {
                        // 내 슬롯 업데이트
                        if (mySlot != null)
                        {
                            yield return StartCoroutine(mySlot.UpdateRoundResult(userResult.silverChipGain, userResult.goldChipGain, userResult.finalSilverChips, userResult.finalGoldChips, userResult.finalFunds));
                        }
                    }
                    else
                    {
                        // 상대방 슬롯 업데이트
                        if (opponentSlots != null)
                        {
                            foreach (var slot in opponentSlots)
                            {
                                if (slot != null && slot.GetUserId() == userId)
                                {
                                    yield return StartCoroutine(slot.UpdateRoundResult(userResult.silverChipGain, userResult.goldChipGain, userResult.finalSilverChips, userResult.finalGoldChips, userResult.finalFunds));
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }

        private System.Collections.IEnumerator MoveHandPlayAndFix(MySlot myPlayer, System.Collections.Generic.List<CardData> cardList)
        {
            yield return StartCoroutine(myPlayer.MoveHandPlayCardsToCenter(cardList));
            myPlayer.FixHandCards();
            if (InGameUIManager.Instance != null)
                InGameUIManager.Instance.DisablePlayButtons();
        }
    }
}