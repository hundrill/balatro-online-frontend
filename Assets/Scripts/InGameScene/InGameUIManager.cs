using UnityEngine;
using UnityEngine.UI;
using BalatroOnline.Common;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using BalatroOnline.Game;

namespace BalatroOnline.InGame
{
    /// <summary>
    /// 인게임 UI 요소들을 관리하는 매니저
    /// </summary>


    public class InGameUIManager : MonoBehaviour
    {
        public Button handPlayReadyButton;
        public Button discardButton;
        public GameObject jokerInfoPanel;
        public GameObject shopPanel;
        public GameObject ownedJokerInfoPanel;

        public static InGameUIManager Instance { get; private set; }

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
            if (handPlayReadyButton == null) throw new System.Exception("handPlayReadyButton이 인스펙터에 연결되지 않았습니다.");
            if (discardButton == null) throw new System.Exception("discardButton이 인스펙터에 연결되지 않았습니다.");
            if (jokerInfoPanel == null) throw new System.Exception("jokerInfoPanel이 인스펙터에 연결되지 않았습니다.");
            if (shopPanel == null) throw new System.Exception("shopPanel이 인스펙터에 연결되지 않았습니다.");
        }

        // TODO: 인게임 UI 관리 (HUD, 상태창 등)
        public void OnClickBack()
        {
            // MessageDialogManager.Instance.Show("방을 나가는 중입니다...");
            string roomId = SessionManager.Instance != null ? SessionManager.Instance.CurrentRoomId : null;
            SocketManager.Instance.EmitToServer(new LeaveRoomRequest(roomId));

            Debug.Log($"🚪 방 퇴장: {roomId}");
        }

        // 테스트용 딜 버튼 핸들러
        public void OnClickTestDeal()
        {
            Debug.Log("[InGameUIManager] test 버튼 클릭됨: ready 메시지 전송 시도");
            if (SocketManager.Instance != null)
            {
                var roomId = SessionManager.Instance.CurrentRoomId;
                Debug.Log($"[InGameUIManager] roomId: {roomId}");
                var data = new Dictionary<string, object> { { "roomId", roomId } };
                SocketManager.Instance.EmitToServer(new ReadyRequest(roomId));
            }
        }

        public void OnClickTest2()
        {


        }

        // 서비스 준비 중 메시지창
        public void OnClickRunInfo()
        {
            MessageDialogManager.Instance.Show("서비스 준비 중입니다");
        }
        public void OnClickOption()
        {
            MessageDialogManager.Instance.Show("서비스 준비 중입니다");
        }

        // Rank 정렬 버튼 클릭 핸들러
        public void OnClickSortRank()
        {
            var myPlayer = GameManager.Instance.myPlayer;
            if (myPlayer != null)
            {
                myPlayer.userSortType = BalatroOnline.Game.MySlot.SortType.Rank;
                myPlayer.SortHandByRank();
                myPlayer.UpdateHandCardPositions();
            }
        }


        // Suit 정렬 버튼 클릭 핸들러
        public void OnClickSortSuit()
        {
            var myPlayer = GameManager.Instance.myPlayer;
            if (myPlayer != null)
            {
                myPlayer.userSortType = BalatroOnline.Game.MySlot.SortType.Suit;
                myPlayer.SortHandBySuit();
                myPlayer.UpdateHandCardPositions();
            }
        }

        // 버리기 버튼 클릭 핸들러
        public void OnClickDiscard()
        {
            var myPlayer = GameManager.Instance.myPlayer;
            var roomId = SessionManager.Instance.CurrentRoomId;
            if (myPlayer != null && !string.IsNullOrEmpty(roomId))
            {
                myPlayer.DiscardSelectedCards(roomId);
            }
        }

        // HandPlayReady 버튼 클릭 핸들러
        public void OnClickHandPlayReady()
        {
            Debug.Log("[InGameUIManager] HandPlayReady 버튼 클릭됨");
            DisablePlayButtons(); // 버튼 즉시 비활성화
            var myPlayer = GameManager.Instance.myPlayer;
            if (myPlayer != null)
            {
                var selected = myPlayer.GetSelectedCardInfos();
                Debug.Log($"[InGameUIManager] 선택된 카드: {selected.Count}장");

                BalatroOnline.InGame.InGameSceneManager.Instance.SendOnHandPlayReady(selected);
            }
            else
            {
                Debug.LogWarning("[InGameUIManager] myPlayer가 null");
            }
        }

        // 다음 라운드/최초 라운드 시작 시 UI/카드/버튼 초기화
        public void ResetForNewRound()
        {
            // 1. 기존 유저 카드 모두 파괴
            var myPlayer = GameManager.Instance.myPlayer;
            if (myPlayer != null)
            {
                foreach (var card in myPlayer.handCards)
                {
                    if (card != null)
                        Destroy(card.gameObject);
                }
                myPlayer.handCards.Clear();
                // 2. 족보/점수 UI 초기화
                var rankTextField = typeof(BalatroOnline.Game.MySlot).GetField("rankText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var scoreTextField = typeof(BalatroOnline.Game.MySlot).GetField("scoreText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var rankText = rankTextField?.GetValue(myPlayer) as TMPro.TextMeshProUGUI;
                var scoreText = scoreTextField?.GetValue(myPlayer) as TMPro.TextMeshProUGUI;
                if (rankText != null) rankText.text = "";
                if (scoreText != null) scoreText.text = "";
            }
            // 3. 버튼 활성화
            handPlayReadyButton.interactable = true;
            discardButton.interactable = true;
        }

        public void OnClickNextRound()
        {
            shopPanel.SetActive(false);
            var roomId = SessionManager.Instance.CurrentRoomId;
            SocketManager.Instance.EmitToServer(new NextRoundReadyRequest(roomId));
            // SocketManager.Instance.EmitToServer("nextRound", new Dictionary<string, object> { { "roomId", roomId } });
        }

        public void OnClickReRoll()
        {
            Debug.Log("[InGameUIManager] ReRoll 버튼 클릭됨");

            // 서버에 다시뽑기 요청 전송
            var roomId = SessionManager.Instance.CurrentRoomId;
            if (!string.IsNullOrEmpty(roomId))
            {
                // var data = new Dictionary<string, object> { { "roomId", roomId } };
                // SocketManager.Instance.EmitToServer("reRollShop", data);
                SocketManager.Instance.EmitToServer(new ReRollShopRequest(roomId));
                Debug.Log("[InGameUIManager] reRollShop 요청 전송");

                // 사용자에게 처리 중 메시지 표시
                MessageDialogManager.Instance.Show("새로운 조커 카드를 준비 중입니다...", null, 1f);
            }
            else
            {
                Debug.LogWarning("[InGameUIManager] roomId가 null입니다.");
                MessageDialogManager.Instance.Show("방 정보를 찾을 수 없습니다.", null, 2f);
            }
        }

        public void OnClickJokerCard()
        {
            GameObject clickedObj = EventSystem.current.currentSelectedGameObject;
            Debug.Log("눌린 놈: " + clickedObj.name);
            jokerInfoPanel.SetActive(true);
        }

        public void OnClickJokerInfoOk()
        {
            Debug.Log("[InGameUIManager] JokerInfoOk 버튼 클릭됨");
            jokerInfoPanel.SetActive(false);
        }

        public void OnClickOwnedJokerInfoOk()
        {
            Debug.Log("[InGameUIManager] OnClickOwnedJokerInfoOk 버튼 클릭됨");
            ownedJokerInfoPanel.SetActive(false);
        }

        public void OnClickOwnedJokerInfoSell()
        {
            Debug.Log("[InGameUIManager] OnClickOwnedJokerInfoSell 버튼 클릭됨");

            // MySlot의 SellJoker 메서드 호출
            if (InGameSceneManager.Instance != null && InGameSceneManager.Instance.mySlot != null)
            {
                InGameSceneManager.Instance.mySlot.SellJoker();
            }
            else
            {
                Debug.LogError("[InGameUIManager] MySlot을 찾을 수 없어서 판매를 진행할 수 없습니다.");
            }
        }

        // handPlayResult 등에서 버튼 비활성화
        public void DisablePlayButtons()
        {
            handPlayReadyButton.interactable = false;
            discardButton.interactable = false;
        }

    }
}