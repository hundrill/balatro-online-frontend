using UnityEngine;
using UnityEngine.UI;
using TMPro;
using BalatroOnline.Common;
using BalatroOnline.Network;
using BalatroOnline.Network.Protocol;
using System.Collections.Generic;
using Best.HTTP.JSON.LitJson;
using System.Linq;
using BalatroOnline.Game;

namespace BalatroOnline.InGame
{
    /// <summary>
    /// 인게임 씬의 전체 흐름을 관리하는 매니저
    /// </summary>
    public class InGameSceneManager : MonoBehaviour
    {
        public static InGameSceneManager Instance { get; private set; }

        // 슬롯 참조 (인스펙터에서 할당)
        public BalatroOnline.Game.MySlot mySlot;
        public BalatroOnline.Game.OpponentSlot[] opponentSlots;

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
            // InitializeSocket();
            // SocketManager에서 직접 핸들러를 호출하므로 이벤트 구독 불필요
        }

        // private void InitializeSocket()
        // {
        //     // 이미 Socket.IO가 연결되어 있으면 아무것도 하지 않음
        //     if (SocketManager.Instance.IsConnected())
        //     {
        //         Debug.Log("Socket.IO가 이미 연결되어 있습니다.");
        //         return;
        //     }
        //     // Socket.IO 연결
        //     SocketManager.Instance.Connect();
        // }

        // public void OnSocketConnected()
        // {
        //     if (!string.IsNullOrEmpty(SessionManager.Instance.CurrentRoomId))
        //     {
        //         SocketManager.Instance.JoinRoom(SessionManager.Instance.CurrentRoomId);
        //     }
        // }

        /*
                public void OnUserJoined(string userId)
                {
                    string myUserId = BalatroOnline.Common.SessionManager.Instance.UserId;

                    Debug.Log(myUserId + " " + userId);

                    if (userId == myUserId)
                    {
                        if (mySlot != null)
                            mySlot.SetUser(userId);
                    }
                    else
                    {
                        if (opponentSlots != null)
                        {
                            foreach (var slot in opponentSlots)
                            {
                                if (slot != null && slot.IsEmpty())
                                {
                                    slot.SetUser(userId);
                                    break;
                                }
                            }
                        }
                    }
                }
        */
        public void OnUserLeft(string userId)
        {
        }

        public void OnMessageReceived(SocketManager.MessageData messageData)
        {
        }

        // private System.Collections.IEnumerator LeaveRoomCoroutine()
        // {
        //     ApiManager.Instance.LeaveRoom(SessionManager.Instance.CurrentRoomId, (response) =>
        //     {
        //         if (response.success)
        //         {
        //             UnityEngine.SceneManagement.SceneManager.LoadScene("LobbyScene");
        //         }
        //         else
        //         {
        //             UnityEngine.SceneManagement.SceneManager.LoadScene("LobbyScene");
        //         }
        //     });
        //     yield break;
        // }

        private void OnDestroy()
        {
            // 이벤트 구독 해제
            // SocketManager에서 직접 핸들러를 호출하므로 이벤트 구독 해제 불필요
        }

        // HandPlayReady 실제 서버 전송/로직 담당
        public void OnHandPlayReady(List<Dictionary<string, object>> selectedCards)
        {
            Debug.Log($"[InGameSceneManager] OnHandPlayReady 호출, 선택된 카드: {selectedCards.Count}장");
            var roomId = BalatroOnline.Common.SessionManager.Instance.CurrentRoomId;
            if (string.IsNullOrEmpty(roomId))
            {
                Debug.LogWarning("[InGameSceneManager] roomId가 null 또는 빈 문자열");
                return;
            }
            var hand = new List<Dictionary<string, object>>();
            foreach (var card in selectedCards)
            {
                hand.Add(new Dictionary<string, object> { { "suit", card["suit"] }, { "rank", card["rank"] } });
            }
            var data = new Dictionary<string, object> { { "roomId", roomId }, { "hand", hand } };
            Debug.Log($"[InGameSceneManager] 서버로 handPlayReady 전송: {JsonMapper.ToJson(data)}");
            SocketManager.Instance.EmitToServer("handPlayReady", data);
        }

        // MySlot에서 discard 요청 위임 시 처리
        public void OnDiscard(List<Dictionary<string, object>> cards, string roomId)
        {
            if (cards == null || cards.Count == 0 || string.IsNullOrEmpty(roomId))
            {
                Debug.LogWarning("[InGameSceneManager] OnDiscard: cards 또는 roomId가 유효하지 않음");
                return;
            }
            var data = new Dictionary<string, object>
            {
                { "roomId", roomId },
                { "cards", cards }
            };
            Debug.Log($"[InGameSceneManager] discard 서버 전송: {JsonMapper.ToJson(data)}");
            SocketManager.Instance.EmitToServer("discard", data);
        }

        // handPlayResult 핸들러 구현
        public void OnHandPlayResult(object data)
        {
            Debug.Log("[InGameSceneManager] handPlayResult 이벤트 수신");
            if (data == null)
            {
                Debug.LogWarning("[InGameSceneManager] handPlayResult 데이터가 null");
                return;
            }
            string jsonStr = JsonMapper.ToJson(data);
            Debug.Log($"[InGameSceneManager] handPlayResult 원본 데이터: {jsonStr}");


            // === 기존 handPlayResult 처리 ===
            // handPlayResult 수신 메시지창 + OK 시 카드 이동
            BalatroOnline.Common.MessageDialogManager.Instance.Show($"handPlayResult 수신!\n{jsonStr}", () =>
            {
                if (data is Dictionary<string, object> dict && dict.TryGetValue("hands", out var handsObj))
                {
                    List<object> handsList = null;
                    if (handsObj is object[] arr) handsList = arr.ToList();
                    else if (handsObj is List<object> list) handsList = list;
                    if (handsList != null)
                    {
                        string myUserId = SessionManager.Instance?.UserId;
                        Debug.Log($"[InGameSceneManager] myUserId: {myUserId}");
                        if (!string.IsNullOrEmpty(myUserId))
                        {
                            foreach (var handObj in handsList)
                            {
                                if (handObj is Dictionary<string, object> handDict && handDict.TryGetValue("userId", out var uidObj) && uidObj.ToString() == myUserId)
                                {
                                    if (handDict.TryGetValue("hand", out var myHandObj))
                                    {
                                        List<object> myHandList = null;
                                        if (myHandObj is List<object> list2)
                                            myHandList = list2;
                                        else if (myHandObj is object[] arr2)
                                            myHandList = arr2.ToList();
                                        Debug.Log($"[InGameSceneManager] myHandObj type: {myHandObj?.GetType()} value: {JsonMapper.ToJson(myHandObj)}");
                                        Debug.Log($"[InGameSceneManager] myHandList: {myHandList}");
                                        if (myHandList != null)
                                        {
                                            var myHand = new List<BalatroOnline.Game.CardData>();
                                            foreach (var c in myHandList)
                                            {
                                                if (c is Dictionary<string, object> cdict)
                                                {
                                                    string suit = cdict["suit"].ToString();
                                                    int rank = int.Parse(cdict["rank"].ToString());
                                                    Debug.Log($"[InGameSceneManager] 카드 변환: suit={suit}, rank={rank}");
                                                    myHand.Add(new BalatroOnline.Game.CardData(suit, rank));
                                                }
                                                else
                                                {
                                                    Debug.LogWarning($"[InGameSceneManager] myHandList 원소가 Dictionary가 아님: {c?.GetType()} {c}");
                                                }
                                            }
                                            var myPlayer = GameManager.Instance?.myPlayer;
                                            if (myPlayer != null)
                                            {
                                                myPlayer.MoveHandPlayCardsToCenter(myHand);
                                                myPlayer.FixHandCards();
                                            }
                                            // 버튼 비활성화
                                            var uiMgr = InGameUIManager.Instance;
                                            if (uiMgr != null)
                                                uiMgr.DisablePlayButtons();

                                            // 5초 후에 모든 핸드 플레이 발표 완료 메시지 박스 띄우기
                                            BalatroOnline.Common.MessageDialogManager.Instance.Show("모든 핸드 플레이 발표가 끝났습니다.", () =>
                                            {
                                                // 샵 오픈은 ShopManager에서 관리
                                                // === shopCards 5장 로그 추가 및 ShopManager 연동 ===
                                                if (data is Dictionary<string, object> dict && dict.TryGetValue("shopCards", out var shopCardsObj) && shopCardsObj is IList<object> shopCardsList)
                                                {
                                                    Debug.Log($"[InGameSceneManager] 이번 라운드 샵 카드 5장: 총 {shopCardsList.Count}장");
                                                    var shopCardDataList = new List<ServerCardData>();
                                                    for (int i = 0; i < shopCardsList.Count; i++)
                                                    {
                                                        if (shopCardsList[i] is Dictionary<string, object> cardDict)
                                                        {
                                                            string id = cardDict.TryGetValue("id", out var idObj) ? idObj.ToString() : "";
                                                            string type = cardDict.TryGetValue("type", out var typeObj) ? typeObj.ToString() : "joker";
                                                            int price = cardDict.TryGetValue("price", out var priceObj) ? int.Parse(priceObj.ToString()) : 0;
                                                            int sprite = cardDict.TryGetValue("sprite", out var spriteObj) ? int.Parse(spriteObj.ToString()) : 0;
                                                            string name = cardDict.TryGetValue("name", out var nameObj) ? nameObj.ToString() : "";
                                                            string description = cardDict.TryGetValue("description", out var descObj) ? descObj.ToString() : "";
                                                            shopCardDataList.Add(new ServerCardData { id = id, type = type, price = price, sprite = sprite, name = name, description = description });
                                                            Debug.Log($"[InGameSceneManager] [샵카드{i + 1}] id={id}, type={type}, price={price}, sprite={sprite}, name={name}, desc={description}");
                                                        }
                                                        else
                                                        {
                                                            Debug.LogWarning($"[InGameSceneManager] shopCards[{i}]가 Dictionary가 아님: {shopCardsList[i]?.GetType()} {shopCardsList[i]}");
                                                        }
                                                    }
                                                    // ShopManager에 전달하여 샵 UI 오픈
                                                    if (ShopManager.Instance != null)
                                                    {
                                                        ShopManager.Instance.ShowShop(shopCardDataList);
                                                    }
                                                }
                                                else
                                                {
                                                    Debug.Log("[InGameSceneManager] shopCards 정보 없음");
                                                }
                                            });
                                        }
                                        else
                                        {
                                            Debug.LogWarning("[InGameSceneManager] myHandList 파싱 실패");
                                        }
                                    }
                                    break;
                                }
                            }
                        }
                    }
                }
            });
        }

        // startGame 핸들러 구현
        public void OnStartGame(object data)
        {
            // 라운드 시작 시 UI/카드/버튼 초기화
            var uiMgr = InGameUIManager.Instance;
            if (uiMgr != null) uiMgr.ResetForNewRound();

            // 구매한 조커 카드들 표시
            if (ShopManager.Instance != null)
            {
                ShopManager.Instance.ShowOwnedJokers();
                Debug.Log($"[InGameSceneManager] startGame - 구매한 조커 카드 {ShopManager.Instance.GetOwnedJokerCount()}개 표시");
            }

            if (data is Dictionary<string, object> dict)
            {
                if (dict.TryGetValue("myCards", out var myCardsObj) && dict.TryGetValue("opponents", out var opponentsObj))
                {
                    var myCardsList = new List<BalatroOnline.Game.CardData>();
                    if (myCardsObj is object[] myCardsArr)
                    {
                        foreach (var card in myCardsArr)
                        {
                            if (card is Dictionary<string, object> cardDict)
                            {
                                string suit = cardDict["suit"].ToString();
                                int rank = int.Parse(cardDict["rank"].ToString());
                                myCardsList.Add(new BalatroOnline.Game.CardData(suit, rank));
                            }
                        }
                    }
                    else if (myCardsObj is List<object> myCardsRaw)
                    {
                        foreach (var card in myCardsRaw)
                        {
                            if (card is Dictionary<string, object> cardDict)
                            {
                                string suit = cardDict["suit"].ToString();
                                int rank = int.Parse(cardDict["rank"].ToString());
                                myCardsList.Add(new BalatroOnline.Game.CardData(suit, rank));
                            }
                        }
                    }
                    var opponentCounts = new List<int>();
                    if (opponentsObj is object[] oppArr)
                    {
                        foreach (var cnt in oppArr)
                            opponentCounts.Add(int.Parse(cnt.ToString()));
                    }
                    else if (opponentsObj is List<object> oppList)
                    {
                        foreach (var cnt in oppList)
                            opponentCounts.Add(int.Parse(cnt.ToString()));
                    }
                    if (BalatroOnline.Common.GameManager.Instance != null)
                    {
                        BalatroOnline.Common.GameManager.Instance.OnReceiveCardDeal(myCardsList, opponentCounts);
                    }
                }
            }
        }

        // discardResult 핸들러 구현
        public void OnDiscardResult(object data)
        {
            if (data is Dictionary<string, object> dict && dict.TryGetValue("newHand", out var newHandObj))
            {
                var newHandList = new List<BalatroOnline.Game.CardData>();
                if (newHandObj is object[] arr)
                {
                    foreach (var card in arr)
                    {
                        if (card is Dictionary<string, object> cardDict)
                        {
                            string suit = cardDict["suit"].ToString();
                            int rank = int.Parse(cardDict["rank"].ToString());
                            newHandList.Add(new BalatroOnline.Game.CardData(suit, rank));
                        }
                    }
                }
                else if (newHandObj is List<object> rawList)
                {
                    foreach (var card in rawList)
                    {
                        if (card is Dictionary<string, object> cardDict)
                        {
                            string suit = cardDict["suit"].ToString();
                            int rank = int.Parse(cardDict["rank"].ToString());
                            newHandList.Add(new BalatroOnline.Game.CardData(suit, rank));
                        }
                    }
                }
                var myPlayer = BalatroOnline.Common.GameManager.Instance?.myPlayer;
                if (myPlayer != null)
                {
                    myPlayer.OnDiscardResult(newHandList);
                }
            }
        }

        // buyCardResult 핸들러 구현
        public void OnBuyCardResult(object data)
        {
            Debug.Log("[InGameSceneManager] buyCardResult 이벤트 수신");
            if (data == null)
            {
                Debug.LogWarning("[InGameSceneManager] buyCardResult 데이터가 null");
                return;
            }
            string jsonStr = JsonMapper.ToJson(data);
            Debug.Log($"[InGameSceneManager] buyCardResult 원본 데이터: {jsonStr}");

            if (data is Dictionary<string, object> dict)
            {
                bool success = dict.TryGetValue("success", out var successObj) && (bool)successObj;
                string message = dict.TryGetValue("message", out var messageObj) ? messageObj.ToString() : "";
                string cardId = dict.TryGetValue("cardId", out var cardIdObj) ? cardIdObj.ToString() : "";
                string cardType = dict.TryGetValue("cardType", out var cardTypeObj) ? cardTypeObj.ToString() : "";
                int price = dict.TryGetValue("price", out var priceObj) ? int.Parse(priceObj.ToString()) : 0;

                if (success)
                {
                    Debug.Log($"[InGameSceneManager] 구매 성공: {cardId} ({cardType}), 가격: {price}");

                    // 서버에서 받은 카드 상세 정보 추출
                    string cardName = dict.TryGetValue("cardName", out var cardNameObj) ? cardNameObj.ToString() : cardId;
                    string cardDescription = dict.TryGetValue("cardDescription", out var cardDescObj) ? cardDescObj.ToString() : "구매한 조커 카드";
                    int cardSprite = dict.TryGetValue("cardSprite", out var cardSpriteObj) ? int.Parse(cardSpriteObj.ToString()) : 0;

                    // 구매한 조커 카드 데이터 생성
                    var purchasedCardData = new ServerCardData
                    {
                        id = cardId,
                        type = cardType,
                        price = price,
                        sprite = cardSprite,
                        name = cardName,
                        description = cardDescription
                    };

                    // 구매 성공 메시지 표시
                    BalatroOnline.Common.MessageDialogManager.Instance.Show($"구매 성공!\n{message}\n카드: {cardName}", () =>
                    {
                        // 구매한 조커 카드를 ShopManager에 추가
                        if (ShopManager.Instance != null)
                        {
                            ShopManager.Instance.AddOwnedJoker(purchasedCardData);
                        }

                        // 샵에서 구매된 카드 제거 (UI 업데이트)
                        if (ShopManager.Instance != null)
                        {
                            ShopManager.Instance.RemovePurchasedCard(cardId);
                        }
                    });
                }
                else
                {
                    Debug.LogWarning($"[InGameSceneManager] 구매 실패: {message}");

                    // 구매 실패 메시지 표시
                    BalatroOnline.Common.MessageDialogManager.Instance.Show($"구매 실패!\n{message}", () =>
                    {
                        Debug.Log("[InGameSceneManager] 구매 실패 처리 완료");
                    });
                }
            }
            else
            {
                Debug.LogWarning("[InGameSceneManager] buyCardResult 데이터가 Dictionary가 아님");
                BalatroOnline.Common.MessageDialogManager.Instance.Show("구매 결과 처리 중 오류가 발생했습니다.", () =>
                {
                    Debug.Log("[InGameSceneManager] 구매 결과 처리 오류");
                });
            }
        }

        public void OnRoomUsers(object payload)
        {
            // 1. roomUsers 유저 목록 파싱
            List<(string userId, string nickname)> userList = new List<(string, string)>();
            if (payload is Dictionary<string, object> dict && dict.TryGetValue("users", out var usersObj))
            {
                List<object> usersList = null;
                if (usersObj is List<object> list)
                    usersList = list;
                else if (usersObj is object[] arr)
                    usersList = arr.ToList();
                else if (usersObj is IList<object> ilist)
                    usersList = ilist.ToList();

                if (usersList != null)
                {
                    foreach (var userObj in usersList)
                    {
                        if (userObj is Dictionary<string, object> userDict)
                        {
                            string userId = userDict.TryGetValue("userId", out var uidObj) ? uidObj.ToString() : "";
                            string nickname = userDict.TryGetValue("nickname", out var nickObj) ? nickObj.ToString() : "";
                            userList.Add((userId, nickname));
                        }
                    }
                }
                else
                {
                    Debug.LogWarning($"[InGameSceneManager] usersObj 타입: {usersObj?.GetType()}");
                    Debug.LogWarning("[InGameSceneManager] roomUsers usersObj 파싱 실패: " + usersObj);
                    return;
                }
            }
            else
            {
                Debug.LogWarning("[InGameSceneManager] roomUsers payload 파싱 실패: " + payload);
                return;
            }

            string myUserId = BalatroOnline.Common.SessionManager.Instance.UserId;

            // 2. 현재 슬롯에 있는 유저 목록 파싱
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

            // 3. roomUsers에 없는 유저만 Clear
            if (mySlot != null && !userList.Exists(u => u.userId == myUserId))
                mySlot.SetUser("", "");
            if (opponentSlots != null)
            {
                foreach (var slot in opponentSlots)
                {
                    if (slot != null && !slot.IsEmpty() && !userList.Exists(u => u.userId == slot.GetUserId()))
                        slot.ClearSlot();
                }
            }

            // 4. roomUsers 유저를 슬롯에 배치/업데이트
            foreach (var (userId, nickname) in userList)
            {
                if (userId == myUserId)
                {
                    if (mySlot != null) mySlot.SetUser(userId, nickname);
                }
                else
                {
                    // 이미 슬롯에 있으면 업데이트, 없으면 빈 슬롯에 배치
                    OpponentSlot found = null;
                    if (opponentSlots != null)
                    {
                        foreach (var slot in opponentSlots)
                        {
                            if (slot != null && !slot.IsEmpty() && slot.GetUserId() == userId)
                            {
                                found = slot;
                                break;
                            }
                        }
                        if (found != null)
                        {
                            found.SetUser(userId, nickname); // 닉네임만 갱신
                        }
                        else
                        {
                            foreach (var slot in opponentSlots)
                            {
                                if (slot != null && slot.IsEmpty())
                                {
                                    slot.SetUser(userId, nickname);
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}