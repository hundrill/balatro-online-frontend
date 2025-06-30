using UnityEngine;
using UnityEngine.UI;
using TMPro;
using BalatroOnline.Common;
using BalatroOnline.Network;
using BalatroOnline.Network.Protocol;

namespace BalatroOnline.InGame
{
    /// <summary>
    /// 인게임 씬의 전체 흐름을 관리하는 매니저
    /// </summary>
    public class InGameSceneManager : MonoBehaviour
    {
        public static InGameSceneManager Instance { get; private set; }

        [Header("UI References")]
        public Button backButton;
        public Button sendMessageButton;
        public TMP_InputField messageInput;
        public TextMeshProUGUI roomInfoText;
        public TextMeshProUGUI userListText;
        public TextMeshProUGUI chatText;

        private string currentRoomId;
        private string currentUserId;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            
            // Random.Range를 Awake에서 초기화
            currentUserId = "Player_" + Random.Range(1000, 9999);
        }

        private void Start()
        {
            InitializeUI();
            InitializeSocket();
            JoinCurrentRoom();
        }

        private void InitializeUI()
        {
            // Back 버튼 이벤트
            if (backButton != null)
            {
                backButton.onClick.AddListener(OnBackButtonClicked);
            }

            // 메시지 전송 버튼 이벤트
            if (sendMessageButton != null)
            {
                sendMessageButton.onClick.AddListener(OnSendMessageClicked);
            }

            // Enter 키로 메시지 전송
            if (messageInput != null)
            {
                messageInput.onEndEdit.AddListener((text) => OnSendMessageClicked());
            }

            // 현재 방 정보 표시
            currentRoomId = BalatroOnline.Common.SessionManager.Instance.CurrentRoomId;
            if (roomInfoText != null)
            {
                roomInfoText.text = $"방 ID: {currentRoomId}";
            }

            // 초기 채팅 텍스트
            if (chatText != null)
            {
                chatText.text = "채팅방에 입장했습니다.\n";
            }
        }

        private void InitializeSocket()
        {
            // 이미 Socket.IO가 연결되어 있으면 이벤트만 구독
            if (SocketManager.Instance.IsConnected())
            {
                Debug.Log("Socket.IO가 이미 연결되어 있습니다.");
                SubscribeToSocketEvents();
                return;
            }

            // Socket.IO 연결
            SocketManager.Instance.Connect();

            // 이벤트 구독
            SubscribeToSocketEvents();
        }

        private void SubscribeToSocketEvents()
        {
            SocketManager.Instance.OnConnected += OnSocketConnected;
            SocketManager.Instance.OnUserJoined += OnUserJoined;
            SocketManager.Instance.OnUserLeft += OnUserLeft;
            SocketManager.Instance.OnMessageReceived += OnMessageReceived;
        }

        private void JoinCurrentRoom()
        {
            if (!string.IsNullOrEmpty(currentRoomId))
            {
                // 이미 방에 입장되어 있는지 확인 (로비에서 이미 입장했을 수 있음)
                Debug.Log($"InGameScene: 현재 방 ID {currentRoomId}에 입장 시도");
                
                // Socket.IO 연결 후 방 입장
                if (SocketManager.Instance.IsConnected())
                {
                    // 이미 연결되어 있으면 방 입장 (중복 방지)
                    Debug.Log("InGameScene: Socket.IO 연결됨, 방 입장 시도");
                    SocketManager.Instance.JoinRoom(currentRoomId);
                }
                else
                {
                    // 연결이 안 되어 있으면 연결 완료 후 자동 입장
                    Debug.Log("InGameScene: Socket.IO 연결 대기 중");
                    SocketManager.Instance.OnConnected += () => {
                        Debug.Log("InGameScene: Socket.IO 연결 완료, 방 입장 시도");
                        SocketManager.Instance.JoinRoom(currentRoomId);
                    };
                }
            }
            else
            {
                Debug.LogWarning("InGameScene: currentRoomId가 비어있습니다.");
            }
        }

        private void OnSocketConnected()
        {
            Debug.Log("Socket.IO 연결됨 - 방 입장 시도");
            if (!string.IsNullOrEmpty(currentRoomId))
            {
                SocketManager.Instance.JoinRoom(currentRoomId);
            }
        }

        private void OnUserJoined(string userId)
        {
            Debug.Log($"유저 입장: {userId}");
            if (chatText != null)
            {
                chatText.text += $"{userId}님이 입장했습니다.\n";
                ScrollToBottom();
            }
            UpdateUserList();
        }

        private void OnUserLeft(string userId)
        {
            Debug.Log($"유저 퇴장: {userId}");
            if (chatText != null)
            {
                chatText.text += $"{userId}님이 퇴장했습니다.\n";
                ScrollToBottom();
            }
            UpdateUserList();
        }

        private void OnMessageReceived(SocketManager.MessageData messageData)
        {
            Debug.Log($"메시지 수신: {messageData.content}");
            if (chatText != null)
            {
                chatText.text += $"{messageData.userId}: {messageData.content}\n";
                ScrollToBottom();
            }
        }

        private void OnSendMessageClicked()
        {
            if (messageInput != null && !string.IsNullOrEmpty(messageInput.text))
            {
                string message = messageInput.text.Trim();
                if (!string.IsNullOrEmpty(message))
                {
                    SocketManager.Instance.SendMessage(currentRoomId, message);
                    messageInput.text = "";
                }
            }
        }

        private void OnBackButtonClicked()
        {
            // 방 퇴장 처리
            if (!string.IsNullOrEmpty(currentRoomId))
            {
                SocketManager.Instance.LeaveRoom(currentRoomId);
            }

            // 백엔드에 방 퇴장 API 호출
            StartCoroutine(LeaveRoomCoroutine());
        }

        private System.Collections.IEnumerator LeaveRoomCoroutine()
        {
            // 백엔드 API 호출
            ApiManager.Instance.LeaveRoom(currentRoomId, (response) => {
                if (response.success)
                {
                    Debug.Log("방 퇴장 성공");
                    // 로비 씬으로 이동
                    UnityEngine.SceneManagement.SceneManager.LoadScene("LobbyScene");
                }
                else
                {
                    Debug.LogError("방 퇴장 실패");
                    // 에러가 있어도 로비로 이동
                    UnityEngine.SceneManagement.SceneManager.LoadScene("LobbyScene");
                }
            });
            yield break;
        }

        private void UpdateUserList()
        {
            // TODO: 백엔드에서 유저 리스트 API 호출하여 업데이트
            if (userListText != null)
            {
                userListText.text = "유저 목록:\n- 나 (나)\n- 다른 유저들...";
            }
        }

        private void ScrollToBottom()
        {
            // InGameUIManager를 통해 스크롤 처리
            if (InGameUIManager.Instance != null)
            {
                InGameUIManager.Instance.ScrollToBottom();
            }
        }

        private void OnDestroy()
        {
            // 이벤트 구독 해제
            if (SocketManager.Instance != null)
            {
                SocketManager.Instance.OnConnected -= OnSocketConnected;
                SocketManager.Instance.OnUserJoined -= OnUserJoined;
                SocketManager.Instance.OnUserLeft -= OnUserLeft;
                SocketManager.Instance.OnMessageReceived -= OnMessageReceived;
            }
        }
    }
} 