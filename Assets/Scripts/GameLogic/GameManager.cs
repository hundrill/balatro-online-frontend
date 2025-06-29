using UnityEngine;

namespace BalatroOnline.Common
{
    /// <summary>
    /// 게임 전체를 관리하는 싱글톤 매니저
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

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

        // TODO: 전체 게임 상태, 데이터 관리 등

        // 방 ID 저장용 프로퍼티 추가
        public string CurrentRoomId { get; set; }
    }
} 