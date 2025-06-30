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

    void Start()
    {
        InitializeSocket();
    }

    private void InitializeSocket()
    {
        // Socket.IO 클라이언트 생성 (백엔드 포트 3000, 기본 네임스페이스 사용)
        var options = new SocketOptions();
        options.Reconnection = false; // 자동 재연결 비활성화
        
        // 기본 네임스페이스로 연결 (네임스페이스 문제 해결을 위해)
        socket = new Best.SocketIO.SocketManager(new Uri("http://localhost:3000"), options);
        
        Debug.Log("🔌 Socket.IO 초기화 완료 - 기본 네임스페이스 사용");
        
        // 연결 이벤트
        socket.Socket.On(SocketIOEventTypes.Connect, () => {
            Debug.Log("✅ Socket.IO 연결 성공!");
            isConnected = true;
            Debug.Log("내 소켓 ID: " + socket.Socket.Id);
            OnConnected?.Invoke();
        });
        
        // 연결 해제 이벤트
        socket.Socket.On(SocketIOEventTypes.Disconnect, () => {
            Debug.Log("❌ Socket.IO 연결 해제");
            isConnected = false;
            // 서버와의 연결이 끊어졌다는 메시지창 표시
            BalatroOnline.Common.MessageDialogManager.Instance?.Show("서버와의 연결이 끊어졌습니다.");
        });
        
        // 사용자 입장 이벤트
        socket.Socket.On<object>("userJoined", (data) => {
            Debug.Log("📨 userJoined 이벤트 수신됨!");
            if (data is Dictionary<string, object> dict && dict.TryGetValue("userId", out var userIdObj))
            {
                string userId = userIdObj.ToString();
                Debug.Log($"👤 유저 입장: {userId}");
                OnUserJoined?.Invoke(userId);
            }
            else
            {
                Debug.LogWarning("userJoined 이벤트에서 userId를 찾을 수 없음");
            }
        });
        
        // 사용자 퇴장 이벤트
        socket.Socket.On<object>("userLeft", (data) => {
            Debug.Log("📨 userLeft 이벤트 수신됨!");
            if (data is Dictionary<string, object> dict && dict.TryGetValue("userId", out var userIdObj))
            {
                string userId = userIdObj.ToString();
                Debug.Log($"👋 유저 퇴장: {userId}");
                OnUserLeft?.Invoke(userId);
            }
            else
            {
                Debug.LogWarning("userLeft 이벤트에서 userId를 찾을 수 없음");
            }
        });
        
        // 메시지 수신 이벤트 (receiveMessage로 변경)
        socket.Socket.On<object>("receiveMessage", (data) => {
            Debug.Log("📨 receiveMessage 이벤트 수신됨!");
            if (data is Dictionary<string, object> dict)
            {
                var msgData = new MessageData
                {
                    userId = dict.TryGetValue("userId", out var userIdObj) ? userIdObj.ToString() : "",
                    content = dict.TryGetValue("message", out var contentObj) ? contentObj.ToString() : "",
                    timestamp = System.DateTime.Now.ToString() // 서버에서 timestamp를 보내지 않으므로 클라이언트에서 생성
                };
                Debug.Log($"💬 메시지 수신: {msgData.content}");
                OnMessageReceived?.Invoke(msgData);
            }
            else
            {
                Debug.LogWarning("receiveMessage 이벤트에서 데이터를 파싱할 수 없음");
            }
        });

        // startGame 이벤트 등록
        socket.Socket.On<object>("startGame", (data) => {
            Debug.Log($"[SocketManager] startGame 이벤트 수신: {data}");
            if (data is Dictionary<string, object> dict)
            {
                Debug.Log($"[SocketManager] startGame dict keys: {string.Join(",", dict.Keys)}");
                if (dict.TryGetValue("myCards", out var myCardsObj) && dict.TryGetValue("opponents", out var opponentsObj))
                {
                    Debug.Log($"[SocketManager] myCardsObj type: {myCardsObj?.GetType()}, opponentsObj type: {opponentsObj?.GetType()}");
                    var myCardsList = new List<BalatroOnline.Game.CardData>();
                    if (myCardsObj is object[] myCardsArr)
                    {
                        Debug.Log($"[SocketManager] myCardsArr length: {myCardsArr.Length}");
                        foreach (var card in myCardsArr)
                        {
                            if (card is Dictionary<string, object> cardDict)
                            {
                                string suit = cardDict["suit"].ToString();
                                int rank = int.Parse(cardDict["rank"].ToString());
                                Debug.Log($"[SocketManager] 카드 파싱: suit={suit}, rank={rank}");
                                myCardsList.Add(new BalatroOnline.Game.CardData(suit, rank));
                            }
                            else
                            {
                                Debug.Log($"[SocketManager] card 타입: {card?.GetType()} 값: {card}");
                            }
                        }
                    }
                    else if (myCardsObj is List<object> myCardsRaw)
                    {
                        Debug.Log($"[SocketManager] myCardsRaw count: {myCardsRaw.Count}");
                        foreach (var card in myCardsRaw)
                        {
                            if (card is Dictionary<string, object> cardDict)
                            {
                                string suit = cardDict["suit"].ToString();
                                int rank = int.Parse(cardDict["rank"].ToString());
                                Debug.Log($"[SocketManager] 카드 파싱: suit={suit}, rank={rank}");
                                myCardsList.Add(new BalatroOnline.Game.CardData(suit, rank));
                            }
                            else
                            {
                                Debug.Log($"[SocketManager] card 타입: {card?.GetType()} 값: {card}");
                            }
                        }
                    }
                    else
                    {
                        Debug.Log($"[SocketManager] myCardsObj가 object[]/List<object>가 아님: {myCardsObj}");
                    }
                    var opponentCounts = new List<int>();
                    if (opponentsObj is object[] oppArr)
                    {
                        Debug.Log($"[SocketManager] opponentsArr length: {oppArr.Length}");
                        foreach (var cnt in oppArr)
                            opponentCounts.Add(int.Parse(cnt.ToString()));
                    }
                    else if (opponentsObj is List<object> oppList)
                    {
                        Debug.Log($"[SocketManager] opponents count: {oppList.Count}");
                        foreach (var cnt in oppList)
                            opponentCounts.Add(int.Parse(cnt.ToString()));
                    }
                    else
                    {
                        Debug.Log($"[SocketManager] opponentsObj가 object[]/List<object>가 아님: {opponentsObj}");
                    }
                    Debug.Log($"[SocketManager] GameManager.OnReceiveCardDeal 호출: myCardsList.Count={myCardsList.Count}, opponentCounts.Count={opponentCounts.Count}");
                    if (BalatroOnline.Common.GameManager.Instance != null)
                    {
                        BalatroOnline.Common.GameManager.Instance.OnReceiveCardDeal(myCardsList, opponentCounts);
                    }
                    else
                    {
                        Debug.LogError("[SocketManager] GameManager.Instance가 null!");
                    }
                }
                else
                {
                    Debug.Log($"[SocketManager] dict에 myCards/opponents 키가 없음: {string.Join(",", dict.Keys)}");
                }
            }
            else
            {
                Debug.Log($"[SocketManager] startGame data가 Dictionary<string, object>가 아님: {data}");
            }
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
            var data = new Dictionary<string, object> {
                { "roomId", roomId },
                { "userId", "hundrill@naver.com" }
            };
            Debug.Log($"🚪 방 입장 시도: {roomId}, 데이터: {{ roomId: {roomId}, userId: hundrill@naver.com }}");
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
} 