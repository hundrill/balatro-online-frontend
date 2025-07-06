using UnityEngine;
using TMPro;
using BalatroOnline.Network;
using BalatroOnline.Common;
using UnityEngine.UI;
using BalatroOnline.Localization;

namespace BalatroOnline.Login
{
    /// <summary>
    /// 로그인 씬 전용 UI를 관리하는 매니저
    /// </summary>
    public class LoginUIManager : MonoBehaviour
    {
        public static LoginUIManager Instance { get; private set; }

        public TMP_InputField idInput;
        public TMP_InputField passwordInput;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        // TODO: 로그인 UI 관리 (입력, 버튼 등)

        public void OnClickLogin()
        {
            // 로그인 중 메시지 다이얼로그 표시 (언어팩)
            MessageDialogManager.Instance.Show(LocalizationManager.GetText("logging_in"));
            ApiManager.Instance.Login(idInput.text, passwordInput.text, OnLoginResult);
        }

        public void OnClickEnglish()
        {
            BalatroOnline.Localization.LocalizationManager.Load("en");
            RefreshLocalizedTexts();
        }

        public void OnClickKorean()
        {
            BalatroOnline.Localization.LocalizationManager.Load("ko");
            RefreshLocalizedTexts();
        }

        public void OnClickIndonesia()
        {
            BalatroOnline.Localization.LocalizationManager.Load("id");
            RefreshLocalizedTexts();
        }

        private void RefreshLocalizedTexts()
        {
            var all = FindObjectsByType<LocalizedText>(UnityEngine.FindObjectsSortMode.None);
            foreach (var lt in all)
            {
                lt.Refresh(); // Start() 대신 Refresh() 호출
            }
        }

        private void OnLoginResult(Network.Protocol.LoginResponse res)
        {
            if (res.success)
            {
                Debug.Log($"Login success! Welcome, {res.user?.nickname}");

                BalatroOnline.Common.SessionManager.Instance.UserId = res.user?.email;

                // 로그인 성공 후 Socket.IO 연결
                SocketManager.Instance.Connect();

                // TODO: 성공 시 씬 전환 등 처리
                UnityEngine.SceneManagement.SceneManager.LoadScene("ChannelScene");
            }
            else
            {
                Debug.LogError($"Login failed: {res.message} (code: {res.code})");
                // TODO: 실패 메시지 UI 표시
                MessageDialogManager.Instance.Show(res.message);
            }
        }
    }
}