using UnityEngine;
using UnityEngine.UI;
using TMPro;
using BalatroOnline.Network;
using BalatroOnline.Network.Protocol;
using BalatroOnline.Common;
using System.Collections.Generic;

namespace BalatroOnline.InGame
{
    /// <summary>
    /// 인게임 UI 요소들을 관리하는 매니저
    /// </summary>
    public class InGameUIManager : MonoBehaviour
    {
        [Header("UI Elements")]
        public Button backButton;
        public Button sendMessageButton;
        public TMP_InputField messageInput;
        public TextMeshProUGUI roomInfoText;
        public TextMeshProUGUI userListText;
        public TextMeshProUGUI chatText;
        public ScrollRect chatScrollRect;

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
            // InGameSceneManager에 UI 참조 전달
            if (InGameSceneManager.Instance != null)
            {
                InGameSceneManager.Instance.backButton = backButton;
                InGameSceneManager.Instance.sendMessageButton = sendMessageButton;
                InGameSceneManager.Instance.messageInput = messageInput;
                InGameSceneManager.Instance.roomInfoText = roomInfoText;
                InGameSceneManager.Instance.userListText = userListText;
                InGameSceneManager.Instance.chatText = chatText;
            }
        }

        // 채팅 스크롤을 맨 아래로 이동하는 메서드
        public void ScrollToBottom()
        {
            if (chatScrollRect != null)
            {
                Canvas.ForceUpdateCanvases();
                chatScrollRect.verticalNormalizedPosition = 0f;
            }
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
    }
} 