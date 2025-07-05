using UnityEngine;
using BalatroOnline.Network;

namespace BalatroOnline.Login
{
    /// <summary>
    /// 로그인 씬의 전체 흐름을 관리하는 매니저
    /// </summary>
    public class LoginSceneManager : MonoBehaviour
    {
        public static LoginSceneManager Instance { get; private set; }

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
            // if (TestConfig.Instance.isTestMode)
            // {
            //     // 소켓 연결 후에 자동 로그인 및 방 입장 실행
            //     SocketManager.Instance.OnConnected += OnSocketConnectedForAutoLogin;
            //     SocketManager.Instance.Connect();
            // }
        }

        private void OnSocketConnectedForAutoLogin()
        {
            // 연결 성공 시 자동 로그인 및 방 입장
            // SocketManager.Instance.OnConnected -= OnSocketConnectedForAutoLogin;
            // AutoLoginAndEnterRoom();
        }

        private void AutoLoginAndEnterRoom()
        {
            string testEmail = "hundrill@naver.com";
            string testPassword = "1111";
            ApiManager.Instance.Login(testEmail, testPassword, (loginResult) =>
            {
                if (loginResult != null && loginResult.success)
                {
                    var user = loginResult.user;
                    // 로그인 성공 시 email을 userId로 세션에 저장
                    BalatroOnline.Common.SessionManager.Instance.UserId = user.email;
                    ApiManager.Instance.CreateRoom("테스트방", 8, (createResult) =>
                    {
                        if (createResult != null && createResult.roomId != null)
                        {
                            Debug.Log($"[AutoLoginAndEnterRoom] 방 생성 성공! RoomId: {createResult.roomId}");

                            BalatroOnline.Common.SessionManager.Instance.CurrentRoomId = createResult.roomId;
                            SocketManager.Instance.JoinRoom(createResult.roomId);

                            UnityEngine.SceneManagement.SceneManager.LoadScene("InGameScene");
                        }
                    });
                }
                else
                {
                    Debug.LogError("자동 로그인 실패: " + (loginResult != null ? loginResult.message : "응답 없음"));
                }
            });
        }

        public void OnSocketConnected()
        {
            // 방 입장 등 연결 후 처리 필요시 구현
        }
    }
}