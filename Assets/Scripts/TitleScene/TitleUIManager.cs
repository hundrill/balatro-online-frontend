using UnityEngine;

namespace BalatroOnline.Title
{
    /// <summary>
    /// 타이틀 씬 전용 UI를 관리하는 매니저
    /// </summary>
    public class TitleUIManager : MonoBehaviour
    {
        public static TitleUIManager Instance { get; private set; }

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

        // TODO: 타이틀 UI 관리 (버튼, 애니메이션 등)
    }
} 