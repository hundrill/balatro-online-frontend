using UnityEngine;
using TMPro;
using BalatroOnline.Network;
using BalatroOnline.Common;
using UnityEngine.UI;
using BalatroOnline.Localization;
using System.Collections.Generic; // Added for Dictionary

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

            SocketManager.Instance.Connect();
        }

        public void OnSocketConnectedForLogin()
        {
            SocketManager.Instance.EmitToServer(new LoginRequest(idInput.text, passwordInput.text));
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

        public void OnLoginResult(LoginResponse loginResponse)
        {
            if (loginResponse != null)
            {
                Debug.Log($"Login success! Welcome, {loginResponse.nickname} (Email: {loginResponse.email}, Silver: {loginResponse.silverChip}, Gold: {loginResponse.goldChip})");

                // 사용자 정보를 SessionManager에 저장
                SessionManager.Instance.UserId = loginResponse.email;
                SessionManager.Instance.UserNickname = loginResponse.nickname;
                SessionManager.Instance.SilverChip = loginResponse.silverChip;
                SessionManager.Instance.GoldChip = loginResponse.goldChip;

                // 로그인 성공 후 씬 전환
                UnityEngine.SceneManagement.SceneManager.LoadScene("LobbyScene");
            }
            else
            {
                Debug.LogError("[LoginUIManager] OnLoginResult: LoginResponse가 null입니다");
                MessageDialogManager.Instance.Show("로그인 처리 중 오류가 발생했습니다.");
            }
        }
    }
}