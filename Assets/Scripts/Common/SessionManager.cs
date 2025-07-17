using UnityEngine;

namespace BalatroOnline.Common
{
    public class SessionManager : MonoBehaviour
    {
        public static SessionManager Instance { get; private set; }
        public string CurrentRoomId { get; set; }
        public string UserId { get; set; }
        public string UserEmail { get; set; }
        public string UserNickname { get; set; }
        public int SilverChip { get; set; }
        public int GoldChip { get; set; }
        public string CurrentChannel { get; set; }
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