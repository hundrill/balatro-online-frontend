using Best.SocketIO;
using UnityEngine;
using System;
using System.Collections.Generic;
using BalatroOnline.Common;
using BalatroOnline.Lobby;
using BalatroOnline.InGame;
using BalatroOnline.Login;

public class SocketManager : MonoBehaviour
{
    private static SocketManager instance;
    public static SocketManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<SocketManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("SocketManager");
                    instance = go.AddComponent<SocketManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return instance;
        }
    }

    private Best.SocketIO.SocketManager socket;
    private bool isConnected = false;

    // 이벤트 큐 정의
    private Queue<SocketEvent> eventQueue = new Queue<SocketEvent>();
    private SocketEvent currentEvent = null;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        InitializeSocket();
    }

    void Update()
    {
        // 큐에 쌓인 이벤트를 1개씩 꺼내서 처리
        if (currentEvent == null && eventQueue.Count > 0)
        {
            currentEvent = eventQueue.Dequeue();
        }
        if (currentEvent != null)
        {
            bool handled = HandleSocketEvent(currentEvent);
            if (handled)
            {
                currentEvent = null;
            }
            // handled가 false면 currentEvent를 유지해서 다음 프레임에 재시도
        }
    }

    private void InitializeSocket()
    {
        var options = new SocketOptions();
        options.Reconnection = false;
        socket = new Best.SocketIO.SocketManager(new Uri(ServerConfig.Instance.GetHttpUrl()), options);
        Debug.Log("🔌 Socket.IO 초기화 완료 - 기본 네임스페이스 사용");

        socket.Socket.On(SocketIOEventTypes.Connect, () =>
        {
            Debug.Log("✅ Socket.IO 연결 성공!");
            isConnected = true;
            Debug.Log("내 소켓 ID: " + socket.Socket.Id);
            eventQueue.Enqueue(new SocketEvent { type = SocketEventType.Connected });
        });

        socket.Socket.On(SocketIOEventTypes.Disconnect, () =>
        {
            Debug.Log("❌ Socket.IO 연결 해제");
            isConnected = false;
            BalatroOnline.Common.MessageDialogManager.Instance?.Show("서버와의 연결이 끊어졌습니다.");
        });

        socket.Socket.On<object>("userJoined", (data) =>
        {
            Debug.Log("📨 userJoined 이벤트 수신됨!");
            if (data is Dictionary<string, object> dict && dict.TryGetValue("userId", out var userIdObj))
            {
                string userId = userIdObj.ToString();
                Debug.Log($"👤 유저 입장: {userId}");
                eventQueue.Enqueue(new SocketEvent { type = SocketEventType.UserJoined, payload = userId });
            }
            else
            {
                Debug.LogWarning("userJoined 이벤트에서 userId를 찾을 수 없음");
            }
        });

        socket.Socket.On<object>("userLeft", (data) =>
        {
            Debug.Log("📨 userLeft 이벤트 수신됨!");
            if (data is Dictionary<string, object> dict && dict.TryGetValue("userId", out var userIdObj))
            {
                string userId = userIdObj.ToString();
                Debug.Log($"👋 유저 퇴장: {userId}");
                eventQueue.Enqueue(new SocketEvent { type = SocketEventType.UserLeft, payload = userId });
            }
            else
            {
                Debug.LogWarning("userLeft 이벤트에서 userId를 찾을 수 없음");
            }
        });

        socket.Socket.On<object>("receiveMessage", (data) =>
        {
            Debug.Log("📨 receiveMessage 이벤트 수신됨!");
            if (data is Dictionary<string, object> dict)
            {
                var msgData = new MessageData
                {
                    userId = dict.TryGetValue("userId", out var userIdObj) ? userIdObj.ToString() : "",
                    content = dict.TryGetValue("message", out var contentObj) ? contentObj.ToString() : "",
                    timestamp = System.DateTime.Now.ToString()
                };
                Debug.Log($"💬 메시지 수신: {msgData.content}");
                eventQueue.Enqueue(new SocketEvent { type = SocketEventType.MessageReceived, payload = msgData });
            }
            else
            {
                Debug.LogWarning("receiveMessage 이벤트에서 데이터를 파싱할 수 없음");
            }
        });

        socket.Socket.On<object>("startGame", (data) =>
        {
            Debug.Log($"[SocketManager] startGame 이벤트 수신: {data}");
            eventQueue.Enqueue(new SocketEvent { type = SocketEventType.StartGame, payload = data });
        });

        socket.Socket.On<object>("discardResult", (data) =>
        {
            Debug.Log("[SocketManager] discardResult 이벤트 수신: " + data);
            eventQueue.Enqueue(new SocketEvent { type = SocketEventType.DiscardResult, payload = data });
        });

        socket.Socket.On<object>("handPlayResult", (data) =>
        {
            Debug.Log("[SocketManager] handPlayResult 이벤트 수신: " + data);
            eventQueue.Enqueue(new SocketEvent { type = SocketEventType.HandPlayResult, payload = data });
        });

        socket.Socket.On<object>("buyCardResult", (data) =>
        {
            Debug.Log("[SocketManager] buyCardResult 이벤트 수신: " + data);
            eventQueue.Enqueue(new SocketEvent { type = SocketEventType.BuyCardResult, payload = data });
        });

        socket.Socket.On<object>("roomUsers", (data) =>
        {
            Debug.Log("📨 roomUsers 이벤트 수신됨!");
            eventQueue.Enqueue(new SocketEvent { type = SocketEventType.RoomUsers, payload = data });
        });

        socket.Socket.On<object>("handPlayReady", (data) =>
        {
            Debug.Log("[SocketManager] handPlayReady 이벤트 수신: " + data);
            eventQueue.Enqueue(new SocketEvent { type = SocketEventType.HandPlayReady, payload = data });
        });

        socket.Socket.On<object>("nextRoundReady", (data) =>
        {
            Debug.Log("[SocketManager] nextRoundReady 이벤트 수신: " + data);
            eventQueue.Enqueue(new SocketEvent { type = SocketEventType.NextRoundReady, payload = data });
        });

        Debug.Log("📡 모든 Socket.IO 이벤트 리스너 등록 완료");
    }

    private bool HandleSocketEvent(SocketEvent socketEvent)
    {
        bool handled = false;
        switch (socketEvent.type)
        {
            case SocketEventType.Connected:
                if (LoginSceneManager.Instance != null) { LoginSceneManager.Instance.OnSocketConnected(); handled = true; }
                // if (InGameSceneManager.Instance != null) { InGameSceneManager.Instance.OnSocketConnected(); handled = true; }
                break;
            case SocketEventType.UserJoined:
                if (LobbyUIManager.Instance != null) { LobbyUIManager.Instance.OnRoomJoinSuccess(socketEvent.payload as string); handled = true; }
                // if (InGameSceneManager.Instance != null) { InGameSceneManager.Instance.OnUserJoined(socketEvent.payload as string); handled = true; }
                break;
            case SocketEventType.UserLeft:
                if (InGameSceneManager.Instance != null) { InGameSceneManager.Instance.OnUserLeft(socketEvent.payload as string); handled = true; }
                break;
            case SocketEventType.MessageReceived:
                if (InGameSceneManager.Instance != null) { InGameSceneManager.Instance.OnMessageReceived(socketEvent.payload as MessageData); handled = true; }
                break;
            case SocketEventType.HandPlayResult:
                if (InGameSceneManager.Instance != null) { InGameSceneManager.Instance.OnHandPlayResult(socketEvent.payload); handled = true; }
                break;
            case SocketEventType.StartGame:
                if (InGameSceneManager.Instance != null) { InGameSceneManager.Instance.OnStartGame(socketEvent.payload); handled = true; }
                break;
            case SocketEventType.DiscardResult:
                if (InGameSceneManager.Instance != null) { InGameSceneManager.Instance.OnDiscardResult(socketEvent.payload); handled = true; }
                break;
            case SocketEventType.BuyCardResult:
                if (InGameSceneManager.Instance != null) { InGameSceneManager.Instance.OnBuyCardResult(socketEvent.payload); handled = true; }
                break;
            case SocketEventType.RoomUsers:
                if (InGameSceneManager.Instance != null) { InGameSceneManager.Instance.OnRoomUsers(socketEvent.payload); handled = true; }
                break;
            case SocketEventType.HandPlayReady:
                if (InGameSceneManager.Instance != null) { InGameSceneManager.Instance.OnHandPlayReady(socketEvent.payload); handled = true; }
                break;
            case SocketEventType.NextRoundReady:
                if (InGameSceneManager.Instance != null) { InGameSceneManager.Instance.OnNextRoundReady(socketEvent.payload); handled = true; }
                break;
        }
        if (!handled)
        {
#if UNITY_EDITOR
            // throw new Exception($"[SocketManager] 이벤트({socketEvent.type})를 처리할 핸들러가 없습니다! payload: {socketEvent.payload}");
            Debug.LogWarning($"[SocketManager] 이벤트({socketEvent.type})를 처리할 핸들러가 없습니다! payload: {socketEvent.payload}");
#endif
        }
        return handled;
    }

    // 연결 시작
    public void Connect()
    {
        if (socket == null)
        {
            InitializeSocket();
        }

        if (!isConnected)
        {
            Debug.Log("🔌 Socket.IO 연결 시도...");
            socket.Open();
        }
    }

    // 연결 해제
    public void Disconnect()
    {
        if (isConnected)
        {
            Debug.Log("🔌 Socket.IO 연결 해제");
            socket.Close();
        }
    }

    // 방 입장
    public void JoinRoom(string roomId)
    {
        if (isConnected)
        {
            var data = new Dictionary<string, object> {
                { "roomId", roomId },
                { "userId", SessionManager.Instance.UserId }
            };
            Debug.Log($"🚪 방 입장 시도: {roomId}, 데이터: {{ roomId: {roomId}, userId: {SessionManager.Instance.UserId} }}");
            socket.Socket.Emit("joinRoom", data);
            Debug.Log($"📤 joinRoom 이벤트 전송 완료: {roomId}");
        }
        else
        {
            Debug.LogWarning("Socket.IO가 연결되지 않았습니다.");
        }
    }

    // 방 퇴장
    public void LeaveRoom(string roomId)
    {
        if (isConnected)
        {
            var data = new Dictionary<string, object> { { "roomId", roomId } };
            socket.Socket.Emit("leaveRoom", data);
            Debug.Log($"🚪 방 퇴장: {roomId}");
        }
    }

    // 메시지 전송
    public void SendMessage(string roomId, string content)
    {
        if (isConnected)
        {
            var data = new Dictionary<string, object> {
                { "roomId", roomId },
                { "content", content }
            };
            socket.Socket.Emit("sendMessage", data);
            Debug.Log($"💬 메시지 전송: {content}");
        }
        else
        {
            Debug.LogWarning("Socket.IO가 연결되지 않았습니다.");
        }
    }

    // 연결 상태 확인
    public bool IsConnected()
    {
        return isConnected;
    }

    public Best.SocketIO.SocketManager GetSocket()
    {
        return socket;
    }

    void OnDestroy()
    {
        Disconnect();
    }

    public void EmitToServer(string eventName, Dictionary<string, object> data)
    {
        string dictLog = "";
        if (data != null)
        {
            foreach (var kv in data)
                dictLog += $"{kv.Key}: {kv.Value}, ";
        }
        Debug.Log($"[SocketManager] EmitToServer: {eventName}, data: {dictLog}");
        if (socket != null && socket.Socket != null)
        {
            socket.Socket.Emit(eventName, data);
            Debug.Log($"[SocketManager] Emit 호출 완료: {eventName}");
        }
        else
        {
            Debug.LogWarning("[SocketManager] socket or socket.Socket is null! Emit 실패");
        }
    }

    // 이벤트 타입 정의
    private enum SocketEventType
    {
        Connected,
        UserJoined,
        UserLeft,
        MessageReceived,
        HandPlayResult,
        StartGame,
        DiscardResult,
        BuyCardResult,
        RoomUsers,
        HandPlayReady,
        NextRoundReady
    }

    private class SocketEvent
    {
        public SocketEventType type;
        public object payload;
    }

    [System.Serializable]
    public class MessageData
    {
        public string userId;
        public string content;
        public string timestamp;
    }
}