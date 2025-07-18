using UnityEngine;
using UnityEngine.SceneManagement;
using BalatroOnline.Common;

namespace BalatroOnline.Lobby
{
    /// <summary>
    /// 로비 씬의 전체 흐름(씬 전환 등)을 관리하는 매니저
    /// </summary>
    public class LobbySceneManager : MonoBehaviour
    {
        public static LobbySceneManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }
    }
}