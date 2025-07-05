using UnityEngine;
using UnityEngine.UI;
using BalatroOnline.Common;
using BalatroOnline.Network;
using BalatroOnline.Network.Protocol;
using System.Collections;
using System.Collections.Generic;
using TMPro;

namespace BalatroOnline.Lobby
{
    /// <summary>
    /// 로비 씬 전용 UI를 관리하는 매니저
    /// </summary>
    public class LobbyUIManager : MonoBehaviour
    {
        public static LobbyUIManager Instance { get; private set; }

        [Header("Room List UI")]
        public ScrollRect roomListScrollRect;
        public Transform roomListContent;
        public GameObject roomListItemPrefab;
        public Button refreshButton;
        public Button createRoomButton;
        public Button backButton;
        public TextMeshProUGUI roomCountText;

        private List<GameObject> roomItems = new List<GameObject>();

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
            // 초기 방 목록 로드
            LoadRoomList();
        }

        public void LoadRoomList()
        {
            Debug.Log("[LobbyUIManager] 방 목록 로드 시작");
            ApiManager.Instance.GetRoomList(OnGetRoomListResult);
        }

        public void OnClickBack()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("ChannelScene");
        }

        public void OnClickCreateRoom()
        {
            Debug.Log("[LobbyUIManager] 방 만들기 버튼 클릭됨");

            // 테스트용 기본값으로 방 생성 (특수문자 제거)
            string roomName = "테스트 방 " + System.DateTime.Now.ToString("HHmmss");
            int maxPlayers = 4;

            // 로딩 메시지 표시
            MessageDialogManager.Instance.Show("방을 생성하고 있습니다...");

            // API 호출
            ApiManager.Instance.CreateRoom(roomName, maxPlayers, OnCreateRoomResult);
        }

        private void OnCreateRoomResult(CreateRoomResponse res)
        {
            if (res.success)
            {
                Debug.Log($"[LobbyUIManager] 방 생성 성공! RoomId: {res.roomId}");
                BalatroOnline.Common.SessionManager.Instance.CurrentRoomId = res.roomId;

                // 방 생성 성공 메시지 표시
                /*
                MessageDialogManager.Instance.Show($"방이 생성되었습니다!\n방 이름: {res.room.name}\n방 ID: {res.roomId}", () => {
                    // OK 클릭 시 Socket.IO로 방 입장
                    JoinRoomViaSocket(res.roomId);
                });
                */
                JoinRoomViaSocket(res.roomId);
            }
            else
            {
                Debug.LogError($"[LobbyUIManager] 방 생성 실패: {res.message}");
                MessageDialogManager.Instance.Show($"방 생성 실패: {res.message}");
            }
        }

        private void JoinRoomViaSocket(string roomId)
        {
            Debug.Log($"[LobbyUIManager] Socket.IO를 통해 방 입장 시도: {roomId}");
            MessageDialogManager.Instance.Show("방에 입장하고 있습니다...");

            SocketManager.Instance.JoinRoom(roomId);

            // // SocketManager에서 직접 핸들러를 호출하므로 이벤트 구독 불필요

            // // 이미 Socket.IO가 연결되어 있는지 확인
            // if (SocketManager.Instance.IsConnected())
            // {
            //     Debug.Log("[LobbyUIManager] Socket.IO가 이미 연결되어 있습니다. 방에 입장합니다.");
            //     SocketManager.Instance.JoinRoom(roomId);
            // }
            // else
            // {
            //     Debug.Log("[LobbyUIManager] Socket.IO 연결이 필요합니다. 연결을 시도합니다.");
            //     // Socket.IO 연결
            //     SocketManager.Instance.Connect();

            //     // 연결 완료 후 방 입장: SocketManager에서 직접 핸들러 호출

            //     // 연결 실패 시 처리 (타임아웃 방식)
            //     StartCoroutine(CheckConnectionTimeout());
            // }
        }

        /*
                private System.Collections.IEnumerator CheckConnectionTimeout()
                {
                    float timeout = 10f; // 10초 타임아웃
                    float elapsed = 0f;

                    while (elapsed < timeout)
                    {
                        if (SocketManager.Instance.IsConnected())
                        {
                            yield break; // 연결 성공
                        }

                        elapsed += Time.deltaTime;
                        yield return null;
                    }

                    // 타임아웃 발생
                    Debug.LogError("[LobbyUIManager] Socket.IO 연결 타임아웃");
                    // SocketManager에서 직접 핸들러를 호출하므로 이벤트 구독 해제 불필요
                    MessageDialogManager.Instance.Show("Socket.IO 연결 시간 초과\n방은 생성되었지만 실시간 통신이 불가능합니다.", () =>
                    {
                        // 연결 실패해도 InGameScene으로 이동 (REST API만 사용)
                        UnityEngine.SceneManagement.SceneManager.LoadScene("InGameScene");
                    });
                }
        */
        public void OnClickRefreshRoomList()
        {
            Debug.Log("[LobbyUIManager] 방 목록 새로고침 버튼 클릭됨");
            LoadRoomList();
        }

        private void OnGetRoomListResult(RoomListResponse res)
        {
            if (res.success)
            {
                Debug.Log($"[LobbyUIManager] 방 목록 조회 성공! 방 개수: {res.rooms?.Length ?? 0}");
                UpdateRoomList(res.rooms);
            }
            else
            {
                Debug.LogError($"[LobbyUIManager] 방 목록 조회 실패: {res.message}");
                MessageDialogManager.Instance.Show($"방 목록 조회 실패: {res.message}");
            }
        }

        private void UpdateRoomList(RoomData[] rooms)
        {
            // 기존 방 아이템들 제거
            ClearRoomList();

            if (rooms == null || rooms.Length == 0)
            {
                if (roomCountText != null)
                {
                    roomCountText.text = "방이 없습니다.";
                }
                return;
            }

            // 방 개수 표시
            if (roomCountText != null)
            {
                roomCountText.text = $"방 개수: {rooms.Length}";
            }

            // 방 아이템들 생성
            foreach (var room in rooms)
            {
                CreateRoomItem(room);
            }
        }

        private void CreateRoomItem(RoomData room)
        {
            if (roomListItemPrefab == null || roomListContent == null)
            {
                Debug.LogError("[LobbyUIManager] roomListItemPrefab 또는 roomListContent가 설정되지 않았습니다.");
                return;
            }

            GameObject roomItem = Instantiate(roomListItemPrefab, roomListContent);
            roomItems.Add(roomItem);

            // 방 아이템 컴포넌트 설정
            RoomListItem roomListItem = roomItem.GetComponent<RoomListItem>();
            if (roomListItem != null)
            {
                roomListItem.SetRoomData(room, OnJoinRoomClicked);
            }
        }

        private void ClearRoomList()
        {
            foreach (var item in roomItems)
            {
                if (item != null)
                {
                    Destroy(item);
                }
            }
            roomItems.Clear();
        }

        private void OnJoinRoomClicked(string roomId)
        {
            Debug.Log($"[LobbyUIManager] 방 입장 시도: {roomId}");
            BalatroOnline.Common.SessionManager.Instance.CurrentRoomId = roomId;

            // Socket.IO를 통해 방 입장
            JoinRoomViaSocket(roomId);
        }

        public void OnRoomJoinSuccess(string userId)
        {
            Debug.Log($"[LobbyUIManager] 방 입장 성공! 사용자: {userId}");

            // SocketManager에서 직접 핸들러를 호출하므로 이벤트 구독 해제 불필요

            UnityEngine.SceneManagement.SceneManager.LoadScene("InGameScene");
        }
    }
}