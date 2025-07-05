using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using BalatroOnline.Network.Protocol;

namespace BalatroOnline.Network
{
    public class ApiManager : MonoBehaviour
    {
        public static ApiManager Instance { get; private set; }
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void Login(string email, string password, System.Action<LoginResponse> onResult)
        {
            Debug.Log($"[ApiManager] Login 요청 시작: email={email}");
            StartCoroutine(LoginCoroutine(email, password, onResult));
        }

        public void CreateRoom(string name, int maxPlayers, System.Action<CreateRoomResponse> onResult)
        {
            Debug.Log($"[ApiManager] 방 생성 요청 시작: name={name}, maxPlayers={maxPlayers}");
            StartCoroutine(CreateRoomCoroutine(name, maxPlayers, onResult));
        }

        // public void LeaveRoom(string roomId, System.Action<BaseResponse> onResult)
        // {
        //     Debug.Log($"[ApiManager] 방 퇴장 요청 시작: roomId={roomId}");
        //     string userId = null;
        //     if (SocketManager.Instance != null && SocketManager.Instance.GetSocket() != null && SocketManager.Instance.GetSocket().Socket != null)
        //     {
        //         userId = SocketManager.Instance.GetSocket().Socket.Id;
        //     }
        //     StartCoroutine(LeaveRoomCoroutine(roomId, userId, onResult));
        // }

        public void GetRoomList(System.Action<RoomListResponse> onResult)
        {
            Debug.Log("[ApiManager] 방 목록 조회 요청 시작");
            StartCoroutine(GetRoomListCoroutine(onResult));
        }

        private IEnumerator LoginCoroutine(string email, string password, System.Action<LoginResponse> onResult)
        {
            string url = ServerConfig.Instance.GetHttpUrl() + "/auth/login";
            var req = new LoginRequest { email = email, password = password };
            string json = JsonUtility.ToJson(req);
            Debug.Log($"[ApiManager] 요청 URL: {url}");
            Debug.Log($"[ApiManager] 요청 바디: {json}");
            var request = new UnityWebRequest(url, "POST");
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            yield return request.SendWebRequest();
            Debug.Log($"[ApiManager] 응답 코드: {request.responseCode}");
            Debug.Log($"[ApiManager] 응답 본문: {request.downloadHandler.text}");
            LoginResponse res = null;
            if (request.result == UnityWebRequest.Result.Success)
            {
                res = JsonUtility.FromJson<LoginResponse>(request.downloadHandler.text);
            }
            else
            {
                Debug.LogError($"[ApiManager] 네트워크 에러: {request.error}");
                res = new LoginResponse { message = "Network error: " + request.error };
            }
            onResult?.Invoke(res);
        }

        private IEnumerator CreateRoomCoroutine(string name, int maxPlayers, System.Action<CreateRoomResponse> onResult)
        {
            string url = ServerConfig.Instance.GetHttpUrl() + "/rooms/create";
            var req = new CreateRoomRequest { name = name, maxPlayers = maxPlayers };
            string json = JsonUtility.ToJson(req);
            Debug.Log($"[ApiManager] 요청 URL: {url}");
            Debug.Log($"[ApiManager] 요청 바디: {json}");
            var request = new UnityWebRequest(url, "POST");
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            yield return request.SendWebRequest();
            Debug.Log($"[ApiManager] 응답 코드: {request.responseCode}");
            Debug.Log($"[ApiManager] 응답 본문: {request.downloadHandler.text}");
            CreateRoomResponse res = null;
            if (request.result == UnityWebRequest.Result.Success)
            {
                res = JsonUtility.FromJson<CreateRoomResponse>(request.downloadHandler.text);
            }
            else
            {
                Debug.LogError($"[ApiManager] 네트워크 에러: {request.error}");
                res = new CreateRoomResponse { message = "Network error: " + request.error };
            }
            onResult?.Invoke(res);
        }

        // private IEnumerator LeaveRoomCoroutine(string roomId, string userId, System.Action<BaseResponse> onResult)
        // {
        //     string url = ServerConfig.Instance.GetHttpUrl() + "/rooms/leave";
        //     var req = new BalatroOnline.Network.Protocol.LeaveRoomRequest { roomId = roomId, userId = userId };
        //     string json = JsonUtility.ToJson(req);
        //     Debug.Log($"[ApiManager] 요청 URL: {url}");
        //     Debug.Log($"[ApiManager] 요청 바디: {json}");
        //     var request = new UnityWebRequest(url, "POST");
        //     byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
        //     request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        //     request.downloadHandler = new DownloadHandlerBuffer();
        //     request.SetRequestHeader("Content-Type", "application/json");
        //     yield return request.SendWebRequest();
        //     Debug.Log($"[ApiManager] 응답 코드: {request.responseCode}");
        //     Debug.Log($"[ApiManager] 응답 본문: {request.downloadHandler.text}");
        //     BaseResponse res = null;
        //     if (request.result == UnityWebRequest.Result.Success)
        //     {
        //         res = JsonUtility.FromJson<BaseResponse>(request.downloadHandler.text);
        //     }
        //     else
        //     {
        //         Debug.LogError($"[ApiManager] 네트워크 에러: {request.error}");
        //         res = new BaseResponse { message = "Network error: " + request.error };
        //     }
        //     onResult?.Invoke(res);
        // }

        private IEnumerator GetRoomListCoroutine(System.Action<RoomListResponse> onResult)
        {
            string url = ServerConfig.Instance.GetHttpUrl() + "/rooms/redis";
            Debug.Log($"[ApiManager] 요청 URL: {url}");
            var request = UnityWebRequest.Get(url);
            request.downloadHandler = new DownloadHandlerBuffer();
            yield return request.SendWebRequest();
            Debug.Log($"[ApiManager] 응답 코드: {request.responseCode}");
            Debug.Log($"[ApiManager] 응답 본문: {request.downloadHandler.text}");
            RoomListResponse res = null;
            if (request.result == UnityWebRequest.Result.Success)
            {
                res = JsonUtility.FromJson<RoomListResponse>(request.downloadHandler.text);
            }
            else
            {
                Debug.LogError($"[ApiManager] 네트워크 에러: {request.error}");
                res = new RoomListResponse { message = "Network error: " + request.error };
            }
            onResult?.Invoke(res);
        }
    }
}