using Best.SocketIO;
using UnityEngine;
using System;
using System.Collections.Generic;
using BalatroOnline.Common;
using BalatroOnline.Lobby;
using BalatroOnline.InGame;
using BalatroOnline.Login;
using BalatroOnline.Network.SocketProtocol;

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
    private readonly Queue<SocketEvent> eventQueue = new Queue<SocketEvent>();
    private SocketEvent currentEvent = null;

    // 이벤트 핸들러 캐시
    private readonly Dictionary<string, Func<object, bool>> eventHandlers = new Dictionary<string, Func<object, bool>>();

    private class SocketEvent
    {
        public string type;
        public object payload;
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeEventHandlers();
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void InitializeEventHandlers()
    {
        eventHandlers[LoginResponse.EventNameConst] = HandleLoginResponse;
        eventHandlers[UserJoinedResponse.EventNameConst] = HandleUserJoinedResponse;
        eventHandlers[UserLeftResponse.EventNameConst] = HandleUserLeftResponse;
        eventHandlers[RoomUsersResponse.EventNameConst] = HandleRoomUsersResponse;
        eventHandlers[StartGameResponse.EventNameConst] = HandleStartGameResponse;
        eventHandlers[DiscardResponse.EventNameConst] = HandleDiscardResponse;
        eventHandlers[HandPlayReadyResponse.EventNameConst] = HandleHandPlayReadyResponse;
        eventHandlers[HandPlayResultResponse.EventNameConst] = HandleHandPlayResultResponse;
        eventHandlers[NextRoundReadyResponse.EventNameConst] = HandleNextRoundReadyResponse;
        eventHandlers[BuyCardResponse.EventNameConst] = HandleBuyCardResponse;
        eventHandlers[ReRollShopResponse.EventNameConst] = HandleReRollShopResponse;
        eventHandlers[SellCardResponse.EventNameConst] = HandleSellCardResponse;
        eventHandlers[CardPurchasedResponse.EventNameConst] = HandleCardPurchasedResponse;
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
            else
            {
                // handled가 false면 currentEvent를 유지해서 다음 프레임에 재시도
#if UNITY_EDITOR
                Debug.LogWarning($"[SocketManager] 핸들러 처리 실패! payload: {currentEvent.payload}");
#endif                
            }
        }
    }

    private void InitializeSocket()
    {
        var options = new SocketOptions();
        options.Reconnection = false;

        socket = new Best.SocketIO.SocketManager(new Uri(ServerConfig.Instance.GetHttpUrl()), options);
        Debug.Log("🔌 Socket.IO 초기화 완료 - 기본 네임스페이스 사용");

        socket.Socket.On(SocketIOEventTypes.Connect, OnSocketConnected);
        socket.Socket.On(SocketIOEventTypes.Disconnect, OnSocketDisconnected);
        socket.Socket.On<object>("response", OnResponseReceived);

        Debug.Log("📡 모든 Socket.IO 이벤트 리스너 등록 완료");
    }

    private void OnSocketConnected()
    {
        Debug.Log("✅ Socket.IO 연결 성공!");
        isConnected = true;
        Debug.Log("내 소켓 ID: " + socket.Socket.Id);
        LoginUIManager.Instance?.OnSocketConnectedForLogin();
    }

    private void OnSocketDisconnected()
    {
        Debug.Log("❌ Socket.IO 연결 해제");
        isConnected = false;
        BalatroOnline.Common.MessageDialogManager.Instance?.Show("서버와의 연결이 끊어졌습니다.");
    }

    private void OnResponseReceived(object data)
    {
        Debug.Log($"[SocketManager] 이벤트 수신: {data}");
        eventQueue.Enqueue(new SocketEvent { payload = data });
    }

    private bool HandleSocketEvent(SocketEvent socketEvent)
    {
        // eventName 확인 및 로그 출력
        string eventName = BaseSocket.GetEventNameFromData(socketEvent.payload);
        Debug.Log($"[SocketManager] HandleSocketEvent - eventName: {eventName}");

        // 딕셔너리에서 핸들러 찾기
        if (eventHandlers.TryGetValue(eventName, out var handler))
        {
            return handler(socketEvent.payload);
        }

#if UNITY_EDITOR
        Debug.LogWarning($"[SocketManager] 이벤트({eventName})를 처리할 핸들러가 없습니다! payload: {socketEvent.payload}");
#endif
        return false;
    }

    // 개별 이벤트 핸들러들
    private bool HandleLoginResponse(object payload)
    {
        var response = LoginResponse.FromPayload(payload);
        if (response != null && LoginUIManager.Instance != null)
        {
            LoginUIManager.Instance.OnLoginResult(response);
            return true;
        }
        return false;
    }

    private bool HandleUserJoinedResponse(object payload)
    {
        var response = UserJoinedResponse.FromPayload(payload);
        if (response != null && LobbyUIManager.Instance != null)
        {
            LobbyUIManager.Instance.OnRoomJoinSuccess(response);
            return true;
        }
        return false;
    }

    private bool HandleUserLeftResponse(object payload)
    {
        var response = UserLeftResponse.FromPayload(payload);
        if (response != null && InGameSceneManager.Instance != null)
        {
            InGameSceneManager.Instance.OnUserLeft(response);
            return true;
        }
        return false;
    }

    private bool HandleRoomUsersResponse(object payload)
    {
        var response = RoomUsersResponse.FromPayload(payload);
        if (response != null && InGameSceneManager.Instance != null)
        {
            InGameSceneManager.Instance.OnRoomUsers(response);
            return true;
        }
        return false;
    }

    private bool HandleStartGameResponse(object payload)
    {
        var response = StartGameResponse.FromPayload(payload);
        if (response != null && InGameSceneManager.Instance != null)
        {
            InGameSceneManager.Instance.OnStartGame(response);
            return true;
        }
        return false;
    }

    private bool HandleDiscardResponse(object payload)
    {
        var response = DiscardResponse.FromPayload(payload);
        if (response != null && InGameSceneManager.Instance != null)
        {
            InGameSceneManager.Instance.OnDiscardResult(response);
            return true;
        }
        return false;
    }

    private bool HandleHandPlayReadyResponse(object payload)
    {
        var response = HandPlayReadyResponse.FromPayload(payload);
        if (response != null && InGameSceneManager.Instance != null)
        {
            InGameSceneManager.Instance.OnHandPlayReady(response);
            return true;
        }
        return false;
    }

    private bool HandleHandPlayResultResponse(object payload)
    {
        var response = HandPlayResultResponse.FromPayload(payload);
        if (response != null && InGameSceneManager.Instance != null)
        {
            InGameSceneManager.Instance.OnHandPlayResult(response);
            return true;
        }
        return false;
    }

    private bool HandleNextRoundReadyResponse(object payload)
    {
        var response = NextRoundReadyResponse.FromPayload(payload);
        if (response != null && InGameSceneManager.Instance != null)
        {
            InGameSceneManager.Instance.OnNextRoundReady(response);
            return true;
        }
        return false;
    }

    private bool HandleBuyCardResponse(object payload)
    {
        var response = BuyCardResponse.FromPayload(payload);
        if (response != null && InGameSceneManager.Instance != null)
        {
            InGameSceneManager.Instance.OnBuyCardResult(response);
            return true;
        }
        return false;
    }

    private bool HandleReRollShopResponse(object payload)
    {
        var response = ReRollShopResponse.FromPayload(payload);
        if (response != null && InGameSceneManager.Instance != null)
        {
            InGameSceneManager.Instance.OnReRollShopResult(response);
            return true;
        }
        return false;
    }

    private bool HandleSellCardResponse(object payload)
    {
        var response = SellCardResponse.FromPayload(payload);
        if (response != null && InGameSceneManager.Instance != null)
        {
            InGameSceneManager.Instance.OnSellCardResult(response);
            return true;
        }
        return false;
    }

    private bool HandleCardPurchasedResponse(object payload)
    {
        var response = CardPurchasedResponse.FromPayload(payload);
        if (response != null && InGameSceneManager.Instance != null)
        {
            InGameSceneManager.Instance.OnCardPurchased(response);
            return true;
        }
        return false;
    }

    // 연결 시작
    public void Connect()
    {
        // 기존 소켓이 있으면 완전히 정리
        if (socket != null)
        {
            Debug.Log("[SocketManager] 기존 소켓 연결 정리");
            socket.Close();
            socket = null;
            isConnected = false;
        }

        // 이벤트 큐 완전 정리
        eventQueue.Clear();
        currentEvent = null;
        Debug.Log("[SocketManager] 이벤트 큐 정리 완료");

        // 새로 초기화
        InitializeSocket();

        Debug.Log("🔌 Socket.IO 연결 시도...");
        socket.Open();
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

    // 강제 소켓 연결 해제 (로그인 화면 등에서 사용)
    public void ForceDisconnect()
    {
        if (socket != null && isConnected)
        {
            Debug.Log("[SocketManager] 강제 소켓 연결 해제");
            socket.Close();
            isConnected = false;
        }
        else
        {
            Debug.Log("[SocketManager] 소켓이 이미 해제되어 있거나 초기화되지 않음");
        }
    }

    // 방 퇴장


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
    public bool IsConnected() => isConnected;

    public Best.SocketIO.SocketManager GetSocket() => socket;

    void OnDestroy()
    {
        Disconnect();
    }

    public void EmitToServer(BaseSocket dto)
    {
        string eventName = dto.EventName;
        var data = dto.ToDictionary();
        Debug.Log($"[SocketManager] EmitToServer: {eventName}, data: {JsonUtility.ToJson(dto)}");

        if (socket?.Socket != null)
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