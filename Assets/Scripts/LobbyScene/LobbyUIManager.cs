using UnityEngine;
using UnityEngine.UI;
using BalatroOnline.Common;
using BalatroOnline.Network;
using BalatroOnline.Network.Protocol;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using BalatroOnline.Localization;

namespace BalatroOnline.Lobby
{
    /// <summary>
    /// 로비 씬 전용 UI를 관리하는 매니저
    /// </summary>
    public class LobbyUIManager : MonoBehaviour
    {
        public static LobbyUIManager Instance { get; private set; }

        [Header("Panel UI")]
        public GameObject channelPanel;
        public GameObject roomListPanel;

        [Header("Dialog UI")]
        public GameObject createRoomDialog;
        public GameObject quickStartDialog;

        [Header("User Info UI")]
        public TMP_Text nicknameText;
        public TMP_Text silverChipText;
        public TMP_Text goldChipText;

        [Header("Room List UI")]
        public ScrollRect roomListScrollRect;
        public Transform roomListContent;
        public GameObject roomListItemPrefab;
        public Button refreshButton;
        public Button createRoomButton;
        public Button backButton;

        [Header("UI")]
        public TMP_Text selectedChannelText; // 인스펙터에서 연결

        private List<GameObject> roomItems = new List<GameObject>();
        private string currentSelectedChannel = string.Empty;

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
            // 사용자 정보 표시 업데이트
            UpdateUserInfoDisplay();

            ChannelSelected(SessionManager.Instance.CurrentChannel);
        }

        /// <summary>
        /// 사용자 정보 표시를 업데이트합니다.
        /// </summary>
        public void UpdateUserInfoDisplay()
        {
            if (nicknameText != null)
            {
                nicknameText.text = SessionManager.Instance.UserNickname;
            }

            if (silverChipText != null)
            {
                silverChipText.text = SessionManager.Instance.SilverChip.ToString("N0");
            }

            if (goldChipText != null)
            {
                goldChipText.text = SessionManager.Instance.GoldChip.ToString("N0");
            }
        }

        public void LoadRoomList()
        {
            Debug.Log("[LobbyUIManager] 방 목록 로드 시작");
            ApiManager.Instance.GetRoomList(OnGetRoomListResult);
        }

        public void OnClickBack()
        {
            SocketManager.Instance.Disconnect();
            UnityEngine.SceneManagement.SceneManager.LoadScene("LoginScene");
        }

        public void OnClickCreateRoom()
        {
            Debug.Log("[LobbyUIManager] 방 만들기 버튼 클릭됨");

            createRoomDialog.SetActive(true);
        }

        public void OnClickCreateRoomDialogCreate()
        {
            createRoomDialog.SetActive(false);

            // 테스트용 기본값으로 방 생성 (특수문자 제거)
            string roomName = "테스트 방 " + System.DateTime.Now.ToString("HHmmss");
            int maxPlayers = 4;
            int silverSeedChip = 1; // 실버 시드 칩
            int goldSeedChip = 0; // 골드 시드 칩
            int silverBettingChip = 1; // 실버 베팅 칩
            int goldBettingChip = 0; // 골드 베팅 칩

            // 로딩 메시지 표시
            MessageDialogManager.Instance.Show("방을 생성하고 있습니다...");

            // API 호출
            ApiManager.Instance.CreateRoom(roomName, maxPlayers, silverSeedChip, goldSeedChip, silverBettingChip, goldBettingChip, OnCreateRoomResult);
        }

        public void OnClickQuickStart()
        {
            quickStartDialog.SetActive(true);
        }

        public void OnClickQuickStartDialogClose()
        {
            quickStartDialog.SetActive(false);
        }

        public void OnClickQuickStartDialogQuickStart()
        {
            quickStartDialog.SetActive(false);
        }

        public void OnClickCreateRoomDialogClose()
        {
            createRoomDialog.SetActive(false);
        }

        public void OnClickQuickBackToChannel()
        {
            channelPanel.SetActive(true);
            roomListPanel.SetActive(false);
        }

        public void ChannelSelected(string channelName)
        {
            currentSelectedChannel = channelName;
            if (selectedChannelText != null)
            {
                selectedChannelText.text = GetChannelDisplayName(channelName);
            }
            // 선택한 채널을 세션에 저장
            BalatroOnline.Common.SessionManager.Instance.CurrentChannel = channelName;
            channelPanel.SetActive(false);
            roomListPanel.SetActive(true);

            // 초기 방 목록 로드
            LoadRoomList();
        }

        // 채널별 입장 제한 로직
        public void OnClickChannelBeginner()
        {
            // 제한 없음
            ChannelSelected("beginner");
        }

        public void OnClickChannelIntermediate()
        {
            if (SessionManager.Instance.SilverChip < 100)
            {
                MessageDialogManager.Instance.Show("실버 칩이 100 이상이어야 입장할 수 있습니다.");
                return;
            }
            ChannelSelected("intermediate");
        }

        public void OnClickChannelExpert()
        {
            if (SessionManager.Instance.GoldChip < 1)
            {
                MessageDialogManager.Instance.Show("골드 칩이 1 이상이어야 입장할 수 있습니다.");
                return;
            }
            ChannelSelected("expert");
        }

        public void OnClickChannelMaster()
        {
            if (SessionManager.Instance.GoldChip < 100)
            {
                MessageDialogManager.Instance.Show("골드 칩이 100 이상이어야 입장할 수 있습니다.");
                return;
            }
            ChannelSelected("master");
        }

        public void OnClickChannelVIP()
        {
            if (SessionManager.Instance.GoldChip < 1000)
            {
                MessageDialogManager.Instance.Show("골드 칩이 1000 이상이어야 입장할 수 있습니다.");
                return;
            }
            ChannelSelected("unlimited_vip");
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

            // SocketManager.Instance.JoinRoom(roomId);

            SocketManager.Instance.EmitToServer(new JoinRoomRequest(roomId, SessionManager.Instance.UserId));

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

        public void OnRoomJoinSuccess(UserJoinedResponse response)
        {
            Debug.Log($"[LobbyUIManager] 방 입장 성공! 사용자: {response.userId}");

            // SocketManager에서 직접 핸들러를 호출하므로 이벤트 구독 해제 불필요

            UnityEngine.SceneManagement.SceneManager.LoadScene("InGameScene");
        }

        private string GetChannelDisplayName(string channelName)
        {
            return LocalizationManager.GetText(channelName);
        }
    }
}