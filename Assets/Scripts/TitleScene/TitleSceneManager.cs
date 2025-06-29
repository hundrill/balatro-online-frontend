using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using BalatroOnline.Common;

namespace BalatroOnline.Title
{
    /// <summary>
    /// 타이틀 씬의 전체 흐름(로고, 타이틀, 씬 전환 등)을 관리하는 매니저
    /// </summary>
    public class TitleSceneManager : MonoBehaviour
    {
        public static TitleSceneManager Instance { get; private set; }

        public TextMeshProUGUI LogoText; // Inspector에서 할당
        public TextMeshProUGUI TitleText; // Inspector에서 할당

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
            if (LogoText != null) LogoText.gameObject.SetActive(true);
            if (TitleText != null) TitleText.gameObject.SetActive(false);
            StartCoroutine(ShowLogoAndTitle());
        }

        private IEnumerator ShowLogoAndTitle()
        {
            // Logo만 먼저 보여줌
            yield return new WaitForSeconds(3f);
            // 3초 뒤 Title 표시
            if (LogoText != null) LogoText.gameObject.SetActive(false);
            if (TitleText != null) TitleText.gameObject.SetActive(true);
            // Title이 보이자마자 바로 LoginScene으로 전환
            yield return new WaitForSeconds(3f);
            SceneManager.LoadScene("LoginScene");
        }
    }
} 