using Best.SocketIO;
using UnityEngine;
using System;
using System.Collections.Generic;

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
    
    // 이벤트 콜백들
    public event Action<string> OnUserJoined;
    public event Action<string> OnUserLeft;
    public event Action<MessageData> OnMessageReceived;
    public event Action OnConnected;

    [System.Serializable]
    public class MessageData
    {
        public string userId;
        public string content;
        public string timestamp;
    }

    void Awake()
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

    // void Start()
    // {
    //     InitializeSocket();
    // }

    private void InitializeSocket()
    {
        // Socket.IO 클라이언트 생성 (백엔드 포트 3000, 기본 네임스페이스 사용)
        var options = new SocketOptions();
        
        // 기본 네임스페이스로 연결 (네임스페이스 문제 해결을 위해)
        socket = new Best.SocketIO.SocketManager(new Uri("http://localhost:3000"), options);
        
        Debug.Log("🔌 Socket.IO 초기화 완료 - 기본 네임스페이스 사용");
        
        // 연결 이벤트
        socket.Socket.On(SocketIOEventTypes.Connect, () => {
            Debug.Log("✅ Socket.IO 연결 성공!");
            isConnected = true;
            OnConnected?.Invoke();
        });
        
        // 연결 해제 이벤트
        socket.Socket.On(SocketIOEventTypes.Disconnect, () => {
            Debug.Log("❌ Socket.IO 연결 해제");
            isConnected = false;
        });
        
        // 사용자 입장 이벤트
        socket.Socket.On("userJoined", () => {
            Debug.Log("📨 userJoined 이벤트 수신됨!");
            // 임시로 하드코딩된 userId 사용
            string userId = "test-user";
            Debug.Log($"👤 유저 입장: {userId}");
            OnUserJoined?.Invoke(userId);
        });
        
        // 사용자 퇴장 이벤트
        socket.Socket.On("userLeft", () => {
            Debug.Log("📨 userLeft 이벤트 수신됨!");
            // 임시로 하드코딩된 userId 사용
            string userId = "test-user";
            Debug.Log($"👋 유저 퇴장: {userId}");
            OnUserLeft?.Invoke(userId);
        });
        
        // 메시지 수신 이벤트
        socket.Socket.On("message", () => {
            Debug.Log("📨 message 이벤트 수신됨!");
            // 임시로 하드코딩된 메시지 사용
            var msgData = new MessageData
            {
                userId = "test-user",
                content = "테스트 메시지",
                timestamp = System.DateTime.Now.ToString()
            };
            Debug.Log($"💬 메시지 수신: {msgData.content}");
            OnMessageReceived?.Invoke(msgData);
        });
        
        Debug.Log("📡 모든 Socket.IO 이벤트 리스너 등록 완료");
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
            var data = new Dictionary<string, object> { { "roomId", roomId } };
            Debug.Log($"🚪 방 입장 시도: {roomId}, 데이터: {JsonUtility.ToJson(data)}");
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

    void OnDestroy()
    {
        Disconnect();
    }
} 