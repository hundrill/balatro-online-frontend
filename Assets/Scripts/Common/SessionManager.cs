using UnityEngine;

namespace BalatroOnline.Common
{
    public class SessionManager : MonoBehaviour
    {
        public static SessionManager Instance { get; private set; }
        public string CurrentRoomId { get; set; }
        public string UserId { get; set; }
        // 필요시 기타 공통 데이터 추가

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
    }
} 