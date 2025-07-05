using UnityEngine;
using UnityEngine.UI;
using BalatroOnline.Common;
using System.Collections.Generic;
using UnityEngine.EventSystems;

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
            // 방 ID는 GameManager 등에서 관리한다고 가정
            string roomId = BalatroOnline.Common.SessionManager.Instance != null ? BalatroOnline.Common.SessionManager.Instance.CurrentRoomId : null;
            MessageDialogManager.Instance.Show("방을 나가는 중입니다...");
            SocketManager.Instance.LeaveRoom(roomId);
            UnityEngine.SceneManagement.SceneManager.LoadScene("LobbyScene");
        }

        // 테스트용 딜 버튼 핸들러
        public void OnClickTestDeal()
        {
            Debug.Log("[InGameUIManager] test 버튼 클릭됨: ready 메시지 전송 시도");
            if (SocketManager.Instance != null)
            {
                var roomId = BalatroOnline.Common.SessionManager.Instance.CurrentRoomId;
                Debug.Log($"[InGameUIManager] roomId: {roomId}");
                Debug.Log($"[InGameUIManager] SocketManager 연결됨? {SocketManager.Instance.IsConnected()}");
                var data = new Dictionary<string, object> { { "roomId", roomId } };
                SocketManager.Instance.EmitToServer("ready", data);
            }
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
            var myPlayer = BalatroOnline.Common.GameManager.Instance.myPlayer;
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
            var myPlayer = BalatroOnline.Common.GameManager.Instance.myPlayer;
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
            var myPlayer = BalatroOnline.Common.GameManager.Instance.myPlayer;
            var roomId = BalatroOnline.Common.SessionManager.Instance.CurrentRoomId;
            if (myPlayer != null && !string.IsNullOrEmpty(roomId))
            {
                myPlayer.DiscardSelectedCards(roomId);
            }
        }

        // HandPlayReady 버튼 클릭 핸들러
        public void OnClickHandPlayReady()
        {
            Debug.Log("[InGameUIManager] HandPlayReady 버튼 클릭됨");
            var myPlayer = BalatroOnline.Common.GameManager.Instance.myPlayer;
            if (myPlayer != null)
            {
                var selected = myPlayer.GetSelectedCardInfos();
                Debug.Log($"[InGameUIManager] 선택된 카드: {selected.Count}장");
                // 준비 완료 메시지창 띄우고 OK 시 실제 서버 전송
                BalatroOnline.Common.MessageDialogManager.Instance.Show("준비 완료!", () =>
                {
                    if (BalatroOnline.InGame.InGameSceneManager.Instance != null)
                    {
                        BalatroOnline.InGame.InGameSceneManager.Instance.OnHandPlayReady(selected);
                    }
                    else
                    {
                        Debug.LogWarning("[InGameUIManager] InGameSceneManager.Instance가 null");
                    }
                });
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
            var myPlayer = BalatroOnline.Common.GameManager.Instance.myPlayer;
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
            // 1. Shop 창 닫기
            shopPanel.SetActive(false);
            // 2. 서버에 nextRound 메시지 전송만
            var roomId = BalatroOnline.Common.SessionManager.Instance.CurrentRoomId;
            SocketManager.Instance.EmitToServer("nextRound", new Dictionary<string, object> { { "roomId", roomId } });
        }

        public void OnClickReRoll()
        {
            Debug.Log("[InGameUIManager] ReRoll 버튼 클릭됨");

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

        // handPlayResult 등에서 버튼 비활성화
        public void DisablePlayButtons()
        {
            handPlayReadyButton.interactable = false;
            discardButton.interactable = false;
        }

    }
}