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
            StartCoroutine(ShowLogoAndTitle());
        }

        private IEnumerator ShowLogoAndTitle()
        {
            yield return new WaitForSeconds(7f);
            SceneManager.LoadScene("LoginScene");
        }
    }
} 