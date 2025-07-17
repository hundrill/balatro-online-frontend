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

        // 공통 POST 요청 유틸리티
        private IEnumerator PostRequestCoroutine<TReq, TRes>(string url, TReq req, System.Action<TRes> onResult)
            where TReq : BalatroOnline.Network.Protocol.BaseApi
            where TRes : BalatroOnline.Network.Protocol.BaseApi, new()
        {
            string json = req.ToJson();
            Debug.Log($"[ApiManager] 요청 URL: {url}");
            Debug.Log($"[ApiManager] 요청 바디: {json}");
            var request = new UnityEngine.Networking.UnityWebRequest(url, "POST");
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UnityEngine.Networking.UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new UnityEngine.Networking.DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            yield return request.SendWebRequest();
            Debug.Log($"[ApiManager] 응답 코드: {request.responseCode}");
            Debug.Log($"[ApiManager] 응답 본문: {request.downloadHandler.text}");
            TRes res = null;
            if (request.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
            {
                res = BalatroOnline.Network.Protocol.BaseApi.FromJson<TRes>(request.downloadHandler.text);
            }
            else
            {
                Debug.LogError($"[ApiManager] 네트워크 에러: {request.error}");
                res = new TRes { message = "Network error: " + request.error };
            }
            onResult?.Invoke(res);
        }

        public void CreateRoom(string name, int maxPlayers, int silverSeedChip, int goldSeedChip, int silverBettingChip, int goldBettingChip, System.Action<CreateRoomResponse> onResult)
        {
            var req = new CreateRoomRequest {
                name = name,
                maxPlayers = maxPlayers,
                silverSeedChip = silverSeedChip,
                goldSeedChip = goldSeedChip,
                silverBettingChip = silverBettingChip,
                goldBettingChip = goldBettingChip
            };
            string url = ServerConfig.Instance.GetHttpUrl() + "/rooms/create";
            StartCoroutine(PostRequestCoroutine<CreateRoomRequest, CreateRoomResponse>(url, req, onResult));
        }

        public void GetRoomList(System.Action<RoomListResponse> onResult)
        {
            Debug.Log("[ApiManager] 방 목록 조회 요청 시작");
            StartCoroutine(GetRoomListCoroutine(onResult));
        }

        private IEnumerator GetRoomListCoroutine(System.Action<RoomListResponse> onResult)
        {
            string url = ServerConfig.Instance.GetHttpUrl() + "/rooms/redis";
            Debug.Log($"[ApiManager] 요청 URL: {url}");
            var request = UnityEngine.Networking.UnityWebRequest.Get(url);
            request.downloadHandler = new UnityEngine.Networking.DownloadHandlerBuffer();
            yield return request.SendWebRequest();
            Debug.Log($"[ApiManager] 응답 코드: {request.responseCode}");
            Debug.Log($"[ApiManager] 응답 본문: {request.downloadHandler.text}");
            RoomListResponse res = null;
            if (request.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
            {
                res = BalatroOnline.Network.Protocol.BaseApi.FromJson<RoomListResponse>(request.downloadHandler.text);
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