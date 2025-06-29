using UnityEngine;

namespace BalatroOnline.Common
{
    /// <summary>
    /// 공통 UI를 관리하는 싱글톤 매니저 (기본 구조만)
    /// </summary>
    public class CommonUIManager : MonoBehaviour
    {
        public static CommonUIManager Instance { get; private set; }

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

        // TODO: 공통 UI 관리, 팝업 등
    }
} 